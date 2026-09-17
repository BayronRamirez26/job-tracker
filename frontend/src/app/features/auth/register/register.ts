import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: '../auth.css',
})
export class Register {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected email = '';
  protected password = '';
  protected displayName = '';
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected submit(): void {
    this.loading.set(true);
    this.error.set(null);

    this.auth
      .register({ email: this.email, password: this.password, displayName: this.displayName })
      .subscribe({
        next: () => {
          // Registered — log straight in so the user lands signed-in rather than on a login form.
          this.auth.login({ email: this.email, password: this.password }).subscribe({
            next: () => this.router.navigateByUrl('/applications'),
            error: () => this.router.navigateByUrl('/login'),
          });
        },
        error: (err: HttpErrorResponse) => {
          this.error.set(this.messageFor(err));
          this.loading.set(false);
        },
      });
  }

  private messageFor(err: HttpErrorResponse): string {
    switch (err.status) {
      case 0:
        return 'Could not reach the API gateway on :8080. Is the Docker stack running?';
      case 409:
        return 'An account with that email already exists.';
      case 400:
        // The API returns RFC 7807 ProblemDetails with a per-field "errors" map; surface the first.
        return this.firstValidationError(err) ?? 'Please check the form and try again.';
      default:
        return 'Something went wrong creating your account. Please try again.';
    }
  }

  private firstValidationError(err: HttpErrorResponse): string | null {
    const errors = err.error?.errors as Record<string, string[]> | undefined;
    const first = errors ? Object.values(errors)[0] : undefined;
    return first?.[0] ?? null;
  }
}
