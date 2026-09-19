import { Injectable, computed, effect, inject, signal, untracked } from '@angular/core';
import { ProfileDetail, ProfileSummary } from '../models/profile';
import { AuthService } from './auth.service';
import { UserProfileService } from './user-profile.service';

const ACTIVE_KEY = 'active-profile-id';

/// Holds the signed-in user's named profiles and tracks which one is "active" — the profile the
/// Profile page edits and the AI features (Summarize, Smart Add) personalize to. The active choice
/// is remembered per browser. Loads whenever the user is authenticated and clears on logout.
@Injectable({ providedIn: 'root' })
export class ProfileStore {
  private readonly auth = inject(AuthService);
  private readonly service = inject(UserProfileService);

  private readonly _summaries = signal<ProfileSummary[]>([]);
  readonly summaries = this._summaries.asReadonly();

  private readonly _activeId = signal<string | null>(readActiveId());
  readonly activeId = this._activeId.asReadonly();

  private readonly _activeDetail = signal<ProfileDetail | null>(null);
  readonly activeDetail = this._activeDetail.asReadonly();

  private readonly _loading = signal(true);
  readonly loading = this._loading.asReadonly();

  readonly hasProfiles = computed(() => this._summaries().length > 0);
  readonly activeContent = computed(() => this._activeDetail()?.content ?? null);

  constructor() {
    // Only re-run when the auth state flips; the body runs untracked so setting our own signals
    // (active id/detail) can't retrigger the effect.
    effect(() => {
      const authed = this.auth.isAuthenticated();
      untracked(() => (authed ? this.loadAll() : this.clear()));
    });
  }

  /// Makes a profile the active one and loads its full detail.
  select(id: string): void {
    this._activeId.set(id);
    persistActiveId(id);
    this.service.get(id).subscribe({
      next: (detail) => this._activeDetail.set(detail),
      error: () => this._activeDetail.set(null),
    });
  }

  /// Reflects a just-created or just-saved profile without a refetch: it becomes active and its
  /// summary row is merged into the list.
  upsert(detail: ProfileDetail): void {
    const summary: ProfileSummary = { id: detail.id, name: detail.name, updatedAt: detail.updatedAt };
    this._summaries.update((list) => {
      const without = list.filter((s) => s.id !== detail.id);
      return [summary, ...without];
    });
    this._activeId.set(detail.id);
    persistActiveId(detail.id);
    this._activeDetail.set(detail);
  }

  /// Reflects a deletion: drops the row and moves the active selection to the next profile.
  removeLocal(id: string): void {
    const remaining = this._summaries().filter((s) => s.id !== id);
    this._summaries.set(remaining);
    if (this._activeId() === id) {
      const next = remaining[0]?.id ?? null;
      next ? this.select(next) : this.setActive(null);
    }
  }

  /// A compact profile for AI prompts — null when there is no active profile to personalize with.
  asPromptText(): string | null {
    const p = this.activeContent();
    if (!p) {
      return null;
    }
    const parts: string[] = [];
    if (p.headline) parts.push(p.headline);
    if (p.yearsOfExperience != null) parts.push(`${p.yearsOfExperience} years of experience`);
    if (p.skills.length) parts.push(`Skills: ${p.skills.join(', ')}`);
    if (p.certifications.length) {
      parts.push(`Certifications: ${p.certifications.map((c) => c.name).join(', ')}`);
    }
    if (p.summary) parts.push(p.summary);
    return parts.length > 0 ? parts.join('. ') : null;
  }

  private loadAll(): void {
    this._loading.set(true);
    this.service.list().subscribe({
      next: (summaries) => {
        this._summaries.set(summaries);
        const persisted = readActiveId();
        const active = summaries.find((s) => s.id === persisted) ?? summaries[0] ?? null;
        active ? this.select(active.id) : this.setActive(null);
        this._loading.set(false);
      },
      error: () => {
        this.clear();
        this._loading.set(false);
      },
    });
  }

  private setActive(id: string | null): void {
    this._activeId.set(id);
    this._activeDetail.set(null);
    persistActiveId(id);
  }

  private clear(): void {
    this._summaries.set([]);
    this._activeId.set(null);
    this._activeDetail.set(null);
  }
}

function readActiveId(): string | null {
  try {
    return localStorage.getItem(ACTIVE_KEY);
  } catch {
    return null;
  }
}

function persistActiveId(id: string | null): void {
  try {
    id ? localStorage.setItem(ACTIVE_KEY, id) : localStorage.removeItem(ACTIVE_KEY);
  } catch {
    // Private mode / blocked storage — the active choice just won't persist across reloads.
  }
}
