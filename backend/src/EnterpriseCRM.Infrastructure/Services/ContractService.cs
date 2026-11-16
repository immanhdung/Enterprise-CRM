using System.Globalization;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.Contracts;
using EnterpriseCRM.Application.Features.Quotations;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class ContractService : IContractService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPdfGenerationService _pdfService;
    private readonly IDocumentStorageService _storageService;
    private readonly ILogger<ContractService> _logger;

    public ContractService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPdfGenerationService pdfService,
        IDocumentStorageService storageService,
        ILogger<ContractService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _pdfService = pdfService;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<PagedContractResult> GetContractsAsync(ContractFilterParams filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Contracts
            .AsNoTracking()
            .Include(c => c.Account)
            .Include(c => c.Contact)
            .Include(c => c.Quotation)
            .Include(c => c.Template)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(c => c.ContractNumber.ToLower().Contains(term) ||
                                     c.Title.ToLower().Contains(term) ||
                                     c.Account.Name.ToLower().Contains(term));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(c => c.Status == filter.Status.Value);
        }

        if (filter.AccountId.HasValue)
        {
            query = query.Where(c => c.AccountId == filter.AccountId.Value);
        }

        if (filter.StartDateFrom.HasValue)
        {
            var d = DateOnly.FromDateTime(filter.StartDateFrom.Value);
            query = query.Where(c => c.StartDate >= d);
        }

        if (filter.EndDateTo.HasValue)
        {
            var d = DateOnly.FromDateTime(filter.EndDateTo.Value);
            query = query.Where(c => c.EndDate <= d);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = filter.SortBy?.ToLower() switch
        {
            "totalvalue" => filter.SortDescending ? query.OrderByDescending(c => c.TotalValue) : query.OrderBy(c => c.TotalValue),
            "startdate" => filter.SortDescending ? query.OrderByDescending(c => c.StartDate) : query.OrderBy(c => c.StartDate),
            "enddate" => filter.SortDescending ? query.OrderByDescending(c => c.EndDate) : query.OrderBy(c => c.EndDate),
            "status" => filter.SortDescending ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
            _ => filter.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
        };

        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var list = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = list.Select(c => MapToDto(c, false)).ToList();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedContractResult(items, totalCount, pageNumber, pageSize, totalPages);
    }

    public async Task<ContractDto?> GetContractByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .AsNoTracking()
            .Include(c => c.Account)
            .Include(c => c.Contact)
            .Include(c => c.Quotation)
            .Include(c => c.Template)
            .Include(c => c.Items)
            .Include(c => c.Appendices)
            .Include(c => c.Histories)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (contract == null) return null;

        return MapToDto(contract, true);
    }

    public async Task<ContractDto> CreateContractAsync(CreateContractRequest request, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts.FindAsync([request.AccountId], cancellationToken);
        if (account == null) throw new KeyNotFoundException($"Account '{request.AccountId}' not found.");

        var count = await _context.Contracts.CountAsync(cancellationToken);
        var contractNumber = $"CTR-{DateTime.UtcNow:yyyyMMdd}-{(count + 1):D4}";

        var contract = new Contract
        {
            ContractNumber = contractNumber,
            Title = request.Title,
            AccountId = request.AccountId,
            ContactId = request.ContactId,
            QuotationId = request.QuotationId,
            TemplateId = request.TemplateId,
            StartDate = DateOnly.FromDateTime(request.StartDate),
            EndDate = DateOnly.FromDateTime(request.EndDate),
            TotalValue = request.TotalValue,
            ScopeOfWork = request.ScopeOfWork,
            PaymentTerms = request.PaymentTerms,
            SpecialTerms = request.SpecialTerms,
            Status = ContractStatus.Draft
        };

        if (request.Items != null && request.Items.Count > 0)
        {
            foreach (var item in request.Items)
            {
                contract.Items.Add(new ContractItem
                {
                    ServiceId = item.ServiceId ?? Guid.Empty,
                    ServiceName = item.ServiceName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.Quantity * item.UnitPrice,
                    Notes = item.Notes
                });
            }
        }

        contract.Histories.Add(new ContractHistory
        {
            Action = "Created",
            OldStatus = null,
            NewStatus = ContractStatus.Draft.ToString(),
            ActorUserId = _currentUserService.UserId,
            ActorName = _currentUserService.Email ?? "System",
            Notes = "Contract created in Draft status.",
            Timestamp = DateTime.UtcNow
        });

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync(cancellationToken);

        return (await GetContractByIdAsync(contract.Id, cancellationToken))!;
    }

    public async Task<ContractDto> GenerateContractFromQuotationAsync(GenerateContractFromQuotationRequest request, CancellationToken cancellationToken = default)
    {
        var quotation = await _context.Quotations
            .Include(q => q.Account)
            .Include(q => q.Contact)
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == request.QuotationId, cancellationToken);

        if (quotation == null) throw new KeyNotFoundException($"Quotation '{request.QuotationId}' not found.");

        var count = await _context.Contracts.CountAsync(cancellationToken);
        var contractNumber = $"CTR-{DateTime.UtcNow:yyyyMMdd}-{(count + 1):D4}";
        var title = request.Title ?? $"Service Agreement for {quotation.Account.Name}";

        ContractTemplate? template = null;
        if (request.TemplateId.HasValue)
        {
            template = await _context.ContractTemplates.FindAsync([request.TemplateId.Value], cancellationToken);
        }
        else
        {
            template = await _context.ContractTemplates.FirstOrDefaultAsync(t => t.IsDefault && t.IsActive, cancellationToken);
        }

        string scopeOfWork = string.Join("\n", quotation.Items.Where(i => !i.IsDeleted).Select(i => $"- {i.ServiceName} ({i.Quantity} {i.Unit} @ {i.UnitPrice:N0} VND) = {i.LineTotal:N0} VND"));
        string paymentTerms = request.PaymentTerms ?? "Payment shall be settled 50% upon signing and 50% upon project completion.";
        string specialTerms = request.SpecialTerms ?? quotation.TermsAndConditions ?? string.Empty;

        if (template != null)
        {
            var templateMerged = template.Content
                .Replace("{{CustomerName}}", quotation.Account.Name)
                .Replace("{{TaxId}}", quotation.Account.TaxCode ?? "N/A")
                .Replace("{{Address}}", quotation.Account.BillingAddress ?? "N/A")
                .Replace("{{ContactName}}", quotation.Contact?.FullName ?? "N/A")
                .Replace("{{ContactEmail}}", quotation.Contact?.Email ?? "N/A")
                .Replace("{{ContractNumber}}", contractNumber)
                .Replace("{{TotalValue}}", $"{quotation.TotalAmount.ToString("N0", CultureInfo.InvariantCulture)} VND")
                .Replace("{{StartDate}}", request.StartDate.ToString("yyyy-MM-dd"))
                .Replace("{{EndDate}}", request.EndDate.ToString("yyyy-MM-dd"))
                .Replace("{{PaymentTerms}}", paymentTerms);

            specialTerms = $"{specialTerms}\n\n[Agreement Content]\n{templateMerged}".Trim();
        }

        var contract = new Contract
        {
            ContractNumber = contractNumber,
            Title = title,
            AccountId = quotation.AccountId,
            ContactId = quotation.ContactId,
            QuotationId = quotation.Id,
            TemplateId = template?.Id,
            StartDate = DateOnly.FromDateTime(request.StartDate),
            EndDate = DateOnly.FromDateTime(request.EndDate),
            TotalValue = quotation.TotalAmount,
            ScopeOfWork = scopeOfWork,
            PaymentTerms = paymentTerms,
            SpecialTerms = specialTerms,
            Status = ContractStatus.Draft
        };

        foreach (var item in quotation.Items.Where(i => !i.IsDeleted))
        {
            contract.Items.Add(new ContractItem
            {
                ServiceId = item.ServiceId,
                ServiceName = item.ServiceName,
                ServiceCode = item.ServiceCode,
                Unit = item.Unit,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal,
                Notes = item.Notes
            });
        }

        contract.Histories.Add(new ContractHistory
        {
            Action = "GeneratedFromQuotation",
            OldStatus = null,
            NewStatus = ContractStatus.Draft.ToString(),
            ActorUserId = _currentUserService.UserId,
            ActorName = _currentUserService.Email ?? "System",
            Notes = $"Generated from Quotation {quotation.QuotationNumber} ({quotation.RevisionNumber}).",
            Timestamp = DateTime.UtcNow
        });

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync(cancellationToken);

        // Pre-generate PDF
        try
        {
            var pdfBytes = await _pdfService.GenerateContractPdfAsync(contract, cancellationToken);
            var filePath = $"contracts/{contract.ContractNumber}.pdf";
            using var ms = new MemoryStream(pdfBytes);
            var url = await _storageService.UploadDocumentAsync(filePath, ms, "application/pdf", cancellationToken);
            contract.DocumentUrl = url;
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to auto-generate PDF for contract {Id}", contract.Id);
        }

        return (await GetContractByIdAsync(contract.Id, cancellationToken))!;
    }

    public async Task<ContractDto> UpdateContractAsync(Guid id, UpdateContractRequest request, CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .Include(c => c.Items)
            .Include(c => c.Histories)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (contract == null) throw new KeyNotFoundException($"Contract '{id}' not found.");

        var oldStatus = contract.Status;

        contract.Title = request.Title;
        contract.ContactId = request.ContactId;
        contract.Status = request.Status;
        contract.StartDate = DateOnly.FromDateTime(request.StartDate);
        contract.EndDate = DateOnly.FromDateTime(request.EndDate);
        contract.TotalValue = request.TotalValue;
        contract.ScopeOfWork = request.ScopeOfWork;
        contract.PaymentTerms = request.PaymentTerms;
        contract.SpecialTerms = request.SpecialTerms;

        if (oldStatus != request.Status)
        {
            var history = new ContractHistory
            {
                ContractId = contract.Id,
                Action = "StatusChanged",
                OldStatus = oldStatus.ToString(),
                NewStatus = request.Status.ToString(),
                ActorUserId = _currentUserService.UserId,
                ActorName = _currentUserService.Email ?? "System",
                Notes = $"Status transitioned from {oldStatus} to {request.Status}.",
                Timestamp = DateTime.UtcNow
            };
            _context.ContractHistories.Add(history);
        }

        if (request.Items != null)
        {
            _context.ContractItems.RemoveRange(contract.Items);
            contract.Items.Clear();

            foreach (var item in request.Items)
            {
                var contractItem = new ContractItem
                {
                    ContractId = contract.Id,
                    ServiceId = item.ServiceId ?? Guid.Empty,
                    ServiceName = item.ServiceName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.Quantity * item.UnitPrice,
                    Notes = item.Notes
                };
                _context.ContractItems.Add(contractItem);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return (await GetContractByIdAsync(id, cancellationToken))!;
    }

    public async Task<bool> DeleteContractAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts.FindAsync([id], cancellationToken);
        if (contract == null) return false;

        contract.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<string> RenderContractContentAsync(Guid contractId, CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .AsNoTracking()
            .Include(c => c.Account)
            .Include(c => c.Contact)
            .Include(c => c.Template)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        if (contract == null) throw new KeyNotFoundException($"Contract '{contractId}' not found.");

        if (contract.Template == null)
        {
            return contract.ScopeOfWork ?? "No content template associated with this contract.";
        }

        return contract.Template.Content
            .Replace("{{CustomerName}}", contract.Account.Name)
            .Replace("{{TaxId}}", contract.Account.TaxCode ?? "N/A")
            .Replace("{{Address}}", contract.Account.BillingAddress ?? "N/A")
            .Replace("{{ContactName}}", contract.Contact?.FullName ?? "N/A")
            .Replace("{{ContactEmail}}", contract.Contact?.Email ?? "N/A")
            .Replace("{{ContractNumber}}", contract.ContractNumber)
            .Replace("{{TotalValue}}", $"{contract.TotalValue.ToString("N0", CultureInfo.InvariantCulture)} VND")
            .Replace("{{StartDate}}", contract.StartDate.ToString("yyyy-MM-dd"))
            .Replace("{{EndDate}}", contract.EndDate.ToString("yyyy-MM-dd"))
            .Replace("{{PaymentTerms}}", contract.PaymentTerms ?? "Standard terms");
    }

    public async Task<byte[]> GenerateContractPdfAsync(Guid contractId, CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .AsNoTracking()
            .Include(c => c.Account)
            .Include(c => c.Contact)
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        if (contract == null) throw new KeyNotFoundException($"Contract '{contractId}' not found.");

        return await _pdfService.GenerateContractPdfAsync(contract, cancellationToken);
    }

    public async Task<IReadOnlyList<ContractAppendixDto>> GetAppendicesAsync(Guid contractId, CancellationToken cancellationToken = default)
    {
        var list = await _context.ContractAppendices
            .AsNoTracking()
            .Where(a => a.ContractId == contractId)
            .OrderBy(a => a.AppendixNumber)
            .ToListAsync(cancellationToken);

        return list.Select(a => new ContractAppendixDto(
            a.Id,
            a.ContractId,
            a.AppendixNumber,
            a.Title,
            a.Description,
            a.AdjustedValue,
            a.EffectiveDate.ToDateTime(TimeOnly.MinValue),
            a.Status,
            a.DocumentUrl,
            a.CreatedAt
        )).ToList();
    }

    public async Task<ContractAppendixDto> CreateAppendixAsync(Guid contractId, CreateContractAppendixRequest request, CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .Include(c => c.Appendices)
            .Include(c => c.Histories)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        if (contract == null) throw new KeyNotFoundException($"Contract '{contractId}' not found.");

        var appendixIndex = contract.Appendices.Count + 1;
        var appendixNumber = $"APP-{appendixIndex:D2}";

        var appendix = new ContractAppendix
        {
            ContractId = contractId,
            AppendixNumber = appendixNumber,
            Title = request.Title,
            Description = request.Description,
            AdjustedValue = request.AdjustedValue,
            EffectiveDate = DateOnly.FromDateTime(request.EffectiveDate),
            Status = request.Status
        };

        if (request.AdjustedValue != 0)
        {
            contract.TotalValue += request.AdjustedValue;
        }

        var history = new ContractHistory
        {
            ContractId = contractId,
            Action = "AppendixAdded",
            OldStatus = contract.Status.ToString(),
            NewStatus = contract.Status.ToString(),
            ActorUserId = _currentUserService.UserId,
            ActorName = _currentUserService.Email ?? "System",
            Notes = $"Appendix {appendixNumber} added: '{request.Title}'. Value adjustment: {request.AdjustedValue:N0} VND.",
            Timestamp = DateTime.UtcNow
        };
        _context.ContractHistories.Add(history);

        _context.ContractAppendices.Add(appendix);
        await _context.SaveChangesAsync(cancellationToken);

        return new ContractAppendixDto(appendix.Id, appendix.ContractId, appendix.AppendixNumber, appendix.Title, appendix.Description, appendix.AdjustedValue, appendix.EffectiveDate.ToDateTime(TimeOnly.MinValue), appendix.Status, appendix.DocumentUrl, appendix.CreatedAt);
    }

    public async Task<ContractDto> RenewContractAsync(Guid contractId, RenewContractRequest request, CancellationToken cancellationToken = default)
    {
        var original = await _context.Contracts
            .Include(c => c.Items)
            .Include(c => c.Histories)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        if (original == null) throw new KeyNotFoundException($"Contract '{contractId}' not found.");

        original.Status = ContractStatus.Renewed;

        var history = new ContractHistory
        {
            ContractId = original.Id,
            Action = "Renewed",
            OldStatus = ContractStatus.Active.ToString(),
            NewStatus = ContractStatus.Renewed.ToString(),
            ActorUserId = _currentUserService.UserId,
            ActorName = _currentUserService.Email ?? "System",
            Notes = $"Contract marked as Renewed. {request.RenewalNotes}",
            Timestamp = DateTime.UtcNow
        };
        _context.ContractHistories.Add(history);

        var count = await _context.Contracts.CountAsync(cancellationToken);
        var newContractNumber = $"CTR-{DateTime.UtcNow:yyyyMMdd}-{(count + 1):D4}";
        var newTotalValue = request.NewTotalValue ?? original.TotalValue;

        var renewalContract = new Contract
        {
            ContractNumber = newContractNumber,
            Title = $"{original.Title} (Renewal #{original.RenewalCount + 1})",
            AccountId = original.AccountId,
            ContactId = original.ContactId,
            QuotationId = original.QuotationId,
            TemplateId = original.TemplateId,
            ParentContractId = original.Id,
            RenewalCount = original.RenewalCount + 1,
            StartDate = DateOnly.FromDateTime(request.NewStartDate),
            EndDate = DateOnly.FromDateTime(request.NewEndDate),
            TotalValue = newTotalValue,
            ScopeOfWork = original.ScopeOfWork,
            PaymentTerms = original.PaymentTerms,
            SpecialTerms = original.SpecialTerms,
            Status = ContractStatus.Draft
        };

        foreach (var item in original.Items.Where(i => !i.IsDeleted))
        {
            renewalContract.Items.Add(new ContractItem
            {
                ServiceId = item.ServiceId,
                ServiceName = item.ServiceName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal,
                Notes = item.Notes
            });
        }

        renewalContract.Histories.Add(new ContractHistory
        {
            Action = "CreatedAsRenewal",
            OldStatus = null,
            NewStatus = ContractStatus.Draft.ToString(),
            ActorUserId = _currentUserService.UserId,
            ActorName = _currentUserService.Email ?? "System",
            Notes = $"Renewal contract created from parent {original.ContractNumber}.",
            Timestamp = DateTime.UtcNow
        });

        _context.Contracts.Add(renewalContract);
        await _context.SaveChangesAsync(cancellationToken);

        return (await GetContractByIdAsync(renewalContract.Id, cancellationToken))!;
    }

    private static ContractDto MapToDto(Contract c, bool includeDetails)
    {
        IReadOnlyList<ContractItemDto>? items = null;
        IReadOnlyList<ContractAppendixDto>? appendices = null;
        IReadOnlyList<ContractHistoryDto>? histories = null;

        if (includeDetails)
        {
            items = c.Items
                .Where(i => !i.IsDeleted)
                .Select(i => new ContractItemDto(i.Id, i.ContractId, i.ServiceId, i.ServiceName, (int)i.Quantity, i.UnitPrice, i.LineTotal, i.Notes))
                .ToList();

            appendices = c.Appendices
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.AppendixNumber)
                .Select(a => new ContractAppendixDto(a.Id, a.ContractId, a.AppendixNumber, a.Title, a.Description, a.AdjustedValue, a.EffectiveDate.ToDateTime(TimeOnly.MinValue), a.Status, a.DocumentUrl, a.CreatedAt))
                .ToList();

            histories = c.Histories
                .OrderByDescending(h => h.Timestamp)
                .Select(h => new ContractHistoryDto(h.Id, h.ContractId, h.Action, h.OldStatus, h.NewStatus, h.ActorUserId ?? Guid.Empty, h.ActorName, h.Notes, h.Timestamp))
                .ToList();
        }

        return new ContractDto(
            c.Id,
            c.ContractNumber,
            c.Title,
            c.AccountId,
            c.Account?.Name ?? "Unknown",
            c.ContactId,
            c.Contact?.FullName,
            c.Contact?.Email,
            c.QuotationId,
            c.Quotation?.QuotationNumber,
            c.TemplateId,
            c.Template?.Name,
            c.ParentContractId,
            null,
            c.RenewalCount,
            c.TotalValue,
            c.Status,
            c.StartDate.ToDateTime(TimeOnly.MinValue),
            c.EndDate.ToDateTime(TimeOnly.MinValue),
            c.ScopeOfWork,
            c.PaymentTerms,
            c.SpecialTerms,
            c.DocumentUrl,
            c.SignedAt,
            c.SignedByClientName,
            c.SignedByClientEmail,
            c.SignerIpAddress,
            c.CreatedAt,
            c.UpdatedAt,
            items,
            appendices,
            histories
        );
    }
}
