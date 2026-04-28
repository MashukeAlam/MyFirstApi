namespace MyFirstApi.DTOs;

public record InventoryMovementResponseDto(
    int Id,
    int ItemId,
    string MovementType,
    decimal Quantity,
    string ReferenceType,
    int ReferenceId,
    DateTime Date,
    string Notes
);

public record InventoryStockResponseDto(
    int ItemId,
    decimal StockLevel
);
