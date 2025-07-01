using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Logs;

namespace UserManagement.Web.Controllers;

[Route("logs")]
public class LogsController(IUserLogService userLogService, IUserService userService) : Controller
{
    [HttpGet]
    public async Task<ViewResult> List(long? userId = null, string? action = null)
    {
        var logs = await userLogService.GetAllLogsAsync();

        // Apply filters
        if (userId.HasValue)
        {
            logs = logs.Where(l => l.UserId == userId.Value);
        }

        if (!string.IsNullOrEmpty(action))
        {
            logs = logs.Where(l => l.Action.Contains(action, StringComparison.OrdinalIgnoreCase));
        }

        var users = await userService.GetAllAsync();
        var userLookup = users.ToDictionary(u => u.Id, u => $"{u.Forename} {u.Surname}");

        var items = logs.OrderByDescending(l => l.Timestamp)
            .Take(50) // Limit to 50 most recent logs for performance
            .Select(l => new UserLogListItemViewModel
            {
                Id = l.Id,
                UserId = l.UserId,
                UserName = userLookup.ContainsKey(l.UserId) ? userLookup[l.UserId] : "Unknown User",
                Action = l.Action,
                Details = l.Details ?? string.Empty,
                Timestamp = l.Timestamp
            });

        var model = new UserLogListViewModel
        {
            Items = items.ToList(),
            FilterUserId = userId?.ToString(),
            FilterAction = action,
            TotalCount = logs.Count()
        };

        return View(model);
    }

    [HttpGet("details/{id:long}")]
    public async Task<IActionResult> Details(long id)
    {
        var logResult = await userLogService.GetLogByIdAsync(id);
        if (logResult.IsFailed)
        {
            TempData["ErrorMessage"] = "Log entry not found.";
            return RedirectToAction(nameof(List));
        }

        var log = logResult.Value;
        var userResult = await userService.GetByIdAsync(log.UserId);
        var userName = userResult.IsSuccess ? $"{userResult.Value.Forename} {userResult.Value.Surname}" : "Unknown User";

        var model = new UserLogDetailsViewModel
        {
            Id = log.Id,
            UserId = log.UserId,
            UserName = userName,
            Action = log.Action,
            Details = log.Details ?? string.Empty,
            Timestamp = log.Timestamp
        };

        return View(model);
    }

    [HttpGet("user/{userId:long}")]
    public async Task<IActionResult> UserLogs(long userId)
    {
        var userResult = await userService.GetByIdAsync(userId);
        if (userResult.IsFailed)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(List));
        }

        var user = userResult.Value;
        var logs = await userLogService.GetUserLogsAsync(userId);
        var userName = $"{user.Forename} {user.Surname}";

        var items = logs.OrderByDescending(l => l.Timestamp)
            .Select(l => new UserLogListItemViewModel
            {
                Id = l.Id,
                UserId = l.UserId,
                UserName = userName,
                Action = l.Action,
                Details = l.Details ?? string.Empty,
                Timestamp = l.Timestamp
            });

        var model = new UserLogListViewModel
        {
            Items = items.ToList(),
            FilterUserId = userId.ToString(),
            TotalCount = logs.Count()
        };

        ViewData["Title"] = $"Activity Log for {userName}";
        ViewData["UserId"] = userId;
        ViewData["UserName"] = userName;

        return View("List", model);
    }
}
