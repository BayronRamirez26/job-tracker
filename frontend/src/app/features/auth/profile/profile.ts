import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Component, effect, inject, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  Profile as ProfileContent,
  emptyProfile,
} from '../../../models/profile';
import { AiService } from '../../../services/ai.service';
import { AuthService } from '../../../services/auth.service';
import { ProfileStore } from '../../../services/profile-store';
import { ToastService } from '../../../services/toast.service';
import { UserProfileService } from '../../../services/user-profile.service';

/// Manages the user's named professional profiles: a switcher across profiles plus a full editor
/// (build-from-CV, then hand-edit every field). The editor mirrors the store's active profile; all
/// state is signal-based because the app runs zoneless.
@Component({
  selector: 'app-profile',
  imports: [DatePipe, FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile {
  private readonly auth = inject(AuthService);
  private readonly ai = inject(AiService);
  private readonly service = inject(UserProfileService);
  private readonly toasts = inject(ToastService);
  protected readonly store = inject(ProfileStore);

  protected readonly user = this.auth.user;

  // Editing state — mirrors the active profile, except while composing a brand-new one.
  protected readonly editingId = signal<string | null>(null);
  protected readonly isNew = signal(false);
  protected readonly name = signal('');
  protected readonly draft = signal<ProfileContent>(emptyProfile());
  protected readonly dirty = signal(false);

  protected readonly cvText = signal('');
  protected readonly skillInput = signal('');
  protected readonly linkInput = signal('');

  protected readonly saving = signal(false);
  protected readonly deleting = signal(false);
  protected readonly extracting = signal(false);

  constructor() {
    // Keep the editor in sync with whichever profile the store has active. Skipped while composing
    // a new profile so we don't clobber the in-progress draft.
    effect(() => {
      const detail = this.store.activeDetail();
      untracked(() => {
        if (this.isNew()) {
          return;
        }
        if (detail) {
          this.editingId.set(detail.id);
          this.name.set(detail.name);
          this.draft.set(structuredClone(detail.content));
          this.dirty.set(false);
        } else {
          this.editingId.set(null);
          this.name.set('');
          this.draft.set(emptyProfile());
          this.dirty.set(false);
        }
      });
    });
  }

  // ---- Switching / creating / deleting -------------------------------------------------------

  protected selectProfile(id: string): void {
    if (id === this.editingId() && !this.isNew()) {
      return;
    }
    this.isNew.set(false);
    this.store.select(id);
  }

  protected startNew(): void {
    this.isNew.set(true);
    this.editingId.set(null);
    this.name.set('New profile');
    this.draft.set(emptyProfile());
    this.cvText.set('');
    this.dirty.set(true);
  }

  protected save(): void {
    const name = this.name().trim();
    if (!name) {
      this.toasts.error('Give the profile a name first.');
      return;
    }

    this.saving.set(true);
    const request = { name, content: cleanContent(this.draft()) };
    const id = this.editingId();
    const op = this.isNew() || !id ? this.service.create(request) : this.service.update(id, request);

    op.subscribe({
      next: (detail) => {
        this.isNew.set(false);
        this.store.upsert(detail); // becomes active → the effect reloads a clean editor
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

  protected remove(): void {
    if (this.isNew()) {
      this.discardNew();
      return;
    }
    const id = this.editingId();
    if (!id) {
      return;
    }

    this.deleting.set(true);
    this.service.delete(id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.toasts.success('Profile deleted.');
        this.store.removeLocal(id); // moves active to the next profile → the effect reloads it
      },
      error: () => {
        this.deleting.set(false);
        this.toasts.error('Could not delete the profile.');
      },
    });
  }

  /// Abandons an in-progress new profile and returns the editor to the active one (or empty state).
  private discardNew(): void {
    this.isNew.set(false);
    const active = this.store.activeDetail();
    if (active) {
      this.editingId.set(active.id);
      this.name.set(active.name);
      this.draft.set(structuredClone(active.content));
    } else {
      this.editingId.set(null);
      this.name.set('');
      this.draft.set(emptyProfile());
    }
    this.dirty.set(false);
  }

  // ---- Build from CV --------------------------------------------------------------------------

  protected buildFromCv(): void {
    const cv = this.cvText().trim();
    if (cv.length === 0) {
      return;
    }
    this.extracting.set(true);
    this.ai.parseCv(cv).subscribe({
      next: (r) => {
        // Coalesce every array so a field the AI omits can't break the editor.
        this.draft.set({
          fullName: r.fullName ?? null,
          headline: r.headline ?? null,
          summary: r.summary ?? null,
          location: r.location ?? null,
          yearsOfExperience: r.yearsOfExperience ?? null,
          skills: r.skills ?? [],
          experience: r.experience ?? [],
          education: r.education ?? [],
          certifications: r.certifications ?? [],
          links: r.links ?? [],
        });
        this.dirty.set(true);
        this.extracting.set(false);
        this.toasts.success('Filled from your CV — review, tweak, and save.');
      },
      error: (err: HttpErrorResponse) => {
        this.extracting.set(false);
        this.toasts.error(
          err.status === 503 ? 'The AI service is unavailable right now.' : 'Could not read that CV.',
        );
      },
    });
  }

  // ---- Field editing (immutable updates; the app is zoneless) ---------------------------------

  private patch(partial: Partial<ProfileContent>): void {
    this.draft.update((d) => ({ ...d, ...partial }));
    this.dirty.set(true);
  }

  protected onName(value: string): void {
    this.name.set(value);
    this.dirty.set(true);
  }

  protected setFullName(v: string): void { this.patch({ fullName: blankToNull(v) }); }
  protected setHeadline(v: string): void { this.patch({ headline: blankToNull(v) }); }
  protected setLocation(v: string): void { this.patch({ location: blankToNull(v) }); }
  protected setSummary(v: string): void { this.patch({ summary: blankToNull(v) }); }

  protected setYears(v: number | null): void {
    // A type="number" control emits a number (or null when cleared), not a string.
    this.patch({ yearsOfExperience: v != null && Number.isFinite(v) ? v : null });
  }

  // Skills
  protected addSkill(): void {
    const s = this.skillInput().trim();
    if (!s) return;
    this.patch({ skills: [...this.draft().skills, s] });
    this.skillInput.set('');
  }
  protected removeSkill(i: number): void {
    this.patch({ skills: this.draft().skills.filter((_, idx) => idx !== i) });
  }

  // Links
  protected addLink(): void {
    const l = this.linkInput().trim();
    if (!l) return;
    this.patch({ links: [...this.draft().links, l] });
    this.linkInput.set('');
  }
  protected removeLink(i: number): void {
    this.patch({ links: this.draft().links.filter((_, idx) => idx !== i) });
  }

  // Certifications
  protected addCertification(): void {
    this.patch({ certifications: [...this.draft().certifications, { name: '', issuer: null, year: null }] });
  }
  protected setCertification(i: number, key: 'name' | 'issuer' | 'year', value: string): void {
    const list = this.draft().certifications.map((c, idx) =>
      idx === i ? { ...c, [key]: key === 'name' ? value : blankToNull(value) } : c,
    );
    this.patch({ certifications: list });
  }
  protected removeCertification(i: number): void {
    this.patch({ certifications: this.draft().certifications.filter((_, idx) => idx !== i) });
  }

  // Experience
  protected addExperience(): void {
    this.patch({
      experience: [...this.draft().experience, { company: '', title: null, period: null, highlights: [] }],
    });
  }
  protected setExperience(i: number, key: 'company' | 'title' | 'period', value: string): void {
    const list = this.draft().experience.map((e, idx) =>
      idx === i ? { ...e, [key]: key === 'company' ? value : blankToNull(value) } : e,
    );
    this.patch({ experience: list });
  }
  protected highlightsText(i: number): string {
    return this.draft().experience[i]?.highlights.join('\n') ?? '';
  }
  protected setHighlights(i: number, text: string): void {
    // Keep raw lines while typing (so a fresh blank line isn't swallowed); cleaned up on save.
    const highlights = text.split('\n');
    const list = this.draft().experience.map((e, idx) => (idx === i ? { ...e, highlights } : e));
    this.patch({ experience: list });
  }
  protected removeExperience(i: number): void {
    this.patch({ experience: this.draft().experience.filter((_, idx) => idx !== i) });
  }

  // Education
  protected addEducation(): void {
    this.patch({ education: [...this.draft().education, { institution: '', degree: null, year: null }] });
  }
  protected setEducation(i: number, key: 'institution' | 'degree' | 'year', value: string): void {
    const list = this.draft().education.map((e, idx) =>
      idx === i ? { ...e, [key]: key === 'institution' ? value : blankToNull(value) } : e,
    );
    this.patch({ education: list });
  }
  protected removeEducation(i: number): void {
    this.patch({ education: this.draft().education.filter((_, idx) => idx !== i) });
  }
}

function blankToNull(value: string): string | null {
  return value.trim().length > 0 ? value : null;
}

/// Drops empty rows and blank highlight lines the user left behind before persisting.
function cleanContent(content: ProfileContent): ProfileContent {
  return {
    ...content,
    skills: content.skills.map((s) => s.trim()).filter((s) => s.length > 0),
    links: content.links.map((l) => l.trim()).filter((l) => l.length > 0),
    certifications: content.certifications
      .map((c) => ({ ...c, name: c.name.trim() }))
      .filter((c) => c.name.length > 0),
    experience: content.experience
      .map((e) => ({ ...e, highlights: e.highlights.map((h) => h.trim()).filter((h) => h.length > 0) }))
      .filter((e) => e.company.trim().length > 0 || e.highlights.length > 0),
    education: content.education.filter((e) => e.institution.trim().length > 0),
  };
}
