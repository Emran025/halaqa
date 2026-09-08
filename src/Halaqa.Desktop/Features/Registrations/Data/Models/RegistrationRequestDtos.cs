using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Registrations.Data.Models;

internal sealed record RegistrationApplicantDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("display_name")] string DisplayName,
    [property: JsonPropertyName("avatar")] string? Avatar,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("submitted_at")] DateTimeOffset SubmittedAt,
    [property: JsonPropertyName("sensitive_fields_hidden")] bool SensitiveFieldsHidden);

internal sealed record RequestedTeacherSummaryDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("display_name")] string DisplayName,
    [property: JsonPropertyName("teacher_code")] string? TeacherCode,
    [property: JsonPropertyName("avatar")] string? Avatar,
    [property: JsonPropertyName("qualification")] string? Qualification);

internal sealed record RequestedHalaqaSummaryDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name);

internal sealed record RegistrationRequestDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("student_summary")] RegistrationApplicantDto StudentSummary,
    [property: JsonPropertyName("requested_teacher")] RequestedTeacherSummaryDto? RequestedTeacher,
    [property: JsonPropertyName("requested_halaqa")] RequestedHalaqaSummaryDto? RequestedHalaqa,
    [property: JsonPropertyName("teacher_code")] string? TeacherCode,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("visibility")] string Visibility,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("decision_note")] string? DecisionNote,
    [property: JsonPropertyName("decided_at")] DateTimeOffset? DecidedAt,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);

internal sealed record RegistrationResponseDto(
    [property: JsonPropertyName("registration_request")] RegistrationRequestDto RegistrationRequest);

internal sealed record RegistrationCollectionResponseDto(
    [property: JsonPropertyName("registration_requests")] IReadOnlyList<RegistrationRequestDto> RegistrationRequests,
    [property: JsonPropertyName("meta")] RegistrationPaginationMetaDto Meta);

internal sealed record ApplicantCollectionResponseDto(
    [property: JsonPropertyName("applicants")] IReadOnlyList<RegistrationRequestDto> Applicants,
    [property: JsonPropertyName("meta")] RegistrationPaginationMetaDto Meta);

internal sealed record RegistrationPaginationMetaDto(
    [property: JsonPropertyName("current_page")] int CurrentPage,
    [property: JsonPropertyName("last_page")] int LastPage,
    [property: JsonPropertyName("per_page")] int PerPage,
    [property: JsonPropertyName("total")] int Total);

internal sealed record DecisionNoteRequestDto(
    [property: JsonPropertyName("note")] string? Note);

internal sealed record CompletionRequestDto(
    [property: JsonPropertyName("required_fields")] IReadOnlyList<string> RequiredFields,
    [property: JsonPropertyName("note")] string? Note);
