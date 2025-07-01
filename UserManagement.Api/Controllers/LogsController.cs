using Microsoft.AspNetCore.Mvc;
using UserManagement.Common.DTOs;
using UserManagement.Common.Extensions;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Models;

namespace UserManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController(IUserLogService logService, IUserService userService, ILogger<LogsController> logger) : ControllerBase
{
    /// <summary>
    /// Get all system logs
    /// </summary>
    /// <returns>List of all logs</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserLogDto>>> GetAllLogs()
    {
        try
        {
            var logs = await logService.GetAllLogsAsync();
            
            var logDtos = await ConvertLogsToDto(logs);

            return Ok(logDtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all logs");
            return StatusCode(500, new { message = "An error occurred while retrieving logs" });
        }
    }

    /// <summary>
    /// Get logs for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of logs for the user</returns>
    [HttpGet("user/{userId:long}")]
    public async Task<ActionResult<IEnumerable<UserLogDto>>> GetUserLogs(long userId)
    {
        try
        {
            // First check if user exists
            var userResult = await userService.GetByIdAsync(userId);
            if (userResult.IsFailed)
            {
                return NotFound(new { message = $"User with ID {userId} not found" });
            }

            var logs = await logService.GetUserLogsAsync(userId);
            var userName = $"{userResult.Value.Forename} {userResult.Value.Surname}";

            var logDtos = logs.Select(log => log.ToDto(userName));

            return Ok(logDtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving logs for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving user logs" });
        }
    }

    /// <summary>
    /// Get a specific log by ID
    /// </summary>
    /// <param name="id">Log ID</param>
    /// <returns>Log details</returns>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserLogDto>> GetLog(long id)
    {
        try
        {
            var logResult = await logService.GetLogByIdAsync(id);
            if (logResult.IsFailed)
            {
                return NotFound(new { message = logResult.Errors.FirstOrDefault()?.Message ?? "Log not found" });
            }

            var log = logResult.Value;
            
            // Get user name
            var userResult = await userService.GetByIdAsync(log.UserId);
            var userName = userResult.IsSuccess 
                ? $"{userResult.Value.Forename} {userResult.Value.Surname}" 
                : "Unknown User";

            var logDto = log.ToDto(userName);

            return Ok(logDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving log {LogId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the log" });
        }
    }

    /// <summary>
    /// Get logs filtered by action type
    /// </summary>
    /// <param name="action">Action type to filter by</param>
    /// <returns>Filtered list of logs</returns>
    [HttpGet("action/{action}")]
    public async Task<ActionResult<IEnumerable<UserLogDto>>> GetLogsByAction(string action)
    {
        try
        {
            var allLogs = await logService.GetAllLogsAsync();
            var filteredLogs = allLogs.Where(l => 
                l.Action.Equals(action, StringComparison.OrdinalIgnoreCase));

            var logDtos = await ConvertLogsToDto(filteredLogs);

            return Ok(logDtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving logs for action {Action}", action);
            return StatusCode(500, new { message = "An error occurred while retrieving filtered logs" });
        }
    }

    /// <summary>
    /// Get logs within a date range
    /// </summary>
    /// <param name="startDate">Start date (optional)</param>
    /// <param name="endDate">End date (optional)</param>
    /// <returns>Filtered list of logs</returns>
    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<UserLogDto>>> GetLogsByDateRange(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var allLogs = await logService.GetAllLogsAsync();
            var filteredLogs = allLogs.AsEnumerable();

            if (startDate.HasValue)
            {
                filteredLogs = filteredLogs.Where(l => l.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                filteredLogs = filteredLogs.Where(l => l.Timestamp <= endDate.Value);
            }

            var logDtos = await ConvertLogsToDto(filteredLogs);

            return Ok(logDtos.OrderByDescending(l => l.Timestamp));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving logs by date range");
            return StatusCode(500, new { message = "An error occurred while retrieving logs by date range" });
        }
    }

    /// <summary>
    /// Get log statistics
    /// </summary>
    /// <returns>Log statistics summary</returns>
    [HttpGet("statistics")]
    public async Task<ActionResult<object>> GetLogStatistics()
    {
        try
        {
            var allLogs = await logService.GetAllLogsAsync();
            var logsList = allLogs.ToList();

            var statistics = new
            {
                TotalLogs = logsList.Count,
                ActionCounts = logsList
                    .GroupBy(l => l.Action)
                    .ToDictionary(g => g.Key, g => g.Count()),
                RecentActivity = logsList
                    .OrderByDescending(l => l.Timestamp)
                    .Take(5)
                    .Select(l => new {
                        l.Action,
                        l.Timestamp,
                        l.Description
                    }),
                LogsByDay = logsList
                    .Where(l => l.Timestamp >= DateTime.Today.AddDays(-7))
                    .GroupBy(l => l.Timestamp.Date)
                    .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), g => g.Count())
            };

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving log statistics");
            return StatusCode(500, new { message = "An error occurred while retrieving log statistics" });
        }
    }

    /// <summary>
    /// Helper method to convert logs to DTOs with username lookup
    /// </summary>
    private async Task<IEnumerable<UserLogDto>> ConvertLogsToDto(IEnumerable<UserLog> logs)
    {
        var users = await userService.GetAllAsync();
        var userLookup = users.ToDictionary(u => u.Id, u => $"{u.Forename} {u.Surname}");

        return logs.Select(log => log.ToDto(
            userLookup.GetValueOrDefault(log.UserId, "Unknown User")
        ));
    }
}
