using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.Services;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class ServiceCatalogService : IServiceCatalogService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ServiceCatalogService> _logger;

    public ServiceCatalogService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<ServiceCatalogService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    #region Categories
    public async Task<IReadOnlyList<ServiceCategoryDto>> GetCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.ServiceCategories.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .OrderBy(c => c.OrderIndex)
            .ThenBy(c => c.Name)
            .Select(c => new ServiceCategoryDto(
                c.Id,
                c.Code,
                c.Name,
                c.Description,
                c.OrderIndex,
                c.IsActive,
                c.Services.Count(s => !s.IsDeleted),
                c.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceCategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _context.ServiceCategories
            .AsNoTracking()
            .Include(c => c.Services)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null) return null;

        return new ServiceCategoryDto(
            category.Id,
            category.Code,
            category.Name,
            category.Description,
            category.OrderIndex,
            category.IsActive,
            category.Services.Count(s => !s.IsDeleted),
            category.CreatedAt
        );
    }

    public async Task<ServiceCategoryDto> CreateCategoryAsync(CreateServiceCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await _context.ServiceCategories.AnyAsync(c => c.Code == request.Code, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Category with code '{request.Code}' already exists.");
        }

        var category = new ServiceCategory
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            OrderIndex = request.OrderIndex,
            IsActive = request.IsActive
        };

        _context.ServiceCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceCategoryDto(category.Id, category.Code, category.Name, category.Description, category.OrderIndex, category.IsActive, 0, category.CreatedAt);
    }

    public async Task<ServiceCategoryDto> UpdateCategoryAsync(Guid id, UpdateServiceCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _context.ServiceCategories
            .Include(c => c.Services)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null)
        {
            throw new KeyNotFoundException($"Category with id '{id}' not found.");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.OrderIndex = request.OrderIndex;
        category.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceCategoryDto(category.Id, category.Code, category.Name, category.Description, category.OrderIndex, category.IsActive, category.Services.Count(s => !s.IsDeleted), category.CreatedAt);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _context.ServiceCategories.FindAsync([id], cancellationToken);
        if (category == null) return false;

        category.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
    #endregion

    #region Services
    public async Task<PagedServiceResult<ServiceDto>> GetServicesAsync(ServiceFilterParams filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Services
            .AsNoTracking()
            .Include(s => s.ServiceCategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(term) || s.Code.ToLower().Contains(term));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(s => s.CategoryId == filter.CategoryId.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == filter.IsActive.Value);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(s => s.BasePrice >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(s => s.BasePrice <= filter.MaxPrice.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = filter.SortBy?.ToLower() switch
        {
            "code" => filter.SortDescending ? query.OrderByDescending(s => s.Code) : query.OrderBy(s => s.Code),
            "baseprice" => filter.SortDescending ? query.OrderByDescending(s => s.BasePrice) : query.OrderBy(s => s.BasePrice),
            "createdat" => filter.SortDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            _ => filter.SortDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name)
        };

        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new ServiceDto(
                s.Id,
                s.Code,
                s.Name,
                s.Description,
                s.Unit,
                s.BasePrice,
                s.CostPrice,
                s.IsActive,
                s.CategoryId,
                s.ServiceCategory != null ? s.ServiceCategory.Name : null,
                s.CreatedAt,
                s.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedServiceResult<ServiceDto>(items, totalCount, pageNumber, pageSize, totalPages);
    }

    public async Task<ServiceDto?> GetServiceByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var service = await _context.Services
            .AsNoTracking()
            .Include(s => s.ServiceCategory)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (service == null) return null;

        return new ServiceDto(
            service.Id,
            service.Code,
            service.Name,
            service.Description,
            service.Unit,
            service.BasePrice,
            service.CostPrice,
            service.IsActive,
            service.CategoryId,
            service.ServiceCategory?.Name,
            service.CreatedAt,
            service.UpdatedAt
        );
    }

    public async Task<ServiceDto> CreateServiceAsync(CreateServiceRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Services.AnyAsync(s => s.Code == request.Code, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Service with code '{request.Code}' already exists.");
        }

        var service = new ServiceItem
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Unit = request.Unit,
            BasePrice = request.BasePrice,
            CostPrice = request.CostPrice,
            CategoryId = request.CategoryId,
            IsActive = request.IsActive
        };

        _context.Services.Add(service);

        // Initial price history
        var history = new ServicePriceHistory
        {
            ServiceId = service.Id,
            OldPrice = 0,
            NewPrice = request.BasePrice,
            ChangedById = _currentUserService.UserId ?? Guid.Empty,
            ChangedByName = _currentUserService.Email ?? "System",
            EffectiveDate = DateTime.UtcNow,
            Reason = "Initial service creation"
        };
        _context.ServicePriceHistories.Add(history);

        await _context.SaveChangesAsync(cancellationToken);

        string? categoryName = null;
        if (request.CategoryId.HasValue)
        {
            categoryName = await _context.ServiceCategories
                .Where(c => c.Id == request.CategoryId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new ServiceDto(
            service.Id,
            service.Code,
            service.Name,
            service.Description,
            service.Unit,
            service.BasePrice,
            service.CostPrice,
            service.IsActive,
            service.CategoryId,
            categoryName,
            service.CreatedAt,
            service.UpdatedAt
        );
    }

    public async Task<ServiceDto> UpdateServiceAsync(Guid id, UpdateServiceRequest request, CancellationToken cancellationToken = default)
    {
        var service = await _context.Services
            .Include(s => s.ServiceCategory)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (service == null)
        {
            throw new KeyNotFoundException($"Service with id '{id}' not found.");
        }

        if (service.BasePrice != request.BasePrice)
        {
            var history = new ServicePriceHistory
            {
                ServiceId = service.Id,
                OldPrice = service.BasePrice,
                NewPrice = request.BasePrice,
                ChangedById = _currentUserService.UserId ?? Guid.Empty,
                ChangedByName = _currentUserService.Email ?? "System",
                EffectiveDate = DateTime.UtcNow,
                Reason = request.PriceChangeReason ?? "Price updated"
            };
            _context.ServicePriceHistories.Add(history);
        }

        service.Name = request.Name;
        service.Description = request.Description;
        service.Unit = request.Unit;
        service.BasePrice = request.BasePrice;
        service.CostPrice = request.CostPrice;
        service.CategoryId = request.CategoryId;
        service.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        string? categoryName = null;
        if (request.CategoryId.HasValue)
        {
            categoryName = await _context.ServiceCategories
                .Where(c => c.Id == request.CategoryId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new ServiceDto(
            service.Id,
            service.Code,
            service.Name,
            service.Description,
            service.Unit,
            service.BasePrice,
            service.CostPrice,
            service.IsActive,
            service.CategoryId,
            categoryName,
            service.CreatedAt,
            service.UpdatedAt
        );
    }

    public async Task<bool> DeleteServiceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var service = await _context.Services.FindAsync([id], cancellationToken);
        if (service == null) return false;

        service.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<ServicePriceHistoryDto>> GetServicePriceHistoryAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await _context.ServicePriceHistories
            .AsNoTracking()
            .Where(h => h.ServiceId == serviceId)
            .OrderByDescending(h => h.EffectiveDate)
            .Select(h => new ServicePriceHistoryDto(
                h.Id,
                h.ServiceId,
                h.OldPrice,
                h.NewPrice,
                h.ChangedByName,
                h.EffectiveDate,
                h.Reason,
                h.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }
    #endregion

    #region Price Lists
    public async Task<IReadOnlyList<PriceListDto>> GetPriceListsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.PriceLists.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        var lists = await query
            .OrderByDescending(p => p.IsDefault)
            .ThenBy(p => p.Name)
            .Select(p => new
            {
                p.Id,
                p.Code,
                p.Name,
                p.Description,
                p.Currency,
                p.StartDate,
                p.EndDate,
                p.IsDefault,
                p.IsActive,
                ItemCount = p.Items.Count(i => !i.IsDeleted),
                p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return lists.Select(p => new PriceListDto(
            p.Id,
            p.Code,
            p.Name,
            p.Description,
            p.Currency,
            p.StartDate.HasValue ? p.StartDate.Value.ToDateTime(TimeOnly.MinValue) : null,
            p.EndDate.HasValue ? p.EndDate.Value.ToDateTime(TimeOnly.MinValue) : null,
            p.IsDefault,
            p.IsActive,
            p.ItemCount,
            p.CreatedAt,
            null
        )).ToList();
    }

    public async Task<PriceListDto?> GetPriceListByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var priceList = await _context.PriceLists
            .AsNoTracking()
            .Include(p => p.Items)
                .ThenInclude(i => i.Service)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (priceList == null) return null;

        var items = priceList.Items
            .Where(i => !i.IsDeleted)
            .Select(i => new PriceListItemDto(
                i.Id,
                i.PriceListId,
                i.ServiceId,
                i.Service.Code,
                i.Service.Name,
                i.Service.Unit,
                i.Service.BasePrice,
                i.UnitPrice,
                (int)i.MinQuantity,
                i.MaxDiscountRate
            ))
            .ToList();

        return new PriceListDto(
            priceList.Id,
            priceList.Code,
            priceList.Name,
            priceList.Description,
            priceList.Currency,
            priceList.StartDate.HasValue ? priceList.StartDate.Value.ToDateTime(TimeOnly.MinValue) : null,
            priceList.EndDate.HasValue ? priceList.EndDate.Value.ToDateTime(TimeOnly.MinValue) : null,
            priceList.IsDefault,
            priceList.IsActive,
            items.Count,
            priceList.CreatedAt,
            items
        );
    }

    public async Task<PriceListDto> CreatePriceListAsync(CreatePriceListRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await _context.PriceLists.AnyAsync(p => p.Code == request.Code, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Price list with code '{request.Code}' already exists.");
        }

        if (request.IsDefault)
        {
            var currentDefaults = await _context.PriceLists.Where(p => p.IsDefault).ToListAsync(cancellationToken);
            foreach (var d in currentDefaults)
            {
                d.IsDefault = false;
            }
        }

        var priceList = new PriceList
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Currency = request.Currency,
            StartDate = request.StartDate.HasValue ? DateOnly.FromDateTime(request.StartDate.Value) : null,
            EndDate = request.EndDate.HasValue ? DateOnly.FromDateTime(request.EndDate.Value) : null,
            IsDefault = request.IsDefault,
            IsActive = request.IsActive
        };

        if (request.Items != null && request.Items.Count > 0)
        {
            foreach (var itemInput in request.Items)
            {
                priceList.Items.Add(new PriceListItem
                {
                    ServiceId = itemInput.ServiceId,
                    UnitPrice = itemInput.UnitPrice,
                    MinQuantity = itemInput.MinQuantity,
                    MaxDiscountRate = itemInput.MaxDiscountRate
                });
            }
        }

        _context.PriceLists.Add(priceList);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetPriceListByIdAsync(priceList.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve created price list.");
    }

    public async Task<PriceListDto> UpdatePriceListAsync(Guid id, UpdatePriceListRequest request, CancellationToken cancellationToken = default)
    {
        var priceList = await _context.PriceLists
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (priceList == null)
        {
            throw new KeyNotFoundException($"Price list with id '{id}' not found.");
        }

        if (request.IsDefault && !priceList.IsDefault)
        {
            var currentDefaults = await _context.PriceLists.Where(p => p.IsDefault && p.Id != id).ToListAsync(cancellationToken);
            foreach (var d in currentDefaults)
            {
                d.IsDefault = false;
            }
        }

        priceList.Name = request.Name;
        priceList.Description = request.Description;
        priceList.Currency = request.Currency;
        priceList.StartDate = request.StartDate.HasValue ? DateOnly.FromDateTime(request.StartDate.Value) : null;
        priceList.EndDate = request.EndDate.HasValue ? DateOnly.FromDateTime(request.EndDate.Value) : null;
        priceList.IsDefault = request.IsDefault;
        priceList.IsActive = request.IsActive;

        if (request.Items != null)
        {
            _context.PriceListItems.RemoveRange(priceList.Items);
            priceList.Items.Clear();

            foreach (var itemInput in request.Items)
            {
                priceList.Items.Add(new PriceListItem
                {
                    PriceListId = priceList.Id,
                    ServiceId = itemInput.ServiceId,
                    UnitPrice = itemInput.UnitPrice,
                    MinQuantity = itemInput.MinQuantity,
                    MaxDiscountRate = itemInput.MaxDiscountRate
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await GetPriceListByIdAsync(priceList.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve updated price list.");
    }

    public async Task<bool> DeletePriceListAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var priceList = await _context.PriceLists.FindAsync([id], cancellationToken);
        if (priceList == null) return false;

        priceList.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<decimal?> GetEffectivePriceAsync(Guid serviceId, Guid? priceListId, int quantity, CancellationToken cancellationToken = default)
    {
        if (priceListId.HasValue)
        {
            var item = await _context.PriceListItems
                .AsNoTracking()
                .Where(i => i.PriceListId == priceListId.Value && i.ServiceId == serviceId && i.MinQuantity <= quantity)
                .OrderByDescending(i => i.MinQuantity)
                .FirstOrDefaultAsync(cancellationToken);

            if (item != null)
            {
                return item.UnitPrice;
            }
        }

        // Try active default price list
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var defaultPriceListItem = await _context.PriceListItems
            .AsNoTracking()
            .Where(i => i.PriceList.IsDefault && i.PriceList.IsActive &&
                        (i.PriceList.StartDate == null || i.PriceList.StartDate <= today) &&
                        (i.PriceList.EndDate == null || i.PriceList.EndDate >= today) &&
                        i.ServiceId == serviceId && i.MinQuantity <= quantity)
            .OrderByDescending(i => i.MinQuantity)
            .FirstOrDefaultAsync(cancellationToken);

        if (defaultPriceListItem != null)
        {
            return defaultPriceListItem.UnitPrice;
        }

        // Fallback to base price
        var service = await _context.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == serviceId, cancellationToken);
        return service?.BasePrice;
    }
    #endregion
}
