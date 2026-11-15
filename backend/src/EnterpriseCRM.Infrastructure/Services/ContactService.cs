using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Contacts;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class ContactService : IContactService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ContactService> _logger;

    public ContactService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<ContactService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<PaginatedList<ContactDto>>> GetContactsAsync(
        string? search,
        Guid? accountId,
        string? status,
        bool? isPrimary,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Contacts
            .Include(c => c.Account)
            .AsQueryable();

        if (accountId.HasValue)
        {
            query = query.Where(c => c.AccountId == accountId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(c => c.Status == status);
        }

        if (isPrimary.HasValue)
        {
            query = query.Where(c => c.IsPrimary == isPrimary.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(s) ||
                c.LastName.ToLower().Contains(s) ||
                c.Phone.Contains(s) ||
                (c.Email != null && c.Email.ToLower().Contains(s)) ||
                c.Account.Name.ToLower().Contains(s));
        }

        var count = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, pageIndex);
        var size = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .OrderByDescending(c => c.IsPrimary)
            .ThenBy(c => c.LastName)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(c => new ContactDto(
                c.Id,
                c.AccountId,
                c.Account.Name,
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
            .ToListAsync(cancellationToken);

        return ApiResponse<PaginatedList<ContactDto>>.SuccessResponse(
            new PaginatedList<ContactDto>(items, count, page, size)
        );
    }

    public async Task<ApiResponse<ContactDto>> GetContactByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var c = await _context.Contacts
            .Include(x => x.Account)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (c == null)
        {
            return ApiResponse<ContactDto>.FailureResponse("Contact not found.", 404);
        }

        var dto = new ContactDto(
            c.Id,
            c.AccountId,
            c.Account.Name,
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
        );

        return ApiResponse<ContactDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<ContactDto>> CreateContactAsync(CreateContactRequest request, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts.FindAsync(new object[] { request.AccountId }, cancellationToken);
        if (account == null)
        {
            return ApiResponse<ContactDto>.FailureResponse("Associated account not found.", 400);
        }

        // If marked primary, unset other contacts for this account
        if (request.IsPrimary)
        {
            var existingPrimaries = await _context.Contacts.Where(c => c.AccountId == request.AccountId && c.IsPrimary).ToListAsync(cancellationToken);
            foreach (var p in existingPrimaries) p.IsPrimary = false;
        }

        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email?.Trim().ToLowerInvariant(),
            Phone = request.Phone.Trim(),
            Position = request.Position?.Trim(),
            Department = request.Department?.Trim(),
            IsPrimary = request.IsPrimary,
            Status = "Active",
            Notes = request.Notes?.Trim()
        };

        _context.Contacts.Add(contact);

        _context.CrmActivities.Add(new CrmActivity
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            ContactId = contact.Id,
            ActivityType = "StatusChange",
            Subject = "Contact Created",
            Notes = $"New contact '{contact.FullName}' added to {account.Name}.",
            PerformedByUserId = _currentUserService.UserId,
            PerformedByName = _currentUserService.Email ?? "System",
            CompletedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created Contact {ContactId} for Account {AccountId}", contact.Id, request.AccountId);

        var dto = new ContactDto(
            contact.Id,
            contact.AccountId,
            account.Name,
            contact.FirstName,
            contact.LastName,
            contact.FullName,
            contact.Email,
            contact.Phone,
            contact.Position,
            contact.Department,
            contact.IsPrimary,
            contact.Status,
            contact.Notes,
            contact.CreatedAt,
            contact.UpdatedAt
        );

        return ApiResponse<ContactDto>.SuccessResponse(dto, "Contact created successfully.", 201);
    }

    public async Task<ApiResponse<ContactDto>> UpdateContactAsync(Guid id, UpdateContactRequest request, CancellationToken cancellationToken = default)
    {
        var contact = await _context.Contacts.Include(c => c.Account).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (contact == null)
        {
            return ApiResponse<ContactDto>.FailureResponse("Contact not found.", 404);
        }

        if (request.IsPrimary && !contact.IsPrimary)
        {
            var existingPrimaries = await _context.Contacts.Where(c => c.AccountId == contact.AccountId && c.Id != id && c.IsPrimary).ToListAsync(cancellationToken);
            foreach (var p in existingPrimaries) p.IsPrimary = false;
        }

        contact.AccountId = request.AccountId;
        contact.FirstName = request.FirstName.Trim();
        contact.LastName = request.LastName.Trim();
        contact.Email = request.Email?.Trim().ToLowerInvariant();
        contact.Phone = request.Phone.Trim();
        contact.Position = request.Position?.Trim();
        contact.Department = request.Department?.Trim();
        contact.IsPrimary = request.IsPrimary;
        contact.Status = request.Status;
        contact.Notes = request.Notes?.Trim();

        await _context.SaveChangesAsync(cancellationToken);

        var accountName = contact.Account?.Name ?? (await _context.Accounts.FindAsync(new object[] { contact.AccountId }, cancellationToken))?.Name ?? "";

        var dto = new ContactDto(
            contact.Id,
            contact.AccountId,
            accountName,
            contact.FirstName,
            contact.LastName,
            contact.FullName,
            contact.Email,
            contact.Phone,
            contact.Position,
            contact.Department,
            contact.IsPrimary,
            contact.Status,
            contact.Notes,
            contact.CreatedAt,
            contact.UpdatedAt
        );

        return ApiResponse<ContactDto>.SuccessResponse(dto, "Contact updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteContactAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (contact == null)
        {
            return ApiResponse<bool>.FailureResponse("Contact not found.", 404);
        }

        contact.IsDeleted = true;
        contact.DeletedAt = DateTime.UtcNow;
        contact.DeletedBy = _currentUserService.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Contact deleted successfully.");
    }
}
