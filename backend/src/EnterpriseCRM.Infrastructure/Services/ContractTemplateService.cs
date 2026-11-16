using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.Contracts;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class ContractTemplateService : IContractTemplateService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ContractTemplateService> _logger;

    public ContractTemplateService(
        IApplicationDbContext context,
        ILogger<ContractTemplateService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ContractTemplateDto>> GetTemplatesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.ContractTemplates.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        return await query
            .OrderByDescending(t => t.IsDefault)
            .ThenBy(t => t.Name)
            .Select(t => new ContractTemplateDto(
                t.Id,
                t.Code,
                t.Name,
                t.Category,
                t.Description,
                t.Content,
                t.IsDefault,
                t.IsActive,
                t.CreatedAt,
                t.UpdatedAt
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<ContractTemplateDto?> GetTemplateByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var t = await _context.ContractTemplates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (t == null) return null;

        return new ContractTemplateDto(t.Id, t.Code, t.Name, t.Category, t.Description, t.Content, t.IsDefault, t.IsActive, t.CreatedAt, t.UpdatedAt);
    }

    public async Task<ContractTemplateDto> CreateTemplateAsync(CreateContractTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await _context.ContractTemplates.AnyAsync(t => t.Code == request.Code, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Contract template with code '{request.Code}' already exists.");
        }

        if (request.IsDefault)
        {
            var defaults = await _context.ContractTemplates.Where(t => t.IsDefault).ToListAsync(cancellationToken);
            foreach (var d in defaults)
            {
                d.IsDefault = false;
            }
        }

        var template = new ContractTemplate
        {
            Code = request.Code,
            Name = request.Name,
            Category = request.Category,
            Description = request.Description,
            Content = request.Content,
            IsDefault = request.IsDefault,
            IsActive = request.IsActive
        };

        _context.ContractTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        return new ContractTemplateDto(template.Id, template.Code, template.Name, template.Category, template.Description, template.Content, template.IsDefault, template.IsActive, template.CreatedAt, template.UpdatedAt);
    }

    public async Task<ContractTemplateDto> UpdateTemplateAsync(Guid id, UpdateContractTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var template = await _context.ContractTemplates.FindAsync([id], cancellationToken);
        if (template == null)
        {
            throw new KeyNotFoundException($"Contract template '{id}' not found.");
        }

        if (request.IsDefault && !template.IsDefault)
        {
            var defaults = await _context.ContractTemplates.Where(t => t.IsDefault && t.Id != id).ToListAsync(cancellationToken);
            foreach (var d in defaults)
            {
                d.IsDefault = false;
            }
        }

        template.Name = request.Name;
        template.Category = request.Category;
        template.Description = request.Description;
        template.Content = request.Content;
        template.IsDefault = request.IsDefault;
        template.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return new ContractTemplateDto(template.Id, template.Code, template.Name, template.Category, template.Description, template.Content, template.IsDefault, template.IsActive, template.CreatedAt, template.UpdatedAt);
    }

    public async Task<bool> DeleteTemplateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await _context.ContractTemplates.FindAsync([id], cancellationToken);
        if (template == null) return false;

        template.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
