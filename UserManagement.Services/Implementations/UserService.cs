using System.Collections.Generic;
using System.Linq;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Constants;
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
    public IEnumerable<User> FilterByActive(bool isActive)
    {
        return _dataAccess.GetAll<User>().Where(x => x.IsActive == isActive);
    }

    public IEnumerable<User> GetAll() => _dataAccess.GetAll<User>();

    /// <summary>
    /// Get a user by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public User? GetById(long id)
    {
        var user = _dataAccess.GetAll<User>().FirstOrDefault(x => x.Id == id);
        
        // Log the view action
        if (user != null)
        {
            _userLogService.LogAction(
                userId: user.Id,
                action: UserActions.Viewed,
                description: $"User profile viewed for {user.Forename} {user.Surname}",
                details: $"Email: {user.Email}, Status: {(user.IsActive ? "Active" : "Inactive")}"
            );
        }
        
        return user;
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="user"></param>
    public void Create(User user)
    {
        _dataAccess.Create(user);
        
        // Log the creation
        _userLogService.LogAction(
            userId: user.Id,
            action: UserActions.Created,
            description: $"New user created: {user.Forename} {user.Surname}",
            details: $"Email: {user.Email}, DateOfBirth: {user.DateOfBirth:yyyy-MM-dd}, Active: {user.IsActive}"
        );
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="user"></param>
    public void Update(User user)
    {
        var originalUser = _dataAccess.GetAll<User>().FirstOrDefault(u => u.Id == user.Id);
        
        _dataAccess.Update(user);
        
        // Log the update with details about what changed
        var changes = new List<string>();
        if (originalUser != null)
        {
            if (originalUser.Forename != user.Forename) changes.Add($"Forename: '{originalUser.Forename}' -> '{user.Forename}'");
            if (originalUser.Surname != user.Surname) changes.Add($"Surname: '{originalUser.Surname}' -> '{user.Surname}'");
            if (originalUser.Email != user.Email) changes.Add($"Email: '{originalUser.Email}' -> '{user.Email}'");
            if (originalUser.DateOfBirth != user.DateOfBirth) changes.Add($"DateOfBirth: '{originalUser.DateOfBirth:yyyy-MM-dd}' -> '{user.DateOfBirth:yyyy-MM-dd}'");
            if (originalUser.IsActive != user.IsActive) changes.Add($"Status: '{(originalUser.IsActive ? "Active" : "Inactive")}' -> '{(user.IsActive ? "Active" : "Inactive")}'");
        }
        
        _userLogService.LogAction(
            userId: user.Id,
            action: UserActions.Updated,
            description: $"User updated: {user.Forename} {user.Surname}",
            details: changes.Any() ? string.Join(", ", changes) : "No specific changes detected"
        );
    }

    /// <summary>
    /// Delete a user by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns>True if user was found and deleted, false otherwise</returns>
    public bool Delete(long id)
    {
        var user = GetById(id);
        if (user == null)
            return false;

        // Log the deletion before actually deleting
        _userLogService.LogAction(
            userId: user.Id,
            action: UserActions.Deleted,
            description: $"User deleted: {user.Forename} {user.Surname}",
            details: $"Email: {user.Email}, DateOfBirth: {user.DateOfBirth:yyyy-MM-dd}, Status: {(user.IsActive ? "Active" : "Inactive")}"
        );

        _dataAccess.Delete(user);
        return true;
    }
}
