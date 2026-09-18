import { Routes } from '@angular/router';
import { ApplicationsList } from './features/applications/applications-list/applications-list';
import { Summarize } from './features/ai/summarize/summarize';
import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';
import { Profile } from './features/auth/profile/profile';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'applications', pathMatch: 'full' },
  { path: 'applications', component: ApplicationsList, canActivate: [authGuard] },
  { path: 'summarize', component: Summarize },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'profile', component: Profile, canActivate: [authGuard] },
];
