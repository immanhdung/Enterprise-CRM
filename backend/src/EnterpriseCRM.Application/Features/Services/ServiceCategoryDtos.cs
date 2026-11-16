namespace EnterpriseCRM.Application.Features.Services;

public record ServiceCategoryDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    int OrderIndex,
    bool IsActive,
    int ServiceCount,
    DateTime CreatedAt
);

public record CreateServiceCategoryRequest(
    string Code,
    string Name,
    string? Description,
    int OrderIndex,
    bool IsActive = true
);

public record UpdateServiceCategoryRequest(
    string Name,
    string? Description,
    int OrderIndex,
    bool IsActive
);
