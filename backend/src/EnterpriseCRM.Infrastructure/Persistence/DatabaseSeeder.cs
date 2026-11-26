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

            await context.Database.ExecuteSqlRawAsync(@"
                DROP INDEX IF EXISTS ""IX_quotations_QuotationNumber"";
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_quotations_QuotationNumber_RevisionNumber"" ON quotations (""QuotationNumber"", ""RevisionNumber"");
            ");

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

            // CEO / BOD: High-level view, approve, reports, reconciliation, performance & payroll approval
            AssignPermissionsToRole(existingRoles[SystemRoles.CeoBod], new[]
            {
                Permissions.LeadsView, Permissions.CustomersView, Permissions.DealsView,
                Permissions.QuotationsView, Permissions.QuotationsApprove,
                Permissions.ContractsView, Permissions.ContractsApprove,
                Permissions.ProjectsView, Permissions.InvoicesView, Permissions.InvoicesApprove,
                Permissions.PaymentsView, Permissions.PaymentsReverse, Permissions.FinanceView, Permissions.FinanceExport,
                Permissions.ReceivablesView, Permissions.ReceivablesManage,
                Permissions.ReconciliationView, Permissions.ReconciliationManage,
                Permissions.CommissionsView, Permissions.CommissionsApprove,
                Permissions.BonusesView, Permissions.BonusesApprove,
                Permissions.PayrollView, Permissions.PayrollApprove, Permissions.PayrollFinalize,
                Permissions.KpisView, Permissions.KpisApprove,
                Permissions.PerformanceViewAll,
                Permissions.EmployeesView,
                Permissions.DepartmentsView, Permissions.TeamsView, Permissions.AuditView,
                Permissions.DashboardExecutiveView, Permissions.ReportsView, Permissions.ReportsGenerate, Permissions.ReportsSchedule,
                Permissions.AiAssistantUse, Permissions.AiAnalyticsView, Permissions.AiQueryExecute,
                Permissions.WorkflowsView, Permissions.AlertsView
            });

            // Manager: Lead assign, Deal manage, Quote approve, Team manage, KPI evaluate
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
                Permissions.CommissionsView, Permissions.BonusesView, Permissions.PayrollView,
                Permissions.KpisView, Permissions.KpisManage, Permissions.KpisEvaluate,
                Permissions.PerformanceViewTeam,
                Permissions.DashboardManagerView, Permissions.DashboardSalesView,
                Permissions.WorkflowsView, Permissions.WorkflowsManage, Permissions.WorkflowsExecute,
                Permissions.AlertsView, Permissions.AlertsManage,
                Permissions.ReportsView, Permissions.ReportsGenerate,
                Permissions.AiAssistantUse, Permissions.AiAnalyticsView, Permissions.AiQueryExecute
            });

            // Sales: Leads, Customers, Deals, Quotes, own performance & own payslip
            AssignPermissionsToRole(existingRoles[SystemRoles.Sales], new[]
            {
                Permissions.LeadsView, Permissions.LeadsCreate, Permissions.LeadsEdit,
                Permissions.CustomersView, Permissions.CustomersCreate, Permissions.CustomersEdit,
                Permissions.DealsView, Permissions.DealsCreate, Permissions.DealsEdit, Permissions.DealsStageChange,
                Permissions.QuotationsView, Permissions.QuotationsCreate, Permissions.QuotationsEdit, Permissions.QuotationsSend,
                Permissions.ContractsView,
                Permissions.ProjectsView,
                Permissions.CommissionsViewOwn,
                Permissions.PayrollViewOwn,
                Permissions.KpisView,
                Permissions.PerformanceViewOwn,
                Permissions.DashboardSalesView, Permissions.AiAssistantUse, Permissions.AlertsView
            });

            // Accountant: Invoices, Payments, Payroll, Finance, Phase 5 Receivables & Reconciliation, Phase 6 Commissions & Payroll Processing
            AssignPermissionsToRole(existingRoles[SystemRoles.Accountant], new[]
            {
                Permissions.InvoicesView, Permissions.InvoicesCreate, Permissions.InvoicesEdit, Permissions.InvoicesApprove, Permissions.InvoicesDelete,
                Permissions.PaymentsView, Permissions.PaymentsCreate, Permissions.PaymentsVerify, Permissions.PaymentsReverse,
                Permissions.ReceivablesView, Permissions.ReceivablesManage,
                Permissions.ReconciliationView, Permissions.ReconciliationManage,
                Permissions.FinanceView, Permissions.FinanceExport,
                Permissions.CommissionsView, Permissions.CommissionsManage, Permissions.CommissionsCalculate, Permissions.CommissionsAdjust, Permissions.CommissionsApprove,
                Permissions.BonusesView, Permissions.BonusesManage, Permissions.BonusesApprove,
                Permissions.PayrollView, Permissions.PayrollProcess, Permissions.PayrollApprove, Permissions.PayrollFinalize,
                Permissions.ReportsView, Permissions.ReportsGenerate, Permissions.ReportsSchedule, Permissions.AlertsView, Permissions.AiAssistantUse, Permissions.AiQueryExecute,
                Permissions.KpisView, Permissions.PerformanceViewAll,
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

            // 11. Seed Phase 4 Service Catalog, Price Lists, Contract Templates, Sample Quotation & Contract
            if (!await context.ServiceCategories.AnyAsync())
            {
                logger.LogInformation("Seeding Phase 4 Service Catalog and Billing data...");

                var catCloud = new ServiceCategory
                {
                    Id = Guid.NewGuid(),
                    Code = "CAT-CLOUD",
                    Name = "Cloud & DevOps Solutions",
                    Description = "Cloud migration, containerization, orchestration, and infrastructure as code.",
                    OrderIndex = 1,
                    IsActive = true
                };

                var catSoft = new ServiceCategory
                {
                    Id = Guid.NewGuid(),
                    Code = "CAT-SOFT",
                    Name = "Custom Software Engineering",
                    Description = "Full-stack web, mobile, and enterprise backend engineering services.",
                    OrderIndex = 2,
                    IsActive = true
                };

                var catSec = new ServiceCategory
                {
                    Id = Guid.NewGuid(),
                    Code = "CAT-SEC",
                    Name = "Security & Compliance",
                    Description = "Penetration testing, code audit, SOC2/ISO compliance consultation.",
                    OrderIndex = 3,
                    IsActive = true
                };

                context.ServiceCategories.AddRange(catCloud, catSoft, catSec);
                await context.SaveChangesAsync();

                // Services
                var svcK8s = new ServiceItem
                {
                    Id = Guid.NewGuid(),
                    Code = "SVC-CLOUD-01",
                    Name = "Kubernetes Architecture & GitOps Setup",
                    CategoryId = catCloud.Id,
                    Category = "Cloud",
                    Unit = "Project",
                    BasePrice = 50000000m,
                    CostPrice = 30000000m,
                    VatRate = 10m,
                    Description = "Production Kubernetes cluster deployment with ArgoCD GitOps and observability.",
                    IsActive = true
                };

                var svcMigrate = new ServiceItem
                {
                    Id = Guid.NewGuid(),
                    Code = "SVC-CLOUD-02",
                    Name = "Multi-Cloud Migration Consultation",
                    CategoryId = catCloud.Id,
                    Category = "Cloud",
                    Unit = "Project",
                    BasePrice = 120000000m,
                    CostPrice = 75000000m,
                    VatRate = 10m,
                    Description = "End-to-end cloud migration assessment, planning, data migration and cutover execution.",
                    IsActive = true
                };

                var svcDevMonth = new ServiceItem
                {
                    Id = Guid.NewGuid(),
                    Code = "SVC-DEV-01",
                    Name = "Full-stack Agile Engineering Team",
                    CategoryId = catSoft.Id,
                    Category = "Development",
                    Unit = "Month",
                    BasePrice = 85000000m,
                    CostPrice = 55000000m,
                    VatRate = 10m,
                    Description = "Dedicated team of 1 Senior Full-stack Engineer, 1 QA and 1 Solution Architect.",
                    IsActive = true
                };

                var svcApi = new ServiceItem
                {
                    Id = Guid.NewGuid(),
                    Code = "SVC-DEV-02",
                    Name = "Enterprise API Gateway Integration",
                    CategoryId = catSoft.Id,
                    Category = "Development",
                    Unit = "Project",
                    BasePrice = 65000000m,
                    CostPrice = 40000000m,
                    VatRate = 10m,
                    Description = "Design and build REST/gRPC API microservices with auth, throttling and monitoring.",
                    IsActive = true
                };

                var svcPenTest = new ServiceItem
                {
                    Id = Guid.NewGuid(),
                    Code = "SVC-SEC-01",
                    Name = "Web & Mobile Security Penetration Testing",
                    CategoryId = catSec.Id,
                    Category = "Security",
                    Unit = "Assessment",
                    BasePrice = 40000000m,
                    CostPrice = 25000000m,
                    VatRate = 10m,
                    Description = "Blackbox & greybox security evaluation compliant with OWASP Top 10 standards.",
                    IsActive = true
                };

                context.Services.AddRange(svcK8s, svcMigrate, svcDevMonth, svcApi, svcPenTest);

                // Initial Price Histories
                context.ServicePriceHistories.Add(new ServicePriceHistory
                {
                    ServiceId = svcK8s.Id,
                    OldPrice = 0,
                    NewPrice = svcK8s.BasePrice,
                    ChangedByName = "System Seeder",
                    EffectiveDate = DateTime.UtcNow,
                    Reason = "Initial baseline catalog pricing"
                });

                context.ServicePriceHistories.Add(new ServicePriceHistory
                {
                    ServiceId = svcDevMonth.Id,
                    OldPrice = 0,
                    NewPrice = svcDevMonth.BasePrice,
                    ChangedByName = "System Seeder",
                    EffectiveDate = DateTime.UtcNow,
                    Reason = "Initial baseline catalog pricing"
                });

                // Price Lists
                var plStandard = new PriceList
                {
                    Id = Guid.NewGuid(),
                    Code = "PL-STD-2026",
                    Name = "Standard Commercial Price List 2026",
                    Description = "Default public catalog rate card for 2026 services.",
                    Currency = "VND",
                    StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
                    EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                    IsDefault = true,
                    IsActive = true
                };

                plStandard.Items.Add(new PriceListItem
                {
                    ServiceId = svcK8s.Id,
                    UnitPrice = 50000000m,
                    MinQuantity = 1,
                    MaxDiscountRate = 10m
                });
                plStandard.Items.Add(new PriceListItem
                {
                    ServiceId = svcDevMonth.Id,
                    UnitPrice = 85000000m,
                    MinQuantity = 1,
                    MaxDiscountRate = 15m
                });

                var plVip = new PriceList
                {
                    Id = Guid.NewGuid(),
                    Code = "PL-VIP-2026",
                    Name = "Enterprise Strategic Partner Price List",
                    Description = "Special negotiated rates for strategic enterprise accounts.",
                    Currency = "VND",
                    StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
                    EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                    IsDefault = false,
                    IsActive = true
                };

                plVip.Items.Add(new PriceListItem
                {
                    ServiceId = svcK8s.Id,
                    UnitPrice = 42000000m,
                    MinQuantity = 1,
                    MaxDiscountRate = 20m
                });
                plVip.Items.Add(new PriceListItem
                {
                    ServiceId = svcDevMonth.Id,
                    UnitPrice = 72000000m,
                    MinQuantity = 3,
                    MaxDiscountRate = 25m
                });

                context.PriceLists.AddRange(plStandard, plVip);

                // Contract Templates
                var tmplSow = new ContractTemplate
                {
                    Id = Guid.NewGuid(),
                    Code = "TMPL-SOW",
                    Name = "Statement of Work (SOW) Standard",
                    Category = "SOW",
                    Description = "Standard technical statement of work agreement template.",
                    Content = @"### STATEMENT OF WORK AGREEMENT

**Contract Code:** {{ContractNumber}}  
**Client:** {{CustomerName}} (Tax ID: {{TaxId}})  
**Billing Address:** {{Address}}  
**Attention:** {{ContactName}} ({{ContactEmail}})  

---

#### 1. ENGAGEMENT TIMELINE
This engagement commences on **{{StartDate}}** and concludes on **{{EndDate}}**.

#### 2. COMMERCIAL TERMS & TOTAL VALUE
- Total Contract Value: **{{TotalValue}}**
- Payment Terms: {{PaymentTerms}}

#### 3. GOVERNING LAW & COMPLIANCE
This agreement is governed by the laws of Vietnam. Any disputes shall be submitted to the competent commercial arbitration center.

#### 4. SIGNATURES & ACCEPTANCE
Both parties agree to execute this statement of work in digital and written format with full legal force.",
                    IsDefault = true,
                    IsActive = true
                };

                var tmplMsa = new ContractTemplate
                {
                    Id = Guid.NewGuid(),
                    Code = "TMPL-MSA",
                    Name = "Master Services Agreement (MSA)",
                    Category = "MSA",
                    Description = "Comprehensive enterprise master services agreement framework.",
                    Content = @"### MASTER SERVICES AGREEMENT

**Agreement Reference:** {{ContractNumber}}  
**Effective Date:** {{StartDate}} to {{EndDate}}  

Between **ENTERPRISE CRM SERVICES INC.** and **{{CustomerName}}** (Tax Code: {{TaxId}}).

#### 1. SCOPE OF MASTER AGREEMENT
This Master Services Agreement sets forth the legal, financial and operational framework under which Provider shall deliver professional technology and cloud services to Client.

#### 2. VALUE & PAYMENT SCHEDULE
Total Value: **{{TotalValue}}**
Terms: {{PaymentTerms}}

#### 3. INTELLECTUAL PROPERTY & CONFIDENTIALITY
All deliverables generated under this agreement shall be transferred to Client upon full settlement of contract fees.",
                    IsDefault = false,
                    IsActive = true
                };

                context.ContractTemplates.AddRange(tmplSow, tmplMsa);
                await context.SaveChangesAsync();

                // Seed Sample Approved Quotation & Contract if account exists
                var sampleAccount = await context.Accounts.Include(a => a.Contacts).FirstOrDefaultAsync();
                if (sampleAccount != null)
                {
                    var sampleContact = sampleAccount.Contacts.FirstOrDefault();

                    var sampleQuote = new Quotation
                    {
                        Id = Guid.NewGuid(),
                        QuotationNumber = $"QT-{DateTime.UtcNow:yyyyMMdd}-0001",
                        Version = 1,
                        RevisionNumber = "REV-01",
                        AccountId = sampleAccount.Id,
                        ContactId = sampleContact?.Id,
                        PriceListId = plStandard.Id,
                        ValidUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                        Subtotal = 135000000m,
                        DiscountAmount = 10000000m,
                        VatAmount = 12500000m,
                        TotalAmount = 137500000m,
                        Status = "Approved",
                        TermsAndConditions = "Payment within 30 days of invoice date. Deliverables backed by 90-day warranty.",
                        Notes = "Initial high-priority enterprise migration quote.",
                        ApprovedAt = DateTime.UtcNow.AddDays(-2),
                        ApprovalNotes = "Approved by Sales Director Elena Rostova."
                    };

                    sampleQuote.Items.Add(new QuotationItem
                    {
                        ServiceId = svcK8s.Id,
                        ServiceCode = svcK8s.Code,
                        ServiceName = svcK8s.Name,
                        Unit = svcK8s.Unit,
                        Quantity = 1,
                        UnitPrice = 50000000m,
                        DiscountRate = 10m,
                        DiscountAmount = 5000000m,
                        VatRate = 10m,
                        VatAmount = 4500000m,
                        LineTotal = 49500000m
                    });

                    sampleQuote.Items.Add(new QuotationItem
                    {
                        ServiceId = svcDevMonth.Id,
                        ServiceCode = svcDevMonth.Code,
                        ServiceName = svcDevMonth.Name,
                        Unit = svcDevMonth.Unit,
                        Quantity = 1,
                        UnitPrice = 85000000m,
                        DiscountRate = 5.88m,
                        DiscountAmount = 5000000m,
                        VatRate = 10m,
                        VatAmount = 8000000m,
                        LineTotal = 88000000m
                    });

                    sampleQuote.ApprovalHistories.Add(new QuotationApprovalHistory
                    {
                        Action = "Approved",
                        ActorName = "Sales Director",
                        Comments = "Commercial terms validated against Q4 targets.",
                        Timestamp = DateTime.UtcNow.AddDays(-2)
                    });

                    context.Quotations.Add(sampleQuote);

                    // Sample Contract
                    var sampleContract = new Contract
                    {
                        Id = Guid.NewGuid(),
                        ContractNumber = $"CTR-{DateTime.UtcNow:yyyyMMdd}-0001",
                        Title = $"Kubernetes & Engineering SOW - {sampleAccount.Name}",
                        AccountId = sampleAccount.Id,
                        ContactId = sampleContact?.Id,
                        QuotationId = sampleQuote.Id,
                        TemplateId = tmplSow.Id,
                        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                        EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
                        TotalValue = 137500000m,
                        Status = ContractStatus.Signed,
                        EsignStatus = "Signed",
                        SignedAt = DateTime.UtcNow.AddDays(-1),
                        SignedByClientName = sampleContact?.FullName ?? "Executive Client",
                        SignedByClientEmail = sampleContact?.Email ?? "client@example.com",
                        SignerIpAddress = "113.161.45.89",
                        ScopeOfWork = "- Kubernetes Architecture & GitOps Setup\n- Full-stack Agile Engineering Team (1 Month)",
                        PaymentTerms = "50% upfront, 50% upon deployment verification."
                    };

                    sampleContract.Items.Add(new ContractItem
                    {
                        ServiceId = svcK8s.Id,
                        ServiceName = svcK8s.Name,
                        ServiceCode = svcK8s.Code,
                        Unit = svcK8s.Unit,
                        Quantity = 1,
                        UnitPrice = 50000000m,
                        LineTotal = 49500000m
                    });

                    sampleContract.Histories.Add(new ContractHistory
                    {
                        Action = "Signed",
                        OldStatus = ContractStatus.InReview.ToString(),
                        NewStatus = ContractStatus.Signed.ToString(),
                        ActorName = sampleContact?.FullName ?? "Executive Client",
                        Notes = "Signed digitally via E-Signature portal.",
                        Timestamp = DateTime.UtcNow.AddDays(-1)
                    });

                    context.Contracts.Add(sampleContract);
                    await context.SaveChangesAsync();
                }

                logger.LogInformation("Phase 4 Service Catalog and Billing seeding completed.");
            }

            // 11. Seed Phase 5 Financial Management (Schedules, Payments, Invoices, Collection Logs)
            if (!await context.PaymentSchedules.AnyAsync())
            {
                logger.LogInformation("Seeding Phase 5 Financial Management data...");
                var signedContract = await context.Contracts
                    .Include(c => c.Account)
                    .FirstOrDefaultAsync(c => c.Status == ContractStatus.Signed || c.Status == ContractStatus.Active);

                if (signedContract != null)
                {
                    // Installment 1: 30% Advance (Paid)
                    var sched1 = new PaymentSchedule
                    {
                        Id = Guid.NewGuid(),
                        ContractId = signedContract.Id,
                        InstallmentNumber = 1,
                        MilestoneTitle = "Advance Payment / Kickoff Milestone",
                        Percentage = 30m,
                        DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)),
                        AmountDue = Math.Round(signedContract.TotalValue * 0.30m, 2),
                        AmountPaid = Math.Round(signedContract.TotalValue * 0.30m, 2),
                        Status = "Paid",
                        Notes = "30% advance on contract signing."
                    };

                    // Installment 2: 40% Architecture & Implementation (Pending)
                    var sched2 = new PaymentSchedule
                    {
                        Id = Guid.NewGuid(),
                        ContractId = signedContract.Id,
                        InstallmentNumber = 2,
                        MilestoneTitle = "Phase 1 Prototype & Core Architecture",
                        Percentage = 40m,
                        DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)),
                        AmountDue = Math.Round(signedContract.TotalValue * 0.40m, 2),
                        AmountPaid = 0m,
                        Status = "Pending",
                        Notes = "40% upon architecture baseline signoff."
                    };

                    // Installment 3: 30% Final Acceptance & Handover (Pending)
                    var sched3 = new PaymentSchedule
                    {
                        Id = Guid.NewGuid(),
                        ContractId = signedContract.Id,
                        InstallmentNumber = 3,
                        MilestoneTitle = "Final Acceptance & Production Go-Live",
                        Percentage = 30m,
                        DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60)),
                        AmountDue = signedContract.TotalValue - sched1.AmountDue - sched2.AmountDue,
                        AmountPaid = 0m,
                        Status = "Pending",
                        Notes = "30% upon final acceptance testing signoff."
                    };

                    context.PaymentSchedules.AddRange(sched1, sched2, sched3);

                    // Seed Confirmed Payment for Sched 1
                    var payment1 = new Payment
                    {
                        Id = Guid.NewGuid(),
                        ContractId = signedContract.Id,
                        AccountId = signedContract.AccountId,
                        PaymentScheduleId = sched1.Id,
                        PaymentNumber = $"PAY-{DateTime.UtcNow:yyyyMMdd}-0001",
                        TransactionReference = "FT26315891290",
                        Amount = sched1.AmountDue,
                        PaymentDate = DateTime.UtcNow.AddDays(-14),
                        PaymentMethod = PaymentMethod.BankTransfer,
                        Status = "Confirmed",
                        BankName = "Techcombank",
                        BankAccountNumber = "19034567891011",
                        SenderName = signedContract.Account.Name,
                        Notes = "Wire transfer received for 30% kickoff milestone.",
                        IsVerified = true,
                        ConfirmedAt = DateTime.UtcNow.AddDays(-14),
                        ConfirmedByName = "Head Accountant"
                    };
                    context.Payments.Add(payment1);

                    // Seed Published Electronic Invoice for Sched 1
                    var invoiceSubtotal = Math.Round(sched1.AmountDue / 1.08m, 2);
                    var invoiceTax = sched1.AmountDue - invoiceSubtotal;
                    var invoice1 = new Invoice
                    {
                        Id = Guid.NewGuid(),
                        ContractId = signedContract.Id,
                        AccountId = signedContract.AccountId,
                        InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-0001",
                        InvoiceSeries = $"1C{DateTime.UtcNow:yy}TBB",
                        EInvoiceProvider = "Mock",
                        ProviderTransactionId = $"EINV-MOCK-{Guid.NewGuid():N}",
                        LookupCode = $"EIN-{DateTime.UtcNow:yyyyMMdd}-AB12CD",
                        LookupUrl = "https://tracuu.einvoice.vn/lookup?code=EIN-20261117-AB12CD",
                        CustomerName = signedContract.Account.Name,
                        CustomerTaxCode = signedContract.Account.TaxCode ?? "0108923456",
                        CustomerAddress = signedContract.Account.BillingAddress ?? "Hanoi, Vietnam",
                        CustomerEmail = signedContract.Account.Email ?? "billing@example.com",
                        IssueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-14)),
                        Subtotal = invoiceSubtotal,
                        TaxRate = 8.0m,
                        TaxAmount = invoiceTax,
                        TotalWithTax = sched1.AmountDue,
                        Status = InvoiceStatus.Published,
                        Items = new List<InvoiceItem>
                        {
                            new()
                            {
                                LineNumber = 1,
                                Description = $"Thanh toán đợt 1 (30%) theo hợp đồng {signedContract.ContractNumber}",
                                Unit = "Gói",
                                Quantity = 1,
                                UnitPrice = invoiceSubtotal,
                                TaxRate = 8.0m,
                                TaxAmount = invoiceTax,
                                LineTotal = sched1.AmountDue
                            }
                        }
                    };
                    context.Invoices.Add(invoice1);

                    // Seed an Overdue Schedule on the contract to test Receivables & Debt Aging
                    var schedOverdue = new PaymentSchedule
                    {
                        Id = Guid.NewGuid(),
                        ContractId = signedContract.Id,
                        InstallmentNumber = 4,
                        MilestoneTitle = "Q3 Infrastructure Maintenance Retainer",
                        Percentage = 0m,
                        DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-18)),
                        AmountDue = 25000000m,
                        AmountPaid = 0m,
                        Status = "Pending",
                        Notes = "Overdue maintenance retainer installment."
                    };
                    context.PaymentSchedules.Add(schedOverdue);

                    // Seed Collection Log
                    var collectionLog = new ReceivableCollectionLog
                    {
                        Id = Guid.NewGuid(),
                        AccountId = signedContract.AccountId,
                        ContractId = signedContract.Id,
                        PaymentScheduleId = schedOverdue.Id,
                        ActivityType = "Phone Call",
                        Notes = "Contacted Finance Manager. Customer committed to wire transfer outstanding payment next week.",
                        LoggedById = Guid.NewGuid(),
                        LoggedByName = "Senior Accountant",
                        LoggedAt = DateTime.UtcNow.AddDays(-2),
                        NextFollowUpDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                        Status = "Customer Promised Payment"
                    };
                    context.ReceivableCollectionLogs.Add(collectionLog);

                    await context.SaveChangesAsync();
                }

                logger.LogInformation("Phase 5 Financial Management seeding completed.");

                // 11. Seed Phase 6 Performance, KPI, Commission & Payroll
                if (!await context.KpiDefinitions.AnyAsync())
                {
                    logger.LogInformation("Seeding Phase 6 Performance & Compensation data...");

                    var kpiDefs = new List<KpiDefinition>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Code = "SALES_REV",
                            Name = "Doanh thu Hợp đồng Ký kết",
                            Category = "Sales",
                            MetricType = "Currency",
                            Unit = "VND",
                            DefaultTarget = 200000000m,
                            Description = "Mục tiêu tổng giá trị hợp đồng ký mới trong kỳ đánh giá."
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Code = "DEALS_WON",
                            Name = "Số Deal Thành công (Won)",
                            Category = "Sales",
                            MetricType = "Count",
                            Unit = "Deals",
                            DefaultTarget = 5m,
                            Description = "Số lượng cơ hội bán hàng chuyển thành công hợp đồng."
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Code = "LEADS_CONV",
                            Name = "Leads Chuyển đổi Khách hàng",
                            Category = "Conversion",
                            MetricType = "Count",
                            Unit = "Leads",
                            DefaultTarget = 15m,
                            Description = "Số khách hàng tiềm năng chuyển đổi thành khách hàng hoặc deal."
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Code = "CALLS_MADE",
                            Name = "Số cuộc gọi tư vấn",
                            Category = "Activity",
                            MetricType = "Count",
                            Unit = "Calls",
                            DefaultTarget = 40m,
                            Description = "Số lượt gọi điện tương tác tư vấn với khách hàng."
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Code = "MEETINGS_HELD",
                            Name = "Số cuộc gặp tư vấn trực tiếp",
                            Category = "Activity",
                            MetricType = "Count",
                            Unit = "Meetings",
                            DefaultTarget = 10m,
                            Description = "Số buổi gặp mặt trực tiếp hoặc online demo dịch vụ."
                        }
                    };
                    context.KpiDefinitions.AddRange(kpiDefs);

                    // Seed sample Commission Policies
                    var policyCollectedCash = new CommissionPolicy
                    {
                        Id = Guid.NewGuid(),
                        Name = "Chính sách Hoa hồng Tiền mặt Thực thu",
                        PolicyType = "CollectedCashBased",
                        CalculationBasis = "CollectedCash",
                        CustomerType = "All",
                        MinThreshold = 10000000m,
                        RatePercent = 4.0m,
                        Version = 1,
                        Description = "Tính hoa hồng 4% trên tổng số tiền khách hàng đã thanh toán thực tế (Confirmed).",
                        EffectiveFrom = new DateOnly(2026, 1, 1),
                        IsActive = true
                    };

                    var policyTiered = new CommissionPolicy
                    {
                        Id = Guid.NewGuid(),
                        Name = "Chính sách Hoa hồng Bậc thang theo KPI",
                        PolicyType = "TargetTierBased",
                        CalculationBasis = "TargetTierBased",
                        CustomerType = "All",
                        RatePercent = 3.0m,
                        Version = 1,
                        Description = "Áp dụng tỷ lệ hoa hồng theo mức độ hoàn thành KPI: <70%: 0%, 70-99%: 2%, 100-119%: 3%, >=120%: 5%.",
                        TierRulesJson = "[{\"minAchievement\":0.0,\"maxAchievement\":69.99,\"rate\":0.0},{\"minAchievement\":70.0,\"maxAchievement\":99.99,\"rate\":2.0},{\"minAchievement\":100.0,\"maxAchievement\":119.99,\"rate\":3.0},{\"minAchievement\":120.0,\"maxAchievement\":9999.0,\"rate\":5.0}]",
                        EffectiveFrom = new DateOnly(2026, 1, 1),
                        IsActive = true
                    };

                    var policyNewCustomer = new CommissionPolicy
                    {
                        Id = Guid.NewGuid(),
                        Name = "Chính sách Khách hàng Mới (New Client Bonus)",
                        PolicyType = "CustomerTypeBased",
                        CalculationBasis = "CustomerTypeBased",
                        CustomerType = "NewCustomer",
                        RatePercent = 5.0m,
                        Version = 1,
                        Description = "Khuyến khích mở rộng thị trường: Hoa hồng 5% cho tất cả hợp đồng với khách hàng mới.",
                        EffectiveFrom = new DateOnly(2026, 1, 1),
                        IsActive = true
                    };

                    context.CommissionPolicies.AddRange(policyCollectedCash, policyTiered, policyNewCustomer);

                    // Seed Bonus Policies
                    var bonusTopPerformer = new BonusPolicy
                    {
                        Id = Guid.NewGuid(),
                        Name = "Thưởng Xuất sắc Tháng (Top Sales)",
                        Type = "IndividualPerformance",
                        Period = "Monthly",
                        TargetThreshold = 150000000m,
                        BonusAmount = 5000000m,
                        Description = "Thưởng nóng 5 triệu VND cho cá nhân đạt doanh số trên 150 triệu trong tháng.",
                        IsActive = true
                    };

                    var bonusNewClient = new BonusPolicy
                    {
                        Id = Guid.NewGuid(),
                        Name = "Thưởng Mốc Khách hàng Mới",
                        Type = "NewCustomerAcquisition",
                        Period = "Monthly",
                        TargetThreshold = 3m,
                        BonusAmount = 2000000m,
                        Description = "Thưởng 2 triệu VND khi nhân viên mang về từ 3 khách hàng mới trở lên.",
                        IsActive = true
                    };

                    context.BonusPolicies.AddRange(bonusTopPerformer, bonusNewClient);

                    // Seed sample active Sales Employee KPI assignment
                    var salesEmp = await context.Employees.FirstOrDefaultAsync(e => e.Position.Contains("Sales") || e.Position.Contains("Specialist"));
                    if (salesEmp != null)
                    {
                        var now = DateTime.UtcNow;
                        var kpiAssignment = new Kpi
                        {
                            Id = Guid.NewGuid(),
                            EmployeeId = salesEmp.Id,
                            DefinitionId = kpiDefs[0].Id,
                            Scope = "Individual",
                            PeriodMonth = now.Month,
                            PeriodYear = now.Year,
                            TargetRevenue = 200000000m,
                            AchievedRevenue = 145000000m,
                            TargetDealsWon = 5,
                            AchievedDealsWon = 4,
                            TargetNewLeads = 15,
                            AchievedNewLeads = 12,
                            TargetCalls = 40,
                            AchievedCalls = 38,
                            TargetMeetings = 10,
                            AchievedMeetings = 9,
                            WeightPercent = 100m,
                            AchievementScore = 85.0m,
                            Status = "Approved",
                            ApprovalNotes = "Target approved by Sales Manager for current month."
                        };
                        context.Kpis.Add(kpiAssignment);
                    }

                    await context.SaveChangesAsync();
                    logger.LogInformation("Phase 6 baseline performance and compensation data seeded successfully.");
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
