using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;
using UserManagement.Web.Models.Logs;

namespace UserManagement.Web.Controllers;

[Route("users")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly IUserLogService _userLogService;
    
    public UsersController(IUserService userService, IUserLogService userLogService) 
    {
        _userService = userService;
        _userLogService = userLogService;
    }

    [HttpGet("list")]
    public ViewResult List(bool? isActive = null)
    {
        var users = isActive.HasValue 
            ? _userService.FilterByActive(isActive.Value)
            : _userService.GetAll();
            
        var items = users.Select(p => new UserListItemViewModel
        {
            Id = p.Id,
            Forename = p.Forename,
            Surname = p.Surname,
            Email = p.Email,
            DateOfBirth = p.DateOfBirth,
            IsActive = p.IsActive
        });

        var model = new UserListViewModel
        {
            Items = items.ToList()
        };

        return View(model);
    }

    [HttpGet("create")]
    public ViewResult Create()
    {
        var model = new CreateUserViewModel();
        return View(model);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new User
        {
            Forename = model.Forename,
            Surname = model.Surname,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth,
            IsActive = model.IsActive
        };

        _userService.Create(user);

        TempData["SuccessMessage"] = "User created successfully!";
        return RedirectToAction("List");
    }

    [HttpGet("edit/{id:long}")]
    public IActionResult Edit(long id)
    {
        var user = _userService.GetById(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("List");
        }

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            IsActive = user.IsActive
        };

        return View(model);
    }

    [HttpPost("edit/{id:long}")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(long id, EditUserViewModel model)
    {
        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid UserID.";
            return RedirectToAction("List");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingUser = _userService.GetById(id);
        if (existingUser == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("List");
        }

        // Update the existing user with new values
        existingUser.Forename = model.Forename;
        existingUser.Surname = model.Surname;
        existingUser.Email = model.Email;
        existingUser.DateOfBirth = model.DateOfBirth;
        existingUser.IsActive = model.IsActive;

        _userService.Update(existingUser);

        TempData["SuccessMessage"] = $"User '{existingUser.Forename} {existingUser.Surname}' updated successfully!";
        return RedirectToAction("List");
    }

    [HttpGet("details/{id:long}")]
    public IActionResult Details(long id)
    {
        var user = _userService.GetById(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("List");
        }

        // Get recent logs for this user (last 10)
        var recentLogs = _userLogService.GetLogsByUserId(id)
            .Take(10)
            .Select(l => new UserLogListItemViewModel
            {
                Id = l.Id,
                UserId = l.UserId,
                UserName = $"{user.Forename} {user.Surname}",
                Action = l.Action,
                Details = l.Details ?? string.Empty,
                Timestamp = l.Timestamp
            })
            .ToList();

        var model = new UserDetailsViewModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            IsActive = user.IsActive,
            RecentLogs = recentLogs
        };

        return View(model);
    }

    [HttpGet("delete/{id:long}")]
    public IActionResult Delete(long id)
    {
        var user = _userService.GetById(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("List");
        }

        var model = new UserDetailsViewModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            IsActive = user.IsActive
        };

        return View(model);
    }

    [HttpPost("delete/{id:long}")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(long id)
    {
        var user = _userService.GetById(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("List");
        }

        var userName = $"{user.Forename} {user.Surname}";
        var wasDeleted = _userService.Delete(id);

        if (wasDeleted)
        {
            TempData["SuccessMessage"] = $"User '{userName}' has been successfully deleted.";
        }
        else
        {
            TempData["ErrorMessage"] = $"Failed to delete user '{userName}'. Please try again.";
        }

        return RedirectToAction("List");
    }
}
