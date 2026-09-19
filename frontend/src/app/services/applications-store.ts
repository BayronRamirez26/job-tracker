import { Injectable, inject, signal } from '@angular/core';
import {
  ApplicationStatus,
  CreateJobApplicationRequest,
  JobApplication,
} from '../models/job-application';
import { humanize } from '../shared/humanize';
import { ApplicationsService } from './applications.service';
import { ToastService } from './toast.service';

/// Single source of truth for the applications list, shared by the table and board views.
/// Owns optimistic add / delete / status-change so both views stay in sync instantly.
@Injectable({ providedIn: 'root' })
export class ApplicationsStore {
  private readonly service = inject(ApplicationsService);
  private readonly toasts = inject(ToastService);

  private readonly _apps = signal<JobApplication[]>([]);
  readonly applications = this._apps.asReadonly();
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  // The application currently open in the detail drawer (null = closed).
  private readonly _selected = signal<JobApplication | null>(null);
  readonly selected = this._selected.asReadonly();

  openEdit(app: JobApplication): void {
    this._selected.set(app);
  }

  closeEdit(): void {
    this._selected.set(null);
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.service.list().subscribe({
      next: (apps) => {
        this._apps.set(apps);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load applications. Is the API gateway running on :8080?');
        this.loading.set(false);
      },
    });
  }

  add(request: CreateJobApplicationRequest, onSuccess?: () => void): void {
    this.service.create(request).subscribe({
      next: (created) => {
        this._apps.update((apps) => [created, ...apps]);
        this.toasts.success(`Added ${created.company}.`);
        onSuccess?.();
      },
      error: () => this.toasts.error('Could not add the application.'),
    });
  }

  /// Moves an application to a new stage. Updates the signal immediately (so the card jumps
  /// columns at once), then PUTs the full record; rolls back on failure.
  changeStatus(app: JobApplication, status: ApplicationStatus): void {
    if (app.status === status) {
      return;
    }
    const previous = this._apps();
    this._apps.update((apps) => apps.map((a) => (a.id === app.id ? { ...a, status } : a)));

    const request: CreateJobApplicationRequest = {
      company: app.company,
      position: app.position,
      status,
      source: app.source,
      appliedDate: app.appliedDate,
      notes: app.notes,
      salary: app.salary,
    };
    this.service.update(app.id, request).subscribe({
      next: (updated) => {
        this._apps.update((apps) => apps.map((a) => (a.id === updated.id ? updated : a)));
        this.toasts.success(`${app.company} → ${humanize(status)}.`);
      },
      error: () => {
        this._apps.set(previous);
        this.toasts.error('Could not move the application.');
      },
    });
  }

  /// Saves a full edit from the detail drawer. Updates the row optimistically, then PUTs the record;
  /// rolls back on failure. `onSettled` reports success so the drawer can close (or stay open).
  update(id: string, request: CreateJobApplicationRequest, onSettled?: (ok: boolean) => void): void {
    const previous = this._apps();
    this._apps.update((apps) => apps.map((a) => (a.id === id ? { ...a, ...request } : a)));

    this.service.update(id, request).subscribe({
      next: (updated) => {
        this._apps.update((apps) => apps.map((a) => (a.id === updated.id ? updated : a)));
        this._selected.update((s) => (s && s.id === updated.id ? updated : s));
        this.toasts.success(`Updated ${updated.company}.`);
        onSettled?.(true);
      },
      error: () => {
        this._apps.set(previous);
        this.toasts.error('Could not update the application.');
        onSettled?.(false);
      },
    });
  }

  remove(app: JobApplication): void {
    const previous = this._apps();
    this._apps.update((apps) => apps.filter((a) => a.id !== app.id));
    this.service.remove(app.id).subscribe({
      next: () => this.toasts.success(`Deleted ${app.company}.`),
      error: () => {
        this._apps.set(previous);
        this.toasts.error('Could not delete the application.');
      },
    });
  }
}
