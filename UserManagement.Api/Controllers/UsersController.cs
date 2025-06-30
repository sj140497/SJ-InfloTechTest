using Microsoft.AspNetCore.Mvc;
using UserManagement.Common.DTOs;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService, IUserLogService userLogService, ILogger<UsersController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers(bool? isActive = null)
    {
        try
        {
            var users = await userService.GetAll();
            return Ok(users);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, "Internal server error");
        }
    }
}
