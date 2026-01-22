using SkillMatrix.Domain.Entities.Taxonomy;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Infrastructure.Persistence.SeedData;

/// <summary>
/// SFIA-based level definitions for C# skill as a template
/// Other skills can follow the same pattern
/// </summary>
public static class SkillLevelDefinitionSeed
{
    public static List<SkillLevelDefinition> GetCSharpLevelDefinitions()
    {
        var skillId = Guid.Parse("30000000-0000-0000-0000-000000000001"); // C#

        return new List<SkillLevelDefinition>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.Follow,
                Description = "Has basic knowledge of C# syntax. Can write simple code with guidance.",
                Autonomy = "Works under close supervision",
                Influence = "Minimal impact, mainly learning",
                Complexity = "Performs simple, well-defined tasks",
                Knowledge = "Understands variables, data types, loops, basic conditionals",
                BehavioralIndicators = "[\"Can write simple functions\", \"Understands basic OOP\", \"Debugs with guidance\"]",
                EvidenceExamples = "[\"Completed basic C# tutorials\", \"Coded basic exercises\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.Assist,
                Description = "Can assist in C# tasks with guidance. Understands OOP and can apply basic patterns.",
                Autonomy = "Works with periodic guidance",
                Influence = "Impacts own work",
                Complexity = "Solves well-structured problems",
                Knowledge = "OOP, basic LINQ, Exception handling, Collections",
                BehavioralIndicators = "[\"Writes well-structured code\", \"Uses basic LINQ\", \"Handles exceptions properly\", \"Can debug common errors independently\"]",
                EvidenceExamples = "[\"Completed small features independently\", \"Fixed bugs with guidance\", \"Wrote basic unit tests\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.Apply,
                Description = "Applies C# independently in daily work. Has deep understanding of the language and best practices.",
                Autonomy = "Works independently, seeks guidance when needed",
                Influence = "Impacts direct team",
                Complexity = "Solves moderately complex problems",
                Knowledge = "Async/await, Generics, Reflection, Design Patterns, SOLID principles",
                BehavioralIndicators = "[\"Writes clean, maintainable code\", \"Applies appropriate design patterns\", \"Performs basic performance optimization\", \"Reviews code for juniors\"]",
                EvidenceExamples = "[\"Led medium and small features\", \"Refactored legacy code\", \"Mentored junior developers\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.Enable,
                Description = "Supports and enhances C# capabilities for the team. Ensures code quality and best practices are followed.",
                Autonomy = "Fully autonomous, guides others",
                Influence = "Impacts multiple teams/projects",
                Complexity = "Solves complex, diverse problems",
                Knowledge = "Advanced patterns, Performance optimization, Memory management, Multithreading",
                BehavioralIndicators = "[\"Establishes coding standards\", \"Reviews architecture decisions\", \"Troubleshoots complex issues\", \"Trains and mentors team\"]",
                EvidenceExamples = "[\"Led technical decisions for team\", \"Significantly improved performance\", \"Built shared libraries\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.EnsureAdvise,
                Description = "Ensures and advises on C# at organizational level. Guides technical strategy.",
                Autonomy = "Strategic direction",
                Influence = "Organizational impact",
                Complexity = "Solves strategic, long-term problems",
                Knowledge = "Language internals, CLR, .NET ecosystem strategy",
                BehavioralIndicators = "[\"Builds technology roadmap\", \"Decides on new technology adoption\", \"Represents company in community\"]",
                EvidenceExamples = "[\"Led technology migration\", \"Contributed to .NET community\", \"Speaking at tech events\"]"
            }
        };
    }

    public static List<SkillLevelDefinition> GetGenericLevelDefinitions(Guid skillId, string skillName)
    {
        return new List<SkillLevelDefinition>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.Follow,
                Description = $"Has basic knowledge of {skillName}. Requires guidance to perform tasks.",
                Autonomy = "Works under supervision",
                Influence = "Learning and developing",
                Complexity = "Simple, well-defined tasks"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.Assist,
                Description = $"Can assist in tasks related to {skillName}.",
                Autonomy = "Requires periodic guidance",
                Influence = "Impacts own work",
                Complexity = "Structured problems"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.Apply,
                Description = $"Applies {skillName} independently in daily work.",
                Autonomy = "Works independently",
                Influence = "Impacts team",
                Complexity = "Moderately complex problems"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.Enable,
                Description = $"Supports and enhances {skillName} capabilities for team.",
                Autonomy = "Fully autonomous",
                Influence = "Multiple teams/projects",
                Complexity = "Highly complex problems"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.EnsureAdvise,
                Description = $"Advises and guides {skillName} at organizational level.",
                Autonomy = "Strategic direction",
                Influence = "Organizational level",
                Complexity = "Long-term strategic"
            }
        };
    }
}
