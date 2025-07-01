using System.Net;
using UserManagement.Common.DTOs;
using UserManagement.Common.DTOs.Contracts;

namespace UserManagement.Api.Integration.Tests;

public class UsersControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UsersControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_ShouldReturnOkWithUsers()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert - with debugging
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorDetails = await GetErrorDetailsAsync(response);
            throw new Exception($"Expected OK but got: {errorDetails}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetUser_WithValidId_ShouldReturnUser()
    {
        // Arrange - first create a user to ensure we have one
        var createUserDto = new CreateUserDto(
            "Test",
            "User", 
            "test.user@example.com",
            new DateTime(1990, 1, 1),
            true
        );

        var createResponse = await _client.PostAsJsonAsync("/api/users", createUserDto);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();

        // Act
        var response = await _client.GetAsync($"/api/users/{createdUser!.Id}");

        // Assert - with debugging
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorDetails = await GetErrorDetailsAsync(response);
            throw new Exception($"Expected OK but got: {errorDetails}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var user = await response.Content.ReadFromJsonAsync<UserDetailDto>();
        user.Should().NotBeNull();
        user!.Id.Should().Be(createdUser.Id);
        user.Email.Should().Be(createUserDto.Email);
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidUserId = 99999;

        // Act
        var response = await _client.GetAsync($"/api/users/{invalidUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateUser_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var newUser = new CreateUserDto(
            "Integration",
            "Test",
            "integration.test@example.com",
            new DateTime(1990, 1, 1),
            true
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", newUser);

        // Assert - with debugging
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorDetails = await GetErrorDetailsAsync(response);
            throw new Exception($"Expected Created but got: {errorDetails}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdUser = await response.Content.ReadFromJsonAsync<UserDto>();
        createdUser.Should().NotBeNull();
        createdUser!.Email.Should().Be(newUser.Email);
        createdUser.Forename.Should().Be(newUser.Forename);
        createdUser.Surname.Should().Be(newUser.Surname);
        createdUser.IsActive.Should().Be(newUser.IsActive);
    }

    [Fact]
    public async Task CreateUser_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange - invalid data (empty forename and invalid email)
        var invalidUser = new
        {
            Forename = "",
            Surname = "Test",
            Email = "invalid-email",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", invalidUser);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUser_WithValidData_ShouldReturnOk()
    {
        // Arrange - first create a user
        var newUser = new CreateUserDto(
            "Original",
            "User",
            "original.user@example.com",
            new DateTime(1990, 1, 1),
            true
        );

        var createResponse = await _client.PostAsJsonAsync("/api/users", newUser);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();

        var updateUser = new UpdateUserDto(
            "Updated",
            "User",
            "updated.user@example.com",
            new DateTime(1990, 1, 1),
            false
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/users/{createdUser!.Id}", updateUser);

        // Assert - with debugging
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorDetails = await GetErrorDetailsAsync(response);
            throw new Exception($"Expected OK but got: {errorDetails}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var updatedUser = await response.Content.ReadFromJsonAsync<UserDto>();
        updatedUser!.Forename.Should().Be("Updated");
        updatedUser.Email.Should().Be("updated.user@example.com");
        updatedUser.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUser_WithValidId_ShouldReturnNoContent()
    {
        // Arrange - first create a user to delete
        var newUser = new CreateUserDto(
            "ToDelete",
            "User",
            "todelete.user@example.com",
            new DateTime(1990, 1, 1),
            true
        );

        var createResponse = await _client.PostAsJsonAsync("/api/users", newUser);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/users/{createdUser!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify user is deleted
        var getResponse = await _client.GetAsync($"/api/users/{createdUser.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUsers_WithActiveFilter_ShouldReturnFilteredUsers()
    {
        // Arrange - create users with different active states
        var activeUser = new CreateUserDto(
            "Active",
            "User",
            "active.user@example.com",
            new DateTime(1990, 1, 1),
            true
        );

        var inactiveUser = new CreateUserDto(
            "Inactive",
            "User",
            "inactive.user@example.com",
            new DateTime(1990, 1, 1),
            false
        );

        await _client.PostAsJsonAsync("/api/users", activeUser);
        await _client.PostAsJsonAsync("/api/users", inactiveUser);

        // Act - filter for active users only
        var response = await _client.GetAsync("/api/users?isActive=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
        users.Should().NotBeNull();
        users.Should().OnlyContain(u => u.IsActive);
    }

    [Fact]
    public async Task UpdateUser_WithInvalidId_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidUserId = 99999;
        var updateUser = new UpdateUserDto(
            "Updated",
            "User",
            "updated@example.com",
            new DateTime(1990, 1, 1),
            true
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/users/{invalidUserId}", updateUser);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteUser_WithInvalidId_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidUserId = 99999;

        // Act
        var response = await _client.DeleteAsync($"/api/users/{invalidUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<string> GetErrorDetailsAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return $"Status: {response.StatusCode}, Content: {content}";
    }
}
