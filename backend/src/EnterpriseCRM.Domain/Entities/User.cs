using EnterpriseCRM.Domain.Common;

namespace EnterpriseCRM.Domain.Entities;

public class User : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsActive { get; set; } = true;
    public string Status { get; set; } = "Active"; // Active, Inactive, Suspended, PendingActivation
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    // Navigation properties
    public Employee? EmployeeProfile { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
