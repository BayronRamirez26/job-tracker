import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, catchError, map, of, switchMap, tap } from 'rxjs';
import { LoginRequest, LoginResponse, RegisterRequest, User } from '../models/auth';

const TOKEN_KEY = 'jobtracker.token';

/// Owns the browser-side session: the stored JWT plus a signal holding the current user.
/// The token is attached to API calls by authInterceptor; this service never sets headers itself.
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  // Private writable signal + public read-only view — the component-facing API can't mutate it.
  private readonly _user = signal<User | null>(null);
  readonly user = this._user.asReadonly();
  readonly isAuthenticated = computed(() => this._user() !== null);

  get token(): string | null {
    // localStorage can throw (private mode, blocked cookies) — never let that crash the app.
    try {
      return localStorage.getItem(TOKEN_KEY);
    } catch {
      return null;
    }
  }

  register(request: RegisterRequest): Observable<User> {
    return this.http.post<User>('/api/auth/register', request);
  }

  login(request: LoginRequest): Observable<User> {
    // Login only returns a token, so we store it and then call /me to learn who we are —
    // switchMap chains the second request onto the first.
    return this.http.post<LoginResponse>('/api/auth/login', request).pipe(
      tap((res) => this.storeToken(res.token)),
      switchMap(() => this.me()),
      tap((user) => this._user.set(user)),
    );
  }

  me(): Observable<User> {
    return this.http.get<User>('/api/users/me');
  }

  logout(): void {
    this.clearToken();
    this._user.set(null);
  }

  /// Runs once at startup (see provideAppInitializer). If a token is stored, ask the API who we
  /// are; a 401 (expired/tampered token) clears it. Always completes without erroring so bootstrap
  /// is never blocked.
  restoreSession(): Observable<void> {
    if (!this.token) {
      return of(undefined);
    }
    return this.me().pipe(
      tap((user) => this._user.set(user)),
      map(() => undefined),
      catchError(() => {
        this.clearToken();
        return of(undefined);
      }),
    );
  }

  private storeToken(token: string): void {
    try {
      localStorage.setItem(TOKEN_KEY, token);
    } catch {
      /* ignore — session just won't survive a reload */
    }
  }

  private clearToken(): void {
    try {
      localStorage.removeItem(TOKEN_KEY);
    } catch {
      /* ignore */
    }
  }
}
