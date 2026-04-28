namespace MyFirstApi.DTOs;

public record BomCreateDto(
    int FinishedItemId,
    string Version,
    bool IsActive,
    string Notes
);

public record BomLineCreateDto(
    int RawMaterialItemId,
    decimal QuantityRequired
);

public record BomLineResponseDto(
    int Id,
    int RawMaterialItemId,
    decimal QuantityRequired
);

public record BomResponseDto(
    int Id,
    int FinishedItemId,
    string Version,
    bool IsActive,
    string Notes,
    List<BomLineResponseDto> Lines
);

public record ProductionOrderCreateDto(
    int BomHeaderId,
    decimal QuantityToProduce,
    string Notes,
    int CreatedByUserId
);

public record ProductionOrderResponseDto(
    int Id,
    int BomHeaderId,
    decimal QuantityToProduce,
    string Status,
    DateTime Date,
    string Notes,
    int CreatedByUserId
);
