import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  APPLICATION_SOURCES,
  APPLICATION_STATUSES,
  CreateJobApplicationRequest,
  JobApplication,
} from '../../../models/job-application';
import { ApplicationsService } from '../../../services/applications.service';

@Component({
  selector: 'app-applications-list',
  imports: [FormsModule],
  templateUrl: './applications-list.html',
  styleUrl: './applications-list.css',
})
export class ApplicationsList {
  private readonly service = inject(ApplicationsService);

  protected readonly statuses = APPLICATION_STATUSES;
  protected readonly sources = APPLICATION_SOURCES;

  protected readonly applications = signal<JobApplication[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected form: CreateJobApplicationRequest = this.blankForm();

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.service.list().subscribe({
      next: (apps) => {
        this.applications.set(apps);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load applications. Is the API gateway running on :8080?');
        this.loading.set(false);
      },
    });
  }

  protected create(): void {
    // Empty optional fields become null (the API expects null, not "").
    const request: CreateJobApplicationRequest = {
      ...this.form,
      appliedDate: this.form.appliedDate || null,
      notes: this.form.notes || null,
    };

    this.service.create(request).subscribe({
      next: () => {
        this.form = this.blankForm();
        this.load();
      },
      error: () => this.error.set('Could not create the application.'),
    });
  }

  protected remove(app: JobApplication): void {
    this.service.remove(app.id).subscribe({
      next: () => this.load(),
      error: () => this.error.set('Could not delete the application.'),
    });
  }

  /** Turn a PascalCase enum value ("PhoneScreen") into a readable label ("Phone Screen"). */
  protected humanize(value: string): string {
    // Brand names that aren't really two words.
    const exceptions: Record<string, string> = { LinkedIn: 'LinkedIn' };
    return exceptions[value] ?? value.replace(/([a-z])([A-Z])/g, '$1 $2');
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
}
