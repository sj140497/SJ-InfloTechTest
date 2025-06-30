namespace UserManagement.Common.DTOs;

public record UserDetailDto(long Id, string Forename, string Surname, string Email, DateTime DateOfBirth, bool IsActive)
    : UserDto(Id, Forename, Surname, Email, DateOfBirth, IsActive);
