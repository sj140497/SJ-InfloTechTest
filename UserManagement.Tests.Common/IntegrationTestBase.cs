using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Integration.Tests;

public abstract class IntegrationTestBase : IDisposable
{
    protected readonly TestDataContext DataContext;
    protected readonly Mock<IUserLogService> MockUserLogService;

    protected IntegrationTestBase()
    {
        // Each test gets a completely isolated database
        var options = new DbContextOptionsBuilder<TestDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DataContext = new TestDataContext(options);
        MockUserLogService = new Mock<IUserLogService>();
        
        // Ensure clean database without seed data
        DataContext.Database.EnsureDeleted();
        DataContext.Database.EnsureCreated();
    }

    /// <summary>
    /// Helper method to create test users when needed
    /// </summary>
    protected async Task<User> CreateTestUserAsync(
        string forename = "Test",
        string surname = "User", 
        string email = "test@example.com",
        bool isActive = true)
    {
        var user = new User
        {
            Forename = forename,
            Surname = surname,
            Email = email,
            DateOfBirth = DateTime.Now.AddYears(-25),
            IsActive = isActive
        };

        await DataContext.CreateAsync(user);
        return user;
    }

    public virtual void Dispose()
    {
        DataContext.Dispose();
    }
}
