using System;
using System.Linq;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Logs;

namespace UserManagement.Web.Controllers;

[Route("logs")]
public class LogsController : Controller
{
    private readonly IUserLogService _userLogService;
    private readonly IUserService _userService;

    public LogsController(IUserLogService userLogService, IUserService userService)
    {
        _userLogService = userLogService;
        _userService = userService;
    }

    [HttpGet]
    public ViewResult List(long? userId = null, string? action = null)
    {
        var logs = _userLogService.GetAllLogs();

        // Apply filters
        if (userId.HasValue)
        {
            logs = logs.Where(l => l.UserId == userId.Value);
        }

        if (!string.IsNullOrEmpty(action))
        {
            logs = logs.Where(l => l.Action.Contains(action, StringComparison.OrdinalIgnoreCase));
        }

        var users = _userService.GetAll().ToDictionary(u => u.Id, u => $"{u.Forename} {u.Surname}");

        var items = logs.OrderByDescending(l => l.Timestamp)
            .Take(50) // Limit to 50 most recent logs for performance
            .Select(l => new UserLogListItemViewModel
            {
                Id = l.Id,
                UserId = l.UserId,
                UserName = users.ContainsKey(l.UserId) ? users[l.UserId] : "Unknown User",
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
    public IActionResult Details(long id)
    {
        var log = _userLogService.GetLogById(id);
        if (log == null)
        {
            TempData["ErrorMessage"] = "Log entry not found.";
            return RedirectToAction(nameof(List));
        }

        var user = _userService.GetById(log.UserId);
        var userName = user != null ? $"{user.Forename} {user.Surname}" : "Unknown User";

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
    public IActionResult UserLogs(long userId)
    {
        var user = _userService.GetById(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(List));
        }

        var logs = _userLogService.GetLogsByUserId(userId);
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
