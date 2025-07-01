using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Validators;
using UserManagement.Common.DTOs;
using UserManagement.Common.DTOs.Contracts;
using UserManagement.Common.Extensions;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(
    IUserService userService,
    IUserLogService logService,
    CreateUserDtoValidator createUserDtoValidator,
    UpdateUserDtoValidator updateUserDtoValidator,
    ILogger<UsersController> logger)
    : ControllerBase
{
    /// <summary>
    /// Get all users or filter by active status
    /// </summary>
    /// <param name="isActive">Optional filter by active status</param>
    /// <returns>List of users</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers([FromQuery] bool? isActive = null)
    {
        try
        {
            var users = isActive.HasValue 
                ? await userService.FilterByActiveAsync(isActive.Value)
                : await userService.GetAllAsync();

            return Ok(users.ToDtos());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, new { message = "An error occurred while retrieving users" });
        }
    }

    /// <summary>
    /// Get a specific user by ID, with logs
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserDetailDto>> GetUser(long id)
    {
        try
        {
            var user = await userService.GetByIdAsync(id);
            if (user.IsFailed)
            {
                return NotFound(new { message = user.Errors.FirstOrDefault()?.Message ?? "User not found" });
            }

            var logs = await logService.GetUserLogsAsync(id);

            //Keeping the User and Log entities separate for responsibility and performance concerns
            return Ok(user.Value.ToDetailDto(logs));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the user" });
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="createUserDto">User creation object</param>
    /// <returns>Created user</returns>
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            var validationResult = await createUserDtoValidator.ValidateAsync(createUserDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var user = createUserDto.ToEntity();
            var result = await userService.CreateAsync(user);
            
            if (result.IsSuccess)
            {
                var userDto = result.Value.ToDto();
                return CreatedAtAction(nameof(GetUser), new { id = userDto.Id }, userDto);
            }
            
            return BadRequest(new { message = result.Errors.FirstOrDefault()?.Message ?? "Failed to create user" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { message = "An error occurred while creating the user" });
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="updateUserDto">User update data</param>
    /// <returns>Updated user</returns>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<UserDto>> UpdateUser(long id, [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            var validationResult = await updateUserDtoValidator.ValidateAsync(updateUserDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var user = updateUserDto.ToEntity(id);
            var result = await userService.UpdateAsync(user);
            
            if (result.IsSuccess)
            {
                return Ok(result.Value.ToDto());
            }
            
            return BadRequest(new { message = result.Errors.FirstOrDefault()?.Message ?? "Failed to update user" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the user" });
        }
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id:long}")]
    public async Task<ActionResult> DeleteUser(long id)
    {
        try
        {
            var result = await userService.DeleteAsync(id);
            
            if (result.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(new { message = result.Errors.FirstOrDefault()?.Message ?? "Failed to delete user" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the user" });
        }
    }
}
