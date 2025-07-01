using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.Web.Extensions;

public static class ResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);
            
        var error = result.Errors.First();
        
        // Map business errors to HTTP status codes
        return error.Message.ToLower() switch
        {
            var msg when msg.Contains("not found") => new NotFoundObjectResult(new { message = error.Message }),
            var msg when msg.Contains("already exists") || msg.Contains("already in use") => new ConflictObjectResult(new { message = error.Message }),
            var msg when msg.Contains("unauthorized") => new UnauthorizedObjectResult(new { message = error.Message }),
            _ => new BadRequestObjectResult(new { message = error.Message })
        };
    }
    
    public static ActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkResult();
            
        var error = result.Errors.First();
        
        return error.Message.ToLower() switch
        {
            var msg when msg.Contains("not found") => new NotFoundObjectResult(new { message = error.Message }),
            var msg when msg.Contains("already exists") || msg.Contains("already in use") => new ConflictObjectResult(new { message = error.Message }),
            _ => new BadRequestObjectResult(new { message = error.Message })
        };
    }
}
