// Verification script - run with: dotnet run verify
using Microsoft.EntityFrameworkCore;
using SkillMatrix.Infrastructure.Persistence;

public static class VerifyDatabase
{
    public static async Task RunAsync()
    {
        // Read connection string from environment variable
        var connectionString = Environment.GetEnvironmentVariable("SKILLMATRIX_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "Connection string not found. Set SKILLMATRIX_CONNECTION_STRING environment variable.");

        var optionsBuilder = new DbContextOptionsBuilder<SkillMatrixDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        using var context = new SkillMatrixDbContext(optionsBuilder.Options);

        Console.WriteLine("=== Database Verification ===\n");

        // Skill Domains
        Console.WriteLine("--- Skill Domains ---");
        var domains = await context.SkillDomains.OrderBy(d => d.DisplayOrder).ToListAsync();
        foreach (var d in domains)
        {
            Console.WriteLine($"  [{d.Code}] {d.Name}");
        }

        // Skills summary by category
        Console.WriteLine("\n--- Skills by Subcategory ---");
        var skills = await context.Skills
            .Include(s => s.Subcategory)
            .OrderBy(s => s.Subcategory.DisplayOrder)
            .ThenBy(s => s.DisplayOrder)
            .ToListAsync();

        var grouped = skills.GroupBy(s => s.Subcategory.Name);
        foreach (var g in grouped)
        {
            Console.WriteLine($"  {g.Key}:");
            foreach (var s in g)
            {
                Console.WriteLine($"    - [{s.Code}] {s.Name} ({s.Category}, {s.SkillType})");
            }
        }

        // Teams
        Console.WriteLine("\n--- Teams ---");
        var teams = await context.Teams.ToListAsync();
        foreach (var t in teams)
        {
            Console.WriteLine($"  - {t.Name}");
        }

        // Job Roles
        Console.WriteLine("\n--- Job Roles (Career Ladder) ---");
        var roles = await context.JobRoles.OrderBy(r => r.LevelInHierarchy).ThenBy(r => r.Code).ToListAsync();
        foreach (var r in roles)
        {
            var level = r.LevelInHierarchy switch
            {
                1 => "Junior",
                2 => "Mid",
                3 => "Senior",
                4 => "Lead",
                _ => "Other"
            };
            Console.WriteLine($"  [{r.Code}] {r.Name} (Level {r.LevelInHierarchy}: {level})");
        }

        // Role Skill Requirements sample
        Console.WriteLine("\n--- Sample Role Skill Requirements (Senior BE) ---");
        var seniorBeId = roles.FirstOrDefault(r => r.Code == "BE-SR")?.Id;
        if (seniorBeId.HasValue)
        {
            var reqs = await context.RoleSkillRequirements
                .Where(r => r.JobRoleId == seniorBeId.Value)
                .Include(r => r.Skill)
                .OrderBy(r => r.Priority)
                .ToListAsync();

            foreach (var req in reqs)
            {
                Console.WriteLine($"  - {req.Skill.Name}: Min={req.MinimumLevel}, Expected={req.ExpectedLevel}, Mandatory={req.IsMandatory}");
            }
        }

        Console.WriteLine("\n=== Verification Complete ===");
    }
}
