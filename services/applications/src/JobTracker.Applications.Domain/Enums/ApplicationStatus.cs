namespace JobTracker.Applications.Domain.Enums;

/// <summary>
/// The lifecycle stage of a job application, from a saved lead through to a final outcome.
/// </summary>
/// <remarks>
/// The numeric values are assigned explicitly and must remain stable: if we persist the
/// enum as an integer, these numbers become part of the stored data contract, and silently
/// reordering the members would corrupt existing rows. (In the Infrastructure phase we'll
/// decide whether to store it as an <c>int</c> or as text — both are valid trade-offs.)
/// </remarks>
public enum ApplicationStatus
{
    /// <summary>Saved as a lead; not applied to yet.</summary>
    Wishlist = 0,

    /// <summary>Application submitted.</summary>
    Applied = 1,

    /// <summary>Initial phone/recruiter screen.</summary>
    PhoneScreen = 2,

    /// <summary>In the interview loop.</summary>
    Interview = 3,

    /// <summary>An offer has been extended.</summary>
    Offer = 4,

    /// <summary>Offer accepted (a positive terminal state).</summary>
    Accepted = 5,

    /// <summary>Rejected by the company (a terminal state).</summary>
    Rejected = 6,

    /// <summary>Withdrawn by the candidate (a terminal state).</summary>
    Withdrawn = 7
}
