using FluentValidation;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserManagement.Api.Validators;
using UserManagement.Common.DTOs.Contracts;

namespace UserManagement.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        services.TryAddSingleton<IValidator<CreateUserDto>, CreateUserDtoValidator>();
        services.TryAddSingleton<IValidator<UpdateUserDto>, UpdateUserDtoValidator>();
        return services;
    }

    public static IServiceCollection RegisterCors(this IServiceCollection services)
    {
        return services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("https://localhost:7020")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }
}
