using System.Text.Json.Serialization;

namespace Halaqa.Desktop.Features.Auth.Data.Models;

internal sealed record LoginRequestDto(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password);

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
    [property: JsonPropertyName("status")] string Status);
