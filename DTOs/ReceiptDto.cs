namespace MyFirstApi.DTOs;

public record ReceiptLineDto(
    string ItemName,
    decimal Quantity,
    decimal UnitPrice,
    decimal VatAmount,
    decimal SdAmount,
    decimal LineTotal
);

public record ReceiptResponseDto(
    int SalesOrderId,
    string CompanyName,
    DateTime Date,
    string ReceiptNumber,
    string CustomerName,
    string CustomerAddress,
    List<ReceiptLineDto> Lines,
    decimal Subtotal,
    decimal TotalVat,
    decimal TotalSd,
    decimal GrandTotal
);
