using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Api.Validators;

namespace UserManagement.Api.Integration.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the registered DataContext
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DataContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add a test database with consistent naming for this factory instance
            services.AddDbContext<DataContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Register validators that the controller expects
            services.AddSingleton<CreateUserDtoValidator>();
            services.AddSingleton<UpdateUserDtoValidator>();

            // Replace System.Text.Json with Newtonsoft.Json to avoid PipeWriter issues
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });

            // Build the service provider and seed data if needed
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();
            context.Database.EnsureCreated();
        });
    }
}
