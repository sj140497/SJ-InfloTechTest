using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using UserManagement.Models;
using UserManagement.Common.DTOs;

namespace UserManagement.Services.Domain.Interfaces;

public interface IUserLogService
{
    /// <summary>
    /// Log an action performed on a user
    /// </summary>
    /// <param name="userId">The ID of the user the action was performed on</param>
    /// <param name="action">The action that was performed</param>
    /// <param name="description">A brief description of the action</param>
    /// <param name="details">Detailed information about the action (optional)</param>
    Task<Result> LogActionAsync(long userId, string action, string? description = null, string? details = null);

    /// <summary>
    /// Get all logs for a specific user
    /// </summary>
    /// <param name="userId">The user ID to get logs for</param>
    /// <returns>List of user logs</returns>
    Task<IEnumerable<UserLog>> GetUserLogsAsync(long userId);

    /// <summary>
    /// Get a specific log by ID
    /// </summary>
    /// <param name="id">The log ID</param>
    /// <returns>User log or null if not found</returns>
    Task<Result<UserLog>> GetLogByIdAsync(long id);

    /// <summary>
    /// Get all logs in the system with pagination
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of user logs</returns>
    Task<PagedResultDto<UserLog>> GetAllLogsAsync(int page, int pageSize);
}
