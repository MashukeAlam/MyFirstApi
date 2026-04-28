namespace MyFirstApi.DTOs;

public record SalesOrderLineCreateDto(
    int ItemId,
    decimal Quantity,
    decimal UnitPrice
);

public record SalesOrderCreateDto(
    int CustomerId,
    DateTime Date,
    string Notes,
    int CreatedByUserId,
    int CompanyId,
    List<SalesOrderLineCreateDto> Lines
);

public record SalesOrderLineResponseDto(
    int Id,
    int ItemId,
    decimal Quantity,
    decimal UnitPrice,
    decimal VatAmount,
    decimal SdAmount
);

public record SalesOrderResponseDto(
    int Id,
    int CustomerId,
    DateTime Date,
    string Status,
    string Notes,
    int CreatedByUserId,
    int CompanyId,
    List<SalesOrderLineResponseDto> Lines
);

public record SalesOrderUpdateDto(
    int CustomerId,
    DateTime Date,
    string Notes,
    int CompanyId,
    List<SalesOrderLineCreateDto> Lines
);
