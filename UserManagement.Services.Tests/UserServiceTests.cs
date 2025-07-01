using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using UserManagement.Integration.Tests;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Services.Tests;

public class UserServiceIntegrationTests : IntegrationTestBase
{
    private readonly UserService _userService;

    public UserServiceIntegrationTests()
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

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnUser()
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

        // User retrieval should not trigger any logging
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

    [Fact]
    public async Task UserManagement_TypicalBusinessScenario_ShouldWorkCorrectly()
    {
        // Simulate a typical business scenario:
        // 1. HR creates new employees
        // 2. Some employees become inactive
        // 3. Employee information gets updated
        // 4. Some employees leave (get deleted)
        
        // Phase 1: HR creates new employees
        var employees = new[]
        {
            new User { Forename = "Alice", Surname = "Johnson", Email = "alice.johnson@company.com", DateOfBirth = new DateTime(1990, 3, 15), IsActive = true },
            new User { Forename = "Bob", Surname = "Smith", Email = "bob.smith@company.com", DateOfBirth = new DateTime(1985, 7, 22), IsActive = true },
            new User { Forename = "Carol", Surname = "Williams", Email = "carol.williams@company.com", DateOfBirth = new DateTime(1992, 11, 8), IsActive = true },
            new User { Forename = "David", Surname = "Brown", Email = "david.brown@company.com", DateOfBirth = new DateTime(1988, 1, 30), IsActive = true }
        };

        var createdEmployees = new List<User>();
        foreach (var employee in employees)
        {
            var result = await _userService.CreateAsync(employee);
            result.IsSuccess.Should().BeTrue();
            createdEmployees.Add(result.Value);
        }

        // Verify all employees are active
        var activeEmployees = await _userService.FilterByActiveAsync(true);
        activeEmployees.Should().HaveCount(4);

        // Phase 2: Some employees go on leave (become inactive)
        var alice = createdEmployees.First(e => e.Forename == "Alice");
        alice.IsActive = false;
        var updateAlice = await _userService.UpdateAsync(alice);
        updateAlice.IsSuccess.Should().BeTrue();

        // Phase 3: Employee information updates (marriage, email change)
        var carol = createdEmployees.First(e => e.Forename == "Carol");
        carol.Surname = "Johnson-Williams"; // Marriage
        carol.Email = "carol.johnson-williams@company.com";
        var updateCarol = await _userService.UpdateAsync(carol);
        updateCarol.IsSuccess.Should().BeTrue();

        // Phase 4: Employee leaves company
        var david = createdEmployees.First(e => e.Forename == "David");
        var deleteDavid = await _userService.DeleteAsync(david.Id);
        deleteDavid.IsSuccess.Should().BeTrue();

        // Final verification
        var finalActiveEmployees = await _userService.FilterByActiveAsync(true);
        var finalInactiveEmployees = await _userService.FilterByActiveAsync(false);
        var allRemainingEmployees = await _userService.GetAllAsync();

        finalActiveEmployees.Should().HaveCount(2); // Bob and Carol
        finalInactiveEmployees.Should().HaveCount(1); // Alice
        allRemainingEmployees.Should().HaveCount(3); // David is deleted

        // Verify specific updates
        var updatedCarol = finalActiveEmployees.First(e => e.Forename == "Carol");
        updatedCarol.Surname.Should().Be("Johnson-Williams");
        updatedCarol.Email.Should().Be("carol.johnson-williams@company.com");

        // Verify logging happened for all operations
        MockUserLogService.Verify(x => x.LogActionAsync(It.IsAny<long>(), "Created", It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(4));
        MockUserLogService.Verify(x => x.LogActionAsync(It.IsAny<long>(), "Updated", It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        MockUserLogService.Verify(x => x.LogActionAsync(It.IsAny<long>(), "Deleted", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}
