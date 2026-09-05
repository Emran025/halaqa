namespace Halaqa.Desktop.Features.Auth.Domain.Entities;

public enum UserRole
{
    Teacher,
    Student
}

public sealed record AuthUser(
    Guid Id,
    UserRole Role,
    string Name,
    string Email,
    string Status,
    bool EmailVerificationRequired = false);

public sealed record AuthenticatedUser(
    AuthUser User,
    string AccessToken,
    DateTimeOffset ExpiresAt);
