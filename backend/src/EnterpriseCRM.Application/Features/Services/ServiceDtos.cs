namespace EnterpriseCRM.Application.Features.Services;

public record ServiceDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Unit,
    decimal BasePrice,
    decimal CostPrice,
    bool IsActive,
    Guid? CategoryId,
    string? CategoryName,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ServicePriceHistoryDto(
    Guid Id,
    Guid ServiceId,
    decimal OldPrice,
    decimal NewPrice,
    string? ChangedByName,
    DateTime EffectiveDate,
    string? Reason,
    DateTime CreatedAt
);

public record CreateServiceRequest(
    string Code,
    string Name,
    string? Description,
    string Unit,
    decimal BasePrice,
    decimal CostPrice,
    Guid? CategoryId,
    bool IsActive = true
);

public record UpdateServiceRequest(
    string Name,
    string? Description,
    string Unit,
    decimal BasePrice,
    decimal CostPrice,
    Guid? CategoryId,
    bool IsActive,
    string? PriceChangeReason
);

public record ServiceFilterParams(
    int PageNumber = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    Guid? CategoryId = null,
    bool? IsActive = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? SortBy = "Name",
    bool SortDescending = false
);

public record PagedServiceResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
