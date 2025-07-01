using UserManagement.Data;
using UserManagement.Models;

namespace UserManagement.Integration.Tests;

/// <summary>
/// Test-specific DataContext that doesn't include seed data
/// </summary>
public class TestDataContext : DbContext, IDataContext
{
    public TestDataContext(DbContextOptions<TestDataContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    // No OnModelCreating override = no seed data

    public DbSet<User>? Users { get; set; }
    public DbSet<UserLog>? UserLogs { get; set; }

    public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
        => base.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync<TEntity>(long id) where TEntity : class
        => await base.Set<TEntity>().FindAsync(id);

    public async Task CreateAsync<TEntity>(TEntity entity) where TEntity : class
    {
        base.Add(entity);
        await SaveChangesAsync();
    }

    public async Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class
    {
        base.Update(entity);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class
    {
        base.Remove(entity);
        await SaveChangesAsync();
    }
}
