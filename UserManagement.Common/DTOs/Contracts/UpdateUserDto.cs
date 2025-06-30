namespace UserManagement.Common.DTOs.Contracts;

public record UpdateUserDto(
    string Forename,
    string Surname,
    string Email,
    DateTime DateOfBirth,
    bool IsActive
);
