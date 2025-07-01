using System.Text.Json;
using UserManagement.Common.DTOs;

namespace UserManagement.BlazorClient.Services;

public interface ILogApiService
{
    Task<IEnumerable<UserLogDto>> GetAllLogsAsync();
    Task<IEnumerable<UserLogDto>> GetUserLogsAsync(long userId);
}

public class LogApiService : ILogApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public LogApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IEnumerable<UserLogDto>> GetAllLogsAsync()
    {
        var response = await _httpClient.GetAsync("api/logs");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<UserLogDto>>(json, _jsonOptions) ?? new List<UserLogDto>();
    }

    public async Task<IEnumerable<UserLogDto>> GetUserLogsAsync(long userId)
    {
        var response = await _httpClient.GetAsync($"api/logs/user/{userId}");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<UserLogDto>>(json, _jsonOptions) ?? new List<UserLogDto>();
    }
}
