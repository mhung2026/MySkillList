using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillMatrix.Domain.Entities.Taxonomy;

namespace SkillMatrix.Infrastructure.Persistence.Configurations;

public class SkillDomainConfiguration : IEntityTypeConfiguration<SkillDomain>
{
    public void Configure(EntityTypeBuilder<SkillDomain> builder)
    {
        builder.ToTable("SkillDomains");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.HasIndex(e => e.Code)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"IsCurrent\" = true");

        builder.HasIndex(e => new { e.IsCurrent, e.IsActive });

        builder.HasMany(e => e.Subcategories)
            .WithOne(e => e.SkillDomain)
            .HasForeignKey(e => e.SkillDomainId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SkillSubcategoryConfiguration : IEntityTypeConfiguration<SkillSubcategory>
{
    public void Configure(EntityTypeBuilder<SkillSubcategory> builder)
    {
        builder.ToTable("SkillSubcategories");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.HasIndex(e => new { e.SkillDomainId, e.Code })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"IsCurrent\" = true");

        builder.HasMany(e => e.Skills)
            .WithOne(e => e.Subcategory)
            .HasForeignKey(e => e.SubcategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(4000);

        builder.Property(e => e.Tags)
            .HasColumnType("jsonb");

        builder.HasIndex(e => e.Code)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"IsCurrent\" = true");

        builder.HasIndex(e => new { e.Category, e.SkillType, e.IsActive });

        builder.HasMany(e => e.LevelDefinitions)
            .WithOne(e => e.Skill)
            .HasForeignKey(e => e.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.RelatedSkillsFrom)
            .WithOne(e => e.FromSkill)
            .HasForeignKey(e => e.FromSkillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.RelatedSkillsTo)
            .WithOne(e => e.ToSkill)
            .HasForeignKey(e => e.ToSkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SkillLevelDefinitionConfiguration : IEntityTypeConfiguration<SkillLevelDefinition>
{
    public void Configure(EntityTypeBuilder<SkillLevelDefinition> builder)
    {
        builder.ToTable("SkillLevelDefinitions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(e => e.Autonomy)
            .HasMaxLength(2000);

        builder.Property(e => e.Influence)
            .HasMaxLength(2000);

        builder.Property(e => e.Complexity)
            .HasMaxLength(2000);

        builder.Property(e => e.BusinessSkills)
            .HasMaxLength(2000);

        builder.Property(e => e.Knowledge)
            .HasMaxLength(2000);

        builder.Property(e => e.BehavioralIndicators)
            .HasColumnType("jsonb");

        builder.Property(e => e.EvidenceExamples)
            .HasColumnType("jsonb");

        builder.HasIndex(e => new { e.SkillId, e.Level })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
    }
}

public class SkillRelationshipConfiguration : IEntityTypeConfiguration<SkillRelationship>
{
    public void Configure(EntityTypeBuilder<SkillRelationship> builder)
    {
        builder.ToTable("SkillRelationships");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(e => new { e.FromSkillId, e.ToSkillId, e.RelationshipType })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
    }
}
