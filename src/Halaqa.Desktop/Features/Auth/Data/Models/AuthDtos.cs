using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Auth.Data.Models;

internal sealed record LoginRequestDto(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password);

internal sealed record ResendVerificationRequestDto(
    [property: JsonPropertyName("email")] string Email);

internal sealed record AuthResponseDto(
    [property: JsonPropertyName("user")] UserDto User,
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_at")] DateTimeOffset ExpiresAt);

internal sealed record UserDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("email_verification_required")] bool EmailVerificationRequired = false);

internal sealed record TeacherVerificationResponseDto(
    [property: JsonPropertyName("valid")] bool Valid,
    [property: JsonPropertyName("teacher")] VerifiedTeacherInfoDto? Teacher,
    [property: JsonPropertyName("message")] string? Message);

internal sealed record VerifiedTeacherInfoDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("teacher_code")] string TeacherCode,
    [property: JsonPropertyName("gender")] string Gender);
