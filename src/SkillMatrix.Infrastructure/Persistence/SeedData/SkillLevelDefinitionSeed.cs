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
                Description = "Có kiến thức cơ bản về cú pháp C#. Có thể viết code đơn giản với sự hướng dẫn.",
                Autonomy = "Làm việc dưới sự giám sát chặt chẽ",
                Influence = "Tác động tối thiểu, chủ yếu học hỏi",
                Complexity = "Thực hiện các task đơn giản, rõ ràng",
                Knowledge = "Hiểu biến, kiểu dữ liệu, vòng lặp, điều kiện cơ bản",
                BehavioralIndicators = "[\"Viết được hàm đơn giản\", \"Hiểu OOP cơ bản\", \"Debug với sự hướng dẫn\"]",
                EvidenceExamples = "[\"Hoàn thành tutorial C# cơ bản\", \"Code được các bài tập cơ bản\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.Assist,
                Description = "Có thể hỗ trợ trong các task C# với sự hướng dẫn. Hiểu OOP và có thể áp dụng patterns cơ bản.",
                Autonomy = "Làm việc với sự hướng dẫn định kỳ",
                Influence = "Tác động đến công việc của bản thân",
                Complexity = "Giải quyết vấn đề có cấu trúc rõ ràng",
                Knowledge = "OOP, LINQ cơ bản, Exception handling, Collections",
                BehavioralIndicators = "[\"Viết code có cấu trúc rõ ràng\", \"Sử dụng LINQ cơ bản\", \"Xử lý exception đúng cách\", \"Tự debug được lỗi thông thường\"]",
                EvidenceExamples = "[\"Hoàn thành feature nhỏ độc lập\", \"Fix bug với hướng dẫn\", \"Viết unit test cơ bản\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.Apply,
                Description = "Áp dụng C# một cách độc lập trong công việc hàng ngày. Hiểu sâu về ngôn ngữ và best practices.",
                Autonomy = "Làm việc độc lập, chỉ cần hướng dẫn khi cần",
                Influence = "Tác động đến team trực tiếp",
                Complexity = "Giải quyết vấn đề phức tạp vừa phải",
                Knowledge = "Async/await, Generics, Reflection, Design Patterns, SOLID principles",
                BehavioralIndicators = "[\"Viết code clean, maintainable\", \"Áp dụng design patterns phù hợp\", \"Tối ưu performance cơ bản\", \"Code review cho junior\"]",
                EvidenceExamples = "[\"Lead feature vừa và nhỏ\", \"Refactor code legacy\", \"Mentor junior developer\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.Enable,
                Description = "Hỗ trợ và nâng cao năng lực C# cho team. Đảm bảo chất lượng code và best practices được tuân thủ.",
                Autonomy = "Hoàn toàn tự chủ, định hướng cho người khác",
                Influence = "Tác động đến nhiều team/project",
                Complexity = "Giải quyết vấn đề phức tạp, đa dạng",
                Knowledge = "Advanced patterns, Performance optimization, Memory management, Multithreading",
                BehavioralIndicators = "[\"Thiết lập coding standards\", \"Review architecture decisions\", \"Troubleshoot vấn đề phức tạp\", \"Training và mentor team\"]",
                EvidenceExamples = "[\"Lead technical decisions cho team\", \"Cải thiện performance đáng kể\", \"Xây dựng shared libraries\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.EnsureAdvise,
                Description = "Đảm bảo và tư vấn về C# ở cấp độ tổ chức. Định hướng chiến lược kỹ thuật.",
                Autonomy = "Định hướng chiến lược",
                Influence = "Tác động cấp tổ chức",
                Complexity = "Giải quyết vấn đề chiến lược, tầm nhìn dài hạn",
                Knowledge = "Language internals, CLR, .NET ecosystem strategy",
                BehavioralIndicators = "[\"Xây dựng roadmap công nghệ\", \"Quyết định adoption của công nghệ mới\", \"Represent công ty ở cộng đồng\"]",
                EvidenceExamples = "[\"Dẫn dắt migration công nghệ\", \"Contribute vào .NET community\", \"Speaking tại tech events\"]"
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
                Description = $"Có kiến thức cơ bản về {skillName}. Cần hướng dẫn để thực hiện công việc.",
                Autonomy = "Làm việc dưới sự giám sát",
                Influence = "Học hỏi và phát triển",
                Complexity = "Tasks đơn giản, rõ ràng"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.Assist,
                Description = $"Có thể hỗ trợ trong các task liên quan đến {skillName}.",
                Autonomy = "Cần hướng dẫn định kỳ",
                Influence = "Tác động đến công việc của mình",
                Complexity = "Vấn đề có cấu trúc"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.Apply,
                Description = $"Áp dụng {skillName} độc lập trong công việc hàng ngày.",
                Autonomy = "Làm việc độc lập",
                Influence = "Tác động đến team",
                Complexity = "Vấn đề phức tạp vừa"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.Enable,
                Description = $"Hỗ trợ và nâng cao năng lực {skillName} cho team.",
                Autonomy = "Tự chủ hoàn toàn",
                Influence = "Nhiều team/project",
                Complexity = "Vấn đề phức tạp cao"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SkillId = skillId,
                Level = ProficiencyLevel.EnsureAdvise,
                Description = $"Tư vấn và định hướng {skillName} ở cấp tổ chức.",
                Autonomy = "Định hướng chiến lược",
                Influence = "Cấp tổ chức",
                Complexity = "Chiến lược dài hạn"
            }
        };
    }
}
