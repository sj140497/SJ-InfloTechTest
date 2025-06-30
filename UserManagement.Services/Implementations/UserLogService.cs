using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Services.Domain.Implementations;

public class UserLogService(IDataContext dataContext) : IUserLogService
{
    public async Task<Result> LogActionAsync(long userId, string action, string? description = null, string? details = null)
    {
        try
        {
            var log = new UserLog
            {
                UserId = userId,
                Action = action,
                Description = description,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            await dataContext.CreateAsync(log);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error logging action: {ex.Message}");
        }
    }

    public async Task<IEnumerable<UserLog>> GetUserLogsAsync(long userId)
    {
        return await dataContext.GetAll<UserLog>()
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();
    }

    public async Task<Result<UserLog>> GetLogByIdAsync(long id)
    {
        try
        {
            var log = await dataContext.GetByIdAsync<UserLog>(id);

            return log == null ? 
                Result.Fail<UserLog>($"Log with Id {id} not found") 
                : Result.Ok(log);
        }
        catch (Exception ex)
        {
            return Result.Fail<UserLog>($"Error retrieving log: {ex.Message}");
        }
    }

    public async Task<IEnumerable<UserLog>> GetAllLogsAsync() =>
        await dataContext.GetAll<UserLog>()
        .OrderByDescending(log => log.Timestamp)
        .ToListAsync();
}
