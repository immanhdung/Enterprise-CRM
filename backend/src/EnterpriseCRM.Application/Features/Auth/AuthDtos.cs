using EnterpriseCRM.Domain.Enums;

namespace EnterpriseCRM.Application.Features.Auth;

public record LoginRequest(string Email, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmNewPassword);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Email, string Token, string NewPassword);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    bool IsActive,
    DateTime? LastLoginAt,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? TeamId,
    string? TeamName,
    DataScope DataScope,
    string? EmployeeCode,
    string? Position
);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    UserDto User
);
