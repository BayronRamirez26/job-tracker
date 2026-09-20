import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Component, computed, effect, inject, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  APPLICATION_SOURCES,
  APPLICATION_STATUSES,
  ApplicationSource,
  ApplicationStatus,
  CreateJobApplicationRequest,
} from '../../../models/job-application';
import { FitResponse } from '../../../models/assistant';
import { AiService } from '../../../services/ai.service';
import { ApplicationsStore } from '../../../services/applications-store';
import { ProfileStore } from '../../../services/profile-store';
import { ToastService } from '../../../services/toast.service';
import { humanize } from '../../../shared/humanize';

/// A slide-in panel to view and edit one application. Opens from the shared store's `selected`
/// signal, so both the table and the board can trigger it. All fields are signal-backed (zoneless).
@Component({
  selector: 'app-application-drawer',
  imports: [FormsModule, DatePipe, RouterLink],
  templateUrl: './application-drawer.html',
  styleUrl: './application-drawer.css',
  host: { '(document:keydown.escape)': 'close()' },
})
export class ApplicationDrawer {
  protected readonly store = inject(ApplicationsStore);
  protected readonly profiles = inject(ProfileStore);
  private readonly ai = inject(AiService);
  private readonly toasts = inject(ToastService);

  protected readonly statuses = APPLICATION_STATUSES;
  protected readonly sources = APPLICATION_SOURCES;
  protected readonly humanize = humanize;

  // AI assistant results for the open application (reset when a different one opens).
  protected readonly fitResult = signal<FitResponse | null>(null);
  protected readonly coverLetter = signal<string | null>(null);
  protected readonly tailoredCv = signal<string | null>(null);
  protected readonly tailoredCvFormat = signal<'markdown' | 'latex'>('markdown');
  protected readonly busy = signal<'fit' | 'letter' | 'cv' | 'tex' | null>(null);

  protected readonly hasProfile = computed(() => this.profiles.activeContent() !== null);
  protected readonly canAssist = computed(() => this.jobDescription().trim().length > 0);

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
        // Assistant output is per-application, so clear it when a different one opens.
        this.fitResult.set(null);
        this.coverLetter.set(null);
        this.tailoredCv.set(null);
        this.tailoredCvFormat.set('markdown');
        this.busy.set(null);
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

  // --- AI assistant ----------------------------------------------------------------------------

  protected scoreFit(): void {
    const jd = this.jobDescription().trim();
    if (!jd) {
      return;
    }
    this.busy.set('fit');
    this.ai.fit(jd, this.profiles.asResumeText()).subscribe({
      next: (r) => {
        this.fitResult.set(r);
        this.busy.set(null);
      },
      error: (err: HttpErrorResponse) => this.assistFailed(err),
    });
  }

  protected writeCoverLetter(): void {
    const jd = this.jobDescription().trim();
    if (!jd) {
      return;
    }
    this.busy.set('letter');
    this.ai.coverLetter(jd, this.profiles.asResumeText()).subscribe({
      next: (r) => {
        this.coverLetter.set(r.letter);
        this.busy.set(null);
      },
      error: (err: HttpErrorResponse) => this.assistFailed(err),
    });
  }

  protected tailorResume(format: 'markdown' | 'latex'): void {
    const jd = this.jobDescription().trim();
    if (!jd) {
      return;
    }
    this.busy.set(format === 'latex' ? 'tex' : 'cv');
    this.ai.tailorCv(jd, this.profiles.asResumeText(), format).subscribe({
      next: (r) => {
        this.tailoredCv.set(r.content);
        this.tailoredCvFormat.set(r.format);
        this.busy.set(null);
      },
      error: (err: HttpErrorResponse) => this.assistFailed(err),
    });
  }

  /// Saves the generated LaTeX résumé as a .tex file (a client-side download the user initiates).
  protected downloadTex(content: string): void {
    try {
      const blob = new Blob([content], { type: 'application/x-tex' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'resume.tex';
      a.click();
      URL.revokeObjectURL(url);
    } catch {
      this.toasts.error('Could not download the file.');
    }
  }

  protected copy(text: string): void {
    navigator.clipboard?.writeText(text).then(
      () => this.toasts.success('Copied to clipboard.'),
      () => this.toasts.error('Could not copy.'),
    );
  }

  private assistFailed(err: HttpErrorResponse): void {
    this.busy.set(null);
    this.toasts.error(
      err.status === 503 ? 'The AI service is unavailable right now.' : 'The assistant could not respond. Try again.',
    );
  }
}
