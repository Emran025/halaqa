using Halaqa.Desktop.Features.Auth.Domain.Entities;

namespace Halaqa.Desktop.Features.Profile.Domain.Entities;

public sealed record UserProfile(Guid Id, UserRole Role, string Name, string Email, string? Phone, string Status);

public sealed record ProfileUpdateField<T>(bool IsSpecified, T? Value)
{
    public static ProfileUpdateField<T> Omit() => new(false, default);

    public static ProfileUpdateField<T> Set(T? value) => new(true, value);
}

public sealed record UpdateUserProfileCommand(
    ProfileUpdateField<string> Name,
    ProfileUpdateField<string> Phone,
    ProfileUpdateField<string> MemorizationLevel,
    ProfileUpdateField<string> ReviewLevel)
{
    public bool HasChanges =>
        Name.IsSpecified ||
        Phone.IsSpecified ||
        MemorizationLevel.IsSpecified ||
        ReviewLevel.IsSpecified;
}
