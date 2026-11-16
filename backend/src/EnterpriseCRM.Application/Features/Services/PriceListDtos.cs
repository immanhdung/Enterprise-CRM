namespace EnterpriseCRM.Application.Features.Services;

public record PriceListItemDto(
    Guid Id,
    Guid PriceListId,
    Guid ServiceId,
    string ServiceCode,
    string ServiceName,
    string ServiceUnit,
    decimal StandardPrice,
    decimal UnitPrice,
    int MinQuantity,
    decimal MaxDiscountRate
);

public record PriceListDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Currency,
    DateTime? StartDate,
    DateTime? EndDate,
    bool IsDefault,
    bool IsActive,
    int ItemCount,
    DateTime CreatedAt,
    IReadOnlyList<PriceListItemDto>? Items = null
);

public record PriceListItemRequest(
    Guid ServiceId,
    decimal UnitPrice,
    int MinQuantity = 1,
    decimal MaxDiscountRate = 0
);

public record CreatePriceListRequest(
    string Code,
    string Name,
    string? Description,
    string Currency = "VND",
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    bool IsDefault = false,
    bool IsActive = true,
    IReadOnlyList<PriceListItemRequest>? Items = null
);

public record UpdatePriceListRequest(
    string Name,
    string? Description,
    string Currency,
    DateTime? StartDate,
    DateTime? EndDate,
    bool IsDefault,
    bool IsActive,
    IReadOnlyList<PriceListItemRequest>? Items = null
);
