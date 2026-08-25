namespace Halaqa.Desktop.Features.Memberships.Domain.Entities;

public enum MembershipStatus
{
    Active,
    Inactive,
    Removed
}

public sealed record MembershipStudent(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string AccountStatus,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record HalaqaMembership(
    Guid Id,
    Guid HalaqaId,
    MembershipStudent Student,
    MembershipStatus Status,
    DateTimeOffset JoinedAt);

public sealed record MembershipPage(
    IReadOnlyList<HalaqaMembership> Memberships,
    int CurrentPage,
    int LastPage,
    int PerPage,
    int Total);

public sealed record AssignStudentToHalaqaCommand(Guid HalaqaId, Guid StudentId);

public sealed record UpdateHalaqaMembershipCommand(
    Guid HalaqaId,
    Guid MembershipId,
    MembershipStatus Status,
    string? Reason);
