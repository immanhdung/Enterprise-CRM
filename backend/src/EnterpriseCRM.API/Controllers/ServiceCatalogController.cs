using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
[Route("api/v1/services")]
public class ServiceCatalogController : BaseApiController
{
    private readonly IServiceCatalogService _serviceCatalogService;

    public ServiceCatalogController(IServiceCatalogService serviceCatalogService)
    {
        _serviceCatalogService = serviceCatalogService;
    }

    #region Categories
    [HttpGet("/api/v1/service-categories")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ServiceCategoryDto>>>> GetCategories([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var result = await _serviceCatalogService.GetCategoriesAsync(includeInactive, cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("/api/v1/service-categories/{id:guid}")]
    public async Task<ActionResult<ApiResponse<ServiceCategoryDto>>> GetCategoryById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _serviceCatalogService.GetCategoryByIdAsync(id, cancellationToken);
        if (result == null) return NotFound(ApiResponse<ServiceCategoryDto>.Failed("Category not found"));
        return OkResponse(result);
    }

    [HttpPost("/api/v1/service-categories")]
    public async Task<ActionResult<ApiResponse<ServiceCategoryDto>>> CreateCategory([FromBody] CreateServiceCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _serviceCatalogService.CreateCategoryAsync(request, cancellationToken);
        return OkResponse(result, "Service category created successfully.");
    }

    [HttpPut("/api/v1/service-categories/{id:guid}")]
    public async Task<ActionResult<ApiResponse<ServiceCategoryDto>>> UpdateCategory(Guid id, [FromBody] UpdateServiceCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _serviceCatalogService.UpdateCategoryAsync(id, request, cancellationToken);
        return OkResponse(result, "Service category updated successfully.");
    }

    [HttpDelete("/api/v1/service-categories/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _serviceCatalogService.DeleteCategoryAsync(id, cancellationToken);
        if (!result) return NotFound(ApiResponse<bool>.Failed("Category not found"));
        return OkResponse(true, "Service category deleted successfully.");
    }
    #endregion

    #region Services
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedServiceResult<ServiceDto>>>> GetServices([FromQuery] ServiceFilterParams filter, CancellationToken cancellationToken = default)
    {
        var result = await _serviceCatalogService.GetServicesAsync(filter, cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ServiceDto>>> GetServiceById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _serviceCatalogService.GetServiceByIdAsync(id, cancellationToken);
        if (result == null) return NotFound(ApiResponse<ServiceDto>.Failed("Service not found"));
        return OkResponse(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ServiceDto>>> CreateService([FromBody] CreateServiceRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _serviceCatalogService.CreateServiceAsync(request, cancellationToken);
        return OkResponse(result, "Service created successfully.");
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ServiceDto>>> UpdateService(Guid id, [FromBody] UpdateServiceRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _serviceCatalogService.UpdateServiceAsync(id, request, cancellationToken);
        return OkResponse(result, "Service updated successfully.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteService(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _serviceCatalogService.DeleteServiceAsync(id, cancellationToken);
        if (!result) return NotFound(ApiResponse<bool>.Failed("Service not found"));
        return OkResponse(true, "Service deleted successfully.");
    }

    [HttpGet("{id:guid}/price-history")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ServicePriceHistoryDto>>>> GetServicePriceHistory(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _serviceCatalogService.GetServicePriceHistoryAsync(id, cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("{id:guid}/effective-price")]
    public async Task<ActionResult<ApiResponse<decimal?>>> GetEffectivePrice(Guid id, [FromQuery] Guid? priceListId = null, [FromQuery] int quantity = 1, CancellationToken cancellationToken = default)
    {
        var price = await _serviceCatalogService.GetEffectivePriceAsync(id, priceListId, quantity, cancellationToken);
        return OkResponse(price);
    }
    #endregion

    #region Price Lists
    [HttpGet("/api/v1/price-lists")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PriceListDto>>>> GetPriceLists([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var result = await _serviceCatalogService.GetPriceListsAsync(includeInactive, cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("/api/v1/price-lists/{id:guid}")]
    public async Task<ActionResult<ApiResponse<PriceListDto>>> GetPriceListById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _serviceCatalogService.GetPriceListByIdAsync(id, cancellationToken);
        if (result == null) return NotFound(ApiResponse<PriceListDto>.Failed("Price list not found"));
        return OkResponse(result);
    }

    [HttpPost("/api/v1/price-lists")]
    public async Task<ActionResult<ApiResponse<PriceListDto>>> CreatePriceList([FromBody] CreatePriceListRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _serviceCatalogService.CreatePriceListAsync(request, cancellationToken);
        return OkResponse(result, "Price list created successfully.");
    }

    [HttpPut("/api/v1/price-lists/{id:guid}")]
    public async Task<ActionResult<ApiResponse<PriceListDto>>> UpdatePriceList(Guid id, [FromBody] UpdatePriceListRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _serviceCatalogService.UpdatePriceListAsync(id, request, cancellationToken);
        return OkResponse(result, "Price list updated successfully.");
    }

    [HttpDelete("/api/v1/price-lists/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePriceList(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _serviceCatalogService.DeletePriceListAsync(id, cancellationToken);
        if (!result) return NotFound(ApiResponse<bool>.Failed("Price list not found"));
        return OkResponse(true, "Price list deleted successfully.");
    }
    #endregion
}
