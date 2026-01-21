using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SkillMatrix.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SkillMatrixDbContext>
{
    public SkillMatrixDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SkillMatrixDbContext>();

        // Connection string for design-time migrations
        // Read from environment variable or use a default for local development
        var connectionString = Environment.GetEnvironmentVariable("SKILLMATRIX_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "Connection string not found. Set SKILLMATRIX_CONNECTION_STRING environment variable. " +
                "Example: Host=localhost;Database=SkillMatrix;Username=postgres;Password=yourpassword");

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public");
        });

        return new SkillMatrixDbContext(optionsBuilder.Options);
    }
}
