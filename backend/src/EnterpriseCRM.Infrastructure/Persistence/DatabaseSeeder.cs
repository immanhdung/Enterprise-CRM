using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Domain.Constants;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
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

            // 9. Seed Sample CRM Records (Leads, Accounts, Contacts, Deals, Tasks, Appointments)
            if (!await context.Leads.AnyAsync())
            {
                var salesEmp = await context.Employees.Include(e => e.User).FirstOrDefaultAsync(e => e.User.Email == "sales@enterprisecrm.local");
                var googleSource = await context.LeadSources.FirstOrDefaultAsync(s => s.Name == "Google Search Ads");
                var linkedinSource = await context.LeadSources.FirstOrDefaultAsync(s => s.Name == "LinkedIn Enterprise");
                var referralSource = await context.LeadSources.FirstOrDefaultAsync(s => s.Name == "Partner / Client Referral");

                var sampleLeads = new[]
                {
                    new Lead
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Jonathan",
                        LastName = "Hayes",
                        CompanyName = "Vanguard Innovations",
                        JobTitle = "VP of Digital Transformation",
                        Phone = "+1 (555) 234-8901",
                        Email = "j.hayes@vanguard-innovations.io",
                        EstimatedBudget = 45000m,
                        Status = LeadStatus.New,
                        Score = 85,
                        IsKeyAccount = true,
                        Priority = "High",
                        Campaign = "Q4 Enterprise Cloud",
                        NextFollowUpDate = DateTime.UtcNow.AddDays(2),
                        Description = "Looking for multi-cloud migration architecture and 24/7 managed support.",
                        SourceId = linkedinSource?.Id,
                        AssignedEmployeeId = salesEmp?.Id
                    },
                    new Lead
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Sophia",
                        LastName = "Chen",
                        CompanyName = "Apex Data Systems",
                        JobTitle = "Chief Technology Officer",
                        Phone = "+1 (555) 345-6789",
                        Email = "schen@apexdatasystems.com",
                        EstimatedBudget = 80000m,
                        Status = LeadStatus.Contacted,
                        Score = 90,
                        IsKeyAccount = true,
                        Priority = "Urgent",
                        Campaign = "Google Inbound SEM",
                        NextFollowUpDate = DateTime.UtcNow.AddDays(1),
                        Description = "Enterprise CRM integration and custom workflow microservices pipeline.",
                        SourceId = googleSource?.Id,
                        AssignedEmployeeId = salesEmp?.Id
                    },
                    new Lead
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Carlos",
                        LastName = "Mendoza",
                        CompanyName = "Solaria Logistics",
                        JobTitle = "Operations Director",
                        Phone = "+1 (555) 456-7890",
                        Email = "carlos.m@solaria-logistics.net",
                        EstimatedBudget = 18000m,
                        Status = LeadStatus.Qualified,
                        Score = 70,
                        IsKeyAccount = false,
                        Priority = "Medium",
                        Campaign = "Partner Referral",
                        NextFollowUpDate = DateTime.UtcNow.AddDays(4),
                        Description = "Fleet tracking API integration and operations dashboard.",
                        SourceId = referralSource?.Id,
                        AssignedEmployeeId = salesEmp?.Id
                    }
                };
                context.Leads.AddRange(sampleLeads);
                await context.SaveChangesAsync();
            }

            // 10. Seed Sample Accounts, Contacts, Deals, Tasks, Appointments
            if (!await context.Accounts.AnyAsync())
            {
                var salesEmp = await context.Employees.Include(e => e.User).FirstOrDefaultAsync(e => e.User.Email == "sales@enterprisecrm.local");
                var stages = await context.PipelineStages.OrderBy(s => s.OrderIndex).ToListAsync();

                // Account 1: Nexus Global Tech
                var accNexus = new Account
                {
                    Id = Guid.NewGuid(),
                    Code = "ACC-2026-NEXUS",
                    Name = "Nexus Global Technologies",
                    TaxCode = "US-987654321",
                    Industry = "Information Technology",
                    Website = "https://nexusglobal.tech",
                    Phone = "+1 (555) 789-0123",
                    Email = "procurement@nexusglobal.tech",
                    BillingAddress = "500 Oracle Parkway, Suite 400, Redwood City, CA 94065",
                    Classification = "VIP",
                    Status = "Active",
                    Description = "Global enterprise software provider with 1,200+ team members.",
                    AssignedEmployeeId = salesEmp?.Id
                };
                context.Accounts.Add(accNexus);

                var contactNexus = new Contact
                {
                    Id = Guid.NewGuid(),
                    AccountId = accNexus.Id,
                    FirstName = "Robert",
                    LastName = "Sterling",
                    Email = "r.sterling@nexusglobal.tech",
                    Phone = "+1 (555) 789-0124",
                    Position = "Senior Director of Procurement",
                    Department = "Global Sourcing",
                    IsPrimary = true,
                    Status = "Active"
                };
                context.Contacts.Add(contactNexus);

                // Account 2: Apex Financial Group
                var accApex = new Account
                {
                    Id = Guid.NewGuid(),
                    Code = "ACC-2026-APEX",
                    Name = "Apex Financial Group",
                    TaxCode = "US-123456789",
                    Industry = "Financial Services",
                    Website = "https://apexfinancial.com",
                    Phone = "+1 (555) 890-1234",
                    Email = "info@apexfinancial.com",
                    BillingAddress = "200 Wall Street, Floor 18, New York, NY 10005",
                    Classification = "VIP",
                    Status = "Active",
                    Description = "Leading wealth management and corporate investment consultancy.",
                    AssignedEmployeeId = salesEmp?.Id
                };
                context.Accounts.Add(accApex);

                var contactApex = new Contact
                {
                    Id = Guid.NewGuid(),
                    AccountId = accApex.Id,
                    FirstName = "Claire",
                    LastName = "Vanderbilt",
                    Email = "claire.v@apexfinancial.com",
                    Phone = "+1 (555) 890-1235",
                    Position = "Managing Partner",
                    Department = "Executive Leadership",
                    IsPrimary = true,
                    Status = "Active"
                };
                context.Contacts.Add(contactApex);
                await context.SaveChangesAsync();

                // Deals for Accounts
                if (stages.Count >= 4)
                {
                    var deal1 = new Deal
                    {
                        Id = Guid.NewGuid(),
                        Code = "DEAL-2026-001",
                        Title = "Nexus Enterprise CRM & ERP Modernization",
                        AccountId = accNexus.Id,
                        ContactId = contactNexus.Id,
                        StageId = stages[2].Id, // Proposal Delivered (50%)
                        Amount = 95000m,
                        ExpectedClosingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
                        Status = DealStatus.Open,
                        Source = "LinkedIn Enterprise",
                        Description = "Comprehensive enterprise service overhaul including cloud data pipeline.",
                        AssignedEmployeeId = salesEmp?.Id
                    };
                    context.Deals.Add(deal1);

                    var deal2 = new Deal
                    {
                        Id = Guid.NewGuid(),
                        Code = "DEAL-2026-002",
                        Title = "Apex Financial Automated Reporting Suite",
                        AccountId = accApex.Id,
                        ContactId = contactApex.Id,
                        StageId = stages[3].Id, // Contract Negotiation (75%)
                        Amount = 145000m,
                        ExpectedClosingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)),
                        Status = DealStatus.Open,
                        Source = "Partner Referral",
                        Description = "SOC2 compliant financial reporting and real-time ledger synchronization.",
                        AssignedEmployeeId = salesEmp?.Id
                    };
                    context.Deals.Add(deal2);

                    var deal3 = new Deal
                    {
                        Id = Guid.NewGuid(),
                        Code = "DEAL-2026-003",
                        Title = "Nexus Phase 2 Security Architecture Audit",
                        AccountId = accNexus.Id,
                        ContactId = contactNexus.Id,
                        StageId = stages[1].Id, // Contacted & Qualified (25%)
                        Amount = 40000m,
                        ExpectedClosingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(2)),
                        Status = DealStatus.Open,
                        Source = "Existing Client",
                        Description = "Penetration testing and zero-trust framework audit.",
                        AssignedEmployeeId = salesEmp?.Id
                    };
                    context.Deals.Add(deal3);
                    await context.SaveChangesAsync();

                    // Seed Stage Histories
                    context.DealStageHistories.Add(new DealStageHistory
                    {
                        Id = Guid.NewGuid(),
                        DealId = deal1.Id,
                        ToStageId = stages[2].Id,
                        ChangedByName = "Elena Rostova",
                        ChangedAt = DateTime.UtcNow.AddDays(-5),
                        Notes = "Proposal delivered during executive demo."
                    });

                    context.DealStageHistories.Add(new DealStageHistory
                    {
                        Id = Guid.NewGuid(),
                        DealId = deal2.Id,
                        ToStageId = stages[3].Id,
                        ChangedByName = "Elena Rostova",
                        ChangedAt = DateTime.UtcNow.AddDays(-2),
                        Notes = "Draft contract sent for legal review."
                    });

                    // Seed Tasks
                    context.Tasks.Add(new TaskItem
                    {
                        Id = Guid.NewGuid(),
                        Title = "Finalize CPQ Pricing Matrix for Nexus",
                        Description = "Confirm tier-3 volume discounts with engineering lead before Wednesday.",
                        Priority = "Urgent",
                        Status = "Pending",
                        DueDate = DateTime.UtcNow.AddDays(1),
                        AssignedEmployeeId = salesEmp?.Id,
                        AccountId = accNexus.Id,
                        DealId = deal1.Id
                    });

                    context.Tasks.Add(new TaskItem
                    {
                        Id = Guid.NewGuid(),
                        Title = "Send Legal Addendum to Claire Vanderbilt",
                        Description = "Incorporate compliance terms requested by Apex general counsel.",
                        Priority = "High",
                        Status = "InProgress",
                        DueDate = DateTime.UtcNow.AddDays(2),
                        AssignedEmployeeId = salesEmp?.Id,
                        AccountId = accApex.Id,
                        DealId = deal2.Id
                    });

                    // Seed Appointments
                    context.Appointments.Add(new Appointment
                    {
                        Id = Guid.NewGuid(),
                        Title = "Contract Review & E-Sign Alignment",
                        Description = "Review SLA clauses and finalize onboarding timeline.",
                        StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(14),
                        EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(15),
                        Location = "Zoom Video Conference",
                        MeetingLink = "https://zoom.us/j/9876543210",
                        Status = "Scheduled",
                        EmployeeId = salesEmp!.Id,
                        AccountId = accApex.Id,
                        ContactId = contactApex.Id,
                        DealId = deal2.Id
                    });

                    // Seed Crm Activities
                    context.CrmActivities.Add(new CrmActivity
                    {
                        Id = Guid.NewGuid(),
                        AccountId = accNexus.Id,
                        ContactId = contactNexus.Id,
                        DealId = deal1.Id,
                        ActivityType = "Meeting",
                        Subject = "Architecture Alignment Call",
                        Notes = "Client agreed on microservices approach; requested revised quotation by Friday.",
                        PerformedByName = "Elena Rostova",
                        CompletedAt = DateTime.UtcNow.AddDays(-3)
                    });

                    context.CrmActivities.Add(new CrmActivity
                    {
                        Id = Guid.NewGuid(),
                        AccountId = accApex.Id,
                        ContactId = contactApex.Id,
                        DealId = deal2.Id,
                        ActivityType = "Call",
                        Subject = "Follow-up regarding SOC2 compliance",
                        Notes = "Addressed all auditor questionnaires; legal signoff expected next week.",
                        PerformedByName = "Elena Rostova",
                        CompletedAt = DateTime.UtcNow.AddDays(-1)
                    });

                    await context.SaveChangesAsync();
                }
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
