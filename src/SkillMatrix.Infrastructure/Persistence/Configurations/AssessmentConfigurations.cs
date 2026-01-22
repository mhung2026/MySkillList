using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillMatrix.Domain.Entities.Assessment;

namespace SkillMatrix.Infrastructure.Persistence.Configurations;

public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.ToTable("Assessments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.AiAnalysis)
            .HasColumnType("jsonb");

        builder.Property(e => e.AiRecommendations)
            .HasColumnType("jsonb");

        builder.Property(e => e.EmployeeFeedback)
            .HasMaxLength(4000);

        builder.Property(e => e.FeedbackResolution)
            .HasMaxLength(4000);

        builder.HasOne(e => e.Employee)
            .WithMany(e => e.Assessments)
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Assessor)
            .WithMany()
            .HasForeignKey(e => e.AssessorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.TestTemplate)
            .WithMany(e => e.Assessments)
            .HasForeignKey(e => e.TestTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.EmployeeId, e.Type, e.Status });
        builder.HasIndex(e => new { e.EmployeeId, e.CompletedAt });
    }
}

public class AssessmentSkillResultConfiguration : IEntityTypeConfiguration<AssessmentSkillResult>
{
    public void Configure(EntityTypeBuilder<AssessmentSkillResult> builder)
    {
        builder.ToTable("AssessmentSkillResults");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Evidence)
            .HasMaxLength(4000);

        builder.Property(e => e.AssessorNotes)
            .HasMaxLength(2000);

        builder.Property(e => e.AiExplanation)
            .HasMaxLength(4000);

        builder.HasOne(e => e.Assessment)
            .WithMany(e => e.SkillResults)
            .HasForeignKey(e => e.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Skill)
            .WithMany()
            .HasForeignKey(e => e.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.AssessmentId, e.SkillId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
    }
}

public class TestTemplateConfiguration : IEntityTypeConfiguration<TestTemplate>
{
    public void Configure(EntityTypeBuilder<TestTemplate> builder)
    {
        builder.ToTable("TestTemplates");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.HasOne(e => e.TargetJobRole)
            .WithMany()
            .HasForeignKey(e => e.TargetJobRoleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.TargetSkill)
            .WithMany()
            .HasForeignKey(e => e.TargetSkillId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.Type, e.IsActive, e.IsCurrent });
    }
}

public class TestSectionConfiguration : IEntityTypeConfiguration<TestSection>
{
    public void Configure(EntityTypeBuilder<TestSection> builder)
    {
        builder.ToTable("TestSections");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.HasOne(e => e.TestTemplate)
            .WithMany(e => e.Sections)
            .HasForeignKey(e => e.TestTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.TestTemplateId, e.DisplayOrder });
    }
}

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Content)
            .IsRequired()
            .HasMaxLength(8000);

        builder.Property(e => e.CodeSnippet)
            .HasMaxLength(16000);

        builder.Property(e => e.MediaUrl)
            .HasMaxLength(500);

        builder.Property(e => e.GradingRubric)
            .HasMaxLength(4000);

        builder.Property(e => e.AiPromptUsed)
            .HasMaxLength(4000);

        builder.Property(e => e.Tags)
            .HasColumnType("jsonb");

        builder.HasOne(e => e.Section)
            .WithMany(e => e.Questions)
            .HasForeignKey(e => e.SectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Skill)
            .WithMany()
            .HasForeignKey(e => e.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.SkillId, e.TargetLevel, e.Difficulty, e.IsActive });
        builder.HasIndex(e => new { e.Type, e.IsActive });
    }
}

public class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable("QuestionOptions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Content)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(e => e.Explanation)
            .HasMaxLength(2000);

        builder.HasOne(e => e.Question)
            .WithMany(e => e.Options)
            .HasForeignKey(e => e.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.QuestionId, e.DisplayOrder });
    }
}

public class AssessmentResponseConfiguration : IEntityTypeConfiguration<AssessmentResponse>
{
    public void Configure(EntityTypeBuilder<AssessmentResponse> builder)
    {
        builder.ToTable("AssessmentResponses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TextResponse)
            .HasMaxLength(8000);

        builder.Property(e => e.CodeResponse)
            .HasMaxLength(32000);

        builder.Property(e => e.SelectedOptions)
            .HasColumnType("jsonb");

        builder.Property(e => e.AiGrading)
            .HasColumnType("jsonb");

        builder.Property(e => e.ManualGrading)
            .HasColumnType("jsonb");

        builder.HasOne(e => e.Assessment)
            .WithMany(e => e.Responses)
            .HasForeignKey(e => e.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Question)
            .WithMany(e => e.Responses)
            .HasForeignKey(e => e.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.AssessmentId, e.QuestionId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
    }
}
