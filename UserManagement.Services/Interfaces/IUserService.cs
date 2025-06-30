using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using UserManagement.Models;

namespace UserManagement.Services.Domain.Interfaces;

public interface IUserService 
{
    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    Task<IEnumerable<User>> FilterByActiveAsync(bool isActive);
    Task<IEnumerable<User>> GetAllAsync();
    
    /// <summary>
    /// Get a user by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Result<User>> GetByIdAsync(long id);
    
    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="user"></param>
    Task<Result<User>> CreateAsync(User user);
    
    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="user"></param>
    Task<Result<User>> UpdateAsync(User user);
    
    /// <summary>
    /// Delete a user by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> DeleteAsync(long id);
}
