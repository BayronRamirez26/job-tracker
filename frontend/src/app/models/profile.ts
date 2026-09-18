/// The professional profile (built from a CV), stored on the user via /api/users/me/profile
/// and produced by the AI at /api/ai/parse-cv.

export interface ProfileExperience {
  company: string;
  title: string | null;
  period: string | null;
  highlights: string[];
}

export interface ProfileEducation {
  institution: string;
  degree: string | null;
  year: string | null;
}

export interface Profile {
  fullName: string | null;
  headline: string | null;
  summary: string | null;
  location: string | null;
  yearsOfExperience: number | null;
  skills: string[];
  experience: ProfileExperience[];
  education: ProfileEducation[];
  links: string[];
}

/// The AI parse-cv response is a Profile plus the model that produced it.
export interface CvProfileResponse extends Profile {
  model: string;
}
