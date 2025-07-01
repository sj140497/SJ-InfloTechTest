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
        
        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponse(response);
        }
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<UserDto>>(json, _jsonOptions) ?? new List<UserDto>();
    }

    public async Task<UserDetailDto> GetUserAsync(long id)
    {
        var response = await _httpClient.GetAsync($"api/users/{id}");
        
        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponse(response);
        }
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<UserDetailDto>(json, _jsonOptions) 
               ?? throw new InvalidOperationException("Failed to deserialize user detail");
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        var json = JsonSerializer.Serialize(createUserDto, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("api/users", content);
        
        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponse(response);
        }
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<UserDto>(responseJson, _jsonOptions)
               ?? throw new InvalidOperationException("Failed to deserialize created user");
    }

    public async Task<UserDto> UpdateUserAsync(long id, UpdateUserDto updateUserDto)
    {
        var json = JsonSerializer.Serialize(updateUserDto, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PutAsync($"api/users/{id}", content);
        
        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponse(response);
        }
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<UserDto>(responseJson, _jsonOptions)
               ?? throw new InvalidOperationException("Failed to deserialize updated user");
    }

    public async Task DeleteUserAsync(long id)
    {
        var response = await _httpClient.DeleteAsync($"api/users/{id}");
        
        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponse(response);
        }
    }

    private async Task HandleErrorResponse(HttpResponseMessage response)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        var statusCode = (int)response.StatusCode;

        try
        {
            // Try to parse as array of strings (validation errors)
            var errorArray = JsonSerializer.Deserialize<string[]>(errorContent, _jsonOptions);
            if (errorArray != null && errorArray.Length > 0)
            {
                throw new ApiException(statusCode, errorArray.ToList());
            }
        }
        catch (JsonException)
        {
            // Not an array, try to parse as object with message property
            try
            {
                var errorObject = JsonSerializer.Deserialize<ErrorResponse>(errorContent, _jsonOptions);
                if (errorObject?.Message != null)
                {
                    throw new ApiException(statusCode, errorObject.Message);
                }
            }
            catch (JsonException)
            {
                // Fallback to raw content
                throw new ApiException(statusCode, string.IsNullOrEmpty(errorContent) ? "An error occurred" : errorContent);
            }
        }

        // Fallback if no specific error format matched
        throw new ApiException(statusCode, $"Request failed with status {response.StatusCode}");
    }

    private class ErrorResponse
    {
        public string? Message { get; set; }
    }
}
