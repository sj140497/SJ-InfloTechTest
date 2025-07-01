using System.Threading.Tasks;
using UserManagement.Integration.Tests;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Services.Tests;

public class UserServiceQueryTests : IntegrationTestBase
{
    private readonly UserService _userService;

    public UserServiceQueryTests()
    {
        _userService = new UserService(DataContext, MockUserLogService.Object);
    }

    [Fact]
    public async Task TestDatabase_ShouldStartEmpty_WithoutSeedData()
    {
        // Act
        var result = await _userService.GetAllAsync();

        // Assert
        result.Should().BeEmpty("Database should start with no seed data for proper test isolation");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
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

    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, 1)]
    public async Task FilterByActiveAsync_ShouldReturnCorrectUsers(bool isActive, int expectedCount)
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
    public async Task GetByIdAsync_WithValidId_ShouldReturnUserAndLogViewAction()
    {
        // Arrange
        var testUser = await CreateTestUserAsync("John", "Doe", "john@example.com", true);

        // Act
        var result = await _userService.GetByIdAsync(testUser.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(testUser.Id);
        result.Value.Email.Should().Be("john@example.com");

        // Verify logging was called for view action
        MockUserLogService.Verify(x => x.LogActionAsync(
            testUser.Id,
            "Viewed",
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnFailureResult()
    {
        // Arrange
        var nonExistentId = 999L;

        // Act
        var result = await _userService.GetByIdAsync(nonExistentId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains($"User with ID {nonExistentId} not found"));

        // Verify no logging was called
        MockUserLogService.Verify(x => x.LogActionAsync(
            It.IsAny<long>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task FilterByActiveAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Act
        var activeUsers = await _userService.FilterByActiveAsync(true);
        var inactiveUsers = await _userService.FilterByActiveAsync(false);

        // Assert
        activeUsers.Should().BeEmpty();
        inactiveUsers.Should().BeEmpty();
    }
}
