using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Domain.Common;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply Global Query Filter for Soft Delete
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Department>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Team>().HasQueryFilter(e => !e.IsDeleted);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(e => e.Salt).HasMaxLength(128).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
        });

        // Department Configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("departments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
        });

        // Team Configuration
        modelBuilder.Entity<Team>(entity =>
        {
            entity.ToTable("teams");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Teams)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Employee Configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("employees");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EmployeeCode).IsUnique();
            entity.Property(e => e.EmployeeCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.BaseSalary).HasPrecision(18, 2);

            entity.HasOne(e => e.User)
                  .WithOne(u => u.EmployeeProfile)
                  .HasForeignKey<Employee>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Employees)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Team)
                  .WithMany(t => t.Members)
                  .HasForeignKey(e => e.TeamId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Role Configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        });

        // Permission Configuration
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(100).IsRequired();
        });

        // UserRole (Many-to-Many)
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.HasOne(e => e.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(e => e.UserId);

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(e => e.RoleId);
        });

        // RolePermission (Many-to-Many)
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions");
            entity.HasKey(e => new { e.RoleId, e.PermissionId });

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.RolePermissions)
                  .HasForeignKey(e => e.RoleId);

            entity.HasOne(e => e.Permission)
                  .WithMany(p => p.RolePermissions)
                  .HasForeignKey(e => e.PermissionId);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
