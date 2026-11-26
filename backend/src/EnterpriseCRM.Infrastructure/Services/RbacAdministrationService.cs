using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.SystemAdministration;
using EnterpriseCRM.Domain.Constants;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class RbacAdministrationService : IRbacAdministrationService
{
    private readonly IApplicationDbContext _context;

    public RbacAdministrationService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _context.Roles
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto(
                r.Id,
                r.Name,
                r.Description,
                r.IsSystem,
                r.UserRoles.Count,
                r.RolePermissions.Select(rp => rp.Permission.Code).ToList()
            ))
            .ToListAsync(cancellationToken);

        return roles;
    }

    public async Task<RoleDto?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var r = await _context.Roles
            .Include(x => x.UserRoles)
            .Include(x => x.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (r == null) return null;

        return new RoleDto(
            r.Id,
            r.Name,
            r.Description,
            r.IsSystem,
            r.UserRoles.Count,
            r.RolePermissions.Select(rp => rp.Permission.Code).ToList()
        );
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var roleName = request.Name.Trim();
        var exists = await _context.Roles.AnyAsync(r => r.Name.ToLower() == roleName.ToLower(), cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Role '{roleName}' already exists.");
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = roleName,
            Description = request.Description.Trim(),
            IsSystem = false
        };

        _context.Roles.Add(role);

        if (request.Permissions != null && request.Permissions.Count > 0)
        {
            var perms = await _context.Permissions
                .Where(p => request.Permissions.Contains(p.Code))
                .ToListAsync(cancellationToken);

            foreach (var p in perms)
            {
                _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = p.Id,
                        DataScope = "Company"
                    });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return (await GetRoleByIdAsync(role.Id, cancellationToken))!;
    }

    public async Task<RoleDto> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (role == null)
        {
            throw new KeyNotFoundException($"Role with ID '{id}' was not found.");
        }

        role.Description = request.Description.Trim();

        // Update permissions
        _context.RolePermissions.RemoveRange(role.RolePermissions);

        if (request.Permissions != null && request.Permissions.Count > 0)
        {
            var perms = await _context.Permissions
                .Where(p => request.Permissions.Contains(p.Code))
                .ToListAsync(cancellationToken);

            foreach (var p in perms)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = p.Id,
                    DataScope = "Company"
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return (await GetRoleByIdAsync(role.Id, cancellationToken))!;
    }

    public async Task<bool> DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (role == null) return false;

        if (role.IsSystem)
        {
            throw new InvalidOperationException("System predefined roles cannot be deleted.");
        }

        if (role.UserRoles.Any())
        {
            throw new InvalidOperationException($"Role '{role.Name}' cannot be deleted because it is currently assigned to {role.UserRoles.Count} user(s).");
        }

        _context.RolePermissions.RemoveRange(role.RolePermissions);
        _context.Roles.Remove(role);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<PermissionDto>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Code)
            .Select(p => new PermissionDto(p.Id, p.Code, p.Module, p.Description))
            .ToListAsync(cancellationToken);
    }

    public async Task<RolePermissionMatrixDto> GetPermissionMatrixAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _context.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync(cancellationToken);
        var permissions = await _context.Permissions.AsNoTracking().OrderBy(p => p.Module).ThenBy(p => p.Code).ToListAsync(cancellationToken);
        var rolePermissions = await _context.RolePermissions.AsNoTracking().ToListAsync(cancellationToken);

        var rpLookup = rolePermissions.ToLookup(rp => $"{rp.RoleId}_{rp.PermissionId}");

        var roleNames = roles.Select(r => r.Name).ToList();
        var rows = new List<PermissionMatrixRowDto>();

        foreach (var p in permissions)
        {
            var roleMap = new Dictionary<string, RolePermissionEntryDto>();
            foreach (var r in roles)
            {
                var match = rpLookup[$"{r.Id}_{p.Id}"].FirstOrDefault();
                roleMap[r.Name] = new RolePermissionEntryDto(
                    r.Name,
                    match != null,
                    match?.DataScope ?? "Company"
                );
            }

            rows.Add(new PermissionMatrixRowDto(p.Code, p.Module, p.Description, roleMap));
        }

        return new RolePermissionMatrixDto(roleNames, rows);
    }

    public async Task<bool> UpdatePermissionMatrixEntryAsync(UpdateRolePermissionMatrixRequest request, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.RoleName, cancellationToken);
        if (role == null) throw new KeyNotFoundException($"Role '{request.RoleName}' was not found.");

        var perm = await _context.Permissions.FirstOrDefaultAsync(p => p.Code == request.PermissionCode, cancellationToken);
        if (perm == null) throw new KeyNotFoundException($"Permission '{request.PermissionCode}' was not found.");

        var existing = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == role.Id && rp.PermissionId == perm.Id, cancellationToken);

        if (request.IsGranted)
        {
            if (existing != null)
            {
                existing.DataScope = request.DataScope ?? existing.DataScope;
            }
            else
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = perm.Id,
                    DataScope = request.DataScope ?? "Company"
                });
            }
        }
        else
        {
            if (existing != null)
            {
                _context.RolePermissions.Remove(existing);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
