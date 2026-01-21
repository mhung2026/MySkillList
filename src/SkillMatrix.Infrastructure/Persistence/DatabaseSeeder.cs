using Microsoft.EntityFrameworkCore;
using SkillMatrix.Domain.Entities.Organization;
using SkillMatrix.Domain.Entities.Taxonomy;
using SkillMatrix.Infrastructure.Persistence.SeedData;

namespace SkillMatrix.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly SkillMatrixDbContext _context;

    public DatabaseSeeder(SkillMatrixDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await SeedSkillDomainsAsync();
        await SeedSkillSubcategoriesAsync();
        await SeedSkillsAsync();
        await SeedSkillLevelDefinitionsAsync();
        await SeedTeamsAsync();
        await SeedJobRolesAsync();
        await SeedRoleSkillRequirementsAsync();

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Reseed skill taxonomy with SFIA 9 data (clears existing taxonomy data first)
    /// Use this to update to latest SFIA 9 framework
    /// </summary>
    public async Task ReseedSkillTaxonomyAsync()
    {
        // Clear existing taxonomy data (in reverse order of dependencies)
        // Note: This will also affect EmployeeSkills and RoleSkillRequirements

        // First clear skill level definitions (ignore query filter)
        var skillLevelDefs = await _context.SkillLevelDefinitions.IgnoreQueryFilters().ToListAsync();
        _context.SkillLevelDefinitions.RemoveRange(skillLevelDefs);
        await _context.SaveChangesAsync();

        // Clear skill relationships (ignore query filter)
        var skillRelations = await _context.SkillRelationships.IgnoreQueryFilters().ToListAsync();
        _context.SkillRelationships.RemoveRange(skillRelations);
        await _context.SaveChangesAsync();

        // Clear role skill requirements that reference skills (ignore query filter)
        var roleSkillReqs = await _context.RoleSkillRequirements.IgnoreQueryFilters().ToListAsync();
        _context.RoleSkillRequirements.RemoveRange(roleSkillReqs);
        await _context.SaveChangesAsync();

        // Clear employee skills (ignore query filter)
        var employeeSkills = await _context.EmployeeSkills.IgnoreQueryFilters().ToListAsync();
        _context.EmployeeSkills.RemoveRange(employeeSkills);
        await _context.SaveChangesAsync();

        // Clear assessment skill results (ignore query filter)
        var assessmentResults = await _context.AssessmentSkillResults.IgnoreQueryFilters().ToListAsync();
        _context.AssessmentSkillResults.RemoveRange(assessmentResults);
        await _context.SaveChangesAsync();

        // Clear skills (ignore query filter)
        var skills = await _context.Skills.IgnoreQueryFilters().ToListAsync();
        _context.Skills.RemoveRange(skills);
        await _context.SaveChangesAsync();

        // Clear subcategories (ignore query filter)
        var subcategories = await _context.SkillSubcategories.IgnoreQueryFilters().ToListAsync();
        _context.SkillSubcategories.RemoveRange(subcategories);
        await _context.SaveChangesAsync();

        // Clear domains (ignore query filter)
        var domains = await _context.SkillDomains.IgnoreQueryFilters().ToListAsync();
        _context.SkillDomains.RemoveRange(domains);
        await _context.SaveChangesAsync();

        // Now seed fresh SFIA 9 data
        var newDomains = SkillTaxonomySeed.GetSkillDomains();
        foreach (var domain in newDomains)
        {
            domain.CreatedAt = DateTime.UtcNow;
        }
        await _context.SkillDomains.AddRangeAsync(newDomains);
        await _context.SaveChangesAsync();

        var newSubcategories = SkillTaxonomySeed.GetSkillSubcategories();
        foreach (var sub in newSubcategories)
        {
            sub.CreatedAt = DateTime.UtcNow;
        }
        await _context.SkillSubcategories.AddRangeAsync(newSubcategories);
        await _context.SaveChangesAsync();

        var newSkills = SkillTaxonomySeed.GetSkills();
        foreach (var skill in newSkills)
        {
            skill.CreatedAt = DateTime.UtcNow;
        }
        await _context.Skills.AddRangeAsync(newSkills);
        await _context.SaveChangesAsync();

        // Generate and seed level definitions for all skills
        var levelDefinitions = SfiaLevelDefinitionSeed.GenerateAllSkillLevelDefinitions(newSkills);
        foreach (var def in levelDefinitions)
        {
            def.CreatedAt = DateTime.UtcNow;
        }
        await _context.SkillLevelDefinitions.AddRangeAsync(levelDefinitions);
        await _context.SaveChangesAsync();
    }

    private async Task SeedSkillDomainsAsync()
    {
        if (await _context.SkillDomains.AnyAsync())
            return;

        var domains = SkillTaxonomySeed.GetSkillDomains();
        foreach (var domain in domains)
        {
            domain.CreatedAt = DateTime.UtcNow;
        }
        await _context.SkillDomains.AddRangeAsync(domains);
        await _context.SaveChangesAsync();
    }

    private async Task SeedSkillSubcategoriesAsync()
    {
        if (await _context.SkillSubcategories.AnyAsync())
            return;

        var subcategories = SkillTaxonomySeed.GetSkillSubcategories();
        foreach (var sub in subcategories)
        {
            sub.CreatedAt = DateTime.UtcNow;
        }
        await _context.SkillSubcategories.AddRangeAsync(subcategories);
        await _context.SaveChangesAsync();
    }

    private async Task SeedSkillsAsync()
    {
        if (await _context.Skills.AnyAsync())
            return;

        var skills = SkillTaxonomySeed.GetSkills();
        foreach (var skill in skills)
        {
            skill.CreatedAt = DateTime.UtcNow;
        }
        await _context.Skills.AddRangeAsync(skills);
        await _context.SaveChangesAsync();
    }

    private async Task SeedSkillLevelDefinitionsAsync()
    {
        if (await _context.SkillLevelDefinitions.AnyAsync())
            return;

        var definitions = SkillLevelDefinitionSeed.GetCSharpLevelDefinitions();
        foreach (var def in definitions)
        {
            def.CreatedAt = DateTime.UtcNow;
        }
        await _context.SkillLevelDefinitions.AddRangeAsync(definitions);
        await _context.SaveChangesAsync();
    }

    private async Task SeedTeamsAsync()
    {
        if (await _context.Teams.AnyAsync())
            return;

        var teams = TeamSeed.GetTeams();
        foreach (var team in teams)
        {
            team.CreatedAt = DateTime.UtcNow;
        }
        await _context.Teams.AddRangeAsync(teams);
        await _context.SaveChangesAsync();
    }

    private async Task SeedJobRolesAsync()
    {
        if (await _context.JobRoles.AnyAsync())
            return;

        var roles = JobRoleSeed.GetJobRoles();
        foreach (var role in roles)
        {
            role.CreatedAt = DateTime.UtcNow;
        }
        await _context.JobRoles.AddRangeAsync(roles);
        await _context.SaveChangesAsync();
    }

    private async Task SeedRoleSkillRequirementsAsync()
    {
        if (await _context.RoleSkillRequirements.AnyAsync())
            return;

        var requirements = JobRoleSeed.GetRoleSkillRequirements();
        foreach (var req in requirements)
        {
            req.CreatedAt = DateTime.UtcNow;
        }
        await _context.RoleSkillRequirements.AddRangeAsync(requirements);
        await _context.SaveChangesAsync();
    }
}
