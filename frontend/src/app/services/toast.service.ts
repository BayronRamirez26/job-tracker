import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'info';

export interface Toast {
  id: number;
  text: string;
  type: ToastType;
}

/// A tiny transient-notification queue exposed as a signal. Components call success()/error()
/// after an action; the <app-toasts> outlet renders them and they auto-dismiss.
@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly _toasts = signal<Toast[]>([]);
  readonly toasts = this._toasts.asReadonly();
  private nextId = 0;

  success(text: string): void {
    this.show(text, 'success');
  }

  error(text: string): void {
    this.show(text, 'error');
  }

  info(text: string): void {
    this.show(text, 'info');
  }

  show(text: string, type: ToastType = 'info', ms = 3200): void {
    const id = this.nextId++;
    this._toasts.update((list) => [...list, { id, text, type }]);
    setTimeout(() => this.dismiss(id), ms);
  }

  dismiss(id: number): void {
    this._toasts.update((list) => list.filter((t) => t.id !== id));
  }
}
