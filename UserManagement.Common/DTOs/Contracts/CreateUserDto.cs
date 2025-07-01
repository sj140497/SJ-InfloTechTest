namespace UserManagement.Common.DTOs.Contracts;

public sealed record CreateUserDto(
    string Forename,
    string Surname,
    string Email,
    DateTime DateOfBirth,
    bool IsActive
);
