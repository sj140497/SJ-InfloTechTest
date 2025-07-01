using System;
using System.Threading.Tasks;
using UserManagement.Integration.Tests;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Services.Tests;

public class UserServiceCreateTests : IntegrationTestBase
{
    private readonly UserService _userService;

    public UserServiceCreateTests()
    {
        _userService = new UserService(DataContext, MockUserLogService.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateUserAndLogAction()
    {
        // Arrange
        var newUser = new User
        {
            Forename = "Test",
            Surname = "User",
            Email = "test@example.com",
            DateOfBirth = new DateTime(1995, 3, 10),
            IsActive = true
        };

        // Act
        var result = await _userService.CreateAsync(newUser);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var allUsers = await _userService.GetAllAsync();
        allUsers.Should().Contain(u => u.Email == "test@example.com");

        // Verify logging was called
        MockUserLogService.Verify(x => x.LogActionAsync(
            It.IsAny<long>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ShouldFail()
    {
        // Arrange
        var user1 = new User
        {
            Forename = "First",
            Surname = "User",
            Email = "duplicate@example.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsActive = true
        };

        var user2 = new User
        {
            Forename = "Second",
            Surname = "User",
            Email = "duplicate@example.com", // Same email
            DateOfBirth = new DateTime(1995, 1, 1),
            IsActive = true
        };

        await _userService.CreateAsync(user1);

        // Act
        var result = await _userService.CreateAsync(user2);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("Email address is already in use"));
    }
}
