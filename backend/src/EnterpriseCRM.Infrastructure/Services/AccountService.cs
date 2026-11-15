using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Accounts;
using EnterpriseCRM.Application.Features.Contacts;
using EnterpriseCRM.Application.Features.Deals;
using EnterpriseCRM.Application.Features.TasksAndActivities;
using EnterpriseCRM.Domain.Constants;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<AccountService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<PaginatedList<AccountDto>>> GetAccountsAsync(
        string? search,
        string? classification,
        string? industry,
        string? status,
        Guid? assignedEmployeeId,
        string? sortBy,
        bool sortDescending,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Accounts
            .Include(a => a.AssignedEmployee!)
                .ThenInclude(e => e.User)
            .Include(a => a.Contacts)
            .Include(a => a.Deals)
            .AsQueryable();

        // RBAC Data Scope Restrictions
        if (_currentUserService.Role != SystemRoles.Admin && _currentUserService.Role != SystemRoles.CeoBod && _currentUserService.Role != SystemRoles.Accountant)
        {
            if (_currentUserService.DataScope == DataScope.Own && _currentUserService.UserId.HasValue)
            {
                var myEmployeeId = await _context.Employees
                    .Where(e => e.UserId == _currentUserService.UserId.Value)
                    .Select(e => e.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                query = query.Where(a => a.AssignedEmployeeId == myEmployeeId);
            }
            else if (_currentUserService.DataScope == DataScope.Team && _currentUserService.TeamId.HasValue)
            {
                query = query.Where(a => a.AssignedEmployee != null && a.AssignedEmployee.TeamId == _currentUserService.TeamId.Value);
            }
            else if (_currentUserService.DataScope == DataScope.Department && _currentUserService.DepartmentId.HasValue)
            {
                query = query.Where(a => a.AssignedEmployee != null && a.AssignedEmployee.DepartmentId == _currentUserService.DepartmentId.Value);
            }
        }

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(a =>
                a.Name.ToLower().Contains(s) ||
                a.Code.ToLower().Contains(s) ||
                (a.TaxCode != null && a.TaxCode.Contains(s)) ||
                (a.Phone != null && a.Phone.Contains(s)) ||
                (a.Email != null && a.Email.ToLower().Contains(s)));
        }

        // Filters
        if (!string.IsNullOrWhiteSpace(classification))
        {
            query = query.Where(a => a.Classification == classification);
        }

        if (!string.IsNullOrWhiteSpace(industry))
        {
            query = query.Where(a => a.Industry == industry);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(a => a.Status == status);
        }

        if (assignedEmployeeId.HasValue)
        {
            query = query.Where(a => a.AssignedEmployeeId == assignedEmployeeId.Value);
        }

        // Sorting
        query = (sortBy?.ToLower()) switch
        {
            "name" => sortDescending ? query.OrderByDescending(a => a.Name) : query.OrderBy(a => a.Name),
            "code" => sortDescending ? query.OrderByDescending(a => a.Code) : query.OrderBy(a => a.Code),
            "classification" => sortDescending ? query.OrderByDescending(a => a.Classification) : query.OrderBy(a => a.Classification),
            _ => sortDescending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
        };

        var count = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, pageIndex);
        var size = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .Select(a => new AccountDto(
                a.Id,
                a.Code,
                a.Name,
                a.TaxCode,
                a.Industry,
                a.Website,
                a.Phone,
                a.Email,
                a.BillingAddress,
                a.Classification,
                a.Status,
                a.Description,
                a.AssignedEmployeeId,
                a.AssignedEmployee != null ? a.AssignedEmployee.User.FullName : null,
                a.Contacts.Count(c => !c.IsDeleted),
                a.Deals.Count(d => !d.IsDeleted),
                a.Deals.Where(d => !d.IsDeleted).Sum(d => d.Amount),
                a.CreatedAt,
                a.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return ApiResponse<PaginatedList<AccountDto>>.SuccessResponse(
            new PaginatedList<AccountDto>(items, count, page, size)
        );
    }

    public async Task<ApiResponse<AccountDetailDto>> GetAccountDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var a = await _context.Accounts
            .Include(x => x.AssignedEmployee!)
                .ThenInclude(e => e.User)
            .Include(x => x.Contacts)
            .Include(x => x.Deals)
                .ThenInclude(d => d.Stage)
            .Include(x => x.Deals)
                .ThenInclude(d => d.AssignedEmployee!)
                    .ThenInclude(e => e.User)
            .Include(x => x.Contracts)
            .Include(x => x.Projects)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (a == null)
        {
            return ApiResponse<AccountDetailDto>.FailureResponse("Account not found.", 404);
        }

        var contactsDto = a.Contacts
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.IsPrimary)
            .ThenBy(c => c.LastName)
            .Select(c => new ContactDto(
                c.Id,
                c.AccountId,
                a.Name,
                c.FirstName,
                c.LastName,
                c.FullName,
                c.Email,
                c.Phone,
                c.Position,
                c.Department,
                c.IsPrimary,
                c.Status,
                c.Notes,
                c.CreatedAt,
                c.UpdatedAt
            ))
            .ToList();

        var dealsDto = a.Deals
            .Where(d => !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DealDto(
                d.Id,
                d.Code,
                d.Title,
                d.AccountId,
                a.Name,
                d.ContactId,
                null,
                d.StageId,
                d.Stage?.Name ?? "",
                d.Stage?.OrderIndex ?? 0,
                d.Stage?.ColorCode ?? "#3B82F6",
                d.Amount,
                d.EffectiveProbability,
                d.ExpectedRevenue,
                d.ExpectedClosingDate,
                d.ActualClosingDate,
                d.Status,
                d.LostReason,
                d.Source,
                d.Description,
                d.AssignedEmployeeId,
                d.AssignedEmployee?.User?.FullName,
                new List<DealStageHistoryDto>(),
                d.CreatedAt,
                d.UpdatedAt
            ))
            .ToList();

        var contractsDto = a.Contracts
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new AccountContractSummaryDto(
                c.Id,
                c.ContractNumber,
                c.Title,
                c.TotalValue,
                c.Status.ToString(),
                c.StartDate,
                c.EndDate
            ))
            .ToList();

        var projectsDto = a.Projects
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new AccountProjectSummaryDto(
                p.Id,
                p.Code,
                p.Name,
                p.Status.ToString(),
                p.ProgressPercent
            ))
            .ToList();

        var timelineEvents = await _context.CrmActivities
            .Where(act => act.AccountId == id)
            .OrderByDescending(act => act.CreatedAt)
            .Take(50)
            .Select(act => new TimelineEventDto(
                act.Id,
                act.ActivityType,
                act.Subject,
                act.Notes,
                act.PerformedByName,
                act.PerformedByUserId,
                act.ScheduledAt,
                act.CompletedAt,
                act.CreatedAt,
                act.LeadId,
                act.AccountId,
                act.ContactId,
                act.DealId
            ))
            .ToListAsync(cancellationToken);

        var detail = new AccountDetailDto(
            a.Id,
            a.Code,
            a.Name,
            a.TaxCode,
            a.Industry,
            a.Website,
            a.Phone,
            a.Email,
            a.BillingAddress,
            a.Classification,
            a.Status,
            a.Description,
            a.AssignedEmployeeId,
            a.AssignedEmployee?.User?.FullName,
            contactsDto,
            dealsDto,
            timelineEvents,
            contractsDto,
            projectsDto,
            a.CreatedAt,
            a.UpdatedAt
        );

        return ApiResponse<AccountDetailDto>.SuccessResponse(detail);
    }

    public async Task<ApiResponse<AccountDto>> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken = default)
    {
        var code = $"ACC-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

        var account = new Account
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = request.Name.Trim(),
            TaxCode = request.TaxCode?.Trim(),
            Industry = request.Industry?.Trim(),
            Website = request.Website?.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim().ToLowerInvariant(),
            BillingAddress = request.BillingAddress?.Trim(),
            Classification = request.Classification ?? "Standard",
            Status = request.Status ?? "Active",
            Description = request.Description?.Trim(),
            AssignedEmployeeId = request.AssignedEmployeeId
        };

        _context.Accounts.Add(account);

        _context.CrmActivities.Add(new CrmActivity
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            ActivityType = "StatusChange",
            Subject = "Account Created",
            Notes = $"New account registered with classification '{account.Classification}'.",
            PerformedByUserId = _currentUserService.UserId,
            PerformedByName = _currentUserService.Email ?? "System",
            CompletedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created Account {AccountId} ({Name})", account.Id, account.Name);

        var empName = account.AssignedEmployeeId.HasValue
            ? await _context.Employees.Where(e => e.Id == account.AssignedEmployeeId.Value).Select(e => e.User.FullName).FirstOrDefaultAsync(cancellationToken)
            : null;

        var dto = new AccountDto(
            account.Id,
            account.Code,
            account.Name,
            account.TaxCode,
            account.Industry,
            account.Website,
            account.Phone,
            account.Email,
            account.BillingAddress,
            account.Classification,
            account.Status,
            account.Description,
            account.AssignedEmployeeId,
            empName,
            0,
            0,
            0m,
            account.CreatedAt,
            account.UpdatedAt
        );

        return ApiResponse<AccountDto>.SuccessResponse(dto, "Account created successfully.", 201);
    }

    public async Task<ApiResponse<AccountDto>> UpdateAccountAsync(Guid id, UpdateAccountRequest request, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (account == null)
        {
            return ApiResponse<AccountDto>.FailureResponse("Account not found.", 404);
        }

        account.Name = request.Name.Trim();
        account.TaxCode = request.TaxCode?.Trim();
        account.Industry = request.Industry?.Trim();
        account.Website = request.Website?.Trim();
        account.Phone = request.Phone?.Trim();
        account.Email = request.Email?.Trim().ToLowerInvariant();
        account.BillingAddress = request.BillingAddress?.Trim();
        account.Classification = request.Classification;
        account.Status = request.Status;
        account.Description = request.Description?.Trim();
        account.AssignedEmployeeId = request.AssignedEmployeeId;

        await _context.SaveChangesAsync(cancellationToken);

        var empName = account.AssignedEmployeeId.HasValue
            ? await _context.Employees.Where(e => e.Id == account.AssignedEmployeeId.Value).Select(e => e.User.FullName).FirstOrDefaultAsync(cancellationToken)
            : null;

        var contactCount = await _context.Contacts.CountAsync(c => c.AccountId == id && !c.IsDeleted, cancellationToken);
        var dealCount = await _context.Deals.CountAsync(d => d.AccountId == id && !d.IsDeleted, cancellationToken);
        var totalDealVal = await _context.Deals.Where(d => d.AccountId == id && !d.IsDeleted).SumAsync(d => (decimal?)d.Amount, cancellationToken) ?? 0m;

        var dto = new AccountDto(
            account.Id,
            account.Code,
            account.Name,
            account.TaxCode,
            account.Industry,
            account.Website,
            account.Phone,
            account.Email,
            account.BillingAddress,
            account.Classification,
            account.Status,
            account.Description,
            account.AssignedEmployeeId,
            empName,
            contactCount,
            dealCount,
            totalDealVal,
            account.CreatedAt,
            account.UpdatedAt
        );

        return ApiResponse<AccountDto>.SuccessResponse(dto, "Account updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAccountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (account == null)
        {
            return ApiResponse<bool>.FailureResponse("Account not found.", 404);
        }

        account.IsDeleted = true;
        account.DeletedAt = DateTime.UtcNow;
        account.DeletedBy = _currentUserService.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Account deleted successfully.");
    }
}
