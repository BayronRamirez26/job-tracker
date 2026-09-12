namespace JobTracker.Applications.Domain.Enums;

/// <summary>
/// Where the job lead originated. Useful later for analytics ("which channels convert?").
/// </summary>
/// <remarks>
/// As with <see cref="ApplicationStatus"/>, the explicit values are a persistence contract
/// and should not be reordered once data exists.
/// </remarks>
public enum ApplicationSource
{
    LinkedIn = 0,
    CompanyWebsite = 1,
    Referral = 2,
    Recruiter = 3,
    JobBoard = 4,
    Other = 5
}
