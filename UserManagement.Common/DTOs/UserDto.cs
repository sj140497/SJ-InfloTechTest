namespace UserManagement.Common.DTOs;

public record UserDto(
    long Id,
    string Forename,
    string Surname,
    string Email,
    DateTime DateOfBirth,
    bool IsActive
);

