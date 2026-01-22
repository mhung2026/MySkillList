using SkillMatrix.Application.DTOs.Assessment;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Application.Services.AI;

/// <summary>
/// Mock AI Service for skill gap analysis
/// </summary>
public class MockAiSkillAnalyzerService : IAiSkillAnalyzerService
{
    private readonly Random _random = new();

    public async Task<AiAnalyzeSkillGapResponse> AnalyzeSkillGapsAsync(AiAnalyzeSkillGapRequest request)
    {
        await Task.Delay(200); // Simulate processing

        var gaps = new List<AiSkillGapAnalysis>();
        var recommendations = new List<AiLearningRecommendation>();

        foreach (var skill in request.CurrentSkills)
        {
            var requiredLevel = skill.RequiredLevel ?? ProficiencyLevel.Apply;
            var gapSize = (int)requiredLevel - (int)skill.CurrentLevel;

            if (gapSize > 0)
            {
                var priority = gapSize switch
                {
                    >= 3 => "Critical",
                    2 => "High",
                    1 => "Medium",
                    _ => "Low"
                };

                gaps.Add(new AiSkillGapAnalysis
                {
                    SkillId = skill.SkillId,
                    SkillName = skill.SkillName,
                    CurrentLevel = skill.CurrentLevel,
                    RequiredLevel = requiredLevel,
                    GapSize = gapSize,
                    Priority = priority,
                    Analysis = GenerateMockAnalysis(skill.SkillName, skill.CurrentLevel, requiredLevel),
                    Recommendation = GenerateMockRecommendation(skill.SkillName, gapSize)
                });

                // Add learning recommendations
                recommendations.AddRange(GenerateMockLearningRecommendations(skill.SkillId, skill.SkillName, gapSize));
            }
        }

        return new AiAnalyzeSkillGapResponse
        {
            Success = true,
            Gaps = gaps.OrderByDescending(g => g.GapSize).ToList(),
            Recommendations = recommendations,
            OverallAssessment = GenerateOverallAssessment(gaps)
        };
    }

    private string GenerateMockAnalysis(string skillName, ProficiencyLevel current, ProficiencyLevel required)
    {
        return $"The {skillName} skill is currently at level {current} ({GetLevelDescription(current)}). " +
               $"To meet job requirements, level {required} ({GetLevelDescription(required)}) is needed. " +
               $"This gap affects the ability to work independently and contribute to the team.";
    }

    private string GenerateMockRecommendation(string skillName, int gapSize)
    {
        if (gapSize >= 3)
            return $"Intensive training required for {skillName}. Recommendations: 1-on-1 mentoring, short bootcamp, and hands-on projects.";
        if (gapSize == 2)
            return $"Combine online courses and real-world project practice to improve {skillName}.";
        return $"Can improve {skillName} through self-study and code reviews from seniors.";
    }

    private List<AiLearningRecommendation> GenerateMockLearningRecommendations(Guid skillId, string skillName, int gapSize)
    {
        var recommendations = new List<AiLearningRecommendation>();

        // Always recommend a course
        recommendations.Add(new AiLearningRecommendation
        {
            SkillId = skillId,
            SkillName = skillName,
            RecommendationType = "Course",
            Title = $"Complete {skillName} Mastery",
            Description = $"Comprehensive {skillName} course from basics to advanced",
            Url = "https://example.com/course",
            EstimatedHours = gapSize * 20,
            Rationale = $"This course covers all necessary topics to bring {skillName} to the required level"
        });

        if (gapSize >= 2)
        {
            recommendations.Add(new AiLearningRecommendation
            {
                SkillId = skillId,
                SkillName = skillName,
                RecommendationType = "Project",
                Title = $"Hands-on {skillName} Project",
                Description = "Practice project to apply knowledge",
                EstimatedHours = gapSize * 15,
                Rationale = "Learning through projects helps consolidate knowledge and build portfolio"
            });
        }

        if (gapSize >= 3)
        {
            recommendations.Add(new AiLearningRecommendation
            {
                SkillId = skillId,
                SkillName = skillName,
                RecommendationType = "Mentorship",
                Title = $"{skillName} Mentorship Program",
                Description = "Mentored by senior engineer",
                EstimatedHours = gapSize * 10,
                Rationale = "1-on-1 mentorship accelerates learning and helps avoid common pitfalls"
            });
        }

        return recommendations;
    }

    private string GenerateOverallAssessment(List<AiSkillGapAnalysis> gaps)
    {
        if (!gaps.Any())
            return "Excellent! No significant skill gaps detected. Continue maintaining and developing skills.";

        var criticalCount = gaps.Count(g => g.Priority == "Critical");
        var highCount = gaps.Count(g => g.Priority == "High");

        if (criticalCount > 0)
            return $"Detected {criticalCount} critical skill gaps requiring immediate attention. " +
                   $"Recommended focus areas: {string.Join(", ", gaps.Where(g => g.Priority == "Critical").Select(g => g.SkillName))}. " +
                   "An intensive training plan is needed for the next 3-6 months.";

        if (highCount > 0)
            return $"Found {highCount} high-priority skill gaps. " +
                   "Build a structured learning path and track progress weekly.";

        return $"Detected {gaps.Count} minor skill gaps. Can be improved through self-learning and on-the-job training.";
    }

    private string GetLevelDescription(ProficiencyLevel level)
    {
        return level switch
        {
            ProficiencyLevel.None => "No experience",
            ProficiencyLevel.Follow => "Needs guidance",
            ProficiencyLevel.Assist => "Can assist",
            ProficiencyLevel.Apply => "Works independently",
            ProficiencyLevel.Enable => "Guides others",
            ProficiencyLevel.EnsureAdvise => "Advises at organizational level",
            ProficiencyLevel.Initiate => "Initiates strategy",
            ProficiencyLevel.SetStrategy => "Sets industry direction",
            _ => "Unknown"
        };
    }
}
