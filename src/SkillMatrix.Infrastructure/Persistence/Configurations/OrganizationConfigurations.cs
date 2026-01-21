using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillMatrix.Domain.Entities.Organization;

namespace SkillMatrix.Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.HasOne(e => e.ParentTeam)
            .WithMany(e => e.SubTeams)
            .HasForeignKey(e => e.ParentTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TeamLead)
            .WithMany()
            .HasForeignKey(e => e.TeamLeadId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.Name)
            .HasFilter("\"IsDeleted\" = false");
    }
}

public class JobRoleConfiguration : IEntityTypeConfiguration<JobRole>
{
    public void Configure(EntityTypeBuilder<JobRole> builder)
    {
        builder.ToTable("JobRoles");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.HasOne(e => e.ParentRole)
            .WithMany(e => e.ChildRoles)
            .HasForeignKey(e => e.ParentRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Code)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"IsCurrent\" = true");

        builder.HasIndex(e => new { e.LevelInHierarchy, e.IsActive });
    }
}

public class RoleSkillRequirementConfiguration : IEntityTypeConfiguration<RoleSkillRequirement>
{
    public void Configure(EntityTypeBuilder<RoleSkillRequirement> builder)
    {
        builder.ToTable("RoleSkillRequirements");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.JobRoleId, e.SkillId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasOne(e => e.JobRole)
            .WithMany(e => e.SkillRequirements)
            .HasForeignKey(e => e.JobRoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Skill)
            .WithMany(e => e.RoleRequirements)
            .HasForeignKey(e => e.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.AvatarUrl)
            .HasMaxLength(500);

        builder.HasOne(e => e.Team)
            .WithMany(e => e.Members)
            .HasForeignKey(e => e.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.JobRole)
            .WithMany(e => e.Employees)
            .HasForeignKey(e => e.JobRoleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Manager)
            .WithMany(e => e.DirectReports)
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(e => e.UserId)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(e => new { e.TeamId, e.Status });
        builder.HasIndex(e => new { e.JobRoleId, e.Status });
    }
}

public class EmployeeSkillConfiguration : IEntityTypeConfiguration<EmployeeSkill>
{
    public void Configure(EntityTypeBuilder<EmployeeSkill> builder)
    {
        builder.ToTable("EmployeeSkills");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Evidence)
            .HasColumnType("jsonb");

        builder.HasOne(e => e.Employee)
            .WithMany(e => e.Skills)
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Skill)
            .WithMany(e => e.EmployeeSkills)
            .HasForeignKey(e => e.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.EmployeeId, e.SkillId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(e => new { e.SkillId, e.CurrentLevel });
    }
}

public class EmployeeSkillHistoryConfiguration : IEntityTypeConfiguration<EmployeeSkillHistory>
{
    public void Configure(EntityTypeBuilder<EmployeeSkillHistory> builder)
    {
        builder.ToTable("EmployeeSkillHistories");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ChangeReason)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Skill)
            .WithMany()
            .HasForeignKey(e => e.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.EmployeeId, e.SkillId, e.ChangedAt });
    }
}

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(4000);

        builder.Property(e => e.ExternalId)
            .HasMaxLength(100);

        builder.HasOne(e => e.Team)
            .WithMany(e => e.Projects)
            .HasForeignKey(e => e.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.ExternalId)
            .HasFilter("\"IsDeleted\" = false AND \"ExternalId\" IS NOT NULL");

        builder.HasIndex(e => new { e.TeamId, e.Status });
    }
}

public class ProjectSkillRequirementConfiguration : IEntityTypeConfiguration<ProjectSkillRequirement>
{
    public void Configure(EntityTypeBuilder<ProjectSkillRequirement> builder)
    {
        builder.ToTable("ProjectSkillRequirements");

        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.Project)
            .WithMany(e => e.SkillRequirements)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Skill)
            .WithMany()
            .HasForeignKey(e => e.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.ProjectId, e.SkillId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
    }
}

public class ProjectAssignmentConfiguration : IEntityTypeConfiguration<ProjectAssignment>
{
    public void Configure(EntityTypeBuilder<ProjectAssignment> builder)
    {
        builder.ToTable("ProjectAssignments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Role)
            .HasMaxLength(100);

        builder.HasOne(e => e.Project)
            .WithMany(e => e.Assignments)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Employee)
            .WithMany(e => e.ProjectAssignments)
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.ProjectId, e.EmployeeId, e.IsActive });
    }
}
