using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Domain.Constants;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            logger.LogInformation("Starting database seeding...");

            // 1. Seed Permissions
            var existingPermissions = await context.Permissions.ToDictionaryAsync(p => p.Code, p => p);
            foreach (var (code, module, desc) in Permissions.AllPermissions)
            {
                if (!existingPermissions.ContainsKey(code))
                {
                    var p = new Permission
                    {
                        Id = Guid.NewGuid(),
                        Code = code,
                        Module = module,
                        Description = desc
                    };
                    context.Permissions.Add(p);
                    existingPermissions[code] = p;
                }
            }
            await context.SaveChangesAsync();

            // 2. Seed Roles
            var existingRoles = await context.Roles.ToDictionaryAsync(r => r.Name, r => r);
            var roleDefinitions = new[]
            {
                (SystemRoles.Admin, "System Administrator with full organizational access", true),
                (SystemRoles.CeoBod, "Board of Directors / Chief Executive Officer", true),
                (SystemRoles.Manager, "Department or Regional Team Manager", true),
                (SystemRoles.Sales, "Sales Representative and Account Executive", true),
                (SystemRoles.Accountant, "Finance Officer and Accountant", true)
            };

            foreach (var (name, desc, isSystem) in roleDefinitions)
            {
                if (!existingRoles.ContainsKey(name))
                {
                    var role = new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Description = desc,
                        IsSystem = isSystem
                    };
                    context.Roles.Add(role);
                    existingRoles[name] = role;
                }
            }
            await context.SaveChangesAsync();

            // 3. Seed Role-Permissions mappings
            var existingRolePermissions = await context.RolePermissions
                .Select(rp => $"{rp.RoleId}_{rp.PermissionId}")
                .ToHashSetAsync();

            void AssignPermissionsToRole(Role role, IEnumerable<string> permCodes)
            {
                foreach (var code in permCodes)
                {
                    if (existingPermissions.TryGetValue(code, out var perm))
                    {
                        var key = $"{role.Id}_{perm.Id}";
                        if (!existingRolePermissions.Contains(key))
                        {
                            context.RolePermissions.Add(new RolePermission
                            {
                                RoleId = role.Id,
                                PermissionId = perm.Id
                            });
                            existingRolePermissions.Add(key);
                        }
                    }
                }
            }

            // Admin: All
            AssignPermissionsToRole(existingRoles[SystemRoles.Admin], Permissions.AllPermissions.Select(p => p.Code));

            // CEO / BOD: High-level view, approve, reports
            AssignPermissionsToRole(existingRoles[SystemRoles.CeoBod], new[]
            {
                Permissions.LeadsView, Permissions.CustomersView, Permissions.DealsView,
                Permissions.QuotationsView, Permissions.QuotationsApprove,
                Permissions.ContractsView, Permissions.ContractsApprove,
                Permissions.ProjectsView, Permissions.InvoicesView, Permissions.InvoicesApprove,
                Permissions.PaymentsView, Permissions.FinanceView, Permissions.FinanceExport,
                Permissions.CommissionsView, Permissions.PayrollView, Permissions.EmployeesView,
                Permissions.DepartmentsView, Permissions.TeamsView, Permissions.AuditView
            });

            // Manager: Lead assign, Deal manage, Quote approve, Team manage
            AssignPermissionsToRole(existingRoles[SystemRoles.Manager], new[]
            {
                Permissions.LeadsView, Permissions.LeadsCreate, Permissions.LeadsEdit, Permissions.LeadsAssign, Permissions.LeadsExport,
                Permissions.CustomersView, Permissions.CustomersCreate, Permissions.CustomersEdit,
                Permissions.DealsView, Permissions.DealsCreate, Permissions.DealsEdit, Permissions.DealsStageChange,
                Permissions.QuotationsView, Permissions.QuotationsCreate, Permissions.QuotationsEdit, Permissions.QuotationsApprove, Permissions.QuotationsSend,
                Permissions.ContractsView, Permissions.ContractsCreate, Permissions.ContractsEdit,
                Permissions.ProjectsView, Permissions.ProjectsCreate, Permissions.ProjectsEdit,
                Permissions.InvoicesView, Permissions.InvoicesCreate,
                Permissions.PaymentsView,
                Permissions.EmployeesView, Permissions.DepartmentsView, Permissions.TeamsView, Permissions.TeamsManage,
                Permissions.CommissionsView
            });

            // Sales: Leads, Customers, Deals, Quotes
            AssignPermissionsToRole(existingRoles[SystemRoles.Sales], new[]
            {
                Permissions.LeadsView, Permissions.LeadsCreate, Permissions.LeadsEdit,
                Permissions.CustomersView, Permissions.CustomersCreate, Permissions.CustomersEdit,
                Permissions.DealsView, Permissions.DealsCreate, Permissions.DealsEdit, Permissions.DealsStageChange,
                Permissions.QuotationsView, Permissions.QuotationsCreate, Permissions.QuotationsEdit, Permissions.QuotationsSend,
                Permissions.ContractsView,
                Permissions.ProjectsView,
                Permissions.CommissionsView
            });

            // Accountant: Invoices, Payments, Payroll, Finance
            AssignPermissionsToRole(existingRoles[SystemRoles.Accountant], new[]
            {
                Permissions.InvoicesView, Permissions.InvoicesCreate, Permissions.InvoicesEdit, Permissions.InvoicesApprove, Permissions.InvoicesDelete,
                Permissions.PaymentsView, Permissions.PaymentsCreate, Permissions.PaymentsVerify,
                Permissions.FinanceView, Permissions.FinanceExport,
                Permissions.CommissionsView, Permissions.CommissionsManage,
                Permissions.PayrollView, Permissions.PayrollProcess,
                Permissions.QuotationsView, Permissions.ContractsView, Permissions.CustomersView
            });

            await context.SaveChangesAsync();

            // 4. Seed Departments
            var deptSales = await context.Departments.FirstOrDefaultAsync(d => d.Code == "DEP-SALES");
            if (deptSales == null)
            {
                deptSales = new Department
                {
                    Id = Guid.NewGuid(),
                    Code = "DEP-SALES",
                    Name = "Sales & Marketing",
                    Description = "Revenue generation, client acquisition, and account growth"
                };
                context.Departments.Add(deptSales);
            }

            var deptTech = await context.Departments.FirstOrDefaultAsync(d => d.Code == "DEP-TECH");
            if (deptTech == null)
            {
                deptTech = new Department
                {
                    Id = Guid.NewGuid(),
                    Code = "DEP-TECH",
                    Name = "Engineering & Delivery",
                    Description = "Client project delivery, implementation and technical consulting"
                };
                context.Departments.Add(deptTech);
            }

            var deptFin = await context.Departments.FirstOrDefaultAsync(d => d.Code == "DEP-FIN");
            if (deptFin == null)
            {
                deptFin = new Department
                {
                    Id = Guid.NewGuid(),
                    Code = "DEP-FIN",
                    Name = "Finance & Accounting",
                    Description = "Financial reporting, billing, invoicing, and treasury"
                };
                context.Departments.Add(deptFin);
            }

            var deptBod = await context.Departments.FirstOrDefaultAsync(d => d.Code == "DEP-BOD");
            if (deptBod == null)
            {
                deptBod = new Department
                {
                    Id = Guid.NewGuid(),
                    Code = "DEP-BOD",
                    Name = "Executive Board",
                    Description = "Corporate governance and strategic executive leadership"
                };
                context.Departments.Add(deptBod);
            }
            await context.SaveChangesAsync();

            // 5. Seed Teams
            var teamEnterprise = await context.Teams.FirstOrDefaultAsync(t => t.Name == "Enterprise Sales" && t.DepartmentId == deptSales.Id);
            if (teamEnterprise == null)
            {
                teamEnterprise = new Team
                {
                    Id = Guid.NewGuid(),
                    DepartmentId = deptSales.Id,
                    Name = "Enterprise Sales"
                };
                context.Teams.Add(teamEnterprise);
            }

            var teamInbound = await context.Teams.FirstOrDefaultAsync(t => t.Name == "Inbound Sales" && t.DepartmentId == deptSales.Id);
            if (teamInbound == null)
            {
                teamInbound = new Team
                {
                    Id = Guid.NewGuid(),
                    DepartmentId = deptSales.Id,
                    Name = "Inbound Sales"
                };
                context.Teams.Add(teamInbound);
            }
            await context.SaveChangesAsync();

            // 6. Seed Users for each Role
            var defaultPassword = "EnterpriseCRM@2026!";
            var (defaultHash, defaultSalt) = hasher.HashPassword(defaultPassword);

            var usersToSeed = new[]
            {
                ("admin@enterprisecrm.local", "Alexander", "Wright", SystemRoles.Admin, deptBod.Id, (Guid?)null, "EMP-001", "Chief Executive Officer / System Admin", 10000m),
                ("ceo@enterprisecrm.local", "Victoria", "Sterling", SystemRoles.CeoBod, deptBod.Id, (Guid?)null, "EMP-002", "Managing Director", 8500m),
                ("manager@enterprisecrm.local", "Marcus", "Vance", SystemRoles.Manager, deptSales.Id, (Guid?)teamEnterprise.Id, "EMP-003", "Head of Sales", 6000m),
                ("sales@enterprisecrm.local", "Elena", "Rostova", SystemRoles.Sales, deptSales.Id, (Guid?)teamEnterprise.Id, "EMP-004", "Senior Sales Executive", 3500m),
                ("accountant@enterprisecrm.local", "David", "Miller", SystemRoles.Accountant, deptFin.Id, (Guid?)null, "EMP-005", "Senior Financial Controller", 4500m)
            };

            foreach (var (email, first, last, roleName, deptId, teamId, empCode, pos, salary) in usersToSeed)
            {
                var user = await context.Users
                    .Include(u => u.UserRoles)
                    .Include(u => u.EmployeeProfile)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = email,
                        PasswordHash = defaultHash,
                        Salt = defaultSalt,
                        FirstName = first,
                        LastName = last,
                        IsActive = true
                    };
                    context.Users.Add(user);
                    await context.SaveChangesAsync();

                    // Assign Role
                    if (existingRoles.TryGetValue(roleName, out var role))
                    {
                        context.UserRoles.Add(new UserRole
                        {
                            UserId = user.Id,
                            RoleId = role.Id
                        });
                    }

                    // Create Employee Profile
                    var emp = new Employee
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        DepartmentId = deptId,
                        TeamId = teamId,
                        EmployeeCode = empCode,
                        Position = pos,
                        Phone = "+1 (555) 019-2834",
                        HireDate = new DateOnly(2025, 1, 15),
                        BaseSalary = salary,
                        IsActive = true
                    };
                    context.Employees.Add(emp);
                    await context.SaveChangesAsync();
                }
            }

            // 7. Seed Pipeline Stages
            if (!await context.PipelineStages.AnyAsync())
            {
                var stages = new[]
                {
                    new PipelineStage { Id = Guid.NewGuid(), Name = "Lead In", OrderIndex = 1, Probability = 10, ColorCode = "#6366F1", IsWonStage = false, IsLostStage = false },
                    new PipelineStage { Id = Guid.NewGuid(), Name = "Contacted & Qualified", OrderIndex = 2, Probability = 25, ColorCode = "#3B82F6", IsWonStage = false, IsLostStage = false },
                    new PipelineStage { Id = Guid.NewGuid(), Name = "Proposal Delivered", OrderIndex = 3, Probability = 50, ColorCode = "#EAB308", IsWonStage = false, IsLostStage = false },
                    new PipelineStage { Id = Guid.NewGuid(), Name = "Contract Negotiation", OrderIndex = 4, Probability = 75, ColorCode = "#F97316", IsWonStage = false, IsLostStage = false },
                    new PipelineStage { Id = Guid.NewGuid(), Name = "Closed Won", OrderIndex = 5, Probability = 100, ColorCode = "#10B981", IsWonStage = true, IsLostStage = false },
                    new PipelineStage { Id = Guid.NewGuid(), Name = "Closed Lost", OrderIndex = 6, Probability = 0, ColorCode = "#EF4444", IsWonStage = false, IsLostStage = true }
                };
                context.PipelineStages.AddRange(stages);
                await context.SaveChangesAsync();
            }

            // 8. Seed Lead Sources
            if (!await context.LeadSources.AnyAsync())
            {
                var sources = new[]
                {
                    new LeadSource { Id = Guid.NewGuid(), Name = "Google Search Ads", Type = "GoogleAds", UtmSource = "google", IsActive = true },
                    new LeadSource { Id = Guid.NewGuid(), Name = "Facebook & Instagram Ads", Type = "FacebookAds", UtmSource = "facebook", IsActive = true },
                    new LeadSource { Id = Guid.NewGuid(), Name = "LinkedIn Enterprise", Type = "Other", UtmSource = "linkedin", IsActive = true },
                    new LeadSource { Id = Guid.NewGuid(), Name = "Website Direct Form", Type = "WebsiteForm", UtmSource = "website", IsActive = true },
                    new LeadSource { Id = Guid.NewGuid(), Name = "Partner / Client Referral", Type = "Other", UtmSource = "referral", IsActive = true }
                };
                context.LeadSources.AddRange(sources);
                await context.SaveChangesAsync();
            }

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database: {Message}", ex.Message);
            throw;
        }
    }
}
