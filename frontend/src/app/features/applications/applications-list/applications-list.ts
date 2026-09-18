import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  APPLICATION_SOURCES,
  APPLICATION_STATUSES,
  CreateJobApplicationRequest,
  JobApplication,
} from '../../../models/job-application';
import { AiService } from '../../../services/ai.service';
import { ApplicationsStore } from '../../../services/applications-store';
import { ProfileStore } from '../../../services/profile-store';
import { ToastService } from '../../../services/toast.service';
import { humanize } from '../../../shared/humanize';
import { ApplicationsBoard } from '../applications-board/applications-board';

type View = 'table' | 'board';
interface SalaryForm {
  min: number | null;
  max: number | null;
  currency: string;
}

const VIEW_KEY = 'jobtracker.appsView';

@Component({
  selector: 'app-applications-list',
  imports: [FormsModule, ApplicationsBoard],
  templateUrl: './applications-list.html',
  styleUrl: './applications-list.css',
})
export class ApplicationsList {
  private readonly store = inject(ApplicationsStore);
  private readonly ai = inject(AiService);
  private readonly profiles = inject(ProfileStore);
  private readonly toasts = inject(ToastService);

  // When the user has a saved profile, Smart Add frames the notes around fit for them.
  protected readonly personalized = this.profiles.hasProfile;

  protected readonly statuses = APPLICATION_STATUSES;
  protected readonly sources = APPLICATION_SOURCES;

  // State lives in the shared store, so the table and board never disagree.
  protected readonly applications = this.store.applications;
  protected readonly loading = this.store.loading;
  protected readonly error = this.store.error;

  protected readonly view = signal<View>(this.initialView());

  protected form: CreateJobApplicationRequest = this.blankForm();
  protected salary: SalaryForm = this.blankSalary();

  // Smart Add: paste a description, let the AI fill the fields.
  protected smartText = '';
  protected readonly extracting = signal(false);

  protected readonly humanize = humanize;

  constructor() {
    this.store.load();
  }

  protected setView(view: View): void {
    this.view.set(view);
    try {
      localStorage.setItem(VIEW_KEY, view);
    } catch {
      /* ignore */
    }
  }

  protected load(): void {
    this.store.load();
  }

  protected extract(): void {
    const text = this.smartText.trim();
    if (text.length === 0) {
      return;
    }
    this.extracting.set(true);
    this.ai.extract(text, this.profiles.asPromptText()).subscribe({
      next: (r) => {
        // Fill only what the model actually found; leave the rest for the user.
        if (r.company) this.form.company = r.company;
        if (r.position) this.form.position = r.position;
        if (r.notes) this.form.notes = r.notes;
        if (r.salary) {
          this.salary = {
            min: r.salary.min,
            max: r.salary.max,
            currency: (r.salary.currency || 'USD').toUpperCase(),
          };
        }
        this.extracting.set(false);
        this.toasts.success('Filled the form from the description.');
      },
      error: (err: HttpErrorResponse) => {
        this.extracting.set(false);
        this.toasts.error(
          err.status === 503
            ? 'The AI service is unavailable — it may be out of credits.'
            : 'Could not read that job description.',
        );
      },
    });
  }

  protected create(): void {
    // Salary is optional — only send it when both bounds are filled in.
    const salary =
      this.salary.min != null && this.salary.max != null
        ? {
            min: this.salary.min,
            max: this.salary.max,
            currency: (this.salary.currency || 'USD').toUpperCase(),
          }
        : null;

    const request: CreateJobApplicationRequest = {
      ...this.form,
      appliedDate: this.form.appliedDate || null,
      notes: this.form.notes || null,
      salary,
    };

    this.store.add(request, () => {
      this.form = this.blankForm();
      this.salary = this.blankSalary();
      this.smartText = '';
    });
  }

  protected remove(app: JobApplication): void {
    this.store.remove(app);
  }

  private blankForm(): CreateJobApplicationRequest {
    return {
      company: '',
      position: '',
      status: 'Wishlist',
      source: 'LinkedIn',
      appliedDate: null,
      notes: null,
      salary: null,
    };
  }

  private blankSalary(): SalaryForm {
    return { min: null, max: null, currency: 'USD' };
  }

  private initialView(): View {
    try {
      const v = localStorage.getItem(VIEW_KEY);
      if (v === 'table' || v === 'board') {
        return v;
      }
    } catch {
      /* ignore */
    }
    return 'table';
  }
}
