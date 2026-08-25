namespace Halaqa.Desktop.Features.Registrations.Domain.Entities;

public enum RegistrationState
{
    Pending,
    Accepted,
    Rejected,
    CompletionRequested,
    Withdrawn,
    Cancelled
}

public sealed record RegistrationApplicant(
    Guid Id,
    string DisplayName,
    string? Avatar,
    RegistrationState Status,
    DateTimeOffset SubmittedAt,
    bool SensitiveFieldsHidden);

public sealed record RegistrationRequest(
    Guid Id,
    RegistrationApplicant Applicant,
    RegistrationState State,
    string Visibility,
    string? Message,
    string? DecisionNote,
    DateTimeOffset? DecidedAt,
    DateTimeOffset CreatedAt);

public sealed record RegistrationRequestPage(
    IReadOnlyList<RegistrationRequest> Requests,
    int CurrentPage,
    int LastPage,
    int PerPage,
    int Total);

public sealed record RejectRegistrationRequestCommand(Guid RegistrationId, string? Note);

public sealed record RequestRegistrationCompletionCommand(
    Guid RegistrationId,
    IReadOnlyList<string> RequiredFields,
    string? Note);
