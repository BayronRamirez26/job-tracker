import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Profile } from '../models/profile';

/// Reads and writes the current user's professional profile. These calls are authenticated —
/// authInterceptor attaches the Bearer token automatically.
@Injectable({ providedIn: 'root' })
export class UserProfileService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/users/me/profile';

  get(): Observable<Profile | null> {
    return this.http.get<Profile | null>(this.baseUrl);
  }

  save(profile: Profile): Observable<Profile> {
    return this.http.put<Profile>(this.baseUrl, profile);
  }
}
