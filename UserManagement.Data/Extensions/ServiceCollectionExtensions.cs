using Microsoft.EntityFrameworkCore;
using UserManagement.Data;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services)
    {
        services.AddDbContext<DataContext>(options =>
            options.UseInMemoryDatabase("UserManagement.Data.DataContext"));
        
        services.AddScoped<IDataContext>(provider => provider.GetRequiredService<DataContext>());
        
        return services;
    }
}
