import { Routes } from '@angular/router';
import { ApplicationsList } from './features/applications/applications-list/applications-list';
import { Summarize } from './features/ai/summarize/summarize';

export const routes: Routes = [
  { path: '', redirectTo: 'applications', pathMatch: 'full' },
  { path: 'applications', component: ApplicationsList },
  { path: 'summarize', component: Summarize },
];
