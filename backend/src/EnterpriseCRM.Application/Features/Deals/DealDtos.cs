using EnterpriseCRM.Domain.Enums;

namespace EnterpriseCRM.Application.Features.Deals;

public record DealDto(
    Guid Id,
    string Code,
    string Title,
    Guid AccountId,
    string AccountName,
    Guid? ContactId,
    string? ContactName,
    Guid StageId,
    string StageName,
    int StageOrder,
    string StageColor,
    decimal Amount,
    decimal Probability,
    decimal ExpectedRevenue,
    DateOnly ExpectedClosingDate,
    DateOnly? ActualClosingDate,
    DealStatus Status,
    string? LostReason,
    string? Source,
    string? Description,
    Guid? AssignedEmployeeId,
    string? AssignedEmployeeName,
    IReadOnlyList<DealStageHistoryDto> StageHistories,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record DealStageHistoryDto(
    Guid Id,
    Guid DealId,
    Guid? FromStageId,
    string? FromStageName,
    Guid ToStageId,
    string ToStageName,
    string? ChangedByName,
    DateTime ChangedAt,
    int DurationDays,
    string? Notes
);

public record CreateDealRequest(
    string Title,
    Guid AccountId,
    Guid? ContactId,
    Guid StageId,
    decimal Amount,
    decimal? ProbabilityOverride,
    DateOnly ExpectedClosingDate,
    string? Source,
    string? Description,
    Guid? AssignedEmployeeId
);

public record UpdateDealRequest(
    string Title,
    Guid AccountId,
    Guid? ContactId,
    Guid StageId,
    decimal Amount,
    decimal? ProbabilityOverride,
    DateOnly ExpectedClosingDate,
    DealStatus Status,
    string? LostReason,
    string? Source,
    string? Description,
    Guid? AssignedEmployeeId
);

public record UpdateDealStageRequest(
    Guid NewStageId,
    string? Notes,
    string? LostReason
);

public record KanbanDealCardDto(
    Guid Id,
    string Code,
    string Title,
    Guid AccountId,
    string AccountName,
    Guid StageId,
    Guid? ContactId,
    string? ContactName,
    decimal Amount,
    decimal Probability,
    decimal ExpectedRevenue,
    DateOnly ExpectedClosingDate,
    DealStatus Status,
    Guid? AssignedEmployeeId,
    string? AssignedEmployeeName,
    DateTime UpdatedAt
);

public record KanbanStageColumnDto(
    Guid StageId,
    string StageName,
    int OrderIndex,
    decimal Probability,
    string ColorCode,
    bool IsWonStage,
    bool IsLostStage,
    decimal TotalValue,
    decimal TotalExpectedRevenue,
    int DealCount,
    List<KanbanDealCardDto> Deals
);

public record KanbanBoardDto(
    IReadOnlyList<KanbanStageColumnDto> Columns,
    decimal TotalPipelineValue,
    decimal TotalExpectedRevenue,
    int TotalDeals
);
