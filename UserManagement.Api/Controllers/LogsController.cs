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
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <returns>Paginated list of logs</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<UserLogDto>>> GetAllLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100; // Limit max page size for performance

            var pagedLogs = await logService.GetAllLogsAsync(page, pageSize);
            
            var logDtos = await ConvertLogsToDto(pagedLogs.Items);

            var result = new PagedResultDto<UserLogDto>
            {
                Items = logDtos,
                CurrentPage = pagedLogs.CurrentPage,
                PageSize = pagedLogs.PageSize,
                TotalCount = pagedLogs.TotalCount
            };

            return Ok(result);
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
