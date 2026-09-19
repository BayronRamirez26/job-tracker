import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ProfileStore } from '../../services/profile-store';

/// A compact picker that chooses which professional profile the AI features personalize to.
/// Selecting one makes it the active profile app-wide (remembered per browser).
@Component({
  selector: 'app-profile-picker',
  imports: [FormsModule, RouterLink],
  template: `
    @if (store.hasProfiles()) {
      <div class="profile-picker">
        <label class="micro-label" for="ai-profile">Personalize to</label>
        <select
          id="ai-profile"
          [ngModel]="store.activeId()"
          (ngModelChange)="store.select($event)"
          [ngModelOptions]="{ standalone: true }"
        >
          @for (p of store.summaries(); track p.id) {
            <option [value]="p.id">{{ p.name }}</option>
          }
        </select>
      </div>
    } @else {
      <p class="profile-hint">
        <a routerLink="/profile">Add a profile</a> to tailor this to you.
      </p>
    }
  `,
  styles: [
    `
      .profile-picker {
        display: inline-flex;
        align-items: center;
        gap: 0.5rem;
      }
      .profile-picker select {
        width: auto;
        padding: 0.3rem 0.5rem;
        font-size: 0.85rem;
      }
      .profile-hint {
        margin: 0;
        font-size: 0.85rem;
        color: var(--muted);
      }
    `,
  ],
})
export class ProfilePicker {
  protected readonly store = inject(ProfileStore);
}
