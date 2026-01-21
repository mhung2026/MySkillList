using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillMatrix.Domain.Entities.Learning;

namespace SkillMatrix.Infrastructure.Persistence.Configurations;

public class SkillGapConfiguration : IEntityTypeConfiguration<SkillGap>
{
    public void Configure(EntityTypeBuilder<SkillGap> builder)
    {
        builder.ToTable("SkillGaps");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.AiAnalysis)
            .HasMaxLength(4000);

        builder.Property(e => e.AiRecommendation)
            .HasMaxLength(4000);

        builder.HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Skill)
            .WithMany()
            .HasForeignKey(e => e.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.JobRole)
            .WithMany()
            .HasForeignKey(e => e.JobRoleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.EmployeeId, e.SkillId })
            .HasFilter("\"IsDeleted\" = false AND \"IsAddressed\" = false");

        builder.HasIndex(e => new { e.Priority, e.IsAddressed });
    }
}

public class TeamSkillGapConfiguration : IEntityTypeConfiguration<TeamSkillGap>
{
    public void Configure(EntityTypeBuilder<TeamSkillGap> builder)
    {
        builder.ToTable("TeamSkillGaps");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.AiAnalysis)
            .HasMaxLength(4000);

        builder.HasOne(e => e.Team)
            .WithMany()
            .HasForeignKey(e => e.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Skill)
            .WithMany()
            .HasForeignKey(e => e.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.TeamId, e.SkillId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(e => new { e.TeamId, e.OverallPriority });
    }
}

public class LearningResourceConfiguration : IEntityTypeConfiguration<LearningResource>
{
    public void Configure(EntityTypeBuilder<LearningResource> builder)
    {
        builder.ToTable("LearningResources");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Description)
            .HasMaxLength(4000);

        builder.Property(e => e.Url)
            .HasMaxLength(1000);

        builder.Property(e => e.Provider)
            .HasMaxLength(200);

        builder.Property(e => e.Tags)
            .HasColumnType("jsonb");

        builder.Property(e => e.Cost)
            .HasPrecision(10, 2);

        builder.HasIndex(e => new { e.Type, e.IsActive });
        builder.HasIndex(e => new { e.IsInternal, e.IsFree });
    }
}

public class LearningResourceSkillConfiguration : IEntityTypeConfiguration<LearningResourceSkill>
{
    public void Configure(EntityTypeBuilder<LearningResourceSkill> builder)
    {
        builder.ToTable("LearningResourceSkills");

        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.LearningResource)
            .WithMany(e => e.TargetSkills)
            .HasForeignKey(e => e.LearningResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Skill)
            .WithMany()
            .HasForeignKey(e => e.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.LearningResourceId, e.SkillId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(e => new { e.SkillId, e.FromLevel, e.ToLevel });
    }
}

public class EmployeeLearningPathConfiguration : IEntityTypeConfiguration<EmployeeLearningPath>
{
    public void Configure(EntityTypeBuilder<EmployeeLearningPath> builder)
    {
        builder.ToTable("EmployeeLearningPaths");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.AiRationale)
            .HasMaxLength(4000);

        builder.Property(e => e.ApprovalNotes)
            .HasMaxLength(2000);

        builder.HasOne(e => e.Employee)
            .WithMany(e => e.LearningPaths)
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.SkillGap)
            .WithMany(e => e.LearningPaths)
            .HasForeignKey(e => e.SkillGapId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.TargetSkill)
            .WithMany()
            .HasForeignKey(e => e.TargetSkillId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.EmployeeId, e.Status });
        builder.HasIndex(e => new { e.TargetSkillId, e.Status });
    }
}

public class LearningPathItemConfiguration : IEntityTypeConfiguration<LearningPathItem>
{
    public void Configure(EntityTypeBuilder<LearningPathItem> builder)
    {
        builder.ToTable("LearningPathItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Notes)
            .HasMaxLength(4000);

        builder.Property(e => e.Outcome)
            .HasMaxLength(2000);

        builder.HasOne(e => e.LearningPath)
            .WithMany(e => e.Items)
            .HasForeignKey(e => e.LearningPathId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.LearningResource)
            .WithMany(e => e.PathItems)
            .HasForeignKey(e => e.LearningResourceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.LearningPathId, e.DisplayOrder });
        builder.HasIndex(e => new { e.LearningPathId, e.Status });
    }
}
