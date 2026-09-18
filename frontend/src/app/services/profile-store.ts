import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { Profile } from '../models/profile';
import { AuthService } from './auth.service';
import { UserProfileService } from './user-profile.service';

/// Holds the signed-in user's professional profile so the AI features can personalize to it.
/// Loads it whenever the user is authenticated and clears it on logout.
@Injectable({ providedIn: 'root' })
export class ProfileStore {
  private readonly auth = inject(AuthService);
  private readonly service = inject(UserProfileService);

  private readonly _profile = signal<Profile | null>(null);
  readonly profile = this._profile.asReadonly();
  readonly hasProfile = computed(() => this._profile() !== null);

  constructor() {
    effect(() => {
      if (this.auth.isAuthenticated()) {
        this.service.get().subscribe({
          next: (p) => this._profile.set(p),
          error: () => this._profile.set(null),
        });
      } else {
        this._profile.set(null);
      }
    });
  }

  /// Called when the profile is saved elsewhere (the Profile page) to keep this in sync.
  set(profile: Profile | null): void {
    this._profile.set(profile);
  }

  /// A compact profile for AI prompts — null when there is no profile to personalize with.
  asPromptText(): string | null {
    const p = this._profile();
    if (!p) {
      return null;
    }
    const parts: string[] = [];
    if (p.headline) parts.push(p.headline);
    if (p.yearsOfExperience != null) parts.push(`${p.yearsOfExperience} years of experience`);
    if (p.skills.length) parts.push(`Skills: ${p.skills.join(', ')}`);
    if (p.summary) parts.push(p.summary);
    return parts.length > 0 ? parts.join('. ') : null;
  }
}
