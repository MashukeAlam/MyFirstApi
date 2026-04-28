namespace MyFirstApi.DTOs;

public record PurchaseOrderLineCreateDto(
    int ItemId,
    decimal Quantity,
    decimal UnitPrice
);

public record PurchaseOrderCreateDto(
    int VendorId,
    DateTime Date,
    string Notes,
    int CreatedByUserId,
    int CompanyId,
    List<PurchaseOrderLineCreateDto> Lines
);

public record PurchaseOrderLineResponseDto(
    int Id,
    int ItemId,
    decimal Quantity,
    decimal UnitPrice,
    decimal VatAmount,
    decimal SdAmount
);

public record PurchaseOrderResponseDto(
    int Id,
    int VendorId,
    DateTime Date,
    string Status,
    string Notes,
    int CreatedByUserId,
    int CompanyId,
    List<PurchaseOrderLineResponseDto> Lines
);

public record PurchaseOrderUpdateDto(
    int VendorId,
    DateTime Date,
    string Notes,
    int CompanyId,
    List<PurchaseOrderLineCreateDto> Lines
);
