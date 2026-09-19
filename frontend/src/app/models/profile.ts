/// A user's professional profile. Users can keep several named profiles (e.g. "Full-Stack
/// Developer", "Sales Engineer"); the AI features personalize to whichever one is active.
/// Managed via /api/users/me/profiles; the content is produced by the AI at /api/ai/parse-cv.

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

export interface ProfileCertification {
  name: string;
  issuer: string | null;
  year: string | null;
}

/// The structured content of a profile (everything except its id/name/timestamps).
export interface Profile {
  fullName: string | null;
  headline: string | null;
  summary: string | null;
  location: string | null;
  yearsOfExperience: number | null;
  skills: string[];
  experience: ProfileExperience[];
  education: ProfileEducation[];
  certifications: ProfileCertification[];
  links: string[];
}

/// A row in the profiles list — enough to label and pick a profile.
export interface ProfileSummary {
  id: string;
  name: string;
  updatedAt: string;
}

/// A full profile: identity + name + timestamps + structured content.
export interface ProfileDetail {
  id: string;
  name: string;
  createdAt: string;
  updatedAt: string;
  content: Profile;
}

/// The request body for creating or replacing a profile.
export interface SaveProfileRequest {
  name: string;
  content: Profile;
}

/// The AI parse-cv response is a Profile plus the model that produced it.
export interface CvProfileResponse extends Profile {
  model: string;
}

/// A blank profile content, used when starting a new profile from scratch.
export function emptyProfile(): Profile {
  return {
    fullName: null,
    headline: null,
    summary: null,
    location: null,
    yearsOfExperience: null,
    skills: [],
    experience: [],
    education: [],
    certifications: [],
    links: [],
  };
}
