using System.Security.Claims;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Domain.Constants;
using EnterpriseCRM.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EnterpriseCRM.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var idClaim = User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                          User?.FindFirstValue("sub");

            return Guid.TryParse(idClaim, out var guid) ? guid : null;
        }
    }

    public string? Email =>
        User?.FindFirstValue(ClaimTypes.Email) ??
        User?.FindFirstValue("email");

    public string? Role =>
        User?.FindFirstValue(ClaimTypes.Role) ??
        User?.FindFirstValue("role");

    public Guid? DepartmentId
    {
        get
        {
            var deptClaim = User?.FindFirstValue("department_id");
            return Guid.TryParse(deptClaim, out var guid) ? guid : null;
        }
    }

    public Guid? TeamId
    {
        get
        {
            var teamClaim = User?.FindFirstValue("team_id");
            return Guid.TryParse(teamClaim, out var guid) ? guid : null;
        }
    }

    public DataScope DataScope
    {
        get
        {
            var scopeStr = User?.FindFirstValue("data_scope");
            return Enum.TryParse<DataScope>(scopeStr, true, out var scope) ? scope : DataScope.Own;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<string> Permissions
    {
        get
        {
            if (User == null) return Array.Empty<string>();

            return User.FindAll("permission")
                .Select(c => c.Value)
                .Distinct()
                .ToList();
        }
    }

    public bool HasPermission(string permission)
    {
        if (!IsAuthenticated) return false;

        // Admin role has implicit bypass for all permissions
        if (Role == SystemRoles.Admin || (User?.IsInRole(SystemRoles.Admin) ?? false))
        {
            return true;
        }

        return Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}
