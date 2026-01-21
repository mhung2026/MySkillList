using Microsoft.EntityFrameworkCore;
using SkillMatrix.Domain.Entities.Assessment;
using SkillMatrix.Domain.Entities.Configuration;
using SkillMatrix.Domain.Entities.Learning;
using SkillMatrix.Domain.Entities.Organization;
using SkillMatrix.Domain.Entities.Taxonomy;

namespace SkillMatrix.Infrastructure.Persistence;

public class SkillMatrixDbContext : DbContext
{
    public SkillMatrixDbContext(DbContextOptions<SkillMatrixDbContext> options)
        : base(options)
    {
    }

    // Taxonomy
    public DbSet<SkillDomain> SkillDomains => Set<SkillDomain>();
    public DbSet<SkillSubcategory> SkillSubcategories => Set<SkillSubcategory>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<SkillLevelDefinition> SkillLevelDefinitions => Set<SkillLevelDefinition>();
    public DbSet<SkillRelationship> SkillRelationships => Set<SkillRelationship>();
    public DbSet<ProficiencyLevelDefinition> ProficiencyLevelDefinitions => Set<ProficiencyLevelDefinition>();

    // Organization
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<JobRole> JobRoles => Set<JobRole>();
    public DbSet<RoleSkillRequirement> RoleSkillRequirements => Set<RoleSkillRequirement>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeSkill> EmployeeSkills => Set<EmployeeSkill>();
    public DbSet<EmployeeSkillHistory> EmployeeSkillHistories => Set<EmployeeSkillHistory>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectSkillRequirement> ProjectSkillRequirements => Set<ProjectSkillRequirement>();
    public DbSet<ProjectAssignment> ProjectAssignments => Set<ProjectAssignment>();

    // Assessment
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<AssessmentSkillResult> AssessmentSkillResults => Set<AssessmentSkillResult>();
    public DbSet<TestTemplate> TestTemplates => Set<TestTemplate>();
    public DbSet<TestSection> TestSections => Set<TestSection>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<AssessmentResponse> AssessmentResponses => Set<AssessmentResponse>();

    // Configuration
    public DbSet<SystemEnumValue> SystemEnumValues => Set<SystemEnumValue>();

    // Learning
    public DbSet<SkillGap> SkillGaps => Set<SkillGap>();
    public DbSet<TeamSkillGap> TeamSkillGaps => Set<TeamSkillGap>();
    public DbSet<LearningResource> LearningResources => Set<LearningResource>();
    public DbSet<LearningResourceSkill> LearningResourceSkills => Set<LearningResourceSkill>();
    public DbSet<EmployeeLearningPath> EmployeeLearningPaths => Set<EmployeeLearningPath>();
    public DbSet<LearningPathItem> LearningPathItems => Set<LearningPathItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SkillMatrixDbContext).Assembly);

        // Global query filters for soft delete
        modelBuilder.Entity<SkillDomain>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SkillSubcategory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Skill>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SkillLevelDefinition>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SkillRelationship>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProficiencyLevelDefinition>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Team>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<JobRole>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RoleSkillRequirement>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<EmployeeSkill>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<EmployeeSkillHistory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Project>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProjectSkillRequirement>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProjectAssignment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Assessment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AssessmentSkillResult>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TestTemplate>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TestSection>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Question>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<QuestionOption>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AssessmentResponse>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SkillGap>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TeamSkillGap>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LearningResource>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LearningResourceSkill>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<EmployeeLearningPath>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LearningPathItem>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SystemEnumValue>().HasQueryFilter(e => !e.IsDeleted);

        // SystemEnumValue configuration
        modelBuilder.Entity<SystemEnumValue>(entity =>
        {
            entity.HasIndex(e => new { e.EnumType, e.Value }).IsUnique();
            entity.HasIndex(e => new { e.EnumType, e.Code }).IsUnique();
            entity.HasIndex(e => e.EnumType);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Domain.Common.BaseEntity baseEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        baseEntity.CreatedAt = DateTime.UtcNow;
                        if (baseEntity.Id == Guid.Empty)
                            baseEntity.Id = Guid.NewGuid();
                        break;
                    case EntityState.Modified:
                        baseEntity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            if (entry.Entity is Domain.Common.VersionedEntity versionedEntity && entry.State == EntityState.Added)
            {
                versionedEntity.EffectiveFrom = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
