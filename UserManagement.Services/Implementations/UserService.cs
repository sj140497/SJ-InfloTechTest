using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Services.Domain.Implementations;

public class UserService : IUserService
{
    private readonly IDataContext _dataAccess;
    private readonly IUserLogService _userLogService;

    public UserService(IDataContext dataAccess, IUserLogService userLogService)
    {
        _dataAccess = dataAccess;
        _userLogService = userLogService;
    }

    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    public async Task<IEnumerable<User>> FilterByActiveAsync(bool isActive) =>
        await _dataAccess.GetAll<User>().Where(x => x.IsActive == isActive).ToListAsync();

    public async Task<IEnumerable<User>> GetAllAsync()
        => await _dataAccess.GetAll<User>().ToListAsync();


    /// <summary>
    /// Get a user by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Result<User>> GetByIdAsync(long id)
    {
        try
        {
            var user = await _dataAccess.GetByIdAsync<User>(id);

            if (user == null)
            {
                return Result.Fail<User>($"User with ID {id} not found");
            }
            
            // Log the view action
            await _userLogService.LogActionAsync(
                userId: user.Id,
                action: Constants.UserActions.Viewed,
                description: $"User profile viewed for {user.Forename} {user.Surname}",
                details: $"Email: {user.Email}, Status: {(user.IsActive ? "Active" : "Inactive")}"
            );
            
            return Result.Ok(user);
        }
        catch (System.Exception ex)
        {
            return Result.Fail<User>($"Error retrieving user: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="user"></param>
    public async Task<Result<User>> CreateAsync(User user)
    {
        try
        {
            // Check for duplicate email
            var existingUser = await _dataAccess.GetAll<User>()
                .FirstOrDefaultAsync(u => u.Email.Equals(user.Email, System.StringComparison.OrdinalIgnoreCase));
            
            if (existingUser != null)
                return Result.Fail<User>("Email address is already in use");

            await _dataAccess.CreateAsync(user);
            
            // Log the creation
            await _userLogService.LogActionAsync(
                userId: user.Id,
                action: Constants.UserActions.Created,
                description: $"New user created: {user.Forename} {user.Surname}",
                details: $"Email: {user.Email}, DateOfBirth: {user.DateOfBirth:yyyy-MM-dd}, Active: {user.IsActive}"
            );
            
            return Result.Ok(user);
        }
        catch (System.Exception ex)
        {
            return Result.Fail<User>($"Error creating user: {ex.Message}");
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="user"></param>
    public async Task<Result<User>> UpdateAsync(User user)
    {
        try
        {
            var originalUser = await _dataAccess.GetByIdAsync<User>(user.Id);
            if (originalUser == null)
                return Result.Fail<User>($"User with ID {user.Id} not found");

            // Check for duplicate email (excluding current user)
            var duplicateEmail = await _dataAccess.GetAll<User>()
                .FirstOrDefaultAsync(u =>
                    u.Email.Equals(user.Email, System.StringComparison.OrdinalIgnoreCase) && u.Id != user.Id);
            
            if (duplicateEmail != null)
                return Result.Fail<User>("Email address is already in use");

            // Capture original values for logging before updating
            var changes = new List<string>();
            if (originalUser.Forename != user.Forename) changes.Add($"Forename: '{originalUser.Forename}' -> '{user.Forename}'");
            if (originalUser.Surname != user.Surname) changes.Add($"Surname: '{originalUser.Surname}' -> '{user.Surname}'");
            if (originalUser.Email != user.Email) changes.Add($"Email: '{originalUser.Email}' -> '{user.Email}'");
            if (originalUser.DateOfBirth != user.DateOfBirth) changes.Add($"DateOfBirth: '{originalUser.DateOfBirth:yyyy-MM-dd}' -> '{user.DateOfBirth:yyyy-MM-dd}'");
            if (originalUser.IsActive != user.IsActive) changes.Add($"Status: '{(originalUser.IsActive ? "Active" : "Inactive")}' -> '{(user.IsActive ? "Active" : "Inactive")}'");

            // Update the tracked entity to avoid tracking conflicts
            originalUser.Forename = user.Forename;
            originalUser.Surname = user.Surname;
            originalUser.Email = user.Email;
            originalUser.DateOfBirth = user.DateOfBirth;
            originalUser.IsActive = user.IsActive;

            await _dataAccess.UpdateAsync(originalUser);
            
            await _userLogService.LogActionAsync(
                userId: originalUser.Id,
                action: Constants.UserActions.Updated,
                description: $"User updated: {originalUser.Forename} {originalUser.Surname}",
                details: changes.Any() ? string.Join(", ", changes) : "No specific changes detected"
            );
            
            return Result.Ok(originalUser);
        }
        catch (System.Exception ex)
        {
            return Result.Fail<User>($"Error updating user: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a user by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result> DeleteAsync(long id)
    {
        try
        {
            var user = await _dataAccess.GetByIdAsync<User>(id);
            if (user == null)
                return Result.Fail($"User with ID {id} not found");

            // Log the deletion before actually deleting
            await _userLogService.LogActionAsync(
                userId: user.Id,
                action: Constants.UserActions.Deleted,
                description: $"User deleted: {user.Forename} {user.Surname}",
                details: $"Email: {user.Email}, DateOfBirth: {user.DateOfBirth:yyyy-MM-dd}, Status: {(user.IsActive ? "Active" : "Inactive")}"
            );

            await _dataAccess.DeleteAsync(user);
            return Result.Ok();
        }
        catch (System.Exception ex)
        {
            return Result.Fail($"Error deleting user: {ex.Message}");
        }
    }
}
