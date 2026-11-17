using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.Finance;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class ReceivableService : IReceivableService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ReceivableService> _logger;

    public ReceivableService(IApplicationDbContext context, ILogger<ReceivableService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ReceivableAgingSummaryDto> GetAgingSummaryAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var schedules = await _context.PaymentSchedules
            .AsNoTracking()
            .Include(s => s.Contract)
            .Where(s => s.Status != "Paid" && s.AmountDue > s.AmountPaid)
            .ToListAsync(cancellationToken);

        decimal currentAmount = 0m;
        decimal overdue1To7 = 0m;
        decimal overdue8To30 = 0m;
        decimal overdue31To60 = 0m;
        decimal overdueMoreThan60 = 0m;

        var debtors = new HashSet<Guid>();
        var overdueContracts = new HashSet<Guid>();

        foreach (var s in schedules)
        {
            var remaining = s.AmountDue - s.AmountPaid;
            if (remaining <= 0) continue;

            debtors.Add(s.Contract.AccountId);

            if (s.DueDate >= today)
            {
                currentAmount += remaining;
            }
            else
            {
                var days = today.DayNumber - s.DueDate.DayNumber;
                overdueContracts.Add(s.ContractId);

                if (days <= 7)
                    overdue1To7 += remaining;
                else if (days <= 30)
                    overdue8To30 += remaining;
                else if (days <= 60)
                    overdue31To60 += remaining;
                else
                    overdueMoreThan60 += remaining;
            }
        }

        var totalOverdue = overdue1To7 + overdue8To30 + overdue31To60 + overdueMoreThan60;
        var totalReceivables = currentAmount + totalOverdue;

        return new ReceivableAgingSummaryDto(
            TotalReceivables: totalReceivables,
            TotalOverdue: totalOverdue,
            CurrentAmount: currentAmount,
            Overdue1To7Days: overdue1To7,
            Overdue8To30Days: overdue8To30,
            Overdue31To60Days: overdue31To60,
            OverdueMoreThan60Days: overdueMoreThan60,
            AccountsWithDebtCount: debtors.Count,
            OverdueContractsCount: overdueContracts.Count
        );
    }

    public async Task<IReadOnlyList<AccountsReceivableItemDto>> GetReceivablesListAsync(ReceivableFilterParams filter, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = _context.PaymentSchedules
            .AsNoTracking()
            .Include(s => s.Contract)
                .ThenInclude(c => c.Account)
                    .ThenInclude(a => a.Contacts)
            .Include(s => s.Contract)
                .ThenInclude(c => c.Contact)
            .Where(s => s.Status != "Paid" && s.AmountDue > s.AmountPaid)
            .AsQueryable();

        if (filter.AccountId.HasValue)
        {
            query = query.Where(s => s.Contract.AccountId == filter.AccountId.Value);
        }

        if (filter.OnlyOverdue)
        {
            query = query.Where(s => s.DueDate < today);
        }

        var schedules = await query.ToListAsync(cancellationToken);

        // Fetch latest collection logs for these accounts
        var accountIds = schedules.Select(s => s.Contract.AccountId).Distinct().ToList();
        var collectionLogs = await _context.ReceivableCollectionLogs
            .AsNoTracking()
            .Where(l => accountIds.Contains(l.AccountId))
            .OrderByDescending(l => l.LoggedAt)
            .ToListAsync(cancellationToken);

        var employees = await _context.Employees
            .AsNoTracking()
            .Include(e => e.User)
            .ToDictionaryAsync(e => e.UserId, e => e.User.FullName, cancellationToken);

        var result = new List<AccountsReceivableItemDto>();

        foreach (var s in schedules)
        {
            var remaining = s.AmountDue - s.AmountPaid;
            var overdueDays = s.DueDate < today ? today.DayNumber - s.DueDate.DayNumber : 0;

            string bracket = overdueDays switch
            {
                0 => "Current",
                <= 7 => "1-7 Days",
                <= 30 => "8-30 Days",
                <= 60 => "31-60 Days",
                _ => ">60 Days"
            };

            if (!string.IsNullOrWhiteSpace(filter.AgingBracket) &&
                !string.Equals(filter.AgingBracket, bracket, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var contact = s.Contract.Contact ?? s.Contract.Account.Contacts.FirstOrDefault();
            var latestLog = collectionLogs.FirstOrDefault(l => l.AccountId == s.Contract.AccountId);

            string? salesName = null;
            Guid? salesId = null;
            if (s.Contract.CreatedBy.HasValue)
            {
                salesId = s.Contract.CreatedBy.Value;
                employees.TryGetValue(salesId.Value, out salesName);
            }

            var item = new AccountsReceivableItemDto(
                AccountId: s.Contract.AccountId,
                AccountName: s.Contract.Account.Name,
                ContactPerson: contact?.FullName,
                ContactPhone: contact?.Phone ?? s.Contract.Account.Phone,
                ContactEmail: contact?.Email ?? s.Contract.Account.Email,
                ContractId: s.ContractId,
                ContractNumber: s.Contract.ContractNumber,
                PaymentScheduleId: s.Id,
                MilestoneTitle: s.MilestoneTitle,
                DueDate: s.DueDate,
                TotalAmount: s.AmountDue,
                PaidAmount: s.AmountPaid,
                OutstandingAmount: remaining,
                OverdueDays: overdueDays,
                AgingBracket: bracket,
                LastCollectionActivityDate: latestLog?.LoggedAt,
                LastCollectionStatus: latestLog?.Status,
                AssignedSalesEmployeeId: salesId,
                AssignedSalesEmployeeName: salesName
            );

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                if (!item.AccountName.ToLower().Contains(term) &&
                    !item.ContractNumber.ToLower().Contains(term) &&
                    !(item.ContactPerson?.ToLower().Contains(term) ?? false))
                {
                    continue;
                }
            }

            result.Add(item);
        }

        return result
            .OrderByDescending(r => r.OverdueDays)
            .ThenByDescending(r => r.OutstandingAmount)
            .ToList();
    }

    public async Task<IReadOnlyList<ReceivableCollectionLogDto>> GetCollectionLogsAsync(Guid? accountId, Guid? contractId, CancellationToken cancellationToken = default)
    {
        var query = _context.ReceivableCollectionLogs
            .AsNoTracking()
            .Include(l => l.Account)
            .Include(l => l.Contract)
            .AsQueryable();

        if (accountId.HasValue)
        {
            query = query.Where(l => l.AccountId == accountId.Value);
        }

        if (contractId.HasValue)
        {
            query = query.Where(l => l.ContractId == contractId.Value);
        }

        var logs = await query
            .OrderByDescending(l => l.LoggedAt)
            .ToListAsync(cancellationToken);

        return logs.Select(l => new ReceivableCollectionLogDto(
            Id: l.Id,
            AccountId: l.AccountId,
            AccountName: l.Account?.Name,
            ContractId: l.ContractId,
            ContractNumber: l.Contract?.ContractNumber,
            PaymentScheduleId: l.PaymentScheduleId,
            ActivityType: l.ActivityType,
            Notes: l.Notes,
            LoggedById: l.LoggedById,
            LoggedByName: l.LoggedByName,
            LoggedAt: l.LoggedAt,
            NextFollowUpDate: l.NextFollowUpDate,
            AssignedToUserId: l.AssignedToUserId,
            AssignedToUserName: l.AssignedToUserName,
            Status: l.Status,
            CreatedAt: l.CreatedAt
        )).ToList();
    }

    public async Task<ReceivableCollectionLogDto> AddCollectionLogAsync(
        CreateReceivableCollectionLogRequest request,
        Guid currentUserId,
        string currentUserName,
        CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts.FindAsync([request.AccountId], cancellationToken);
        if (account == null)
            throw new InvalidOperationException($"Account with ID {request.AccountId} was not found.");

        var contract = await _context.Contracts.FindAsync([request.ContractId], cancellationToken);
        if (contract == null)
            throw new InvalidOperationException($"Contract with ID {request.ContractId} was not found.");

        var log = new ReceivableCollectionLog
        {
            AccountId = request.AccountId,
            ContractId = request.ContractId,
            PaymentScheduleId = request.PaymentScheduleId,
            ActivityType = request.ActivityType,
            Notes = request.Notes,
            LoggedById = currentUserId,
            LoggedByName = currentUserName,
            LoggedAt = DateTime.UtcNow,
            NextFollowUpDate = request.NextFollowUpDate,
            AssignedToUserId = currentUserId,
            AssignedToUserName = currentUserName,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Open" : request.Status
        };

        _context.ReceivableCollectionLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);

        log.Account = account;
        log.Contract = contract;

        _logger.LogInformation("Receivable collection log added for Account {AccountName} by {UserName}",
            account.Name, currentUserName);

        return new ReceivableCollectionLogDto(
            Id: log.Id,
            AccountId: log.AccountId,
            AccountName: account.Name,
            ContractId: log.ContractId,
            ContractNumber: contract.ContractNumber,
            PaymentScheduleId: log.PaymentScheduleId,
            ActivityType: log.ActivityType,
            Notes: log.Notes,
            LoggedById: log.LoggedById,
            LoggedByName: log.LoggedByName,
            LoggedAt: log.LoggedAt,
            NextFollowUpDate: log.NextFollowUpDate,
            AssignedToUserId: log.AssignedToUserId,
            AssignedToUserName: log.AssignedToUserName,
            Status: log.Status,
            CreatedAt: log.CreatedAt
        );
    }
}
