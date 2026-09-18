import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  APPLICATION_SOURCES,
  APPLICATION_STATUSES,
  CreateJobApplicationRequest,
  JobApplication,
} from '../../../models/job-application';
import { ApplicationsStore } from '../../../services/applications-store';
import { humanize } from '../../../shared/humanize';
import { ApplicationsBoard } from '../applications-board/applications-board';

type View = 'table' | 'board';
const VIEW_KEY = 'jobtracker.appsView';

@Component({
  selector: 'app-applications-list',
  imports: [FormsModule, ApplicationsBoard],
  templateUrl: './applications-list.html',
  styleUrl: './applications-list.css',
})
export class ApplicationsList {
  private readonly store = inject(ApplicationsStore);

  protected readonly statuses = APPLICATION_STATUSES;
  protected readonly sources = APPLICATION_SOURCES;

  // State lives in the shared store, so the table and board never disagree.
  protected readonly applications = this.store.applications;
  protected readonly loading = this.store.loading;
  protected readonly error = this.store.error;

  protected readonly view = signal<View>(this.initialView());

  protected form: CreateJobApplicationRequest = this.blankForm();

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

  protected create(): void {
    // Empty optional fields become null (the API expects null, not "").
    const request: CreateJobApplicationRequest = {
      ...this.form,
      appliedDate: this.form.appliedDate || null,
      notes: this.form.notes || null,
    };
    this.store.add(request, () => {
      this.form = this.blankForm();
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
