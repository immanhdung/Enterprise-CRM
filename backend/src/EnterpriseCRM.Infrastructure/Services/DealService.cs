using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Deals;
using EnterpriseCRM.Domain.Constants;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class DealService : IDealService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DealService> _logger;

    public DealService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<DealService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<PaginatedList<DealDto>>> GetDealsAsync(
        string? search,
        Guid? accountId,
        Guid? stageId,
        DealStatus? status,
        Guid? assignedEmployeeId,
        DateTime? fromDate,
        DateTime? toDate,
        string? sortBy,
        bool sortDescending,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Deals
            .Include(d => d.Account)
            .Include(d => d.Contact)
            .Include(d => d.Stage)
            .Include(d => d.AssignedEmployee!)
                .ThenInclude(e => e.User)
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

                query = query.Where(d => d.AssignedEmployeeId == myEmployeeId);
            }
            else if (_currentUserService.DataScope == DataScope.Team && _currentUserService.TeamId.HasValue)
            {
                query = query.Where(d => d.AssignedEmployee != null && d.AssignedEmployee.TeamId == _currentUserService.TeamId.Value);
            }
            else if (_currentUserService.DataScope == DataScope.Department && _currentUserService.DepartmentId.HasValue)
            {
                query = query.Where(d => d.AssignedEmployee != null && d.AssignedEmployee.DepartmentId == _currentUserService.DepartmentId.Value);
            }
        }

        // Filters
        if (accountId.HasValue) query = query.Where(d => d.AccountId == accountId.Value);
        if (stageId.HasValue) query = query.Where(d => d.StageId == stageId.Value);
        if (status.HasValue) query = query.Where(d => d.Status == status.Value);
        if (assignedEmployeeId.HasValue) query = query.Where(d => d.AssignedEmployeeId == assignedEmployeeId.Value);
        if (fromDate.HasValue) query = query.Where(d => d.CreatedAt >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(d => d.CreatedAt <= toDate.Value);

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(d =>
                d.Title.ToLower().Contains(s) ||
                d.Code.ToLower().Contains(s) ||
                d.Account.Name.ToLower().Contains(s));
        }

        // Sorting
        query = (sortBy?.ToLower()) switch
        {
            "title" => sortDescending ? query.OrderByDescending(d => d.Title) : query.OrderBy(d => d.Title),
            "amount" => sortDescending ? query.OrderByDescending(d => d.Amount) : query.OrderBy(d => d.Amount),
            "stage" => sortDescending ? query.OrderByDescending(d => d.Stage.OrderIndex) : query.OrderBy(d => d.Stage.OrderIndex),
            "closingdate" => sortDescending ? query.OrderByDescending(d => d.ExpectedClosingDate) : query.OrderBy(d => d.ExpectedClosingDate),
            _ => sortDescending ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt),
        };

        var count = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, pageIndex);
        var size = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .Select(d => new DealDto(
                d.Id,
                d.Code,
                d.Title,
                d.AccountId,
                d.Account.Name,
                d.ContactId,
                d.Contact != null ? d.Contact.FullName : null,
                d.StageId,
                d.Stage.Name,
                d.Stage.OrderIndex,
                d.Stage.ColorCode,
                d.Amount,
                d.ProbabilityOverride ?? d.Stage.Probability,
                d.Amount * ((d.ProbabilityOverride ?? d.Stage.Probability) / 100m),
                d.ExpectedClosingDate,
                d.ActualClosingDate,
                d.Status,
                d.LostReason,
                d.Source,
                d.Description,
                d.AssignedEmployeeId,
                d.AssignedEmployee != null ? d.AssignedEmployee.User.FullName : null,
                new List<DealStageHistoryDto>(),
                d.CreatedAt,
                d.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return ApiResponse<PaginatedList<DealDto>>.SuccessResponse(
            new PaginatedList<DealDto>(items, count, page, size)
        );
    }

    public async Task<ApiResponse<DealDto>> GetDealByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var d = await _context.Deals
            .Include(x => x.Account)
            .Include(x => x.Contact)
            .Include(x => x.Stage)
            .Include(x => x.AssignedEmployee!)
                .ThenInclude(e => e.User)
            .Include(x => x.StageHistories)
                .ThenInclude(h => h.FromStage)
            .Include(x => x.StageHistories)
                .ThenInclude(h => h.ToStage)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (d == null)
        {
            return ApiResponse<DealDto>.FailureResponse("Deal not found.", 404);
        }

        var histories = d.StageHistories
            .OrderByDescending(h => h.ChangedAt)
            .Select(h => new DealStageHistoryDto(
                h.Id,
                h.DealId,
                h.FromStageId,
                h.FromStage?.Name,
                h.ToStageId,
                h.ToStage.Name,
                h.ChangedByName,
                h.ChangedAt,
                h.DurationDays,
                h.Notes
            ))
            .ToList();

        var prob = d.ProbabilityOverride ?? d.Stage.Probability;
        var expRev = d.Amount * (prob / 100m);

        var dto = new DealDto(
            d.Id,
            d.Code,
            d.Title,
            d.AccountId,
            d.Account.Name,
            d.ContactId,
            d.Contact?.FullName,
            d.StageId,
            d.Stage.Name,
            d.Stage.OrderIndex,
            d.Stage.ColorCode,
            d.Amount,
            prob,
            expRev,
            d.ExpectedClosingDate,
            d.ActualClosingDate,
            d.Status,
            d.LostReason,
            d.Source,
            d.Description,
            d.AssignedEmployeeId,
            d.AssignedEmployee?.User?.FullName,
            histories,
            d.CreatedAt,
            d.UpdatedAt
        );

        return ApiResponse<DealDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<DealDto>> CreateDealAsync(CreateDealRequest request, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts.FindAsync(new object[] { request.AccountId }, cancellationToken);
        if (account == null)
        {
            return ApiResponse<DealDto>.FailureResponse("Associated account not found.", 400);
        }

        var stage = await _context.PipelineStages.FindAsync(new object[] { request.StageId }, cancellationToken);
        if (stage == null)
        {
            return ApiResponse<DealDto>.FailureResponse("Pipeline stage not found.", 400);
        }

        var code = $"DEAL-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

        var deal = new Deal
        {
            Id = Guid.NewGuid(),
            Code = code,
            Title = request.Title.Trim(),
            AccountId = request.AccountId,
            ContactId = request.ContactId,
            StageId = request.StageId,
            Amount = request.Amount,
            ProbabilityOverride = request.ProbabilityOverride,
            ExpectedClosingDate = request.ExpectedClosingDate,
            Status = stage.IsWonStage ? DealStatus.Won : (stage.IsLostStage ? DealStatus.Lost : DealStatus.Open),
            Source = request.Source?.Trim(),
            Description = request.Description?.Trim(),
            AssignedEmployeeId = request.AssignedEmployeeId
        };

        _context.Deals.Add(deal);

        // Record Initial Stage History
        _context.DealStageHistories.Add(new DealStageHistory
        {
            Id = Guid.NewGuid(),
            DealId = deal.Id,
            ToStageId = deal.StageId,
            ChangedByUserId = _currentUserService.UserId,
            ChangedByName = _currentUserService.Email ?? "System",
            ChangedAt = DateTime.UtcNow,
            Notes = "Deal created."
        });

        _context.CrmActivities.Add(new CrmActivity
        {
            Id = Guid.NewGuid(),
            AccountId = deal.AccountId,
            ContactId = deal.ContactId,
            DealId = deal.Id,
            ActivityType = "StageChange",
            Subject = $"Deal Created in stage '{stage.Name}'",
            Notes = $"Initial deal value: {deal.Amount:C0}",
            PerformedByUserId = _currentUserService.UserId,
            PerformedByName = _currentUserService.Email ?? "System",
            CompletedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created Deal {DealId} ({Code})", deal.Id, deal.Code);

        return await GetDealByIdAsync(deal.Id, cancellationToken);
    }

    public async Task<ApiResponse<DealDto>> UpdateDealAsync(Guid id, UpdateDealRequest request, CancellationToken cancellationToken = default)
    {
        var deal = await _context.Deals
            .Include(d => d.Stage)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (deal == null)
        {
            return ApiResponse<DealDto>.FailureResponse("Deal not found.", 404);
        }

        var oldStageId = deal.StageId;

        deal.Title = request.Title.Trim();
        deal.AccountId = request.AccountId;
        deal.ContactId = request.ContactId;
        deal.StageId = request.StageId;
        deal.Amount = request.Amount;
        deal.ProbabilityOverride = request.ProbabilityOverride;
        deal.ExpectedClosingDate = request.ExpectedClosingDate;
        deal.Status = request.Status;
        deal.LostReason = request.LostReason?.Trim();
        deal.Source = request.Source?.Trim();
        deal.Description = request.Description?.Trim();
        deal.AssignedEmployeeId = request.AssignedEmployeeId;

        if (oldStageId != request.StageId)
        {
            var targetStage = await _context.PipelineStages.FindAsync(new object[] { request.StageId }, cancellationToken);
            if (targetStage != null)
            {
                if (targetStage.IsWonStage)
                {
                    deal.Status = DealStatus.Won;
                    deal.ActualClosingDate = DateOnly.FromDateTime(DateTime.UtcNow);
                }
                else if (targetStage.IsLostStage)
                {
                    deal.Status = DealStatus.Lost;
                }

                _context.DealStageHistories.Add(new DealStageHistory
                {
                    Id = Guid.NewGuid(),
                    DealId = deal.Id,
                    FromStageId = oldStageId,
                    ToStageId = request.StageId,
                    ChangedByUserId = _currentUserService.UserId,
                    ChangedByName = _currentUserService.Email ?? "System",
                    ChangedAt = DateTime.UtcNow,
                    Notes = "Stage updated via Deal edit."
                });

                _context.CrmActivities.Add(new CrmActivity
                {
                    Id = Guid.NewGuid(),
                    AccountId = deal.AccountId,
                    ContactId = deal.ContactId,
                    DealId = deal.Id,
                    ActivityType = "StageChange",
                    Subject = $"Moved to stage '{targetStage.Name}'",
                    PerformedByUserId = _currentUserService.UserId,
                    PerformedByName = _currentUserService.Email ?? "System",
                    CompletedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetDealByIdAsync(deal.Id, cancellationToken);
    }

    public async Task<ApiResponse<DealDto>> UpdateDealStageAsync(Guid id, UpdateDealStageRequest request, CancellationToken cancellationToken = default)
    {
        var deal = await _context.Deals
            .Include(d => d.Stage)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (deal == null)
        {
            return ApiResponse<DealDto>.FailureResponse("Deal not found.", 404);
        }

        var targetStage = await _context.PipelineStages.FindAsync(new object[] { request.NewStageId }, cancellationToken);
        if (targetStage == null)
        {
            return ApiResponse<DealDto>.FailureResponse("Target pipeline stage does not exist.", 400);
        }

        var oldStageId = deal.StageId;
        var oldStageName = deal.Stage.Name;

        // Calculate days spent in previous stage
        var lastChange = await _context.DealStageHistories
            .Where(h => h.DealId == id)
            .OrderByDescending(h => h.ChangedAt)
            .FirstOrDefaultAsync(cancellationToken);

        int durationDays = lastChange != null ? (int)(DateTime.UtcNow - lastChange.ChangedAt).TotalDays : 0;

        deal.StageId = request.NewStageId;

        if (targetStage.IsWonStage)
        {
            deal.Status = DealStatus.Won;
            deal.ActualClosingDate = DateOnly.FromDateTime(DateTime.UtcNow);
            deal.LostReason = null;
        }
        else if (targetStage.IsLostStage)
        {
            deal.Status = DealStatus.Lost;
            deal.LostReason = request.LostReason ?? "Closed Lost";
        }
        else if (deal.Status == DealStatus.Won || deal.Status == DealStatus.Lost)
        {
            // Reopened
            deal.Status = DealStatus.Open;
            deal.ActualClosingDate = null;
            deal.LostReason = null;
        }

        // Record Stage History
        _context.DealStageHistories.Add(new DealStageHistory
        {
            Id = Guid.NewGuid(),
            DealId = deal.Id,
            FromStageId = oldStageId,
            ToStageId = request.NewStageId,
            ChangedByUserId = _currentUserService.UserId,
            ChangedByName = _currentUserService.Email ?? "User",
            ChangedAt = DateTime.UtcNow,
            DurationDays = durationDays,
            Notes = request.Notes
        });

        // Record Timeline Activity
        _context.CrmActivities.Add(new CrmActivity
        {
            Id = Guid.NewGuid(),
            AccountId = deal.AccountId,
            ContactId = deal.ContactId,
            DealId = deal.Id,
            ActivityType = "StageChange",
            Subject = $"Stage changed from '{oldStageName}' to '{targetStage.Name}'",
            Notes = request.Notes,
            PerformedByUserId = _currentUserService.UserId,
            PerformedByName = _currentUserService.Email ?? "User",
            CompletedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Moved Deal {DealId} to Stage {StageName}", deal.Id, targetStage.Name);

        return await GetDealByIdAsync(deal.Id, cancellationToken);
    }

    public async Task<ApiResponse<bool>> DeleteDealAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deal = await _context.Deals.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (deal == null)
        {
            return ApiResponse<bool>.FailureResponse("Deal not found.", 404);
        }

        deal.IsDeleted = true;
        deal.DeletedAt = DateTime.UtcNow;
        deal.DeletedBy = _currentUserService.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Deal deleted successfully.");
    }

    public async Task<ApiResponse<KanbanBoardDto>> GetKanbanBoardAsync(
        Guid? assignedEmployeeId,
        DealStatus? status,
        CancellationToken cancellationToken = default)
    {
        var stages = await _context.PipelineStages
            .OrderBy(s => s.OrderIndex)
            .ToListAsync(cancellationToken);

        var query = _context.Deals
            .Include(d => d.Account)
            .Include(d => d.Contact)
            .Include(d => d.Stage)
            .Include(d => d.AssignedEmployee!)
                .ThenInclude(e => e.User)
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

                query = query.Where(d => d.AssignedEmployeeId == myEmployeeId);
            }
            else if (_currentUserService.DataScope == DataScope.Team && _currentUserService.TeamId.HasValue)
            {
                query = query.Where(d => d.AssignedEmployee != null && d.AssignedEmployee.TeamId == _currentUserService.TeamId.Value);
            }
        }

        if (assignedEmployeeId.HasValue)
        {
            query = query.Where(d => d.AssignedEmployeeId == assignedEmployeeId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        var allDeals = await query
            .OrderByDescending(d => d.Amount)
            .Select(d => new KanbanDealCardDto(
                d.Id,
                d.Code,
                d.Title,
                d.AccountId,
                d.Account.Name,
                d.StageId,
                d.ContactId,
                d.Contact != null ? d.Contact.FullName : null,
                d.Amount,
                d.ProbabilityOverride ?? d.Stage.Probability,
                d.Amount * ((d.ProbabilityOverride ?? d.Stage.Probability) / 100m),
                d.ExpectedClosingDate,
                d.Status,
                d.AssignedEmployeeId,
                d.AssignedEmployee != null ? d.AssignedEmployee.User.FullName : null,
                d.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        var columns = stages.Select(s =>
        {
            var stageDeals = allDeals.Where(d => d.StageId == s.Id).ToList();
            var totalVal = stageDeals.Sum(d => d.Amount);
            var totalExp = stageDeals.Sum(d => d.ExpectedRevenue);

            return new KanbanStageColumnDto(
                s.Id,
                s.Name,
                s.OrderIndex,
                s.Probability,
                s.ColorCode,
                s.IsWonStage,
                s.IsLostStage,
                totalVal,
                totalExp,
                stageDeals.Count,
                stageDeals
            );
        }).ToList();

        var grandTotalVal = allDeals.Sum(d => d.Amount);
        var grandTotalExp = allDeals.Sum(d => d.ExpectedRevenue);

        return ApiResponse<KanbanBoardDto>.SuccessResponse(new KanbanBoardDto(
            columns,
            grandTotalVal,
            grandTotalExp,
            allDeals.Count
        ));
    }
}
