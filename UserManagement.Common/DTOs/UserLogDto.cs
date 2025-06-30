namespace UserManagement.Common.DTOs;

public record UserLogDto(
    long Id,
    long UserId,
    string UserName,
    string Action,
    string? Description,
    string? Details
);

