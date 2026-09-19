import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ExtractRequest, ExtractionResponse } from '../models/extraction';
import { CvProfileResponse } from '../models/profile';
import { SummarizeRequest, SummaryResponse } from '../models/summary';
import { CoverLetterResponse, FitResponse, TailoredCvResponse } from '../models/assistant';

/// Talks to the AI service through the gateway. The '/api' prefix is proxied to
/// http://localhost:8080 in dev (see proxy.conf.json), so these are same-origin calls.
@Injectable({ providedIn: 'root' })
export class AiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/ai';

  summarize(jobDescription: string, candidateProfile: string | null = null): Observable<SummaryResponse> {
    const request: SummarizeRequest = { jobDescription, candidateProfile };
    return this.http.post<SummaryResponse>(`${this.baseUrl}/summarize`, request);
  }

  extract(jobDescription: string, candidateProfile: string | null = null): Observable<ExtractionResponse> {
    const request: ExtractRequest = { jobDescription, candidateProfile };
    return this.http.post<ExtractionResponse>(`${this.baseUrl}/extract`, request);
  }

  parseCv(cv: string): Observable<CvProfileResponse> {
    return this.http.post<CvProfileResponse>(`${this.baseUrl}/parse-cv`, { cv });
  }

  // --- Application assistant: profile + a specific posting -------------------------------------

  fit(jobDescription: string, candidateProfile: string | null): Observable<FitResponse> {
    return this.http.post<FitResponse>(`${this.baseUrl}/fit`, { jobDescription, candidateProfile });
  }

  coverLetter(jobDescription: string, candidateProfile: string | null): Observable<CoverLetterResponse> {
    return this.http.post<CoverLetterResponse>(`${this.baseUrl}/cover-letter`, { jobDescription, candidateProfile });
  }

  tailorCv(jobDescription: string, candidateProfile: string | null): Observable<TailoredCvResponse> {
    return this.http.post<TailoredCvResponse>(`${this.baseUrl}/tailor-cv`, { jobDescription, candidateProfile });
  }
}
