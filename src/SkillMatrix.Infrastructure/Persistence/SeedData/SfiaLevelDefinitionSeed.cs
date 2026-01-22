using SkillMatrix.Domain.Entities.Taxonomy;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Infrastructure.Persistence.SeedData;

/// <summary>
/// SFIA 9 Level Definitions - Generic descriptions for all proficiency levels
/// Based on SFIA 9 framework: https://sfia-online.org/
/// </summary>
public static class SfiaLevelDefinitionSeed
{
    /// <summary>
    /// SFIA 9 Generic Attributes for each level
    /// These define Autonomy, Influence, Complexity, Knowledge, and Business Skills
    /// </summary>
    public static class SfiaLevelAttributes
    {
        public static readonly Dictionary<ProficiencyLevel, SfiaLevelAttribute> Attributes = new()
        {
            [ProficiencyLevel.Follow] = new SfiaLevelAttribute
            {
                Level = 1,
                LevelName = "Follow",
                Description = "Works under close direction. Uses limited discretion in resolving issues or enquiries. Works without frequent reference to others only in relation to specified, routine activities.",
                Autonomy = "Works under close direction. Uses limited discretion.",
                Influence = "Minimal influence. May work alone or interact with immediate team.",
                Complexity = "Performs routine activities in a structured environment.",
                Knowledge = "Has a basic generic knowledge. Applies newly acquired knowledge to develop new skills.",
                BusinessSkills = "Has sufficient communication skills. Contributes to identifying own development opportunities.",
                BehavioralIndicators = new[]
                {
                    "Follows instructions and guidelines",
                    "Asks questions when uncertain",
                    "Completes assigned tasks on time",
                    "Documents work as required",
                    "Participates in learning activities"
                },
                EvidenceExamples = new[]
                {
                    "Completed basic training/certification",
                    "Successfully followed documented procedures",
                    "Received positive feedback on routine tasks"
                }
            },

            [ProficiencyLevel.Assist] = new SfiaLevelAttribute
            {
                Level = 2,
                LevelName = "Assist",
                Description = "Works under routine direction. Uses limited discretion in resolving issues or enquiries. Determines when to seek guidance in unexpected situations.",
                Autonomy = "Works under routine direction within a clear framework of accountability.",
                Influence = "Interacts with and may influence immediate colleagues. May have some external contact.",
                Complexity = "Performs a range of work activities in varied environments.",
                Knowledge = "Demonstrates application of essential generic knowledge typically found in the industry body of knowledge.",
                BusinessSkills = "Has sufficient communication skills. Plans and monitors own work. Aware of need to adhere to quality standards.",
                BehavioralIndicators = new[]
                {
                    "Assists more experienced practitioners",
                    "Performs defined tasks under supervision",
                    "Uses established methods and procedures",
                    "Identifies when to escalate issues",
                    "Contributes to team activities"
                },
                EvidenceExamples = new[]
                {
                    "Completed tasks with minimal supervision",
                    "Contributed to team deliverables",
                    "Received positive peer feedback",
                    "Passed intermediate assessments"
                }
            },

            [ProficiencyLevel.Apply] = new SfiaLevelAttribute
            {
                Level = 3,
                LevelName = "Apply",
                Description = "Works under general direction. Uses discretion in identifying and responding to complex issues and assignments. Receives specific direction, accepts guidance and has work reviewed at agreed milestones.",
                Autonomy = "Works under general direction. Determines own approach within defined frameworks.",
                Influence = "Interacts with and influences team members and business stakeholders. May supervise others.",
                Complexity = "Performs a range of work, sometimes complex and non-routine.",
                Knowledge = "Demonstrates effective application of knowledge. Has a sound generic knowledge and domain specific knowledge.",
                BusinessSkills = "Communicates effectively. Takes initiative in identifying and negotiating appropriate development opportunities.",
                BehavioralIndicators = new[]
                {
                    "Works independently on assigned tasks",
                    "Applies established methods appropriately",
                    "Resolves routine issues without supervision",
                    "Collaborates with team members effectively",
                    "Identifies improvements to processes"
                },
                EvidenceExamples = new[]
                {
                    "Led small features or components independently",
                    "Mentored junior team members",
                    "Resolved complex issues with minimal guidance",
                    "Received recognition for quality work"
                }
            },

            [ProficiencyLevel.Enable] = new SfiaLevelAttribute
            {
                Level = 4,
                LevelName = "Enable",
                Description = "Works under broad direction. Work is often self-initiated. Is fully responsible for meeting allocated technical and/or project/supervisory objectives.",
                Autonomy = "Works under broad direction. Full accountability for own technical work or project/supervisory responsibilities.",
                Influence = "Influences team and specialist peers. Contributes to decision-making at business unit level.",
                Complexity = "Work includes complex technical activities. Contributes to implementing change.",
                Knowledge = "Has a thorough understanding of recognised generic industry bodies of knowledge and specialist bodies of knowledge.",
                BusinessSkills = "Communicates fluently. Facilitates collaboration. Plans and directs own work and that of others.",
                BehavioralIndicators = new[]
                {
                    "Takes ownership of significant deliverables",
                    "Enables and supports team members",
                    "Defines approaches and methods",
                    "Reviews and improves team practices",
                    "Manages stakeholder expectations"
                },
                EvidenceExamples = new[]
                {
                    "Led technical initiatives across teams",
                    "Established best practices and standards",
                    "Delivered complex projects successfully",
                    "Developed team capabilities"
                }
            },

            [ProficiencyLevel.EnsureAdvise] = new SfiaLevelAttribute
            {
                Level = 5,
                LevelName = "Ensure, advise",
                Description = "Works under broad direction. Work is often self-initiated. Is accountable for actions and decisions taken in achieving significant results for a defined area.",
                Autonomy = "Works under broad direction. Accountable for significant results. Influences strategy.",
                Influence = "Influences policy and strategy. Has significant influence across organisation.",
                Complexity = "Performs highly complex work activities. Leads on the implementation of strategy.",
                Knowledge = "Has a deep understanding of recognised industry knowledge. Develops new knowledge and extends industry body of knowledge.",
                BusinessSkills = "Demonstrates leadership. Advises on the application of standards and methods. Ensures compliance.",
                BehavioralIndicators = new[]
                {
                    "Ensures quality and standards across area",
                    "Advises on complex technical decisions",
                    "Drives improvements at organisational level",
                    "Represents organisation externally",
                    "Develops organisational capabilities"
                },
                EvidenceExamples = new[]
                {
                    "Led organisational transformation initiatives",
                    "Established enterprise standards and policies",
                    "Advised executive leadership on technical matters",
                    "Published thought leadership content"
                }
            },

            [ProficiencyLevel.Initiate] = new SfiaLevelAttribute
            {
                Level = 6,
                LevelName = "Initiate, influence",
                Description = "Has defined authority and accountability for actions and decisions within a significant area of work. Sets organisational strategy and advances industry knowledge.",
                Autonomy = "Has defined authority for a significant area of work. Makes decisions critical to success.",
                Influence = "Influences industry direction. Shapes policy and advances the profession.",
                Complexity = "Leads on initiating and implementing strategy. Addresses problems that impact the wider organisation.",
                Knowledge = "Recognised as an expert. Has an extensive understanding and contributes to the evolution of knowledge.",
                BusinessSkills = "Leads, manages and develops people. Develops strategic alliances. Communicates with authority at all levels.",
                BehavioralIndicators = new[]
                {
                    "Initiates strategic direction for the organisation",
                    "Influences industry standards and practices",
                    "Develops future leaders and experts",
                    "Drives innovation at scale",
                    "Builds strategic partnerships"
                },
                EvidenceExamples = new[]
                {
                    "Shaped industry standards or frameworks",
                    "Led major organisational transformation",
                    "Recognised as industry thought leader",
                    "Keynote speaker at major conferences"
                }
            },

            [ProficiencyLevel.SetStrategy] = new SfiaLevelAttribute
            {
                Level = 7,
                LevelName = "Set strategy, inspire, mobilise",
                Description = "Has authority and accountability for all aspects of a significant area of work including strategy, policy and operational responsibility. Sets direction, policy and strategy.",
                Autonomy = "The highest level of authority and responsibility. Accountable for overall success and policy.",
                Influence = "Sets and advances the strategy of the organisation and/or industry. Highest levels of influence.",
                Complexity = "Leads on the formulation and implementation of strategy. Makes decisions critical to organisational success.",
                Knowledge = "Leads the advancement of knowledge. Applies the highest levels of industry knowledge and business acumen.",
                BusinessSkills = "Inspires others. Drives culture and values. Has outstanding leadership and communication skills.",
                BehavioralIndicators = new[]
                {
                    "Sets strategic direction for the organisation/industry",
                    "Inspires and mobilises large-scale change",
                    "Shapes industry evolution and standards",
                    "Develops organisation-wide capabilities",
                    "Represents organisation at highest levels"
                },
                EvidenceExamples = new[]
                {
                    "Defined industry-wide frameworks",
                    "Led transformation affecting multiple organisations",
                    "Internationally recognised expert",
                    "Author of definitive works in the field"
                }
            }
        };
    }

    /// <summary>
    /// Generate level definitions for all skills based on their ApplicableLevels
    /// </summary>
    public static List<SkillLevelDefinition> GenerateAllSkillLevelDefinitions(List<Skill> skills)
    {
        var definitions = new List<SkillLevelDefinition>();
        var idCounter = 1;

        foreach (var skill in skills)
        {
            // Parse applicable levels (e.g., "2,3,4,5,6")
            var applicableLevels = ParseApplicableLevels(skill.ApplicableLevels);

            foreach (var level in applicableLevels)
            {
                if (SfiaLevelAttributes.Attributes.TryGetValue(level, out var attr))
                {
                    definitions.Add(new SkillLevelDefinition
                    {
                        Id = Guid.Parse($"40000000-0000-0000-0000-{idCounter:D12}"),
                        SkillId = skill.Id,
                        Level = level,
                        Description = $"[{skill.Name}] {attr.Description}",
                        Autonomy = attr.Autonomy,
                        Influence = attr.Influence,
                        Complexity = attr.Complexity,
                        Knowledge = attr.Knowledge,
                        BusinessSkills = attr.BusinessSkills,
                        BehavioralIndicators = System.Text.Json.JsonSerializer.Serialize(attr.BehavioralIndicators),
                        EvidenceExamples = System.Text.Json.JsonSerializer.Serialize(attr.EvidenceExamples)
                    });
                    idCounter++;
                }
            }
        }

        return definitions;
    }

    private static List<ProficiencyLevel> ParseApplicableLevels(string? applicableLevels)
    {
        if (string.IsNullOrWhiteSpace(applicableLevels))
            return new List<ProficiencyLevel>();

        return applicableLevels
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => int.TryParse(s, out _))
            .Select(s => (ProficiencyLevel)int.Parse(s))
            .Where(l => l >= ProficiencyLevel.Follow && l <= ProficiencyLevel.SetStrategy)
            .ToList();
    }

    /// <summary>
    /// Get standalone SFIA level reference data (not tied to specific skills)
    /// Useful for display purposes
    /// </summary>
    public static List<SfiaLevelReference> GetSfiaLevelReferences()
    {
        return SfiaLevelAttributes.Attributes
            .OrderBy(a => a.Value.Level)
            .Select(a => new SfiaLevelReference
            {
                Level = (int)a.Key,
                LevelName = a.Value.LevelName,
                Description = a.Value.Description,
                Autonomy = a.Value.Autonomy,
                Influence = a.Value.Influence,
                Complexity = a.Value.Complexity,
                Knowledge = a.Value.Knowledge,
                BusinessSkills = a.Value.BusinessSkills,
                BehavioralIndicators = a.Value.BehavioralIndicators.ToList()
            })
            .ToList();
    }
}

/// <summary>
/// SFIA Level Attribute definition
/// </summary>
public class SfiaLevelAttribute
{
    public int Level { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Autonomy { get; set; } = string.Empty;
    public string Influence { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty;
    public string Knowledge { get; set; } = string.Empty;
    public string BusinessSkills { get; set; } = string.Empty;
    public string[] BehavioralIndicators { get; set; } = Array.Empty<string>();
    public string[] EvidenceExamples { get; set; } = Array.Empty<string>();
}

/// <summary>
/// SFIA Level Reference DTO for API responses
/// </summary>
public class SfiaLevelReference
{
    public int Level { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Autonomy { get; set; } = string.Empty;
    public string Influence { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty;
    public string Knowledge { get; set; } = string.Empty;
    public string BusinessSkills { get; set; } = string.Empty;
    public List<string> BehavioralIndicators { get; set; } = new();
}
