using System.Collections.Generic;
using UserManagement.Models;

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
    /// <param name="performedBy">Who performed the action (optional)</param>
    /// <param name="ipAddress">IP address of the person performing the action (optional)</param>
    /// <param name="userAgent">User agent of the browser (optional)</param>
    void LogAction(long userId, string action, string? description = null, string? details = null);

    /// <summary>
    /// Get all logs for a specific user
    /// </summary>
    /// <param name="userId">The user ID to get logs for</param>
    /// <returns>List of user logs</returns>
    IEnumerable<UserLog> GetUserLogs(long userId);

    /// <summary>
    /// Get logs for a specific user (alias for GetUserLogs)
    /// </summary>
    /// <param name="userId">The user ID to get logs for</param>
    /// <returns>List of user logs</returns>
    IEnumerable<UserLog> GetLogsByUserId(long userId);

    /// <summary>
    /// Get a specific log by ID
    /// </summary>
    /// <param name="id">The log ID</param>
    /// <returns>User log or null if not found</returns>
    UserLog? GetLogById(long id);

    /// <summary>
    /// Get all logs in the system
    /// </summary>
    /// <returns>List of all user logs</returns>
    IEnumerable<UserLog> GetAllLogs();
}
