namespace UserManagement.BlazorClient.Services;

public class ApiException : Exception
{
    public int StatusCode { get; }
    public List<string> Errors { get; }

    public ApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
        Errors = new List<string> { message };
    }

    public ApiException(int statusCode, List<string> errors) : base(string.Join(", ", errors))
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}
