using System.Security.Cryptography;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.SystemAdministration;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class UserAdministrationService : IUserAdministrationService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UserAdministrationService(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyList<UserSummaryDto>> GetAllUsersAsync(
        string? search = null,
        string? status = null,
        Guid? departmentId = null,
        string? role = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.EmployeeProfile!)
                .ThenInclude(e => e.Department)
            .Include(u => u.EmployeeProfile!)
                .ThenInclude(e => e.Team)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(u => u.Status == status);
        }

        if (departmentId.HasValue)
        {
            query = query.Where(u => u.EmployeeProfile != null && u.EmployeeProfile.DepartmentId == departmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == role));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s) ||
                u.Email.ToLower().Contains(s));
        }

        var users = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new UserSummaryDto(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.FullName,
                u.IsActive,
                u.Status,
                u.LastLoginAt,
                u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                u.EmployeeProfile != null ? u.EmployeeProfile.Department.Name : null,
                u.EmployeeProfile != null && u.EmployeeProfile.Team != null ? u.EmployeeProfile.Team.Name : null,
                u.EmployeeProfile != null ? u.EmployeeProfile.Position : null,
                u.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return users;
    }

    public async Task<UserDetailDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var u = await _context.Users
            .Include(x => x.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(x => x.EmployeeProfile!)
                .ThenInclude(e => e.Department)
            .Include(x => x.EmployeeProfile!)
                .ThenInclude(e => e.Team)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (u == null) return null;

        return new UserDetailDto(
            u.Id,
            u.Email,
            u.FirstName,
            u.LastName,
            u.FullName,
            u.IsActive,
            u.Status,
            u.FailedLoginAttempts,
            u.LastLoginAt,
            u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            u.EmployeeProfile?.DepartmentId,
            u.EmployeeProfile?.Department?.Name,
            u.EmployeeProfile?.TeamId,
            u.EmployeeProfile?.Team?.Name,
            u.EmployeeProfile?.Position,
            u.EmployeeProfile?.Phone,
            u.EmployeeProfile?.BaseSalary ?? 0m,
            u.CreatedAt
        );
    }

    public async Task<UserDetailDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower(), cancellationToken);
        if (existing)
        {
            throw new InvalidOperationException($"User with email '{request.Email}' already exists.");
        }

        var (hash, salt) = _passwordHasher.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = hash,
            Salt = salt,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            IsActive = true,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        // Assign Roles
        if (request.Roles != null && request.Roles.Count > 0)
        {
            var roles = await _context.Roles
                .Where(r => request.Roles.Contains(r.Name))
                .ToListAsync(cancellationToken);

            foreach (var r in roles)
            {
                _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = r.Id });
            }
        }

        // Create Employee Profile if department is specified
        if (request.DepartmentId.HasValue)
        {
            var empCode = "EMP-" + RandomNumberGenerator.GetInt32(1000, 9999);
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                DepartmentId = request.DepartmentId.Value,
                TeamId = request.TeamId,
                EmployeeCode = empCode,
                Position = request.Position?.Trim() ?? "Staff",
                Phone = request.Phone?.Trim() ?? string.Empty,
                HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
                BaseSalary = request.BaseSalary ?? 15000000m,
                IsActive = true
            };
            _context.Employees.Add(employee);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return (await GetUserByIdAsync(user.Id, cancellationToken))!;
    }

    public async Task<UserDetailDto> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.EmployeeProfile)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID '{id}' was not found.");
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            user.Status = request.Status;
            user.IsActive = request.Status == "Active";
        }

        if (user.EmployeeProfile != null)
        {
            if (request.DepartmentId.HasValue) user.EmployeeProfile.DepartmentId = request.DepartmentId.Value;
            user.EmployeeProfile.TeamId = request.TeamId;
            if (!string.IsNullOrWhiteSpace(request.Position)) user.EmployeeProfile.Position = request.Position.Trim();
            if (!string.IsNullOrWhiteSpace(request.Phone)) user.EmployeeProfile.Phone = request.Phone.Trim();
            if (request.BaseSalary.HasValue) user.EmployeeProfile.BaseSalary = request.BaseSalary.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return (await GetUserByIdAsync(user.Id, cancellationToken))!;
    }

    public async Task<bool> UpdateUserStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user == null) return false;

        user.Status = status;
        user.IsActive = status == "Active";

        if (status != "Active")
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;

            // Revoke active sessions
            var activeSessions = await _context.UserSessions
                .Where(s => s.UserId == id && !s.IsRevoked)
                .ToListAsync(cancellationToken);

            foreach (var s in activeSessions)
            {
                s.IsRevoked = true;
                s.RevokedAtUtc = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AdminResetPasswordAsync(Guid id, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user == null) return false;

        var (hash, salt) = _passwordHasher.HashPassword(newPassword);
        user.PasswordHash = hash;
        user.Salt = salt;
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AssignRolesAsync(Guid id, List<string> roles, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null) return false;

        _context.UserRoles.RemoveRange(user.UserRoles);

        if (roles != null && roles.Count > 0)
        {
            var rolesToAdd = await _context.Roles
                .Where(r => roles.Contains(r.Name))
                .ToListAsync(cancellationToken);

            foreach (var r in rolesToAdd)
            {
                _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = r.Id });
            }
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<UserSessionDto>> GetUserSessionsAsync(Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.UserSessions
            .Include(s => s.User)
            .AsNoTracking()
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(s => s.UserId == userId.Value);
        }

        return await query
            .OrderByDescending(s => s.CreatedAtUtc)
            .Take(100)
            .Select(s => new UserSessionDto(
                s.Id,
                s.UserId,
                s.User.Email,
                s.IpAddress,
                s.UserAgent,
                s.CreatedAtUtc,
                s.ExpiresAtUtc,
                s.IsRevoked,
                s.RevokedAtUtc
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
        if (session == null) return false;

        session.IsRevoked = true;
        session.RevokedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
