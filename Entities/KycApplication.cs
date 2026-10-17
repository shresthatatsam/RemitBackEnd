namespace EverestRemit.Api.Entities;

public enum KycStatus
{
    Draft,
    Pending,
    UnderReview,
    Approved,
    Rejected,
    ActionRequired
}

public sealed class KycApplication
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }

    // Step 1
    public string FullName { get; set; } = "";
    public DateOnly DateOfBirth { get; set; }
    public string Nationality { get; set; } = "";
    public string? Address { get; set; }

    // Step 2
    public string DocumentType { get; set; } = "";
    public string DocumentNumber { get; set; } = "";
    public string? DocumentFrontUrl { get; set; }
    public string? DocumentBackUrl { get; set; }

    // Step 3
    public string? SelfieUrl { get; set; }

    // Workflow
    public int CurrentStep { get; set; } = 1;
    public KycStatus Status { get; set; } = KycStatus.Draft;

    // Admin review
    public string? AdminNote { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}