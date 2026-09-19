export type ApplicationStatus =
  | 'Wishlist'
  | 'Applied'
  | 'PhoneScreen'
  | 'Interview'
  | 'Offer'
  | 'Accepted'
  | 'Rejected'
  | 'Withdrawn';

export type ApplicationSource =
  | 'LinkedIn'
  | 'CompanyWebsite'
  | 'Referral'
  | 'Recruiter'
  | 'JobBoard'
  | 'Other';

export const APPLICATION_STATUSES: readonly ApplicationStatus[] = [
  'Wishlist', 'Applied', 'PhoneScreen', 'Interview', 'Offer', 'Accepted', 'Rejected', 'Withdrawn',
];

export const APPLICATION_SOURCES: readonly ApplicationSource[] = [
  'LinkedIn', 'CompanyWebsite', 'Referral', 'Recruiter', 'JobBoard', 'Other',
];

export interface SalaryRange {
  min: number;
  max: number;
  currency: string;
}

export interface JobApplication {
  id: string;
  company: string;
  position: string;
  status: ApplicationStatus;
  source: ApplicationSource;
  appliedDate: string | null;
  notes: string | null;
  salary: SalaryRange | null;
  jobDescription: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateJobApplicationRequest {
  company: string;
  position: string;
  status: ApplicationStatus;
  source: ApplicationSource;
  appliedDate: string | null;
  notes: string | null;
  salary: SalaryRange | null;
  jobDescription: string | null;
}
