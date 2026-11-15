using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Auth;
using EnterpriseCRM.Domain.Constants;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<AuthService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.EmployeeProfile!)
                .ThenInclude(e => e.Department)
            .Include(u => u.EmployeeProfile!)
                .ThenInclude(e => e.Team)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Failed login attempt for email {Email}: User not found or inactive.", request.Email);
            return ApiResponse<AuthResponse>.FailureResponse("Invalid email or password.", 401);
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.Salt);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Failed login attempt for user {Email}: Incorrect password.", request.Email);
            return ApiResponse<AuthResponse>.FailureResponse("Invalid email or password.", 401);
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();

        var permissions = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        // If user is Admin, add all system permissions
        if (roles.Contains(SystemRoles.Admin))
        {
            permissions = Permissions.AllPermissions.Select(p => p.Code).Distinct().ToList();
        }

        var dataScope = DetermineDataScope(roles);
        var deptId = user.EmployeeProfile?.DepartmentId;
        var teamId = user.EmployeeProfile?.TeamId;

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions, deptId, teamId, dataScope);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        user.LastLoginAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.IsActive,
            user.LastLoginAt,
            roles,
            permissions,
            deptId,
            user.EmployeeProfile?.Department?.Name,
            teamId,
            user.EmployeeProfile?.Team?.Name,
            dataScope,
            user.EmployeeProfile?.EmployeeCode,
            user.EmployeeProfile?.Position
        );

        _logger.LogInformation("User {Email} logged in successfully.", user.Email);

        return ApiResponse<AuthResponse>.SuccessResponse(
            new AuthResponse(accessToken, refreshToken, 3600, userDto),
            "Login successful."
        );
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return ApiResponse<AuthResponse>.FailureResponse("Refresh token is required.", 400);
        }

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.EmployeeProfile!)
                .ThenInclude(e => e.Department)
            .Include(u => u.EmployeeProfile!)
                .ThenInclude(e => e.Team)
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

        if (user == null || !user.IsActive || user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Invalid or expired refresh token attempt.");
            return ApiResponse<AuthResponse>.FailureResponse("Invalid or expired refresh token.", 401);
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();

        var permissions = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (roles.Contains(SystemRoles.Admin))
        {
            permissions = Permissions.AllPermissions.Select(p => p.Code).Distinct().ToList();
        }

        var dataScope = DetermineDataScope(roles);
        var deptId = user.EmployeeProfile?.DepartmentId;
        var teamId = user.EmployeeProfile?.TeamId;

        // Token Rotation: issue new access token AND new refresh token
        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions, deptId, teamId, dataScope);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.IsActive,
            user.LastLoginAt,
            roles,
            permissions,
            deptId,
            user.EmployeeProfile?.Department?.Name,
            teamId,
            user.EmployeeProfile?.Team?.Name,
            dataScope,
            user.EmployeeProfile?.EmployeeCode,
            user.EmployeeProfile?.Position
        );

        return ApiResponse<AuthResponse>.SuccessResponse(
            new AuthResponse(newAccessToken, newRefreshToken, 3600, userDto),
            "Token refreshed successfully."
        );
    }

    public async Task<ApiResponse<bool>> LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse<bool>.SuccessResponse(true, "Logged out successfully.");
    }

    public async Task<ApiResponse<UserDto>> GetCurrentUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.EmployeeProfile!)
                .ThenInclude(e => e.Department)
            .Include(u => u.EmployeeProfile!)
                .ThenInclude(e => e.Team)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || !user.IsActive)
        {
            return ApiResponse<UserDto>.FailureResponse("User not found or inactive.", 404);
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();

        var permissions = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (roles.Contains(SystemRoles.Admin))
        {
            permissions = Permissions.AllPermissions.Select(p => p.Code).Distinct().ToList();
        }

        var dataScope = DetermineDataScope(roles);

        var userDto = new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.IsActive,
            user.LastLoginAt,
            roles,
            permissions,
            user.EmployeeProfile?.DepartmentId,
            user.EmployeeProfile?.Department?.Name,
            user.EmployeeProfile?.TeamId,
            user.EmployeeProfile?.Team?.Name,
            dataScope,
            user.EmployeeProfile?.EmployeeCode,
            user.EmployeeProfile?.Position
        );

        return ApiResponse<UserDto>.SuccessResponse(userDto);
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            return ApiResponse<bool>.FailureResponse("User not found.", 404);
        }

        var isValid = _passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash, user.Salt);
        if (!isValid)
        {
            return ApiResponse<bool>.FailureResponse("Current password does not match.", 400);
        }

        var (newHash, newSalt) = _passwordHasher.HashPassword(request.NewPassword);
        user.PasswordHash = newHash;
        user.Salt = newSalt;
        // Revoke active refresh token on password change
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Password changed successfully for user {UserId}.", userId);

        return ApiResponse<bool>.SuccessResponse(true, "Password changed successfully. Please log in with your new password.");
    }

    public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (user != null && user.IsActive)
        {
            var resetToken = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(2);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password reset token generated for user {Email}.", user.Email);
            // In production: dispatch notification / email via background queue
        }

        // Return positive response regardless to prevent user email enumeration
        return ApiResponse<bool>.SuccessResponse(true, "If your email is registered in our system, you will receive password reset instructions.");
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(
            u => u.Email.ToLower() == normalizedEmail && u.PasswordResetToken == request.Token,
            cancellationToken);

        if (user == null || !user.IsActive || user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
        {
            return ApiResponse<bool>.FailureResponse("Invalid or expired password reset token.", 400);
        }

        var (newHash, newSalt) = _passwordHasher.HashPassword(request.NewPassword);
        user.PasswordHash = newHash;
        user.Salt = newSalt;
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Password successfully reset for user {Email}.", user.Email);

        return ApiResponse<bool>.SuccessResponse(true, "Password has been successfully reset. You can now log in.");
    }

    private static DataScope DetermineDataScope(IEnumerable<string> roles)
    {
        if (roles.Contains(SystemRoles.Admin) || roles.Contains(SystemRoles.CeoBod))
        {
            return DataScope.All;
        }

        if (roles.Contains(SystemRoles.Accountant))
        {
            return DataScope.All;
        }

        if (roles.Contains(SystemRoles.Manager))
        {
            return DataScope.Department;
        }

        if (roles.Contains(SystemRoles.Sales))
        {
            return DataScope.Team;
        }

        return DataScope.Own;
    }
}
