namespace MyFirstApi.DTOs;

public record ItemCreateDto(
    string Name,
    string Code,
    string Unit,
    string ItemType,
    decimal VatRate,
    decimal SdRate
);

public record ItemUpdateDto(
    string Name,
    string Unit,
    string ItemType,
    decimal VatRate,
    decimal SdRate
);

public record ItemResponseDto(
    int Id,
    string Name,
    string Code,
    string Unit,
    string ItemType,
    decimal VatRate,
    decimal SdRate,
    bool IsActive,
    DateTime CreatedAt
);

public record ItemImportRow(
    string Name,
    string Code,
    string Unit,
    string ItemType,
    decimal VatRate,
    decimal SdRate
);