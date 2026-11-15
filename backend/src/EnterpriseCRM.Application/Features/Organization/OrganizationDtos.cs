namespace EnterpriseCRM.Application.Features.Organization;

public record DepartmentDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Guid? ManagerId,
    string? ManagerName,
    int MemberCount,
    int TeamCount,
    DateTime CreatedAt
);

public record CreateDepartmentRequest(
    string Code,
    string Name,
    string? Description,
    Guid? ManagerId
);

public record UpdateDepartmentRequest(
    string Name,
    string? Description,
    Guid? ManagerId
);

public record TeamDto(
    Guid Id,
    Guid DepartmentId,
    string DepartmentName,
    string Name,
    Guid? TeamLeaderId,
    string? TeamLeaderName,
    int MemberCount,
    DateTime CreatedAt
);

public record CreateTeamRequest(
    Guid DepartmentId,
    string Name,
    Guid? TeamLeaderId
);

public record UpdateTeamRequest(
    string Name,
    Guid? TeamLeaderId
);

public record EmployeeDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string EmployeeCode,
    string Position,
    string Phone,
    DateOnly HireDate,
    decimal BaseSalary,
    bool IsActive,
    Guid DepartmentId,
    string DepartmentName,
    Guid? TeamId,
    string? TeamName,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt
);

public record CreateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string EmployeeCode,
    string Position,
    string Phone,
    DateOnly HireDate,
    decimal BaseSalary,
    Guid DepartmentId,
    Guid? TeamId,
    List<string> Roles
);

public record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    string Position,
    string Phone,
    DateOnly HireDate,
    decimal BaseSalary,
    Guid DepartmentId,
    Guid? TeamId,
    bool IsActive
);

public record UserManagementDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    bool IsActive,
    DateTime? LastLoginAt,
    IReadOnlyList<string> Roles,
    string? DepartmentName,
    string? Position,
    DateTime CreatedAt
);

public record AssignRolesRequest(
    List<string> Roles
);
