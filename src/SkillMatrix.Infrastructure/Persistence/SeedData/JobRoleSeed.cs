using SkillMatrix.Domain.Entities.Organization;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Infrastructure.Persistence.SeedData;

public static class JobRoleSeed
{
    public static List<JobRole> GetJobRoles()
    {
        return new List<JobRole>
        {
            // Backend Developer hierarchy
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                Code = "BE-JR",
                Name = "Junior Backend Developer",
                Description = "Entry-level backend developer position",
                LevelInHierarchy = 1,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000002"),
                Code = "BE-MID",
                Name = "Backend Developer",
                Description = "Mid-level backend developer position",
                ParentRoleId = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                LevelInHierarchy = 2,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000003"),
                Code = "BE-SR",
                Name = "Senior Backend Developer",
                Description = "Senior backend developer position",
                ParentRoleId = Guid.Parse("40000000-0000-0000-0000-000000000002"),
                LevelInHierarchy = 3,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000004"),
                Code = "BE-LEAD",
                Name = "Backend Lead",
                Description = "Backend team lead position",
                ParentRoleId = Guid.Parse("40000000-0000-0000-0000-000000000003"),
                LevelInHierarchy = 4,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },

            // Frontend Developer hierarchy
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000011"),
                Code = "FE-JR",
                Name = "Junior Frontend Developer",
                Description = "Entry-level frontend developer position",
                LevelInHierarchy = 1,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000012"),
                Code = "FE-MID",
                Name = "Frontend Developer",
                Description = "Mid-level frontend developer position",
                ParentRoleId = Guid.Parse("40000000-0000-0000-0000-000000000011"),
                LevelInHierarchy = 2,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000013"),
                Code = "FE-SR",
                Name = "Senior Frontend Developer",
                Description = "Senior frontend developer position",
                ParentRoleId = Guid.Parse("40000000-0000-0000-0000-000000000012"),
                LevelInHierarchy = 3,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000014"),
                Code = "FE-LEAD",
                Name = "Frontend Lead",
                Description = "Frontend team lead position",
                ParentRoleId = Guid.Parse("40000000-0000-0000-0000-000000000013"),
                LevelInHierarchy = 4,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },

            // Fullstack
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000020"),
                Code = "FS",
                Name = "Fullstack Developer",
                Description = "Fullstack developer position",
                LevelInHierarchy = 2,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },

            // QA/QC hierarchy
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000031"),
                Code = "QC-JR",
                Name = "Junior QC Engineer",
                Description = "Entry-level QC position",
                LevelInHierarchy = 1,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000032"),
                Code = "QC",
                Name = "QC Engineer",
                Description = "Mid-level QC position",
                ParentRoleId = Guid.Parse("40000000-0000-0000-0000-000000000031"),
                LevelInHierarchy = 2,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000033"),
                Code = "QC-SR",
                Name = "Senior QC Engineer",
                Description = "Senior QC position",
                ParentRoleId = Guid.Parse("40000000-0000-0000-0000-000000000032"),
                LevelInHierarchy = 3,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },

            // BA hierarchy
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000041"),
                Code = "BA-JR",
                Name = "Junior Business Analyst",
                Description = "Entry-level BA position",
                LevelInHierarchy = 1,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000042"),
                Code = "BA",
                Name = "Business Analyst",
                Description = "Mid-level BA position",
                ParentRoleId = Guid.Parse("40000000-0000-0000-0000-000000000041"),
                LevelInHierarchy = 2,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000043"),
                Code = "BA-SR",
                Name = "Senior Business Analyst",
                Description = "Senior BA position",
                ParentRoleId = Guid.Parse("40000000-0000-0000-0000-000000000042"),
                LevelInHierarchy = 3,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },

            // AI Engineer hierarchy
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000051"),
                Code = "AI-JR",
                Name = "Junior AI Engineer",
                Description = "Entry-level AI Engineer position",
                LevelInHierarchy = 1,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000052"),
                Code = "AI",
                Name = "AI Engineer",
                Description = "Mid-level AI Engineer position",
                ParentRoleId = Guid.Parse("40000000-0000-0000-0000-000000000051"),
                LevelInHierarchy = 2,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000053"),
                Code = "AI-SR",
                Name = "Senior AI Engineer",
                Description = "Senior AI Engineer position",
                ParentRoleId = Guid.Parse("40000000-0000-0000-0000-000000000052"),
                LevelInHierarchy = 3,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },

            // PM
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000061"),
                Code = "PM",
                Name = "Project Manager",
                Description = "Project Manager position",
                LevelInHierarchy = 3,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },

            // PSS (Product Support Specialist)
            new()
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000071"),
                Code = "PSS",
                Name = "Product Support Specialist",
                Description = "Product Support Specialist position",
                LevelInHierarchy = 2,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            }
        };
    }

    public static List<RoleSkillRequirement> GetRoleSkillRequirements()
    {
        var requirements = new List<RoleSkillRequirement>();

        // Junior Backend Developer requirements
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000001", "30000000-0000-0000-0000-000000000001", // C#
            ProficiencyLevel.Assist, ProficiencyLevel.Apply, true, 1);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000001", "30000000-0000-0000-0000-000000000010", // .NET Core
            ProficiencyLevel.Follow, ProficiencyLevel.Assist, true, 2);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000001", "30000000-0000-0000-0000-000000000031", // SQL
            ProficiencyLevel.Assist, ProficiencyLevel.Apply, true, 3);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000001", "30000000-0000-0000-0000-000000000110", // Git
            ProficiencyLevel.Assist, ProficiencyLevel.Apply, true, 4);

        // Senior Backend Developer requirements
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000003", "30000000-0000-0000-0000-000000000001", // C#
            ProficiencyLevel.Enable, ProficiencyLevel.EnsureAdvise, true, 1);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000003", "30000000-0000-0000-0000-000000000010", // .NET Core
            ProficiencyLevel.Enable, ProficiencyLevel.EnsureAdvise, true, 2);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000003", "30000000-0000-0000-0000-000000000012", // ASP.NET Core
            ProficiencyLevel.Enable, ProficiencyLevel.EnsureAdvise, true, 3);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000003", "30000000-0000-0000-0000-000000000011", // EF Core
            ProficiencyLevel.Apply, ProficiencyLevel.Enable, true, 4);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000003", "30000000-0000-0000-0000-000000000030", // PostgreSQL
            ProficiencyLevel.Apply, ProficiencyLevel.Enable, true, 5);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000003", "30000000-0000-0000-0000-000000000032", // DB Design
            ProficiencyLevel.Apply, ProficiencyLevel.Enable, false, 6);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000003", "30000000-0000-0000-0000-000000000101", // Tech Leadership
            ProficiencyLevel.Apply, ProficiencyLevel.Enable, false, 7);

        // Frontend Developer requirements
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000012", "30000000-0000-0000-0000-000000000002", // JavaScript
            ProficiencyLevel.Apply, ProficiencyLevel.Enable, true, 1);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000012", "30000000-0000-0000-0000-000000000003", // TypeScript
            ProficiencyLevel.Apply, ProficiencyLevel.Enable, true, 2);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000012", "30000000-0000-0000-0000-000000000020", // React
            ProficiencyLevel.Apply, ProficiencyLevel.Enable, true, 3);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000012", "30000000-0000-0000-0000-000000000005", // HTML/CSS
            ProficiencyLevel.Apply, ProficiencyLevel.Enable, true, 4);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000012", "30000000-0000-0000-0000-000000000022", // Responsive
            ProficiencyLevel.Apply, ProficiencyLevel.Enable, true, 5);

        // QC Engineer requirements
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000032", "30000000-0000-0000-0000-000000000040", // Manual Testing
            ProficiencyLevel.Apply, ProficiencyLevel.Enable, true, 1);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000032", "30000000-0000-0000-0000-000000000043", // Test Planning
            ProficiencyLevel.Assist, ProficiencyLevel.Apply, true, 2);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000032", "30000000-0000-0000-0000-000000000041", // Automation
            ProficiencyLevel.Assist, ProficiencyLevel.Apply, false, 3);

        // BA requirements
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000042", "30000000-0000-0000-0000-000000000060", // Requirements
            ProficiencyLevel.Apply, ProficiencyLevel.Enable, true, 1);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000042", "30000000-0000-0000-0000-000000000061", // Process Modeling
            ProficiencyLevel.Assist, ProficiencyLevel.Apply, true, 2);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000042", "30000000-0000-0000-0000-000000000062", // Use Cases
            ProficiencyLevel.Apply, ProficiencyLevel.Enable, true, 3);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000042", "30000000-0000-0000-0000-000000000063", // Stakeholder
            ProficiencyLevel.Apply, ProficiencyLevel.Enable, true, 4);

        // AI Engineer requirements
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000052", "30000000-0000-0000-0000-000000000004", // Python
            ProficiencyLevel.Apply, ProficiencyLevel.Enable, true, 1);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000052", "30000000-0000-0000-0000-000000000050", // ML Basics
            ProficiencyLevel.Apply, ProficiencyLevel.Enable, true, 2);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000052", "30000000-0000-0000-0000-000000000051", // Deep Learning
            ProficiencyLevel.Assist, ProficiencyLevel.Apply, true, 3);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000052", "30000000-0000-0000-0000-000000000053", // LLM Integration
            ProficiencyLevel.Apply, ProficiencyLevel.Enable, true, 4);

        // PM requirements
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000061", "30000000-0000-0000-0000-000000000070", // Agile
            ProficiencyLevel.Enable, ProficiencyLevel.EnsureAdvise, true, 1);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000061", "30000000-0000-0000-0000-000000000071", // Project Planning
            ProficiencyLevel.Enable, ProficiencyLevel.EnsureAdvise, true, 2);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000061", "30000000-0000-0000-0000-000000000072", // Risk Management
            ProficiencyLevel.Apply, ProficiencyLevel.Enable, true, 3);
        AddRequirement(requirements, "40000000-0000-0000-0000-000000000061", "30000000-0000-0000-0000-000000000063", // Stakeholder
            ProficiencyLevel.Enable, ProficiencyLevel.EnsureAdvise, true, 4);

        return requirements;
    }

    private static void AddRequirement(List<RoleSkillRequirement> list, string roleId, string skillId,
        ProficiencyLevel min, ProficiencyLevel expected, bool mandatory, int priority)
    {
        list.Add(new RoleSkillRequirement
        {
            Id = Guid.NewGuid(),
            JobRoleId = Guid.Parse(roleId),
            SkillId = Guid.Parse(skillId),
            MinimumLevel = min,
            ExpectedLevel = expected,
            IsMandatory = mandatory,
            Priority = priority
        });
    }
}
