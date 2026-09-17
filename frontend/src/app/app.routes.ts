import { Routes } from '@angular/router';
import { ApplicationsList } from './features/applications/applications-list/applications-list';

export const routes: Routes = [
  { path: '', redirectTo: 'applications', pathMatch: 'full' },
  { path: 'applications', component: ApplicationsList },
];
