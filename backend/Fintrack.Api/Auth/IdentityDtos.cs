using System.ComponentModel.DataAnnotations;

namespace Fintrack.Api.Auth;

public sealed record SignInRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    CurrentUserResponse User);

public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    Guid CompanyId,
    string CompanyName,
    IReadOnlyCollection<string> Roles);

public sealed record CreateUserRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required] string Role);

public sealed record UserResponse(
    Guid Id,
    string Email,
    Guid CompanyId,
    IReadOnlyCollection<string> Roles);
