using SkillMatrix.Application.DTOs.Assessment;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Application.Services.AI;

/// <summary>
/// Mock AI Service cho phân tích skill gaps
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
        return $"Kỹ năng {skillName} hiện đang ở mức {current} ({GetLevelDescription(current)}). " +
               $"Để đáp ứng yêu cầu công việc, cần đạt mức {required} ({GetLevelDescription(required)}). " +
               $"Gap này ảnh hưởng đến khả năng làm việc độc lập và đóng góp vào team.";
    }

    private string GenerateMockRecommendation(string skillName, int gapSize)
    {
        if (gapSize >= 3)
            return $"Cần tập trung intensive training cho {skillName}. Đề xuất: mentor 1-1, bootcamp ngắn hạn, và hands-on projects.";
        if (gapSize == 2)
            return $"Kết hợp học online courses và thực hành qua dự án thực tế để nâng cao {skillName}.";
        return $"Có thể nâng cao {skillName} thông qua self-study và code review từ senior.";
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
            Description = $"Khóa học comprehensive về {skillName} từ cơ bản đến nâng cao",
            Url = "https://example.com/course",
            EstimatedHours = gapSize * 20,
            Rationale = $"Khóa học này cover đầy đủ các topic cần thiết để nâng {skillName} lên level yêu cầu"
        });

        if (gapSize >= 2)
        {
            recommendations.Add(new AiLearningRecommendation
            {
                SkillId = skillId,
                SkillName = skillName,
                RecommendationType = "Project",
                Title = $"Hands-on {skillName} Project",
                Description = "Dự án thực hành để áp dụng kiến thức",
                EstimatedHours = gapSize * 15,
                Rationale = "Thực hành qua dự án giúp consolidate kiến thức và build portfolio"
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
                Description = "Được mentor bởi senior engineer",
                EstimatedHours = gapSize * 10,
                Rationale = "Mentorship 1-1 giúp học nhanh hơn và tránh common pitfalls"
            });
        }

        return recommendations;
    }

    private string GenerateOverallAssessment(List<AiSkillGapAnalysis> gaps)
    {
        if (!gaps.Any())
            return "Tuyệt vời! Không phát hiện skill gap đáng kể. Tiếp tục duy trì và phát triển.";

        var criticalCount = gaps.Count(g => g.Priority == "Critical");
        var highCount = gaps.Count(g => g.Priority == "High");

        if (criticalCount > 0)
            return $"Phát hiện {criticalCount} skill gap nghiêm trọng cần được xử lý ngay. " +
                   $"Đề xuất tập trung vào: {string.Join(", ", gaps.Where(g => g.Priority == "Critical").Select(g => g.SkillName))}. " +
                   "Cần có kế hoạch training intensive trong 3-6 tháng tới.";

        if (highCount > 0)
            return $"Có {highCount} skill gap mức độ cao. " +
                   "Nên xây dựng learning path có cấu trúc và theo dõi tiến độ hàng tuần.";

        return $"Phát hiện {gaps.Count} skill gaps nhỏ. Có thể cải thiện qua self-learning và on-the-job training.";
    }

    private string GetLevelDescription(ProficiencyLevel level)
    {
        return level switch
        {
            ProficiencyLevel.None => "Chưa có kinh nghiệm",
            ProficiencyLevel.Follow => "Cần hướng dẫn",
            ProficiencyLevel.Assist => "Có thể hỗ trợ",
            ProficiencyLevel.Apply => "Làm việc độc lập",
            ProficiencyLevel.Enable => "Hướng dẫn người khác",
            ProficiencyLevel.EnsureAdvise => "Tư vấn cấp tổ chức",
            ProficiencyLevel.Initiate => "Khởi xướng chiến lược",
            ProficiencyLevel.SetStrategy => "Định hướng ngành",
            _ => "Không xác định"
        };
    }
}
