import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/// Attaches the stored JWT as a Bearer token to our own /api requests, and signs the user out on a
/// 401 from a protected call (an expired/invalid session), bouncing them to login.
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const token = auth.token;

  const request =
    token && req.url.startsWith('/api/')
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      // Auth endpoints 401 on bad credentials — leave those to the form. Any other 401 means the
      // session is gone, so sign out and send the user to login.
      if (error.status === 401 && !req.url.includes('/api/auth/')) {
        auth.logout();
        router.navigateByUrl('/login');
      }
      return throwError(() => error);
    }),
  );
};
