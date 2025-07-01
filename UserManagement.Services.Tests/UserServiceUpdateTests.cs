using System;
using System.Threading.Tasks;
using UserManagement.Integration.Tests;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Services.Tests;

public class UserServiceUpdateTests : IntegrationTestBase
{
    private readonly UserService _userService;

    public UserServiceUpdateTests()
    {
        _userService = new UserService(DataContext, MockUserLogService.Object);
    }

    [Fact]
    public async Task UpdateAsync_WithValidUser_ShouldUpdateUserAndLogChanges()
    {
        // Arrange
        var originalUser = await CreateTestUserAsync("John", "Doe", "john@example.com", true);
        var updatedUser = new User
        {
            Id = originalUser.Id,
            Forename = "Johnny",
            Surname = "Updated",
            Email = "johnny.updated@example.com",
            DateOfBirth = new DateTime(1990, 5, 15),
            IsActive = false
        };

        // Act
        var result = await _userService.UpdateAsync(updatedUser);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Forename.Should().Be("Johnny");
        result.Value.Surname.Should().Be("Updated");
        result.Value.Email.Should().Be("johnny.updated@example.com");
        result.Value.IsActive.Should().BeFalse();

        // Verify the user was actually updated in database
        var userInDb = await _userService.GetByIdAsync(originalUser.Id);
        userInDb.Value.Forename.Should().Be("Johnny");

        // Verify logging was called for update action
        MockUserLogService.Verify(x => x.LogActionAsync(
            originalUser.Id,
            "Updated",
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentUser = new User
        {
            Id = 999L,
            Forename = "Non",
            Surname = "Existent",
            Email = "nonexistent@example.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsActive = true
        };

        // Act
        var result = await _userService.UpdateAsync(nonExistentUser);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("User with ID 999 not found"));
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        var user1 = await CreateTestUserAsync("User1", "Test", "user1@example.com", true);
        var user2 = await CreateTestUserAsync("User2", "Test", "user2@example.com", true);

        // Try to update user2 with user1's email
        var updatedUser = new User
        {
            Id = user2.Id,
            Forename = user2.Forename,
            Surname = user2.Surname,
            Email = "user1@example.com", // Duplicate email
            DateOfBirth = user2.DateOfBirth,
            IsActive = user2.IsActive
        };

        // Act
        var result = await _userService.UpdateAsync(updatedUser);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("Email address is already in use"));
    }

    [Fact]
    public async Task UpdateAsync_WithSameEmail_ShouldSucceed()
    {
        // Arrange
        var originalUser = await CreateTestUserAsync("John", "Doe", "john@example.com", true);
        var updatedUser = new User
        {
            Id = originalUser.Id,
            Forename = "Johnny", // Changed
            Surname = originalUser.Surname,
            Email = originalUser.Email, // Same email should be allowed
            DateOfBirth = originalUser.DateOfBirth,
            IsActive = originalUser.IsActive
        };

        // Act
        var result = await _userService.UpdateAsync(updatedUser);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Forename.Should().Be("Johnny");
        result.Value.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task UpdateAsync_ChangingActiveStatus_ShouldReflectInFiltering()
    {
        // Arrange
        var user = await CreateTestUserAsync("Status", "Change", "status@example.com", isActive: true);
        
        // Verify user appears in active filter
        var initialActiveUsers = await _userService.FilterByActiveAsync(true);
        initialActiveUsers.Should().Contain(u => u.Id == user.Id);

        // Act - Change status to inactive
        user.IsActive = false;
        var updateResult = await _userService.UpdateAsync(user);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        
        // Verify user now appears in inactive filter
        var activeUsers = await _userService.FilterByActiveAsync(true);
        var inactiveUsers = await _userService.FilterByActiveAsync(false);
        
        activeUsers.Should().NotContain(u => u.Id == user.Id);
        inactiveUsers.Should().Contain(u => u.Id == user.Id);
    }
}
