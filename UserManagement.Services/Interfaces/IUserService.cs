using System.Collections.Generic;
using UserManagement.Models;

namespace UserManagement.Services.Domain.Interfaces;

public interface IUserService 
{
    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    IEnumerable<User> FilterByActive(bool isActive);
    IEnumerable<User> GetAll();
    
    /// <summary>
    /// Get a user by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    User? GetById(long id);
    
    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="user"></param>
    void Create(User user);
    
    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="user"></param>
    void Update(User user);
    
    /// <summary>
    /// Delete a user by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns>True if user was found and deleted, false otherwise</returns>
    bool Delete(long id);
}
