namespace MyFirstApi.DTOs;

public record PurchaseReportItemDto(
    int PurchaseOrderId,
    DateTime Date,
    string Status,
    int VendorId,
    string VendorName,
    int ItemId,
    decimal Quantity,
    decimal UnitPrice,
    decimal VatAmount,
    decimal SdAmount
);

public record SalesReportItemDto(
    int SalesOrderId,
    DateTime Date,
    string Status,
    int CustomerId,
    string CustomerName,
    int ItemId,
    decimal Quantity,
    decimal UnitPrice,
    decimal VatAmount,
    decimal SdAmount
);

public record InventoryReportItemDto(
    int ItemId,
    string ItemName,
    decimal StockLevel
);
