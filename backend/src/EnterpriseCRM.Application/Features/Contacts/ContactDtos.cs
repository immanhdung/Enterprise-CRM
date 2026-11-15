namespace EnterpriseCRM.Application.Features.Contacts;

public record ContactDto(
    Guid Id,
    Guid AccountId,
    string AccountName,
    string FirstName,
    string LastName,
    string FullName,
    string? Email,
    string Phone,
    string? Position,
    string? Department,
    bool IsPrimary,
    string Status,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateContactRequest(
    Guid AccountId,
    string FirstName,
    string LastName,
    string? Email,
    string Phone,
    string? Position,
    string? Department,
    bool IsPrimary,
    string? Notes
);

public record UpdateContactRequest(
    Guid AccountId,
    string FirstName,
    string LastName,
    string? Email,
    string Phone,
    string? Position,
    string? Department,
    bool IsPrimary,
    string Status,
    string? Notes
);
