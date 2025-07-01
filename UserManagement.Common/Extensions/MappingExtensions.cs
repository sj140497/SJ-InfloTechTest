using UserManagement.Common.DTOs;
using UserManagement.Common.DTOs.Contracts;
using UserManagement.Models;

namespace UserManagement.Common.Extensions;

public static class MappingExtensions
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto(
            user.Id, 
            user.Forename, 
            user.Surname, 
            user.Email, 
            user.DateOfBirth, 
            user.IsActive);
    }

    public static UserLogDto ToDto(this UserLog log, string userName)
    {
        return new UserLogDto(
            log.Id, 
            log.UserId, 
            userName,
            log.Action, 
            log.Timestamp, 
            log.Description ?? string.Empty, 
            log.Details ?? string.Empty);
    }

    public static UserDetailDto ToDetailDto(this User user, IEnumerable<UserLog> logs)
    {
        var userName = $"{user.Forename} {user.Surname}";
        var logDtos = logs.Select(l => l.ToDto(userName)).ToList();

        return new UserDetailDto(
            user.Id, 
            user.Forename, 
            user.Surname, 
            user.Email,
            user.DateOfBirth, 
            user.IsActive,
            logDtos);
    }

    public static IEnumerable<UserDto> ToDtos(this IEnumerable<User> users)
    {
        return users.Select(u => u.ToDto());
    }

    public static User ToEntity(this CreateUserDto dto)
    {
        return new User
        {
            Forename = dto.Forename,
            Surname = dto.Surname,
            Email = dto.Email,
            DateOfBirth = dto.DateOfBirth,
            IsActive = dto.IsActive
        };
    }

    public static User ToEntity(this UpdateUserDto dto, long id)
    {
        return new User
        {
            Id = id,
            Forename = dto.Forename,
            Surname = dto.Surname,
            Email = dto.Email,
            DateOfBirth = dto.DateOfBirth,
            IsActive = dto.IsActive
        };
    }
}
