namespace MyFirstApi.DTOs;

public record VendorCreateDto(
    string Name,
    string Address,
    string Phone,
    string Email,
    string VatRegNo,
    int CompanyId
);

public record VendorUpdateDto(
    string Name,
    string Address,
    string Phone,
    string Email,
    string VatRegNo,
    bool IsActive,
    int CompanyId
);

public record VendorResponseDto(
    int Id,
    string Name,
    string Address,
    string Phone,
    string Email,
    string VatRegNo,
    bool IsActive,
    int CompanyId,
    DateTime CreatedAt
);
