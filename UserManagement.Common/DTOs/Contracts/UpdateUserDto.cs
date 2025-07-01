namespace UserManagement.Common.DTOs.Contracts;

public sealed record UpdateUserDto(
    string Forename,
    string Surname,
    string Email,
    DateTime DateOfBirth,
    bool IsActive
);
