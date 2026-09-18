import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ExtractRequest, ExtractionResponse } from '../models/extraction';
import { SummarizeRequest, SummaryResponse } from '../models/summary';

/// Talks to the AI service through the gateway. The '/api' prefix is proxied to
/// http://localhost:8080 in dev (see proxy.conf.json), so these are same-origin calls.
@Injectable({ providedIn: 'root' })
export class AiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/ai';

  summarize(jobDescription: string): Observable<SummaryResponse> {
    const request: SummarizeRequest = { jobDescription };
    return this.http.post<SummaryResponse>(`${this.baseUrl}/summarize`, request);
  }

  extract(jobDescription: string): Observable<ExtractionResponse> {
    const request: ExtractRequest = { jobDescription };
    return this.http.post<ExtractionResponse>(`${this.baseUrl}/extract`, request);
  }
}
