using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SkillMatrix.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SkillMatrixDbContext>
{
    public SkillMatrixDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SkillMatrixDbContext>();

        // Connection string for design-time migrations
        var connectionString = "Host=192.168.0.21;Database=MySkillList_NGE_DEV;Username=postgres;Password=@ll1@nceP@ss2o21;Maximum Pool Size=1000";

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public");
        });

        return new SkillMatrixDbContext(optionsBuilder.Options);
    }
}
