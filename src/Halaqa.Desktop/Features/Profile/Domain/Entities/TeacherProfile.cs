namespace Halaqa.Desktop.Features.Profile.Domain.Entities;

public enum TeacherGender
{
    Male,
    Female
}

public sealed record TeacherHalaqaSummary(
    Guid Id,
    string Name,
    string Status,
    TeacherGender Gender,
    string Country,
    string Residence,
    int? AvailableCapacity);

public sealed record TeacherDocumentSummary(
    int Id,
    string Name,
    string CertificateType,
    string? CertificateTypeOther,
    string? Riwayah,
    string? IssuingPlace,
    DateOnly? IssuingDate,
    string? FileUrl,
    bool HasFile);

public sealed record TeacherProfile(
    Guid Id,
    string DisplayName,
    string TeacherCode,
    string? Avatar,
    TeacherGender Gender,
    string Country,
    string City,
    string? Qualification,
    int? ExperienceYears,
    bool CapacityAvailable,
    string? Bio,
    int ActiveHalaqaCount,
    IReadOnlyList<TeacherHalaqaSummary> PublicHalaqas,
    DateOnly? BirthDate,
    string? Email,
    string? Phone,
    string? PhoneZone,
    string? WhatsappPhone,
    string? WhatsappZone,
    string? Residence,
    string? AvailableTime,
    IReadOnlyList<TeacherDocumentSummary> Documents);

public sealed record TeacherProfileUpdateField<T>(bool IsSpecified, T? Value)
{
    public static TeacherProfileUpdateField<T> Omit() => new(false, default);
    public static TeacherProfileUpdateField<T> Set(T? value) => new(true, value);
}

public sealed record UpdateTeacherProfileCommand(
    TeacherProfileUpdateField<string> Name,
    TeacherProfileUpdateField<DateOnly?> BirthDate,
    TeacherProfileUpdateField<TeacherGender?> Gender,
    TeacherProfileUpdateField<string> Country,
    TeacherProfileUpdateField<string> City,
    TeacherProfileUpdateField<string> Residence,
    TeacherProfileUpdateField<string> Phone,
    TeacherProfileUpdateField<string> PhoneZone,
    TeacherProfileUpdateField<string> WhatsappPhone,
    TeacherProfileUpdateField<string> WhatsappZone,
    TeacherProfileUpdateField<string> Qualification,
    TeacherProfileUpdateField<int?> ExperienceYears,
    TeacherProfileUpdateField<string> AvailableTime,
    TeacherProfileUpdateField<string> Bio,
    TeacherProfileUpdateField<int?> MaxHalaqas)
{
    public bool HasChanges =>
        Name.IsSpecified ||
        BirthDate.IsSpecified ||
        Gender.IsSpecified ||
        Country.IsSpecified ||
        City.IsSpecified ||
        Residence.IsSpecified ||
        Phone.IsSpecified ||
        PhoneZone.IsSpecified ||
        WhatsappPhone.IsSpecified ||
        WhatsappZone.IsSpecified ||
        Qualification.IsSpecified ||
        ExperienceYears.IsSpecified ||
        AvailableTime.IsSpecified ||
        Bio.IsSpecified ||
        MaxHalaqas.IsSpecified;
}
