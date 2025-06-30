namespace UserManagement.Common.DTOs.Contracts;

public record CreateUserDto(
    string Forename,
    string Surname,
    string Email,
    DateTime DateOfBirth,
    bool IsActive
);
