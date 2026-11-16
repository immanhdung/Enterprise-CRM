using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.Quotations;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class QuotationService : IQuotationService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPdfGenerationService _pdfService;
    private readonly IDocumentStorageService _storageService;
    private readonly IEmailService _emailService;
    private readonly ILogger<QuotationService> _logger;

    public QuotationService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPdfGenerationService pdfService,
        IDocumentStorageService storageService,
        IEmailService emailService,
        ILogger<QuotationService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _pdfService = pdfService;
        _storageService = storageService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<PagedQuotationResult> GetQuotationsAsync(QuotationFilterParams filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Quotations
            .AsNoTracking()
            .Include(q => q.Account)
            .Include(q => q.Contact)
            .Include(q => q.Deal)
            .Include(q => q.PriceList)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(q => q.QuotationNumber.ToLower().Contains(term) || q.Account.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(q => q.Status == filter.Status);
        }

        if (filter.AccountId.HasValue)
        {
            query = query.Where(q => q.AccountId == filter.AccountId.Value);
        }

        if (filter.DealId.HasValue)
        {
            query = query.Where(q => q.DealId == filter.DealId.Value);
        }

        if (filter.FromDate.HasValue)
        {
            var fromDateOnly = DateOnly.FromDateTime(filter.FromDate.Value);
            query = query.Where(q => q.ValidUntil >= fromDateOnly);
        }

        if (filter.ToDate.HasValue)
        {
            var toDateOnly = DateOnly.FromDateTime(filter.ToDate.Value);
            query = query.Where(q => q.ValidUntil <= toDateOnly);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = filter.SortBy?.ToLower() switch
        {
            "totalamount" => filter.SortDescending ? query.OrderByDescending(q => q.TotalAmount) : query.OrderBy(q => q.TotalAmount),
            "status" => filter.SortDescending ? query.OrderByDescending(q => q.Status) : query.OrderBy(q => q.Status),
            "validuntil" => filter.SortDescending ? query.OrderByDescending(q => q.ValidUntil) : query.OrderBy(q => q.ValidUntil),
            _ => filter.SortDescending ? query.OrderByDescending(q => q.CreatedAt) : query.OrderBy(q => q.CreatedAt)
        };

        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var list = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = list.Select(q => MapToDto(q, false)).ToList();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedQuotationResult(items, totalCount, pageNumber, pageSize, totalPages);
    }

    public async Task<QuotationDto?> GetQuotationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var quotation = await _context.Quotations
            .AsNoTracking()
            .Include(q => q.Account)
            .Include(q => q.Contact)
            .Include(q => q.Deal)
            .Include(q => q.PriceList)
            .Include(q => q.Items)
            .Include(q => q.ApprovalHistories)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (quotation == null) return null;

        return MapToDto(quotation, true);
    }

    public async Task<QuotationDto> CreateQuotationAsync(CreateQuotationRequest request, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts.FindAsync([request.AccountId], cancellationToken);
        if (account == null)
        {
            throw new KeyNotFoundException($"Account with id '{request.AccountId}' not found.");
        }

        var quotationCount = await _context.Quotations.CountAsync(cancellationToken);
        var quotationNumber = $"QT-{DateTime.UtcNow:yyyyMMdd}-{(quotationCount + 1):D4}";

        var quotation = new Quotation
        {
            QuotationNumber = quotationNumber,
            Version = 1,
            RevisionNumber = "REV-01",
            DealId = request.DealId,
            AccountId = request.AccountId,
            ContactId = request.ContactId,
            PriceListId = request.PriceListId,
            ValidUntil = DateOnly.FromDateTime(request.ValidUntil),
            TermsAndConditions = request.TermsAndConditions,
            Notes = request.Notes,
            Status = "Draft"
        };

        decimal subtotal = 0;
        decimal totalDiscount = 0;
        decimal totalVat = 0;

        foreach (var itemInput in request.Items)
        {
            var lineSubtotal = itemInput.Quantity * itemInput.UnitPrice;
            var discount = lineSubtotal * (itemInput.DiscountRate / 100m);
            var net = lineSubtotal - discount;
            var vat = net * (itemInput.VatRate / 100m);
            var lineTotal = net + vat;

            subtotal += lineSubtotal;
            totalDiscount += discount;
            totalVat += vat;

            var targetServiceId = itemInput.ServiceId ?? Guid.Empty;
            if (targetServiceId == Guid.Empty)
            {
                var matched = await _context.Services.FirstOrDefaultAsync(s => s.Code == itemInput.ServiceCode, cancellationToken);
                if (matched != null)
                {
                    targetServiceId = matched.Id;
                }
            }

            quotation.Items.Add(new QuotationItem
            {
                ServiceId = targetServiceId,
                ServiceCode = itemInput.ServiceCode,
                ServiceName = itemInput.ServiceName,
                Unit = itemInput.Unit,
                Quantity = itemInput.Quantity,
                UnitPrice = itemInput.UnitPrice,
                DiscountRate = itemInput.DiscountRate,
                DiscountAmount = discount,
                VatRate = itemInput.VatRate,
                VatAmount = vat,
                LineTotal = lineTotal,
                Notes = itemInput.Notes
            });
        }

        quotation.Subtotal = subtotal;
        quotation.DiscountAmount = totalDiscount;
        quotation.VatAmount = totalVat;
        quotation.TotalAmount = subtotal - totalDiscount + totalVat;

        quotation.ApprovalHistories.Add(new QuotationApprovalHistory
        {
            Action = "Created",
            ActorUserId = _currentUserService.UserId,
            ActorName = _currentUserService.Email ?? "System",
            Comments = "Quotation created in Draft status.",
            Timestamp = DateTime.UtcNow
        });

        _context.Quotations.Add(quotation);
        await _context.SaveChangesAsync(cancellationToken);

        return (await GetQuotationByIdAsync(quotation.Id, cancellationToken))!;
    }

    public async Task<QuotationDto> UpdateQuotationAsync(Guid id, UpdateQuotationRequest request, CancellationToken cancellationToken = default)
    {
        var quotation = await _context.Quotations
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (quotation == null)
        {
            throw new KeyNotFoundException($"Quotation with id '{id}' not found.");
        }

        if (quotation.Status == "Approved" || quotation.Status == "Accepted")
        {
            throw new InvalidOperationException($"Cannot modify a quotation in '{quotation.Status}' status. Please create a revision instead.");
        }

        quotation.ContactId = request.ContactId;
        quotation.PriceListId = request.PriceListId;
        quotation.ValidUntil = DateOnly.FromDateTime(request.ValidUntil);
        quotation.TermsAndConditions = request.TermsAndConditions;
        quotation.Notes = request.Notes;

        _context.QuotationItems.RemoveRange(quotation.Items);
        quotation.Items.Clear();

        decimal subtotal = 0;
        decimal totalDiscount = 0;
        decimal totalVat = 0;

        foreach (var itemInput in request.Items)
        {
            var lineSubtotal = itemInput.Quantity * itemInput.UnitPrice;
            var discount = lineSubtotal * (itemInput.DiscountRate / 100m);
            var net = lineSubtotal - discount;
            var vat = net * (itemInput.VatRate / 100m);
            var lineTotal = net + vat;

            subtotal += lineSubtotal;
            totalDiscount += discount;
            totalVat += vat;

            var targetServiceId = itemInput.ServiceId ?? Guid.Empty;
            if (targetServiceId == Guid.Empty)
            {
                var matched = await _context.Services.FirstOrDefaultAsync(s => s.Code == itemInput.ServiceCode, cancellationToken);
                if (matched != null)
                {
                    targetServiceId = matched.Id;
                }
            }

            quotation.Items.Add(new QuotationItem
            {
                QuotationId = quotation.Id,
                ServiceId = targetServiceId,
                ServiceCode = itemInput.ServiceCode,
                ServiceName = itemInput.ServiceName,
                Unit = itemInput.Unit,
                Quantity = itemInput.Quantity,
                UnitPrice = itemInput.UnitPrice,
                DiscountRate = itemInput.DiscountRate,
                DiscountAmount = discount,
                VatRate = itemInput.VatRate,
                VatAmount = vat,
                LineTotal = lineTotal,
                Notes = itemInput.Notes
            });
        }

        quotation.Subtotal = subtotal;
        quotation.DiscountAmount = totalDiscount;
        quotation.VatAmount = totalVat;
        quotation.TotalAmount = subtotal - totalDiscount + totalVat;

        await _context.SaveChangesAsync(cancellationToken);

        return (await GetQuotationByIdAsync(quotation.Id, cancellationToken))!;
    }

    public async Task<bool> DeleteQuotationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var quotation = await _context.Quotations.FindAsync([id], cancellationToken);
        if (quotation == null) return false;

        quotation.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<QuotationDto> SubmitForApprovalAsync(Guid id, SubmitQuotationApprovalRequest request, CancellationToken cancellationToken = default)
    {
        var quotation = await _context.Quotations
            .Include(q => q.ApprovalHistories)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (quotation == null) throw new KeyNotFoundException($"Quotation '{id}' not found.");

        quotation.Status = "Submitted";
        quotation.SubmittedAt = DateTime.UtcNow;
        quotation.SubmittedById = _currentUserService.UserId;

        var history = new QuotationApprovalHistory
        {
            QuotationId = quotation.Id,
            Action = "Submitted",
            ActorUserId = _currentUserService.UserId,
            ActorName = _currentUserService.Email ?? "System",
            Comments = request.Notes ?? "Submitted for manager approval.",
            Timestamp = DateTime.UtcNow
        };
        _context.QuotationApprovalHistories.Add(history);

        await _context.SaveChangesAsync(cancellationToken);
        return (await GetQuotationByIdAsync(id, cancellationToken))!;
    }

    public async Task<QuotationDto> ApproveQuotationAsync(Guid id, ApproveQuotationRequest request, CancellationToken cancellationToken = default)
    {
        var quotation = await _context.Quotations
            .Include(q => q.Account)
            .Include(q => q.Contact)
            .Include(q => q.Items)
            .Include(q => q.ApprovalHistories)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (quotation == null) throw new KeyNotFoundException($"Quotation '{id}' not found.");

        quotation.Status = "Approved";
        quotation.ApprovedAt = DateTime.UtcNow;
        quotation.ApprovedBy = _currentUserService.UserId;
        quotation.ApprovalNotes = request.ApprovalNotes;

        var history = new QuotationApprovalHistory
        {
            QuotationId = quotation.Id,
            Action = "Approved",
            ActorUserId = _currentUserService.UserId,
            ActorName = _currentUserService.Email ?? "Manager",
            Comments = request.ApprovalNotes ?? "Quotation approved.",
            Timestamp = DateTime.UtcNow
        };
        _context.QuotationApprovalHistories.Add(history);

        try
        {
            var pdfBytes = await _pdfService.GenerateQuotationPdfAsync(quotation, cancellationToken);
            var fileName = $"quotations/{quotation.QuotationNumber}_{quotation.RevisionNumber}.pdf";
            using var ms = new MemoryStream(pdfBytes);
            var url = await _storageService.UploadDocumentAsync(fileName, ms, "application/pdf", cancellationToken);
            quotation.PdfUrl = url;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to auto-generate PDF during approval for quotation {Id}", id);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return (await GetQuotationByIdAsync(id, cancellationToken))!;
    }

    public async Task<QuotationDto> RejectQuotationAsync(Guid id, RejectQuotationRequest request, CancellationToken cancellationToken = default)
    {
        var quotation = await _context.Quotations
            .Include(q => q.ApprovalHistories)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (quotation == null) throw new KeyNotFoundException($"Quotation '{id}' not found.");

        quotation.Status = "Rejected";
        quotation.RejectedAt = DateTime.UtcNow;
        quotation.RejectedBy = _currentUserService.UserId;
        quotation.RejectionReason = request.Reason;

        var history = new QuotationApprovalHistory
        {
            QuotationId = quotation.Id,
            Action = "Rejected",
            ActorUserId = _currentUserService.UserId,
            ActorName = _currentUserService.Email ?? "Manager",
            Comments = $"Rejected: {request.Reason}",
            Timestamp = DateTime.UtcNow
        };
        _context.QuotationApprovalHistories.Add(history);

        await _context.SaveChangesAsync(cancellationToken);
        return (await GetQuotationByIdAsync(id, cancellationToken))!;
    }

    public async Task<QuotationDto> CreateRevisionAsync(Guid id, CreateRevisionRequest request, CancellationToken cancellationToken = default)
    {
        var parent = await _context.Quotations
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (parent == null) throw new KeyNotFoundException($"Quotation '{id}' not found.");

        parent.Status = "Revised";

        var newVersion = parent.Version + 1;
        var newRevision = new Quotation
        {
            QuotationNumber = parent.QuotationNumber,
            Version = newVersion,
            RevisionNumber = $"REV-{newVersion:D2}",
            ParentQuotationId = parent.Id,
            DealId = parent.DealId,
            AccountId = parent.AccountId,
            ContactId = parent.ContactId,
            PriceListId = parent.PriceListId,
            ValidUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            Subtotal = parent.Subtotal,
            DiscountAmount = parent.DiscountAmount,
            VatAmount = parent.VatAmount,
            TotalAmount = parent.TotalAmount,
            Status = "Draft",
            TermsAndConditions = parent.TermsAndConditions,
            Notes = request.ChangeNotes ?? $"Revision created from {parent.RevisionNumber}"
        };

        foreach (var item in parent.Items.Where(i => !i.IsDeleted))
        {
            newRevision.Items.Add(new QuotationItem
            {
                ServiceId = item.ServiceId,
                ServiceCode = item.ServiceCode,
                ServiceName = item.ServiceName,
                Unit = item.Unit,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountRate = item.DiscountRate,
                DiscountAmount = item.DiscountAmount,
                VatRate = item.VatRate,
                VatAmount = item.VatAmount,
                LineTotal = item.LineTotal,
                Notes = item.Notes
            });
        }

        newRevision.ApprovalHistories.Add(new QuotationApprovalHistory
        {
            Action = "Revision Created",
            ActorUserId = _currentUserService.UserId,
            ActorName = _currentUserService.Email ?? "System",
            Comments = $"Revised from {parent.RevisionNumber}. {request.ChangeNotes}",
            Timestamp = DateTime.UtcNow
        });

        _context.Quotations.Add(newRevision);
        await _context.SaveChangesAsync(cancellationToken);

        return (await GetQuotationByIdAsync(newRevision.Id, cancellationToken))!;
    }

    public async Task<IReadOnlyList<QuotationDto>> GetQuotationVersionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var current = await _context.Quotations.AsNoTracking().FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
        if (current == null) return Array.Empty<QuotationDto>();

        var list = await _context.Quotations
            .AsNoTracking()
            .Include(q => q.Account)
            .Include(q => q.Contact)
            .Where(q => q.QuotationNumber == current.QuotationNumber)
            .OrderByDescending(q => q.Version)
            .ToListAsync(cancellationToken);

        return list.Select(q => MapToDto(q, false)).ToList();
    }

    public async Task<byte[]> GenerateQuotationPdfAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var quotation = await _context.Quotations
            .AsNoTracking()
            .Include(q => q.Account)
            .Include(q => q.Contact)
            .Include(q => q.PriceList)
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (quotation == null) throw new KeyNotFoundException($"Quotation '{id}' not found.");

        return await _pdfService.GenerateQuotationPdfAsync(quotation, cancellationToken);
    }

    public async Task<bool> SendQuotationEmailAsync(Guid id, SendQuotationEmailRequest request, CancellationToken cancellationToken = default)
    {
        var quotation = await _context.Quotations
            .Include(q => q.Account)
            .Include(q => q.Contact)
            .Include(q => q.PriceList)
            .Include(q => q.Items)
            .Include(q => q.ApprovalHistories)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (quotation == null) throw new KeyNotFoundException($"Quotation '{id}' not found.");

        var pdfBytes = await _pdfService.GenerateQuotationPdfAsync(quotation, cancellationToken);
        var subject = request.Subject ?? $"Quotation {quotation.QuotationNumber} ({quotation.RevisionNumber}) from Enterprise CRM";
        var body = $"<p>Dear {quotation.Contact?.FullName ?? quotation.Account.Name},</p>" +
                   $"<p>Please find attached our official service quotation <b>{quotation.QuotationNumber}</b> ({quotation.RevisionNumber}) for your review.</p>" +
                   $"<p>{request.CustomMessage}</p>" +
                   $"<p>Total Amount: <b>{quotation.TotalAmount:N0} VND</b><br/>Valid Until: <b>{quotation.ValidUntil:yyyy-MM-dd}</b></p>" +
                   $"<p>Best regards,<br/>Enterprise CRM Team</p>";

        var sent = await _emailService.SendEmailAsync(
            request.RecipientEmail,
            subject,
            body,
            pdfBytes,
            $"{quotation.QuotationNumber}_{quotation.RevisionNumber}.pdf",
            cancellationToken);

        if (sent)
        {
            quotation.Status = "Sent";
            quotation.SentAt = DateTime.UtcNow;
            quotation.SentToEmail = request.RecipientEmail;
            quotation.SentById = _currentUserService.UserId;

            var history = new QuotationApprovalHistory
            {
                QuotationId = quotation.Id,
                Action = "Email Sent",
                ActorUserId = _currentUserService.UserId,
                ActorName = _currentUserService.Email ?? "System",
                Comments = $"Quotation sent to {request.RecipientEmail}.",
                Timestamp = DateTime.UtcNow
            };
            _context.QuotationApprovalHistories.Add(history);

            await _context.SaveChangesAsync(cancellationToken);
        }

        return sent;
    }

    private static QuotationDto MapToDto(Quotation q, bool includeDetails)
    {
        IReadOnlyList<QuotationItemDto>? items = null;
        IReadOnlyList<QuotationApprovalHistoryDto>? histories = null;

        if (includeDetails)
        {
            items = q.Items
                .Where(i => !i.IsDeleted)
                .Select(i => new QuotationItemDto(
                    i.Id,
                    i.QuotationId,
                    i.ServiceId,
                    i.ServiceCode,
                    i.ServiceName,
                    i.Unit,
                    (int)i.Quantity,
                    i.UnitPrice,
                    i.DiscountRate,
                    i.DiscountAmount,
                    i.VatRate,
                    i.VatAmount,
                    i.LineTotal,
                    i.Notes
                ))
                .ToList();

            histories = q.ApprovalHistories
                .OrderByDescending(h => h.Timestamp)
                .Select(h => new QuotationApprovalHistoryDto(
                    h.Id,
                    h.QuotationId,
                    h.Action,
                    h.ActorUserId ?? Guid.Empty,
                    h.ActorName,
                    h.Comments,
                    h.Timestamp
                ))
                .ToList();
        }

        return new QuotationDto(
            q.Id,
            q.QuotationNumber,
            q.Version,
            q.RevisionNumber,
            q.ParentQuotationId,
            q.DealId,
            q.Deal?.Title,
            q.AccountId,
            q.Account?.Name ?? "Unknown",
            q.ContactId,
            q.Contact?.FullName,
            q.Contact?.Email,
            q.PriceListId,
            q.PriceList?.Name,
            q.CreatedAt, // IssueDate
            q.ValidUntil.ToDateTime(TimeOnly.MinValue),
            q.Subtotal,
            q.DiscountAmount,
            q.VatAmount,
            q.TotalAmount,
            q.Status,
            q.TermsAndConditions,
            q.Notes,
            q.PdfUrl,
            q.SubmittedAt,
            null,
            q.ApprovedBy?.ToString(),
            q.ApprovedAt,
            q.ApprovalNotes,
            q.RejectedBy?.ToString(),
            q.RejectedAt,
            q.RejectionReason,
            q.SentAt,
            q.SentToEmail,
            q.CreatedAt,
            q.UpdatedAt,
            items,
            histories
        );
    }
}
