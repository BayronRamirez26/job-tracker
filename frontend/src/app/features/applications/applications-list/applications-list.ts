import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  APPLICATION_SOURCES,
  APPLICATION_STATUSES,
  ApplicationStatus,
  CreateJobApplicationRequest,
  JobApplication,
} from '../../../models/job-application';
import { AiService } from '../../../services/ai.service';
import { ApplicationsStore } from '../../../services/applications-store';
import { ProfileStore } from '../../../services/profile-store';
import { ToastService } from '../../../services/toast.service';
import { humanize } from '../../../shared/humanize';
import { ApplicationsBoard } from '../applications-board/applications-board';
import { ApplicationDrawer } from '../application-drawer/application-drawer';
import { ProfilePicker } from '../../../components/profile-picker/profile-picker';

type View = 'table' | 'board';
interface SalaryForm {
  min: number | null;
  max: number | null;
  currency: string;
}

type StatusFilter = 'all' | ApplicationStatus;

const VIEW_KEY = 'jobtracker.appsView';
const FILTER_KEY = 'jobtracker.appsStatusFilter';

@Component({
  selector: 'app-applications-list',
  imports: [FormsModule, ApplicationsBoard, ApplicationDrawer, ProfilePicker],
  templateUrl: './applications-list.html',
  styleUrl: './applications-list.css',
})
export class ApplicationsList {
  private readonly store = inject(ApplicationsStore);
  private readonly ai = inject(AiService);
  private readonly profiles = inject(ProfileStore);
  private readonly toasts = inject(ToastService);

  protected readonly statuses = APPLICATION_STATUSES;
  protected readonly sources = APPLICATION_SOURCES;

  // State lives in the shared store, so the table and board never disagree.
  protected readonly applications = this.store.applications;
  protected readonly loading = this.store.loading;
  protected readonly error = this.store.error;

  protected readonly view = signal<View>(this.initialView());

  // Status filter for the table view (persisted, like the view toggle).
  protected readonly statusFilter = signal<StatusFilter>(this.initialFilter());
  protected readonly filteredApplications = computed(() => {
    const filter = this.statusFilter();
    const apps = this.applications();
    return filter === 'all' ? apps : apps.filter((a) => a.status === filter);
  });
  // Count per status (plus a total), so each filter chip can show how many it holds.
  protected readonly statusCounts = computed(() => {
    const counts = { all: this.applications().length } as Record<StatusFilter, number>;
    for (const s of APPLICATION_STATUSES) counts[s] = 0;
    for (const a of this.applications()) counts[a.status]++;
    return counts;
  });

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

  protected setStatusFilter(filter: StatusFilter): void {
    this.statusFilter.set(filter);
    try {
      localStorage.setItem(FILTER_KEY, filter);
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
      // Keep the pasted posting on the record so the AI assistant can use it later.
      jobDescription: this.smartText.trim() || null,
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

  protected openEdit(app: JobApplication): void {
    this.store.openEdit(app);
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
      jobDescription: null,
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

  private initialFilter(): StatusFilter {
    try {
      const f = localStorage.getItem(FILTER_KEY);
      if (f === 'all' || (f && (APPLICATION_STATUSES as readonly string[]).includes(f))) {
        return f as StatusFilter;
      }
    } catch {
      /* ignore */
    }
    return 'all';
  }
}
