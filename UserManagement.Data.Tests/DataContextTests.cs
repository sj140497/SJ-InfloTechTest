using System;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Integration.Tests;
using UserManagement.Models;

namespace UserManagement.Data.Tests;

public class DataContextIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateAsync_ShouldAddEntityToDatabase()
    {
        // Arrange
        var user = new User
        {
            Forename = "Integration",
            Surname = "Test",
            Email = "integration.test@example.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsActive = true
        };

        // Act
        await DataContext.CreateAsync(user);

        // Assert
        var users = await DataContext.GetAll<User>().ToListAsync();
        users.Should().HaveCountGreaterThan(0);
        users.Should().Contain(u => u.Email == "integration.test@example.com");
        
        var savedUser = users.First(u => u.Email == "integration.test@example.com");
        savedUser.Id.Should().BeGreaterThan(0); // Should have been assigned an ID
        savedUser.Forename.Should().Be("Integration");
        savedUser.Surname.Should().Be("Test");
    }

    [Fact]
    public async Task GetAll_WhenNewEntityAdded_MustIncludeNewEntity()
    {
        // Arrange
        var entity = new User
        {
            Forename = "Brand New",
            Surname = "User",
            Email = "brandnewuser@example.com",
            DateOfBirth = new DateTime(1995, 5, 15),
            IsActive = true
        };
        await DataContext.CreateAsync(entity);

        // Act
        var result = await DataContext.GetAll<User>().ToListAsync();

        // Assert
        result.Should().Contain(s => s.Email == entity.Email)
            .Which.Should().BeEquivalentTo(entity, options => options.Excluding(u => u.Id));
    }

    [Fact]
    public async Task GetAll_WhenDeleted_MustNotIncludeDeletedEntity()
    {
        // Arrange - First create a user to delete
        var userToDelete = new User
        {
            Forename = "To Be",
            Surname = "Deleted",
            Email = "tobedeleted@example.com",
            DateOfBirth = new DateTime(1980, 12, 25),
            IsActive = true
        };
        await DataContext.CreateAsync(userToDelete);
        
        // Verify it exists
        var usersBeforeDelete = await DataContext.GetAll<User>().ToListAsync();
        usersBeforeDelete.Should().Contain(u => u.Email == "tobedeleted@example.com");
        
        var entityToDelete = usersBeforeDelete.First(u => u.Email == "tobedeleted@example.com");

        // Act - Delete the entity
        await DataContext.DeleteAsync(entityToDelete);

        // Assert
        var result = await DataContext.GetAll<User>().ToListAsync();
        result.Should().NotContain(s => s.Email == entityToDelete.Email);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingEntity()
    {
        // Arrange - Create a user first
        var originalUser = new User
        {
            Forename = "Original",
            Surname = "Name",
            Email = "original@example.com",
            DateOfBirth = new DateTime(1985, 3, 10),
            IsActive = true
        };
        await DataContext.CreateAsync(originalUser);

        // Get the saved user (with ID)
        var savedUser = await DataContext.GetAll<User>()
            .FirstAsync(u => u.Email == "original@example.com");

        // Modify the user
        savedUser.Forename = "Updated";
        savedUser.Surname = "User";
        savedUser.IsActive = false;

        // Act
        await DataContext.UpdateAsync(savedUser);

        // Assert
        var updatedUser = await DataContext.GetByIdAsync<User>(savedUser.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.Forename.Should().Be("Updated");
        updatedUser.Surname.Should().Be("User");
        updatedUser.IsActive.Should().BeFalse();
        updatedUser.Email.Should().Be("original@example.com"); // Email unchanged
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnEntity()
    {
        // Arrange
        var user = new User
        {
            Forename = "Find",
            Surname = "Me",
            Email = "findme@example.com",
            DateOfBirth = new DateTime(1992, 7, 20),
            IsActive = true
        };
        await DataContext.CreateAsync(user);

        var savedUser = await DataContext.GetAll<User>()
            .FirstAsync(u => u.Email == "findme@example.com");

        // Act
        var result = await DataContext.GetByIdAsync<User>(savedUser.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(savedUser.Id);
        result.Email.Should().Be("findme@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await DataContext.GetByIdAsync<User>(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_ShouldReturnQueryableInterface()
    {
        // Arrange
        var user1 = new User { Forename = "User", Surname = "One", Email = "user1@example.com", DateOfBirth = DateTime.Now.AddYears(-25), IsActive = true };
        var user2 = new User { Forename = "User", Surname = "Two", Email = "user2@example.com", DateOfBirth = DateTime.Now.AddYears(-30), IsActive = false };
        
        await DataContext.CreateAsync(user1);
        await DataContext.CreateAsync(user2);

        // Act
        var queryable = DataContext.GetAll<User>();
        var activeUsers = await queryable.Where(u => u.IsActive).ToListAsync();
        var allUsers = await queryable.ToListAsync();

        // Assert
        queryable.Should().BeAssignableTo<IQueryable<User>>();
        activeUsers.Should().HaveCount(1);
        activeUsers.Single().Email.Should().Be("user1@example.com");
        allUsers.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
