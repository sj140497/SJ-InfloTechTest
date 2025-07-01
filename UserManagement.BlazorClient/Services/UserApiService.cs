using System.Text.Json;
using UserManagement.Common.DTOs;
using UserManagement.Common.DTOs.Contracts;

namespace UserManagement.BlazorClient.Services;

public interface IUserApiService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync(bool? isActive = null);
    Task<UserDetailDto> GetUserAsync(long id);
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserDto> UpdateUserAsync(long id, UpdateUserDto updateUserDto);
    Task DeleteUserAsync(long id);
}

public class UserApiService : IUserApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public UserApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync(bool? isActive = null)
    {
        var url = isActive.HasValue ? $"api/users?isActive={isActive}" : "api/users";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<UserDto>>(json, _jsonOptions) ?? new List<UserDto>();
    }

    public async Task<UserDetailDto> GetUserAsync(long id)
    {
        var response = await _httpClient.GetAsync($"api/users/{id}");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<UserDetailDto>(json, _jsonOptions) 
               ?? throw new InvalidOperationException("Failed to deserialize user detail");
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        var json = JsonSerializer.Serialize(createUserDto, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("api/users", content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<UserDto>(responseJson, _jsonOptions)
               ?? throw new InvalidOperationException("Failed to deserialize created user");
    }

    public async Task<UserDto> UpdateUserAsync(long id, UpdateUserDto updateUserDto)
    {
        var json = JsonSerializer.Serialize(updateUserDto, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PutAsync($"api/users/{id}", content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<UserDto>(responseJson, _jsonOptions)
               ?? throw new InvalidOperationException("Failed to deserialize updated user");
    }

    public async Task DeleteUserAsync(long id)
    {
        var response = await _httpClient.DeleteAsync($"api/users/{id}");
        response.EnsureSuccessStatusCode();
    }
}
