using System.Threading.Tasks;
using UserManagement.Integration.Tests;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Services.Tests;

public class UserServiceDeleteTests : IntegrationTestBase
{
    private readonly UserService _userService;

    public UserServiceDeleteTests()
    {
        _userService = new UserService(DataContext, MockUserLogService.Object);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteUserAndLogAction()
    {
        // Arrange
        var testUser = await CreateTestUserAsync("John", "Doe", "john@example.com", true);
        var userId = testUser.Id;

        // Act
        var result = await _userService.DeleteAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify user is deleted
        var deletedUser = await _userService.GetByIdAsync(userId);
        deletedUser.IsFailed.Should().BeTrue();

        // Verify all users no longer contains the deleted user
        var allUsers = await _userService.GetAllAsync();
        allUsers.Should().NotContain(u => u.Id == userId);

        // Verify logging was called for delete action
        MockUserLogService.Verify(x => x.LogActionAsync(
            userId,
            "Deleted",
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentId = 999L;

        // Act
        var result = await _userService.DeleteAsync(nonExistentId);

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
}
