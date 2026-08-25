using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Memberships.Data.Models;

internal sealed record MembershipStudentDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("created_at")] DateTimeOffset? CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset? UpdatedAt);

internal sealed record HalaqaMembershipDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("halaqa_id")] Guid HalaqaId,
    [property: JsonPropertyName("student")] MembershipStudentDto Student,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("joined_at")] DateTimeOffset JoinedAt);

internal sealed record MembershipResponseDto(
    [property: JsonPropertyName("membership")] HalaqaMembershipDto Membership);

internal sealed record MembershipCollectionResponseDto(
    [property: JsonPropertyName("memberships")] IReadOnlyList<HalaqaMembershipDto> Memberships,
    [property: JsonPropertyName("meta")] MembershipPaginationMetaDto Meta);

internal sealed record MembershipPaginationMetaDto(
    [property: JsonPropertyName("current_page")] int CurrentPage,
    [property: JsonPropertyName("last_page")] int LastPage,
    [property: JsonPropertyName("per_page")] int PerPage,
    [property: JsonPropertyName("total")] int Total);

internal sealed record AssignStudentRequestDto(
    [property: JsonPropertyName("student_id")] Guid StudentId);

internal sealed record UpdateMembershipRequestDto(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("reason")] string? Reason);
