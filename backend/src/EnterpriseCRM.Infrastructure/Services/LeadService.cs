using System.Text;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Leads;
using EnterpriseCRM.Domain.Constants;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class LeadService : ILeadService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LeadService> _logger;

    public LeadService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<LeadService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<PaginatedList<LeadDto>>> GetLeadsAsync(
        string? search,
        LeadStatus? status,
        Guid? sourceId,
        Guid? assignedEmployeeId,
        DateTime? fromDate,
        DateTime? toDate,
        string? sortBy,
        bool sortDescending,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Leads
            .Include(l => l.Source)
            .Include(l => l.AssignedEmployee!)
                .ThenInclude(e => e.User)
            .AsQueryable();

        // RBAC Data Scope Restrictions
        if (_currentUserService.Role != SystemRoles.Admin && _currentUserService.Role != SystemRoles.CeoBod)
        {
            if (_currentUserService.DataScope == DataScope.Own && _currentUserService.UserId.HasValue)
            {
                var myEmployeeId = await _context.Employees
                    .Where(e => e.UserId == _currentUserService.UserId.Value)
                    .Select(e => e.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                query = query.Where(l => l.AssignedEmployeeId == myEmployeeId);
            }
            else if (_currentUserService.DataScope == DataScope.Team && _currentUserService.TeamId.HasValue)
            {
                query = query.Where(l => l.AssignedEmployee != null && l.AssignedEmployee.TeamId == _currentUserService.TeamId.Value);
            }
            else if (_currentUserService.DataScope == DataScope.Department && _currentUserService.DepartmentId.HasValue)
            {
                query = query.Where(l => l.AssignedEmployee != null && l.AssignedEmployee.DepartmentId == _currentUserService.DepartmentId.Value);
            }
        }

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(l =>
                l.FirstName.ToLower().Contains(s) ||
                l.LastName.ToLower().Contains(s) ||
                l.Phone.Contains(s) ||
                (l.Email != null && l.Email.ToLower().Contains(s)) ||
                (l.CompanyName != null && l.CompanyName.ToLower().Contains(s)));
        }

        // Filters
        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        if (sourceId.HasValue)
        {
            query = query.Where(l => l.SourceId == sourceId.Value);
        }

        if (assignedEmployeeId.HasValue)
        {
            query = query.Where(l => l.AssignedEmployeeId == assignedEmployeeId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= toDate.Value);
        }

        // Sorting
        query = (sortBy?.ToLower()) switch
        {
            "name" => sortDescending ? query.OrderByDescending(l => l.LastName).ThenByDescending(l => l.FirstName) : query.OrderBy(l => l.LastName).ThenBy(l => l.FirstName),
            "budget" => sortDescending ? query.OrderByDescending(l => l.EstimatedBudget) : query.OrderBy(l => l.EstimatedBudget),
            "status" => sortDescending ? query.OrderByDescending(l => l.Status) : query.OrderBy(l => l.Status),
            "score" => sortDescending ? query.OrderByDescending(l => l.Score) : query.OrderBy(l => l.Score),
            _ => sortDescending ? query.OrderByDescending(l => l.CreatedAt) : query.OrderBy(l => l.CreatedAt),
        };

        var count = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, pageIndex);
        var size = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .Select(l => new LeadDto(
                l.Id,
                l.FirstName,
                l.LastName,
                l.FullName,
                l.CompanyName,
                l.Phone,
                l.Email,
                l.JobTitle,
                l.EstimatedBudget,
                l.Status,
                l.Score,
                l.IsKeyAccount,
                l.Priority,
                l.Campaign,
                l.NextFollowUpDate,
                l.Description,
                l.Notes,
                l.SourceId,
                l.Source != null ? l.Source.Name : null,
                l.AssignedEmployeeId,
                l.AssignedEmployee != null ? l.AssignedEmployee.User.FullName : null,
                l.ConvertedAccountId,
                l.ConvertedContactId,
                l.ConvertedDealId,
                l.ConvertedAt,
                l.CreatedAt,
                l.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return ApiResponse<PaginatedList<LeadDto>>.SuccessResponse(
            new PaginatedList<LeadDto>(items, count, page, size)
        );
    }

    public async Task<ApiResponse<LeadDto>> GetLeadByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var l = await _context.Leads
            .Include(x => x.Source)
            .Include(x => x.AssignedEmployee!)
                .ThenInclude(e => e.User)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (l == null)
        {
            return ApiResponse<LeadDto>.FailureResponse("Lead not found.", 404);
        }

        var dto = new LeadDto(
            l.Id,
            l.FirstName,
            l.LastName,
            l.FullName,
            l.CompanyName,
            l.Phone,
            l.Email,
            l.JobTitle,
            l.EstimatedBudget,
            l.Status,
            l.Score,
            l.IsKeyAccount,
            l.Priority,
            l.Campaign,
            l.NextFollowUpDate,
            l.Description,
            l.Notes,
            l.SourceId,
            l.Source?.Name,
            l.AssignedEmployeeId,
            l.AssignedEmployee?.User?.FullName,
            l.ConvertedAccountId,
            l.ConvertedContactId,
            l.ConvertedDealId,
            l.ConvertedAt,
            l.CreatedAt,
            l.UpdatedAt
        );

        return ApiResponse<LeadDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<DuplicateCheckResult>> CheckDuplicateAsync(DuplicateCheckRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedPhone = new string(request.Phone.Where(char.IsDigit).ToArray());

        var matchByPhone = await _context.Leads
            .FirstOrDefaultAsync(l => l.Phone.Replace(" ", "").Replace("-", "").EndsWith(normalizedPhone), cancellationToken);

        if (matchByPhone != null)
        {
            return ApiResponse<DuplicateCheckResult>.SuccessResponse(new DuplicateCheckResult(
                true,
                $"Lead with matching phone number exists: {matchByPhone.FullName} ({matchByPhone.Phone})",
                matchByPhone.Id,
                matchByPhone.FullName,
                matchByPhone.Status.ToString()
            ));
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var matchByEmail = await _context.Leads
                .FirstOrDefaultAsync(l => l.Email != null && l.Email.ToLower() == normalizedEmail, cancellationToken);

            if (matchByEmail != null)
            {
                return ApiResponse<DuplicateCheckResult>.SuccessResponse(new DuplicateCheckResult(
                    true,
                    $"Lead with matching email exists: {matchByEmail.FullName} ({matchByEmail.Email})",
                    matchByEmail.Id,
                    matchByEmail.FullName,
                    matchByEmail.Status.ToString()
                ));
            }
        }

        return ApiResponse<DuplicateCheckResult>.SuccessResponse(new DuplicateCheckResult(false, null, null, null, null));
    }

    public async Task<ApiResponse<LeadDto>> CreateLeadAsync(CreateLeadRequest request, CancellationToken cancellationToken = default)
    {
        // Calculate initial Lead Quality Score
        var score = 10;
        if (!string.IsNullOrWhiteSpace(request.Email)) score += 20;
        if (!string.IsNullOrWhiteSpace(request.CompanyName)) score += 20;
        if (request.EstimatedBudget > 5000) score += 30;
        if (request.EstimatedBudget > 20000) score += 20;

        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            CompanyName = request.CompanyName?.Trim(),
            Phone = request.Phone.Trim(),
            Email = request.Email?.Trim().ToLowerInvariant(),
            JobTitle = request.JobTitle?.Trim(),
            EstimatedBudget = request.EstimatedBudget,
            Status = LeadStatus.New,
            Score = score,
            IsKeyAccount = request.EstimatedBudget >= 25000,
            Priority = request.Priority ?? (request.EstimatedBudget >= 25000 ? "High" : "Medium"),
            Campaign = request.Campaign?.Trim(),
            NextFollowUpDate = request.NextFollowUpDate,
            Description = request.Description?.Trim(),
            Notes = request.Notes?.Trim(),
            SourceId = request.SourceId,
            AssignedEmployeeId = request.AssignedEmployeeId
        };

        _context.Leads.Add(lead);

        // Record initial activity
        _context.CrmActivities.Add(new CrmActivity
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            ActivityType = "StatusChange",
            Subject = "Lead Created",
            Notes = $"New lead captured with estimated budget {request.EstimatedBudget:C0}.",
            PerformedByUserId = _currentUserService.UserId,
            PerformedByName = _currentUserService.Email ?? "System",
            CompletedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created new Lead {LeadId} ({FullName})", lead.Id, lead.FullName);

        return await GetLeadByIdAsync(lead.Id, cancellationToken);
    }

    public async Task<ApiResponse<LeadDto>> UpdateLeadAsync(Guid id, UpdateLeadRequest request, CancellationToken cancellationToken = default)
    {
        var lead = await _context.Leads.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (lead == null)
        {
            return ApiResponse<LeadDto>.FailureResponse("Lead not found.", 404);
        }

        var oldStatus = lead.Status;

        lead.FirstName = request.FirstName.Trim();
        lead.LastName = request.LastName.Trim();
        lead.CompanyName = request.CompanyName?.Trim();
        lead.Phone = request.Phone.Trim();
        lead.Email = request.Email?.Trim().ToLowerInvariant();
        lead.JobTitle = request.JobTitle?.Trim();
        lead.EstimatedBudget = request.EstimatedBudget;
        lead.Status = request.Status;
        lead.Score = request.Score;
        lead.IsKeyAccount = request.IsKeyAccount;
        lead.SourceId = request.SourceId;
        lead.AssignedEmployeeId = request.AssignedEmployeeId;
        lead.Priority = request.Priority;
        lead.Campaign = request.Campaign?.Trim();
        lead.NextFollowUpDate = request.NextFollowUpDate;
        lead.Description = request.Description?.Trim();
        lead.Notes = request.Notes?.Trim();

        if (oldStatus != request.Status)
        {
            _context.CrmActivities.Add(new CrmActivity
            {
                Id = Guid.NewGuid(),
                LeadId = lead.Id,
                ActivityType = "StatusChange",
                Subject = $"Status updated to {request.Status}",
                Notes = $"Lead status changed from {oldStatus} to {request.Status}.",
                PerformedByUserId = _currentUserService.UserId,
                PerformedByName = _currentUserService.Email ?? "System",
                CompletedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetLeadByIdAsync(lead.Id, cancellationToken);
    }

    public async Task<ApiResponse<bool>> DeleteLeadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lead = await _context.Leads.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (lead == null)
        {
            return ApiResponse<bool>.FailureResponse("Lead not found.", 404);
        }

        lead.IsDeleted = true;
        lead.DeletedAt = DateTime.UtcNow;
        lead.DeletedBy = _currentUserService.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Lead deleted successfully.");
    }

    public async Task<ApiResponse<LeadDto>> AssignLeadAsync(Guid id, AssignLeadRequest request, CancellationToken cancellationToken = default)
    {
        var lead = await _context.Leads.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (lead == null)
        {
            return ApiResponse<LeadDto>.FailureResponse("Lead not found.", 404);
        }

        Guid? targetEmployeeId = request.EmployeeId;

        // Round Robin Assignment
        if (string.Equals(request.Mode, "RoundRobin", StringComparison.OrdinalIgnoreCase))
        {
            var salesEmployees = await _context.Employees
                .Where(e => e.IsActive && e.User.UserRoles.Any(ur => ur.Role.Name == SystemRoles.Sales || ur.Role.Name == SystemRoles.Manager))
                .OrderBy(e => e.Id)
                .ToListAsync(cancellationToken);

            if (salesEmployees.Any())
            {
                var lastAssignedLead = await _context.Leads
                    .Where(l => l.AssignedEmployeeId != null && l.Id != id)
                    .OrderByDescending(l => l.UpdatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                var lastIndex = lastAssignedLead != null
                    ? salesEmployees.FindIndex(e => e.Id == lastAssignedLead.AssignedEmployeeId)
                    : -1;

                var nextIndex = (lastIndex + 1) % salesEmployees.Count;
                targetEmployeeId = salesEmployees[nextIndex].Id;
            }
        }
        // Workload-Based Assignment
        else if (string.Equals(request.Mode, "Workload", StringComparison.OrdinalIgnoreCase))
        {
            var repWithLeastWorkload = await _context.Employees
                .Where(e => e.IsActive && e.User.UserRoles.Any(ur => ur.Role.Name == SystemRoles.Sales))
                .Select(e => new
                {
                    EmployeeId = e.Id,
                    ActiveLeadCount = _context.Leads.Count(l => l.AssignedEmployeeId == e.Id && l.Status != LeadStatus.Converted && l.Status != LeadStatus.Unqualified)
                })
                .OrderBy(x => x.ActiveLeadCount)
                .FirstOrDefaultAsync(cancellationToken);

            if (repWithLeastWorkload != null)
            {
                targetEmployeeId = repWithLeastWorkload.EmployeeId;
            }
        }

        lead.AssignedEmployeeId = targetEmployeeId;
        lead.Status = LeadStatus.Contacted; // Automatically elevate from New to Contacted upon assignment

        var repName = targetEmployeeId.HasValue
            ? await _context.Employees.Where(e => e.Id == targetEmployeeId.Value).Select(e => e.User.FullName).FirstOrDefaultAsync(cancellationToken)
            : "Unassigned";

        _context.CrmActivities.Add(new CrmActivity
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            ActivityType = "Assignment",
            Subject = $"Assigned to {repName} ({request.Mode})",
            Notes = request.Notes,
            PerformedByUserId = _currentUserService.UserId,
            PerformedByName = _currentUserService.Email ?? "System",
            CompletedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        return await GetLeadByIdAsync(lead.Id, cancellationToken);
    }

    public async Task<ApiResponse<ConvertLeadResult>> ConvertLeadAsync(Guid id, ConvertLeadRequest request, CancellationToken cancellationToken = default)
    {
        var lead = await _context.Leads
            .Include(l => l.Activities)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (lead == null)
        {
            return ApiResponse<ConvertLeadResult>.FailureResponse("Lead not found.", 404);
        }

        if (lead.ConvertedAt.HasValue || lead.Status == LeadStatus.Converted)
        {
            return ApiResponse<ConvertLeadResult>.FailureResponse("This Lead has already been converted. Duplicate conversion is prohibited.", 400);
        }

        // Execute in database transaction to ensure atomicity
        Guid accountId;
        string accountName;

        if (request.ExistingAccountId.HasValue)
        {
            var acc = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.ExistingAccountId.Value, cancellationToken);
            if (acc == null) return ApiResponse<ConvertLeadResult>.FailureResponse("Existing account not found.", 400);
            accountId = acc.Id;
            accountName = acc.Name;
        }
        else
        {
            accountName = !string.IsNullOrWhiteSpace(request.AccountName)
                ? request.AccountName.Trim()
                : (!string.IsNullOrWhiteSpace(lead.CompanyName) ? lead.CompanyName : $"{lead.FullName}'s Organization");

            var newAccount = new Account
            {
                Id = Guid.NewGuid(),
                Code = $"ACC-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
                Name = accountName,
                Phone = lead.Phone,
                Email = lead.Email,
                Classification = lead.IsKeyAccount ? "VIP" : "Standard",
                Status = "Active",
                AssignedEmployeeId = lead.AssignedEmployeeId
            };
            _context.Accounts.Add(newAccount);
            accountId = newAccount.Id;
        }

        Guid contactId;
        string contactName;

        if (request.ExistingContactId.HasValue)
        {
            var c = await _context.Contacts.FirstOrDefaultAsync(x => x.Id == request.ExistingContactId.Value, cancellationToken);
            if (c == null) return ApiResponse<ConvertLeadResult>.FailureResponse("Existing contact not found.", 400);
            contactId = c.Id;
            contactName = c.FullName;
        }
        else
        {
            var cFirst = !string.IsNullOrWhiteSpace(request.ContactFirstName) ? request.ContactFirstName.Trim() : lead.FirstName;
            var cLast = !string.IsNullOrWhiteSpace(request.ContactLastName) ? request.ContactLastName.Trim() : lead.LastName;

            var newContact = new Contact
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                FirstName = cFirst,
                LastName = cLast,
                Email = lead.Email,
                Phone = lead.Phone,
                Position = lead.JobTitle,
                IsPrimary = true
            };
            _context.Contacts.Add(newContact);
            contactId = newContact.Id;
            contactName = newContact.FullName;
        }

        // Pipeline stage for Deal
        var stageId = request.PipelineStageId;
        if (!stageId.HasValue)
        {
            var firstStage = await _context.PipelineStages.OrderBy(s => s.OrderIndex).FirstOrDefaultAsync(cancellationToken);
            stageId = firstStage?.Id ?? Guid.NewGuid();
        }

        var deal = new Deal
        {
            Id = Guid.NewGuid(),
            Code = $"DEAL-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            Title = request.DealTitle.Trim(),
            AccountId = accountId,
            ContactId = contactId,
            StageId = stageId.Value,
            Amount = request.DealAmount > 0 ? request.DealAmount : lead.EstimatedBudget,
            ExpectedClosingDate = request.ExpectedClosingDate,
            Status = DealStatus.Open,
            Source = lead.Source?.Name,
            AssignedEmployeeId = lead.AssignedEmployeeId
        };
        _context.Deals.Add(deal);

        // Record Stage History
        _context.DealStageHistories.Add(new DealStageHistory
        {
            Id = Guid.NewGuid(),
            DealId = deal.Id,
            ToStageId = deal.StageId,
            ChangedByUserId = _currentUserService.UserId,
            ChangedByName = _currentUserService.Email ?? "System",
            ChangedAt = DateTime.UtcNow,
            Notes = "Deal created from Lead conversion."
        });

        // Update Lead entity
        lead.Status = LeadStatus.Converted;
        lead.ConvertedAccountId = accountId;
        lead.ConvertedContactId = contactId;
        lead.ConvertedDealId = deal.Id;
        lead.ConvertedAt = DateTime.UtcNow;

        // Log Timeline Activities
        _context.CrmActivities.Add(new CrmActivity
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            AccountId = accountId,
            ContactId = contactId,
            DealId = deal.Id,
            ActivityType = "DealConverted",
            Subject = $"Converted to Account '{accountName}' and Deal '{deal.Title}'",
            Notes = request.Notes,
            PerformedByUserId = _currentUserService.UserId,
            PerformedByName = _currentUserService.Email ?? "System",
            CompletedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully converted Lead {LeadId} -> Account {AccountId}, Deal {DealId}", lead.Id, accountId, deal.Id);

        return ApiResponse<ConvertLeadResult>.SuccessResponse(new ConvertLeadResult(
            accountId,
            accountName,
            contactId,
            contactName,
            deal.Id,
            deal.Title,
            lead.ConvertedAt.Value
        ), "Lead converted successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<LeadActivityDto>>> GetLeadActivitiesAsync(Guid leadId, CancellationToken cancellationToken = default)
    {
        var activities = await _context.LeadActivities
            .Include(a => a.Employee)
                .ThenInclude(e => e!.User)
            .Where(a => a.LeadId == leadId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new LeadActivityDto(
                a.Id,
                a.LeadId,
                a.ActivityType,
                a.Subject,
                a.Notes,
                a.EmployeeId,
                a.Employee != null ? a.Employee.User.FullName : null,
                a.ScheduledAt,
                a.CompletedAt,
                a.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<LeadActivityDto>>.SuccessResponse(activities);
    }

    public async Task<ApiResponse<LeadActivityDto>> AddLeadActivityAsync(Guid leadId, CreateLeadActivityRequest request, CancellationToken cancellationToken = default)
    {
        var lead = await _context.Leads.FindAsync(new object[] { leadId }, cancellationToken);
        if (lead == null)
        {
            return ApiResponse<LeadActivityDto>.FailureResponse("Lead not found.", 404);
        }

        Guid? empId = null;
        if (_currentUserService.UserId.HasValue)
        {
            empId = await _context.Employees
                .Where(e => e.UserId == _currentUserService.UserId.Value)
                .Select(e => e.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var activity = new LeadActivity
        {
            Id = Guid.NewGuid(),
            LeadId = leadId,
            EmployeeId = empId,
            ActivityType = request.ActivityType,
            Subject = request.Subject.Trim(),
            Notes = request.Notes?.Trim(),
            ScheduledAt = request.ScheduledAt,
            CompletedAt = request.CompletedAt ?? DateTime.UtcNow
        };

        _context.LeadActivities.Add(activity);

        // Also add to global CrmActivities timeline
        _context.CrmActivities.Add(new CrmActivity
        {
            Id = Guid.NewGuid(),
            LeadId = leadId,
            ActivityType = request.ActivityType,
            Subject = request.Subject.Trim(),
            Notes = request.Notes?.Trim(),
            PerformedByUserId = _currentUserService.UserId,
            PerformedByName = _currentUserService.Email ?? "User",
            ScheduledAt = request.ScheduledAt,
            CompletedAt = request.CompletedAt ?? DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        var empName = empId.HasValue
            ? await _context.Employees.Where(e => e.Id == empId.Value).Select(e => e.User.FullName).FirstOrDefaultAsync(cancellationToken)
            : null;

        var dto = new LeadActivityDto(
            activity.Id,
            activity.LeadId,
            activity.ActivityType,
            activity.Subject,
            activity.Notes,
            activity.EmployeeId,
            empName,
            activity.ScheduledAt,
            activity.CompletedAt,
            activity.CreatedAt
        );

        return ApiResponse<LeadActivityDto>.SuccessResponse(dto, "Activity recorded.");
    }

    public async Task<byte[]> ExportLeadsCsvAsync(
        string? search,
        LeadStatus? status,
        Guid? sourceId,
        Guid? assignedEmployeeId,
        CancellationToken cancellationToken = default)
    {
        var response = await GetLeadsAsync(search, status, sourceId, assignedEmployeeId, null, null, "createdAt", true, 1, 5000, cancellationToken);
        var leads = response.Data?.Items ?? new List<LeadDto>();

        var sb = new StringBuilder();
        sb.AppendLine("ID,FullName,CompanyName,Phone,Email,JobTitle,EstimatedBudget,Status,Priority,Score,Source,AssignedEmployee,CreatedAt");

        foreach (var l in leads)
        {
            sb.AppendLine($"\"{l.Id}\",\"{EscapeCsv(l.FullName)}\",\"{EscapeCsv(l.CompanyName)}\",\"{EscapeCsv(l.Phone)}\",\"{EscapeCsv(l.Email)}\",\"{EscapeCsv(l.JobTitle)}\",{l.EstimatedBudget},\"{l.Status}\",\"{l.Priority}\",{l.Score},\"{EscapeCsv(l.SourceName)}\",\"{EscapeCsv(l.AssignedEmployeeName)}\",\"{l.CreatedAt:yyyy-MM-dd HH:mm:ss}\"");
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    public async Task<ApiResponse<LeadImportResult>> ImportLeadsAsync(IReadOnlyList<LeadImportItem> items, CancellationToken cancellationToken = default)
    {
        int success = 0;
        int duplicates = 0;
        int errors = 0;
        var errorMessages = new List<string>();

        var sources = await _context.LeadSources.ToDictionaryAsync(s => s.Name.ToLower(), s => s.Id, cancellationToken);
        var defaultSourceId = sources.Values.FirstOrDefault();

        foreach (var item in items)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(item.FirstName) || string.IsNullOrWhiteSpace(item.LastName) || string.IsNullOrWhiteSpace(item.Phone))
                {
                    errors++;
                    errorMessages.Add($"Row skipped: First name, last name, and phone are required.");
                    continue;
                }

                // Check duplicate
                var dupCheck = await CheckDuplicateAsync(new DuplicateCheckRequest(item.Phone, item.Email), cancellationToken);
                if (dupCheck.Data?.IsDuplicate == true)
                {
                    duplicates++;
                    continue;
                }

                Guid? sourceId = null;
                if (!string.IsNullOrWhiteSpace(item.SourceName) && sources.TryGetValue(item.SourceName.Trim().ToLower(), out var sId))
                {
                    sourceId = sId;
                }
                else
                {
                    sourceId = defaultSourceId;
                }

                await CreateLeadAsync(new CreateLeadRequest(
                    item.FirstName,
                    item.LastName,
                    item.CompanyName,
                    item.Phone,
                    item.Email,
                    item.JobTitle,
                    item.EstimatedBudget,
                    sourceId,
                    null,
                    item.Priority ?? "Medium",
                    null,
                    null,
                    null,
                    item.Notes
                ), cancellationToken);

                success++;
            }
            catch (Exception ex)
            {
                errors++;
                errorMessages.Add($"Error importing {item.FirstName} {item.LastName}: {ex.Message}");
            }
        }

        return ApiResponse<LeadImportResult>.SuccessResponse(new LeadImportResult(
            items.Count,
            success,
            duplicates,
            errors,
            errorMessages
        ), $"Import completed: {success} succeeded, {duplicates} duplicates skipped, {errors} errors.");
    }

    private static string EscapeCsv(string? val)
    {
        if (string.IsNullOrEmpty(val)) return "";
        return val.Replace("\"", "\"\"");
    }
}
