namespace UserManagement.Common.DTOs;

public sealed record UserLogDto(
    long Id,
    long UserId,
    string UserName,
    string Action,
    DateTime Timestamp,
    string? Description,
    string? Details
);

