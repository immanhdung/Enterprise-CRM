namespace EnterpriseCRM.Application.Features.Services;

public interface IServiceCatalogService
{
    // Categories
    Task<IReadOnlyList<ServiceCategoryDto>> GetCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<ServiceCategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceCategoryDto> CreateCategoryAsync(CreateServiceCategoryRequest request, CancellationToken cancellationToken = default);
    Task<ServiceCategoryDto> UpdateCategoryAsync(Guid id, UpdateServiceCategoryRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);

    // Services
    Task<PagedServiceResult<ServiceDto>> GetServicesAsync(ServiceFilterParams filter, CancellationToken cancellationToken = default);
    Task<ServiceDto?> GetServiceByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceDto> CreateServiceAsync(CreateServiceRequest request, CancellationToken cancellationToken = default);
    Task<ServiceDto> UpdateServiceAsync(Guid id, UpdateServiceRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteServiceAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServicePriceHistoryDto>> GetServicePriceHistoryAsync(Guid serviceId, CancellationToken cancellationToken = default);

    // Price Lists
    Task<IReadOnlyList<PriceListDto>> GetPriceListsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<PriceListDto?> GetPriceListByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PriceListDto> CreatePriceListAsync(CreatePriceListRequest request, CancellationToken cancellationToken = default);
    Task<PriceListDto> UpdatePriceListAsync(Guid id, UpdatePriceListRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeletePriceListAsync(Guid id, CancellationToken cancellationToken = default);
    Task<decimal?> GetEffectivePriceAsync(Guid serviceId, Guid? priceListId, int quantity, CancellationToken cancellationToken = default);
}
