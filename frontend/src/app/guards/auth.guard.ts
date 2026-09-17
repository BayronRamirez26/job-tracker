import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/// Blocks a route unless the user is signed in. Because the session is restored during app
/// bootstrap (provideAppInitializer), isAuthenticated() is already settled when this runs —
/// even on a hard refresh of the protected page.
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    return true;
  }

  // Send them to login, remembering where they were headed so we can return them there.
  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};
