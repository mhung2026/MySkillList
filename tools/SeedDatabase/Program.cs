using Microsoft.EntityFrameworkCore;
using SkillMatrix.Infrastructure.Persistence;

if (args.Length > 0 && args[0] == "verify")
{
    await VerifyDatabase.RunAsync();
    return;
}

Console.WriteLine("Starting database seeding...");

var connectionString = "Host=192.168.0.21;Database=MySkillList_NGE_DEV;Username=postgres;Password=@ll1@nceP@ss2o21;Maximum Pool Size=1000";

var optionsBuilder = new DbContextOptionsBuilder<SkillMatrixDbContext>();
optionsBuilder.UseNpgsql(connectionString);

using var context = new SkillMatrixDbContext(optionsBuilder.Options);

var seeder = new DatabaseSeeder(context);
await seeder.SeedAsync();

Console.WriteLine("Database seeding completed successfully!");

// Display summary
var domainCount = await context.SkillDomains.CountAsync();
var subcategoryCount = await context.SkillSubcategories.CountAsync();
var skillCount = await context.Skills.CountAsync();
var teamCount = await context.Teams.CountAsync();
var roleCount = await context.JobRoles.CountAsync();
var requirementCount = await context.RoleSkillRequirements.CountAsync();

Console.WriteLine();
Console.WriteLine("=== Seed Data Summary ===");
Console.WriteLine($"Skill Domains: {domainCount}");
Console.WriteLine($"Skill Subcategories: {subcategoryCount}");
Console.WriteLine($"Skills: {skillCount}");
Console.WriteLine($"Teams: {teamCount}");
Console.WriteLine($"Job Roles: {roleCount}");
Console.WriteLine($"Role Skill Requirements: {requirementCount}");
