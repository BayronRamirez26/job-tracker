import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateJobApplicationRequest, JobApplication } from '../models/job-application';

/// Talks to the Applications service through the gateway. The '/api' prefix is proxied to
/// http://localhost:8080 in dev (see proxy.conf.json), so these are same-origin calls.
@Injectable({ providedIn: 'root' })
export class ApplicationsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/job-applications';

  list(): Observable<JobApplication[]> {
    return this.http.get<JobApplication[]>(this.baseUrl);
  }

  create(request: CreateJobApplicationRequest): Observable<JobApplication> {
    return this.http.post<JobApplication>(this.baseUrl, request);
  }

  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
