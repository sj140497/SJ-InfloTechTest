using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Integration.Tests;

public class UserServiceIntegrationTests : IntegrationTestBase
{
    private readonly UserService _userService;

    public UserServiceIntegrationTests()
    {
        _userService = new UserService(DataContext, MockUserLogService.Object);
    }

    [Fact]
    public async Task GetAllAsync_WithRealDatabase_ShouldReturnAllUsers()
    {
        // Arrange - Create test data explicitly
        await CreateTestUserAsync("John", "Doe", "john@example.com", true);
        await CreateTestUserAsync("Jane", "Smith", "jane@example.com", false);

        // Act
        var result = await _userService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Email == "john@example.com");
        result.Should().Contain(u => u.Email == "jane@example.com");
    }

    [Fact]
    public async Task CreateAsync_WithRealDatabase_ShouldCreateUserAndLogAction()
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

    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, 1)]
    public async Task FilterByActiveAsync_WithRealDatabase_ShouldReturnCorrectUsers(bool isActive, int expectedCount)
    {
        // Arrange - Create test data explicitly
        await CreateTestUserAsync("Active1", "User", "active1@example.com", true);
        await CreateTestUserAsync("Active2", "User", "active2@example.com", true);
        await CreateTestUserAsync("Inactive", "User", "inactive@example.com", false);

        // Act
        var result = await _userService.FilterByActiveAsync(isActive);

        // Assert
        result.Should().HaveCount(expectedCount);
        result.Should().OnlyContain(u => u.IsActive == isActive);
    }

    [Fact]
    public async Task Database_ShouldStartEmpty_WithoutSeedData()
    {
        // Act
        var result = await _userService.GetAllAsync();

        // Assert
        result.Should().BeEmpty("Database should start with no seed data for proper test isolation");
    }

    [Fact]
    public async Task Example_ExplicitTestData_ShouldBeReadableAndMaintainable()
    {
        // Arrange - Explicit test data (recommended for business logic tests)
        await CreateTestUserAsync("John", "Active", "john@test.com", isActive: true);
        await CreateTestUserAsync("Jane", "Inactive", "jane@test.com", isActive: false);

        // Act
        var activeUsers = await _userService.FilterByActiveAsync(true);

        // Assert
        activeUsers.Should().HaveCount(1);
        activeUsers.First().Forename.Should().Be("John");
    }
}
