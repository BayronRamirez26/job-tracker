import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Profile as ProfileModel } from '../../../models/profile';
import { AiService } from '../../../services/ai.service';
import { AuthService } from '../../../services/auth.service';
import { ProfileStore } from '../../../services/profile-store';
import { ToastService } from '../../../services/toast.service';
import { UserProfileService } from '../../../services/user-profile.service';

@Component({
  selector: 'app-profile',
  imports: [DatePipe, FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile {
  private readonly auth = inject(AuthService);
  private readonly ai = inject(AiService);
  private readonly profiles = inject(UserProfileService);
  private readonly profileStore = inject(ProfileStore);
  private readonly toasts = inject(ToastService);

  protected readonly user = this.auth.user;

  protected readonly profile = signal<ProfileModel | null>(null);
  protected readonly loadingProfile = signal(true);
  protected readonly extracting = signal(false);
  protected readonly saving = signal(false);
  protected readonly dirty = signal(false);

  protected cvText = '';

  constructor() {
    this.profiles.get().subscribe({
      next: (p) => {
        this.profile.set(p);
        this.loadingProfile.set(false);
      },
      error: () => this.loadingProfile.set(false),
    });
  }

  protected buildFromCv(): void {
    const cv = this.cvText.trim();
    if (cv.length === 0) {
      return;
    }
    this.extracting.set(true);
    this.ai.parseCv(cv).subscribe({
      next: (r) => {
        const { model, ...profile } = r; // drop the model field; keep the profile
        void model;
        this.profile.set(profile);
        this.dirty.set(true);
        this.extracting.set(false);
        this.toasts.success('Profile built from your CV — review and save.');
      },
      error: (err: HttpErrorResponse) => {
        this.extracting.set(false);
        this.toasts.error(
          err.status === 503 ? 'The AI service is unavailable right now.' : 'Could not read that CV.',
        );
      },
    });
  }

  protected save(): void {
    const p = this.profile();
    if (!p) {
      return;
    }
    this.saving.set(true);
    this.profiles.save(p).subscribe({
      next: (saved) => {
        // Keep the shared store fresh so Summarize / Smart Add personalize to the new profile.
        this.profileStore.set(saved);
        this.saving.set(false);
        this.dirty.set(false);
        this.toasts.success('Profile saved.');
      },
      error: () => {
        this.saving.set(false);
        this.toasts.error('Could not save the profile.');
      },
    });
  }
}
