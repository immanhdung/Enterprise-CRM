using System.Text;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.Finance;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IApplicationDbContext _context;
    private readonly IElectronicInvoiceProvider _einvoiceProvider;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        IApplicationDbContext context,
        IElectronicInvoiceProvider einvoiceProvider,
        ILogger<InvoiceService> logger)
    {
        _context = context;
        _einvoiceProvider = einvoiceProvider;
        _logger = logger;
    }

    public async Task<PagedInvoiceResult> GetInvoicesAsync(InvoiceFilterParams filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Invoices
            .AsNoTracking()
            .Include(i => i.Contract)
            .Include(i => i.Account)
            .Include(i => i.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(i => i.InvoiceNumber.ToLower().Contains(term) ||
                                     i.InvoiceSeries.ToLower().Contains(term) ||
                                     i.CustomerName.ToLower().Contains(term) ||
                                     (i.CustomerTaxCode != null && i.CustomerTaxCode.ToLower().Contains(term)) ||
                                     (i.LookupCode != null && i.LookupCode.ToLower().Contains(term)));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(i => i.Status == filter.Status.Value);
        }

        if (filter.ContractId.HasValue)
        {
            query = query.Where(i => i.ContractId == filter.ContractId.Value);
        }

        if (filter.AccountId.HasValue)
        {
            query = query.Where(i => i.AccountId == filter.AccountId.Value);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(i => i.IssueDate >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(i => i.IssueDate <= filter.ToDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = filter.SortBy?.ToLower() switch
        {
            "totalamount" => filter.SortDescending ? query.OrderByDescending(i => i.TotalWithTax) : query.OrderBy(i => i.TotalWithTax),
            "issuedate" => filter.SortDescending ? query.OrderByDescending(i => i.IssueDate) : query.OrderBy(i => i.IssueDate),
            "invoicenumber" => filter.SortDescending ? query.OrderByDescending(i => i.InvoiceNumber) : query.OrderBy(i => i.InvoiceNumber),
            _ => filter.SortDescending ? query.OrderByDescending(i => i.CreatedAt) : query.OrderBy(i => i.CreatedAt)
        };

        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(MapToDto).ToList();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedInvoiceResult(dtos, totalCount, pageNumber, pageSize, totalPages);
    }

    public async Task<InvoiceDto?> GetInvoiceByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Contract)
            .Include(i => i.Account)
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        return invoice == null ? null : MapToDto(invoice);
    }

    public async Task<InvoiceDto> IssueInvoiceAsync(
        IssueInvoiceRequest request,
        Guid currentUserId,
        string currentUserName,
        CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

        if (contract == null)
            throw new InvalidOperationException($"Contract with ID {request.ContractId} was not found.");

        var accountId = request.AccountId ?? contract.AccountId;

        // Generate Invoice Number
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var countToday = await _context.Invoices
            .CountAsync(i => i.InvoiceNumber.StartsWith($"INV-{today}"), cancellationToken);
        var invoiceNumber = $"INV-{today}-{(countToday + 1):D4}";

        // Calculate totals
        decimal subtotal = 0m;
        decimal totalTax = 0m;
        var items = new List<InvoiceItem>();
        int line = 1;

        foreach (var itemInput in request.Items)
        {
            var lineSubtotal = Math.Round(itemInput.Quantity * itemInput.UnitPrice, 2);
            var lineTax = Math.Round(lineSubtotal * (itemInput.TaxRate / 100m), 2);
            var lineTotal = lineSubtotal + lineTax;

            subtotal += lineSubtotal;
            totalTax += lineTax;

            items.Add(new InvoiceItem
            {
                ServiceId = itemInput.ServiceId,
                LineNumber = line++,
                Description = itemInput.Description,
                Unit = itemInput.Unit ?? "Unit",
                Quantity = itemInput.Quantity,
                UnitPrice = itemInput.UnitPrice,
                TaxRate = itemInput.TaxRate,
                TaxAmount = lineTax,
                LineTotal = lineTotal
            });
        }

        var totalAmount = subtotal + totalTax;
        var providerName = string.IsNullOrWhiteSpace(request.ElectronicInvoiceProvider) ? _einvoiceProvider.ProviderName : request.ElectronicInvoiceProvider;

        var invoice = new Invoice
        {
            ContractId = request.ContractId,
            AccountId = accountId,
            InvoiceRequestId = request.InvoiceRequestId,
            InvoiceNumber = invoiceNumber,
            InvoiceSeries = $"1C{DateTime.UtcNow:yy}TBB",
            CustomerName = request.CustomerName,
            CustomerTaxCode = request.CustomerTaxCode,
            CustomerAddress = request.CustomerAddress,
            CustomerEmail = request.CustomerEmail,
            IssueDate = request.IssueDate == default ? DateOnly.FromDateTime(DateTime.UtcNow) : request.IssueDate,
            Subtotal = subtotal,
            TaxRate = items.Count > 0 ? items.Average(i => i.TaxRate) : 8m,
            TaxAmount = totalTax,
            TotalWithTax = totalAmount,
            Status = InvoiceStatus.Published,
            EInvoiceProvider = providerName,
            Items = items
        };

        // Call electronic invoice provider
        try
        {
            var einvResult = await _einvoiceProvider.IssueInvoiceAsync(invoice, items, cancellationToken);
            if (einvResult.IsSuccess)
            {
                invoice.ProviderTransactionId = einvResult.ElectronicInvoiceId;
                invoice.LookupCode = einvResult.LookupCode;
                invoice.LookupUrl = einvResult.LookupUrl;
                if (!string.IsNullOrWhiteSpace(einvResult.InvoiceSeries))
                    invoice.InvoiceSeries = einvResult.InvoiceSeries;
            }
            else
            {
                invoice.ErrorMessage = einvResult.ErrorMessage;
                invoice.RetryCount = 1;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to electronic invoice provider for Invoice {InvoiceNumber}", invoiceNumber);
            invoice.ErrorMessage = ex.Message;
            invoice.RetryCount = 1;
        }

        _context.Invoices.Add(invoice);

        // Update InvoiceRequest if linked
        if (request.InvoiceRequestId.HasValue)
        {
            var invReq = await _context.InvoiceRequests.FindAsync([request.InvoiceRequestId.Value], cancellationToken);
            if (invReq != null)
            {
                invReq.Status = "Issued";
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        invoice.Contract = contract;
        invoice.Account = contract.Account;

        _logger.LogInformation("Invoice {InvoiceNumber} created and issued by {UserName} for Account {AccountName}, Total: {TotalAmount}",
            invoice.InvoiceNumber, currentUserName, invoice.CustomerName, totalAmount);

        return MapToDto(invoice);
    }

    public async Task<InvoiceDto> CancelInvoiceAsync(Guid id, CancelInvoiceRequest request, Guid currentUserId, string currentUserName, CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Contract)
            .Include(i => i.Account)
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (invoice == null)
            throw new InvalidOperationException($"Invoice with ID {id} was not found.");

        if (invoice.Status == InvoiceStatus.Cancelled)
            throw new InvalidOperationException("Invoice has already been cancelled.");

        // Cancel with e-invoice provider if issued
        if (!string.IsNullOrWhiteSpace(invoice.ProviderTransactionId))
        {
            await _einvoiceProvider.CancelInvoiceAsync(invoice.ProviderTransactionId, request.CancellationReason, cancellationToken);
        }

        invoice.Status = InvoiceStatus.Cancelled;
        invoice.CancellationReason = request.CancellationReason;
        invoice.CancelledAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Invoice {InvoiceNumber} was CANCELLED by {UserName}. Reason: {Reason}",
            invoice.InvoiceNumber, currentUserName, request.CancellationReason);

        return MapToDto(invoice);
    }

    public async Task<InvoiceDto> SyncInvoiceStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Contract)
            .Include(i => i.Account)
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (invoice == null)
            throw new InvalidOperationException($"Invoice with ID {id} was not found.");

        if (!string.IsNullOrWhiteSpace(invoice.ProviderTransactionId))
        {
            var syncResult = await _einvoiceProvider.SyncStatusAsync(invoice.ProviderTransactionId, cancellationToken);
            if (syncResult.IsSuccess && !string.IsNullOrWhiteSpace(syncResult.Status))
            {
                if (syncResult.Status == "Cancelled")
                    invoice.Status = InvoiceStatus.Cancelled;
                else if (syncResult.Status == "SentToCustomer")
                    invoice.Status = InvoiceStatus.SentToCustomer;
                else
                    invoice.Status = InvoiceStatus.Published;

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        return MapToDto(invoice);
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Contract)
            .Include(i => i.Account)
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (invoice == null)
            throw new InvalidOperationException($"Invoice with ID {id} was not found.");

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'><title>HÓA ĐƠN ĐIỆN TỬ</title>");
        sb.AppendLine("<style>body{font-family:Arial,sans-serif;padding:30px;}table{width:100%;border-collapse:collapse;margin-top:20px;}th,td{border:1px solid #ddd;padding:8px;}th{background-color:#f4f4f4;text-align:left;}</style></head><body>");
        sb.AppendLine($"<h2>HÓA ĐƠN GIÁ TRỊ GIA TĂNG (ELECTRONIC INVOICE)</h2>");
        sb.AppendLine($"<p><strong>Mẫu số - Ký hiệu (Series):</strong> {invoice.InvoiceSeries} | <strong>Số hóa đơn:</strong> {invoice.InvoiceNumber}</p>");
        sb.AppendLine($"<p><strong>Ngày lập (Date):</strong> {invoice.IssueDate:dd/MM/yyyy}</p>");
        sb.AppendLine($"<p><strong>Mã tra cứu (Lookup Code):</strong> {invoice.LookupCode ?? "N/A"}</p>");
        sb.AppendLine($"<hr/>");
        sb.AppendLine($"<p><strong>Khách hàng (Customer):</strong> {invoice.CustomerName}</p>");
        sb.AppendLine($"<p><strong>Mã số thuế (Tax Code):</strong> {invoice.CustomerTaxCode ?? "N/A"}</p>");
        sb.AppendLine($"<p><strong>Địa chỉ (Address):</strong> {invoice.CustomerAddress ?? "N/A"}</p>");
        sb.AppendLine($"<table><thead><tr><th>STT</th><th>Tên hàng hóa, dịch vụ</th><th>ĐVT</th><th>Số lượng</th><th>Đơn giá</th><th>Thành tiền</th></tr></thead><tbody>");

        int idx = 1;
        foreach (var item in invoice.Items)
        {
            sb.AppendLine($"<tr><td>{idx++}</td><td>{item.Description}</td><td>{item.Unit}</td><td>{item.Quantity:N0}</td><td>{item.UnitPrice:N0}</td><td>{item.LineTotal:N0}</td></tr>");
        }

        sb.AppendLine($"</tbody></table>");
        sb.AppendLine($"<p style='text-align:right;margin-top:20px;'><strong>Tiền hàng:</strong> {invoice.Subtotal:N0} VND<br/>");
        sb.AppendLine($"<strong>Tiền thuế GTGT ({invoice.TaxRate}%):</strong> {invoice.TaxAmount:N0} VND<br/>");
        sb.AppendLine($"<strong>Tổng tiền thanh toán:</strong> {invoice.TotalWithTax:N0} VND</p>");
        sb.AppendLine("</body></html>");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    // Invoice Requests
    public async Task<PagedInvoiceRequestResult> GetInvoiceRequestsAsync(InvoiceRequestFilterParams filter, CancellationToken cancellationToken = default)
    {
        var query = _context.InvoiceRequests
            .AsNoTracking()
            .Include(r => r.Contract)
            .Include(r => r.Account)
            .Include(r => r.Payment)
            .Include(r => r.IssuedInvoice)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(r => r.Status == filter.Status);
        }

        if (filter.ContractId.HasValue)
        {
            query = query.Where(r => r.ContractId == filter.ContractId.Value);
        }

        if (filter.AccountId.HasValue)
        {
            query = query.Where(r => r.AccountId == filter.AccountId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(r => new InvoiceRequestDto(
            Id: r.Id,
            ContractId: r.ContractId,
            ContractNumber: r.Contract?.ContractNumber,
            AccountId: r.AccountId,
            AccountName: r.Account?.Name,
            PaymentId: r.PaymentId,
            PaymentNumber: r.Payment?.PaymentNumber,
            Amount: r.Amount,
            Status: r.Status,
            RequestedByUserId: r.RequestedBy,
            RequestedByName: r.RequestedByName,
            RequestDate: r.RequestDate,
            Notes: r.Notes,
            IssuedInvoiceId: r.IssuedInvoice?.Id,
            IssuedInvoiceNumber: r.IssuedInvoice?.InvoiceNumber,
            CreatedAt: r.CreatedAt
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedInvoiceRequestResult(dtos, totalCount, pageNumber, pageSize, totalPages);
    }

    public async Task<InvoiceRequestDto> CreateInvoiceRequestAsync(
        CreateInvoiceRequestInput request,
        Guid currentUserId,
        string currentUserName,
        CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

        if (contract == null)
            throw new InvalidOperationException($"Contract with ID {request.ContractId} was not found.");

        var accountId = request.AccountId ?? contract.AccountId;

        var invoiceRequest = new InvoiceRequest
        {
            ContractId = request.ContractId,
            AccountId = accountId,
            PaymentId = request.PaymentId,
            Amount = request.Amount,
            Status = "Pending",
            RequestedBy = currentUserId,
            RequestedByName = currentUserName,
            RequestDate = DateTime.UtcNow,
            Notes = request.Notes
        };

        _context.InvoiceRequests.Add(invoiceRequest);
        await _context.SaveChangesAsync(cancellationToken);

        invoiceRequest.Contract = contract;
        invoiceRequest.Account = contract.Account;

        _logger.LogInformation("Invoice Request created by {UserName} for Contract {ContractNumber}, Amount: {Amount}",
            currentUserName, contract.ContractNumber, request.Amount);

        return new InvoiceRequestDto(
            Id: invoiceRequest.Id,
            ContractId: invoiceRequest.ContractId,
            ContractNumber: contract.ContractNumber,
            AccountId: accountId,
            AccountName: contract.Account.Name,
            PaymentId: request.PaymentId,
            PaymentNumber: null,
            Amount: invoiceRequest.Amount,
            Status: invoiceRequest.Status,
            RequestedByUserId: currentUserId,
            RequestedByName: currentUserName,
            RequestDate: invoiceRequest.RequestDate,
            Notes: invoiceRequest.Notes,
            IssuedInvoiceId: null,
            IssuedInvoiceNumber: null,
            CreatedAt: invoiceRequest.CreatedAt
        );
    }

    public async Task<InvoiceDto> ApproveAndIssueInvoiceRequestAsync(
        Guid invoiceRequestId,
        Guid currentUserId,
        string currentUserName,
        CancellationToken cancellationToken = default)
    {
        var invReq = await _context.InvoiceRequests
            .Include(r => r.Contract)
                .ThenInclude(c => c.Account)
            .Include(r => r.Payment)
            .FirstOrDefaultAsync(r => r.Id == invoiceRequestId, cancellationToken);

        if (invReq == null)
            throw new InvalidOperationException($"Invoice Request with ID {invoiceRequestId} was not found.");

        if (invReq.Status == "Issued")
            throw new InvalidOperationException("Invoice has already been issued for this request.");

        var issueRequest = new IssueInvoiceRequest(
            ContractId: invReq.ContractId,
            AccountId: invReq.AccountId,
            InvoiceRequestId: invReq.Id,
            PaymentId: invReq.PaymentId,
            CustomerName: invReq.Account.Name,
            CustomerTaxCode: invReq.Account.TaxCode,
            CustomerAddress: invReq.Account.BillingAddress,
            CustomerEmail: invReq.Account.Email,
            IssueDate: DateOnly.FromDateTime(DateTime.UtcNow),
            ElectronicInvoiceProvider: "Mock",
            Items: new List<InvoiceItemInput>
            {
                new(
                    ServiceId: null,
                    Description: $"Thanh toán theo hợp đồng {invReq.Contract.ContractNumber}",
                    Unit: "Gói",
                    Quantity: 1,
                    UnitPrice: Math.Round(invReq.Amount / 1.08m, 2),
                    TaxRate: 8m
                )
            }
        );

        var invoiceDto = await IssueInvoiceAsync(issueRequest, currentUserId, currentUserName, cancellationToken);
        return invoiceDto;
    }

    public async Task<bool> RejectInvoiceRequestAsync(
        Guid invoiceRequestId,
        string reason,
        Guid currentUserId,
        string currentUserName,
        CancellationToken cancellationToken = default)
    {
        var invReq = await _context.InvoiceRequests.FindAsync([invoiceRequestId], cancellationToken);
        if (invReq == null)
            return false;

        invReq.Status = "Rejected";
        invReq.Notes = string.IsNullOrWhiteSpace(invReq.Notes)
            ? $"Rejected by {currentUserName}. Reason: {reason}"
            : $"{invReq.Notes}\nRejected by {currentUserName}. Reason: {reason}";

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static InvoiceDto MapToDto(Invoice i)
    {
        var items = i.Items?.Select(item => new InvoiceItemDto(
            Id: item.Id,
            InvoiceId: item.InvoiceId,
            ServiceId: item.ServiceId,
            ServiceName: item.Service?.Name,
            LineNumber: item.LineNumber,
            Description: item.Description,
            Unit: item.Unit,
            Quantity: item.Quantity,
            UnitPrice: item.UnitPrice,
            TaxRate: item.TaxRate,
            TaxAmount: item.TaxAmount,
            LineTotal: item.LineTotal
        )).ToList();

        return new InvoiceDto(
            Id: i.Id,
            InvoiceNumber: i.InvoiceNumber,
            InvoiceSeries: i.InvoiceSeries,
            ContractId: i.ContractId,
            ContractNumber: i.Contract?.ContractNumber,
            AccountId: i.AccountId,
            AccountName: i.Account?.Name,
            CustomerName: i.CustomerName,
            CustomerTaxCode: i.CustomerTaxCode,
            CustomerAddress: i.CustomerAddress,
            CustomerEmail: i.CustomerEmail,
            IssueDate: i.IssueDate,
            Subtotal: i.Subtotal,
            TaxRate: i.TaxRate,
            TaxAmount: i.TaxAmount,
            TotalAmount: i.TotalWithTax,
            Status: i.Status,
            ElectronicInvoiceProvider: i.EInvoiceProvider,
            ElectronicInvoiceId: i.ProviderTransactionId,
            LookupCode: i.LookupCode,
            LookupUrl: i.LookupUrl,
            ErrorMessage: i.ErrorMessage,
            RetryCount: i.RetryCount,
            CancellationReason: i.CancellationReason,
            CancelledAt: i.CancelledAt,
            CreatedAt: i.CreatedAt,
            Items: items
        );
    }
}
