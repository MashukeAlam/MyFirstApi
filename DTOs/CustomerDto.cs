namespace MyFirstApi.DTOs;

public record CustomerCreateDto(
    string Name,
    string Address,
    string Phone,
    string Email,
    string VatRegNo,
    int CompanyId
);

public record CustomerUpdateDto(
    string Name,
    string Address,
    string Phone,
    string Email,
    string VatRegNo,
    bool IsActive,
    int CompanyId
);

public record CustomerResponseDto(
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
