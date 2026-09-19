import { DatePipe } from '@angular/common';
import { Component, effect, inject, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  APPLICATION_SOURCES,
  APPLICATION_STATUSES,
  ApplicationSource,
  ApplicationStatus,
  CreateJobApplicationRequest,
} from '../../../models/job-application';
import { ApplicationsStore } from '../../../services/applications-store';
import { ToastService } from '../../../services/toast.service';
import { humanize } from '../../../shared/humanize';

/// A slide-in panel to view and edit one application. Opens from the shared store's `selected`
/// signal, so both the table and the board can trigger it. All fields are signal-backed (zoneless).
@Component({
  selector: 'app-application-drawer',
  imports: [FormsModule, DatePipe],
  templateUrl: './application-drawer.html',
  styleUrl: './application-drawer.css',
  host: { '(document:keydown.escape)': 'close()' },
})
export class ApplicationDrawer {
  protected readonly store = inject(ApplicationsStore);
  private readonly toasts = inject(ToastService);

  protected readonly statuses = APPLICATION_STATUSES;
  protected readonly sources = APPLICATION_SOURCES;
  protected readonly humanize = humanize;

  // Editable copy — repopulated whenever a different application is opened.
  protected readonly company = signal('');
  protected readonly position = signal('');
  protected readonly status = signal<ApplicationStatus>('Wishlist');
  protected readonly source = signal<ApplicationSource>('Other');
  protected readonly appliedDate = signal('');
  protected readonly notes = signal('');
  protected readonly jobDescription = signal('');
  protected readonly salaryMin = signal<number | null>(null);
  protected readonly salaryMax = signal<number | null>(null);
  protected readonly currency = signal('USD');
  protected readonly saving = signal(false);

  constructor() {
    effect(() => {
      const app = this.store.selected();
      untracked(() => {
        if (!app) {
          return;
        }
        this.company.set(app.company);
        this.position.set(app.position);
        this.status.set(app.status);
        this.source.set(app.source);
        this.appliedDate.set(app.appliedDate ?? '');
        this.notes.set(app.notes ?? '');
        this.jobDescription.set(app.jobDescription ?? '');
        this.salaryMin.set(app.salary?.min ?? null);
        this.salaryMax.set(app.salary?.max ?? null);
        this.currency.set(app.salary?.currency ?? 'USD');
        this.saving.set(false);
      });
    });
  }

  protected save(): void {
    const app = this.store.selected();
    if (!app) {
      return;
    }
    const company = this.company().trim();
    const position = this.position().trim();
    if (!company || !position) {
      this.toasts.error('Company and position are required.');
      return;
    }

    const min = this.salaryMin();
    const max = this.salaryMax();
    const salary =
      min != null && max != null ? { min, max, currency: (this.currency() || 'USD').toUpperCase() } : null;

    const request: CreateJobApplicationRequest = {
      company,
      position,
      status: this.status(),
      source: this.source(),
      appliedDate: this.appliedDate() || null,
      notes: this.notes().trim() || null,
      salary,
      jobDescription: this.jobDescription().trim() || null,
    };

    this.saving.set(true);
    this.store.update(app.id, request, (ok) => {
      this.saving.set(false);
      if (ok) {
        this.store.closeEdit();
      }
    });
  }

  protected remove(): void {
    const app = this.store.selected();
    if (!app) {
      return;
    }
    this.store.remove(app);
    this.store.closeEdit();
  }

  protected close(): void {
    this.store.closeEdit();
  }
}
