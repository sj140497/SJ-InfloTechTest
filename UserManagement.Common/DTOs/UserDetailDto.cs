namespace UserManagement.Common.DTOs;

public sealed record UserDetailDto(
    long Id,
    string Forename,
    string Surname,
    string Email,
    DateTime DateOfBirth,
    bool IsActive,
    IEnumerable<UserLogDto> RecentLogs);
