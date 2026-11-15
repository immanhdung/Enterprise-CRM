using EnterpriseCRM.Domain.Enums;

namespace EnterpriseCRM.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    Guid? DepartmentId { get; }
    Guid? TeamId { get; }
    DataScope DataScope { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<string> Permissions { get; }
    bool HasPermission(string permission);
}
