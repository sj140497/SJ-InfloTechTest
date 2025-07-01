namespace UserManagement.Common.DTOs;

public sealed record UserDto(
    long Id,
    string Forename,
    string Surname,
    string Email,
    DateTime DateOfBirth,
    bool IsActive
);

