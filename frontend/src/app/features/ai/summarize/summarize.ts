import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SummaryResponse } from '../../../models/summary';
import { AiService } from '../../../services/ai.service';
import { ProfileStore } from '../../../services/profile-store';

@Component({
  selector: 'app-summarize',
  imports: [FormsModule],
  templateUrl: './summarize.html',
  styleUrl: './summarize.css',
})
export class Summarize {
  private readonly ai = inject(AiService);
  private readonly profiles = inject(ProfileStore);

  // When the user has a saved profile, the summary is tailored to them.
  protected readonly personalized = this.profiles.hasProfile;

  // Matches JobDescription.MaxLength on the server, so we stop the user before the API does.
  protected readonly maxLength = 20_000;

  protected jobDescription = '';

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly result = signal<SummaryResponse | null>(null);

  // The model returns one plain string; split it into individual bullet lines and
  // strip any leading "-", "*" or "•" marker so we can render a real <ul>.
  protected readonly bullets = computed(() => {
    const text = this.result()?.summary ?? '';
    return text
      .split('\n')
      .map((line) =>
        line
          .replace(/^\s*[-*•]\s+/, '') // drop a leading "-", "*" or "•" bullet marker
          .replace(/\*\*/g, '') // drop markdown bold markers Claude sometimes adds
          .trim(),
      )
      .filter((line) => line.length > 0);
  });

  protected summarize(): void {
    const text = this.jobDescription.trim();
    if (text.length === 0) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.result.set(null);

    this.ai.summarize(text, this.profiles.asPromptText()).subscribe({
      next: (response) => {
        this.result.set(response);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.error.set(this.messageFor(err));
        this.loading.set(false);
      },
    });
  }

  protected clear(): void {
    this.jobDescription = '';
    this.result.set(null);
    this.error.set(null);
  }

  // Turn the HTTP status into something a human can act on.
  private messageFor(err: HttpErrorResponse): string {
    switch (err.status) {
      case 0:
        return 'Could not reach the API gateway on :8080. Is the Docker stack running?';
      case 400:
        return 'Please paste a job description first (up to 20,000 characters).';
      case 503:
        return 'The AI service is unavailable — it may be out of API credits, or the ai-api container is down.';
      default:
        return 'Something went wrong generating the summary. Please try again.';
    }
  }
}
