import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ProfileDetail, ProfileSummary, SaveProfileRequest } from '../models/profile';

/// CRUD for the current user's named professional profiles. These calls are authenticated —
/// authInterceptor attaches the Bearer token automatically.
@Injectable({ providedIn: 'root' })
export class UserProfileService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/users/me/profiles';

  list(): Observable<ProfileSummary[]> {
    return this.http.get<ProfileSummary[]>(this.baseUrl);
  }

  get(id: string): Observable<ProfileDetail> {
    return this.http.get<ProfileDetail>(`${this.baseUrl}/${id}`);
  }

  create(request: SaveProfileRequest): Observable<ProfileDetail> {
    return this.http.post<ProfileDetail>(this.baseUrl, request);
  }

  update(id: string, request: SaveProfileRequest): Observable<ProfileDetail> {
    return this.http.put<ProfileDetail>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
