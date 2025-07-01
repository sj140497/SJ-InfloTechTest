using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Integration.Tests;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Services.Tests;

public class UserServiceIntegrationScenarioTests : IntegrationTestBase
{
    private readonly UserService _userService;

    public UserServiceIntegrationScenarioTests()
    {
        _userService = new UserService(DataContext, MockUserLogService.Object);
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

    [Fact]
    public async Task CRUD_FullWorkflow_ShouldWorkEndToEnd()
    {
        // Complete CRUD workflow test
        
        // CREATE
        var createUser = new User
        {
            Forename = "Workflow",
            Surname = "Test",
            Email = "workflow@example.com",
            DateOfBirth = new DateTime(1985, 6, 15),
            IsActive = true
        };
        
        var createResult = await _userService.CreateAsync(createUser);
        createResult.IsSuccess.Should().BeTrue();
        var userId = createResult.Value.Id;

        // READ
        var readResult = await _userService.GetByIdAsync(userId);
        readResult.IsSuccess.Should().BeTrue();
        readResult.Value.Email.Should().Be("workflow@example.com");

        // UPDATE
        var updateUser = readResult.Value;
        updateUser.Forename = "Updated";
        updateUser.Email = "updated@example.com";
        updateUser.IsActive = false;

        var updateResult = await _userService.UpdateAsync(updateUser);
        updateResult.IsSuccess.Should().BeTrue();
        updateResult.Value.Forename.Should().Be("Updated");
        updateResult.Value.Email.Should().Be("updated@example.com");
        updateResult.Value.IsActive.Should().BeFalse();

        // DELETE
        var deleteResult = await _userService.DeleteAsync(userId);
        deleteResult.IsSuccess.Should().BeTrue();

        // VERIFY DELETION
        var verifyResult = await _userService.GetByIdAsync(userId);
        verifyResult.IsFailed.Should().BeTrue();

        // Verify logging was called for each operation
        MockUserLogService.Verify(x => x.LogActionAsync(userId, "Created", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        MockUserLogService.Verify(x => x.LogActionAsync(userId, "Updated", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        MockUserLogService.Verify(x => x.LogActionAsync(userId, "Deleted", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}
