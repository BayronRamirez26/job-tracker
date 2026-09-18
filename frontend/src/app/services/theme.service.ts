import { Injectable, signal } from '@angular/core';

type Theme = 'light' | 'dark';
const KEY = 'jobtracker.theme';

/// Owns the light/dark theme. index.html stamps <html data-theme> before first paint (no flash);
/// this service keeps that in sync, persists an explicit choice, and follows the OS until one is made.
@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly _theme = signal<Theme>(this.resolve());
  readonly theme = this._theme.asReadonly();

  constructor() {
    this.stamp(this._theme());

    const mq = window.matchMedia?.('(prefers-color-scheme: dark)');
    mq?.addEventListener?.('change', (e) => {
      if (!this.stored()) {
        const next: Theme = e.matches ? 'dark' : 'light';
        this._theme.set(next);
        this.stamp(next);
      }
    });
  }

  toggle(): void {
    const next: Theme = this._theme() === 'dark' ? 'light' : 'dark';
    this._theme.set(next);
    this.stamp(next);
    try {
      localStorage.setItem(KEY, next);
    } catch {
      /* ignore — the choice just won't persist */
    }
  }

  private stamp(theme: Theme): void {
    document.documentElement.setAttribute('data-theme', theme);
  }

  private stored(): Theme | null {
    try {
      const v = localStorage.getItem(KEY);
      return v === 'light' || v === 'dark' ? v : null;
    } catch {
      return null;
    }
  }

  private resolve(): Theme {
    return (
      this.stored() ??
      (window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light')
    );
  }
}
