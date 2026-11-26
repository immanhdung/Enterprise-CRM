using EnterpriseCRM.Domain.Common;

namespace EnterpriseCRM.Domain.Entities;

public class Department : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ManagerId { get; set; }

    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

public class Team : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public Guid? TeamLeaderId { get; set; }

    public ICollection<Employee> Members { get; set; } = new List<Employee>();
}

public class Employee : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
    public decimal BaseSalary { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystem { get; set; } = false;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;

    public string DataScope { get; set; } = "Company"; // Own, Assigned, Team, Department, Company
}
