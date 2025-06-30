using System;
using System.Collections.Generic;
using System.Linq;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Services.Domain.Implementations;

public class UserLogService : IUserLogService
{
    private readonly IDataContext _dataContext;

    public UserLogService(IDataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public void LogAction(long userId, string action, string? description = null, string? details = null)
    {
        var log = new UserLog
        {
            UserId = userId,
            Action = action,
            Description = description,
            Details = details,
            Timestamp = DateTime.UtcNow
        };

        _dataContext.Create(log);
    }

    public IEnumerable<UserLog> GetUserLogs(long userId)
    {
        return _dataContext.GetAll<UserLog>()
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.Timestamp);
    }

    public IEnumerable<UserLog> GetLogsByUserId(long userId)
    {
        return GetUserLogs(userId);
    }

    public UserLog? GetLogById(long id)
    {
        return _dataContext.GetAll<UserLog>()
            .FirstOrDefault(log => log.Id == id);
    }

    public IEnumerable<UserLog> GetAllLogs()
    {
        return _dataContext.GetAll<UserLog>()
            .OrderByDescending(log => log.Timestamp);
    }
}
