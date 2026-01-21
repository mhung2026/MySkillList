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
                LevelNameVi = "Theo dõi",
                Description = "Works under close direction. Uses limited discretion in resolving issues or enquiries. Works without frequent reference to others only in relation to specified, routine activities.",
                DescriptionVi = "Làm việc dưới sự chỉ đạo chặt chẽ. Sử dụng quyền tự quyết hạn chế trong việc giải quyết vấn đề hoặc thắc mắc. Chỉ làm việc độc lập với các hoạt động thường xuyên, được chỉ định.",
                Autonomy = "Works under close direction. Uses limited discretion.",
                AutonomyVi = "Làm việc dưới sự giám sát chặt chẽ. Quyền tự quyết hạn chế.",
                Influence = "Minimal influence. May work alone or interact with immediate team.",
                InfluenceVi = "Ảnh hưởng tối thiểu. Có thể làm việc một mình hoặc tương tác với team trực tiếp.",
                Complexity = "Performs routine activities in a structured environment.",
                ComplexityVi = "Thực hiện các hoạt động thường xuyên trong môi trường có cấu trúc.",
                Knowledge = "Has a basic generic knowledge. Applies newly acquired knowledge to develop new skills.",
                KnowledgeVi = "Có kiến thức cơ bản chung. Áp dụng kiến thức mới để phát triển kỹ năng mới.",
                BusinessSkills = "Has sufficient communication skills. Contributes to identifying own development opportunities.",
                BusinessSkillsVi = "Có kỹ năng giao tiếp đủ dùng. Góp phần xác định cơ hội phát triển của bản thân.",
                BehavioralIndicators = new[]
                {
                    "Follows instructions and guidelines",
                    "Asks questions when uncertain",
                    "Completes assigned tasks on time",
                    "Documents work as required",
                    "Participates in learning activities"
                },
                BehavioralIndicatorsVi = new[]
                {
                    "Tuân theo hướng dẫn và quy trình",
                    "Hỏi khi không chắc chắn",
                    "Hoàn thành công việc được giao đúng hạn",
                    "Ghi chép công việc theo yêu cầu",
                    "Tham gia các hoạt động học tập"
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
                LevelNameVi = "Hỗ trợ",
                Description = "Works under routine direction. Uses limited discretion in resolving issues or enquiries. Determines when to seek guidance in unexpected situations.",
                DescriptionVi = "Làm việc dưới sự chỉ đạo thường xuyên. Sử dụng quyền tự quyết hạn chế. Biết khi nào cần tìm kiếm hướng dẫn trong các tình huống bất ngờ.",
                Autonomy = "Works under routine direction within a clear framework of accountability.",
                AutonomyVi = "Làm việc dưới sự chỉ đạo thường xuyên trong khuôn khổ trách nhiệm rõ ràng.",
                Influence = "Interacts with and may influence immediate colleagues. May have some external contact.",
                InfluenceVi = "Tương tác và có thể ảnh hưởng đến đồng nghiệp trực tiếp. Có thể có một số liên hệ bên ngoài.",
                Complexity = "Performs a range of work activities in varied environments.",
                ComplexityVi = "Thực hiện nhiều hoạt động công việc trong các môi trường đa dạng.",
                Knowledge = "Demonstrates application of essential generic knowledge typically found in the industry body of knowledge.",
                KnowledgeVi = "Thể hiện việc áp dụng kiến thức chung thiết yếu thường được tìm thấy trong lĩnh vực.",
                BusinessSkills = "Has sufficient communication skills. Plans and monitors own work. Aware of need to adhere to quality standards.",
                BusinessSkillsVi = "Có kỹ năng giao tiếp đủ dùng. Lập kế hoạch và theo dõi công việc của mình. Nhận thức được nhu cầu tuân thủ tiêu chuẩn chất lượng.",
                BehavioralIndicators = new[]
                {
                    "Assists more experienced practitioners",
                    "Performs defined tasks under supervision",
                    "Uses established methods and procedures",
                    "Identifies when to escalate issues",
                    "Contributes to team activities"
                },
                BehavioralIndicatorsVi = new[]
                {
                    "Hỗ trợ những người có kinh nghiệm hơn",
                    "Thực hiện các nhiệm vụ được xác định dưới sự giám sát",
                    "Sử dụng các phương pháp và quy trình đã thiết lập",
                    "Xác định khi nào cần báo cáo vấn đề",
                    "Đóng góp vào các hoạt động của team"
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
                LevelNameVi = "Áp dụng",
                Description = "Works under general direction. Uses discretion in identifying and responding to complex issues and assignments. Receives specific direction, accepts guidance and has work reviewed at agreed milestones.",
                DescriptionVi = "Làm việc dưới sự chỉ đạo chung. Sử dụng quyền tự quyết trong việc xác định và phản hồi các vấn đề và nhiệm vụ phức tạp. Nhận chỉ đạo cụ thể, chấp nhận hướng dẫn và được đánh giá công việc tại các mốc đã thỏa thuận.",
                Autonomy = "Works under general direction. Determines own approach within defined frameworks.",
                AutonomyVi = "Làm việc dưới sự chỉ đạo chung. Xác định cách tiếp cận của riêng mình trong các khuôn khổ đã định.",
                Influence = "Interacts with and influences team members and business stakeholders. May supervise others.",
                InfluenceVi = "Tương tác và ảnh hưởng đến các thành viên trong team và các bên liên quan kinh doanh. Có thể giám sát người khác.",
                Complexity = "Performs a range of work, sometimes complex and non-routine.",
                ComplexityVi = "Thực hiện nhiều công việc, đôi khi phức tạp và không thường xuyên.",
                Knowledge = "Demonstrates effective application of knowledge. Has a sound generic knowledge and domain specific knowledge.",
                KnowledgeVi = "Thể hiện việc áp dụng kiến thức hiệu quả. Có kiến thức chung vững chắc và kiến thức chuyên ngành.",
                BusinessSkills = "Communicates effectively. Takes initiative in identifying and negotiating appropriate development opportunities.",
                BusinessSkillsVi = "Giao tiếp hiệu quả. Chủ động xác định và đàm phán các cơ hội phát triển phù hợp.",
                BehavioralIndicators = new[]
                {
                    "Works independently on assigned tasks",
                    "Applies established methods appropriately",
                    "Resolves routine issues without supervision",
                    "Collaborates with team members effectively",
                    "Identifies improvements to processes"
                },
                BehavioralIndicatorsVi = new[]
                {
                    "Làm việc độc lập với các nhiệm vụ được giao",
                    "Áp dụng các phương pháp đã thiết lập một cách phù hợp",
                    "Giải quyết các vấn đề thường xuyên mà không cần giám sát",
                    "Hợp tác hiệu quả với các thành viên trong team",
                    "Xác định các cải tiến cho quy trình"
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
                LevelNameVi = "Hỗ trợ phát triển",
                Description = "Works under broad direction. Work is often self-initiated. Is fully responsible for meeting allocated technical and/or project/supervisory objectives.",
                DescriptionVi = "Làm việc dưới sự chỉ đạo rộng. Công việc thường tự khởi xướng. Hoàn toàn chịu trách nhiệm đáp ứng các mục tiêu kỹ thuật và/hoặc dự án/giám sát được phân bổ.",
                Autonomy = "Works under broad direction. Full accountability for own technical work or project/supervisory responsibilities.",
                AutonomyVi = "Làm việc dưới sự chỉ đạo rộng. Hoàn toàn chịu trách nhiệm về công việc kỹ thuật hoặc trách nhiệm dự án/giám sát của mình.",
                Influence = "Influences team and specialist peers. Contributes to decision-making at business unit level.",
                InfluenceVi = "Ảnh hưởng đến team và các đồng nghiệp chuyên gia. Đóng góp vào việc ra quyết định ở cấp đơn vị kinh doanh.",
                Complexity = "Work includes complex technical activities. Contributes to implementing change.",
                ComplexityVi = "Công việc bao gồm các hoạt động kỹ thuật phức tạp. Đóng góp vào việc triển khai thay đổi.",
                Knowledge = "Has a thorough understanding of recognised generic industry bodies of knowledge and specialist bodies of knowledge.",
                KnowledgeVi = "Có hiểu biết sâu sắc về các lĩnh vực kiến thức chung được công nhận trong ngành và các lĩnh vực kiến thức chuyên gia.",
                BusinessSkills = "Communicates fluently. Facilitates collaboration. Plans and directs own work and that of others.",
                BusinessSkillsVi = "Giao tiếp lưu loát. Tạo điều kiện hợp tác. Lập kế hoạch và chỉ đạo công việc của mình và của người khác.",
                BehavioralIndicators = new[]
                {
                    "Takes ownership of significant deliverables",
                    "Enables and supports team members",
                    "Defines approaches and methods",
                    "Reviews and improves team practices",
                    "Manages stakeholder expectations"
                },
                BehavioralIndicatorsVi = new[]
                {
                    "Chịu trách nhiệm về các sản phẩm quan trọng",
                    "Hỗ trợ và tạo điều kiện cho các thành viên trong team",
                    "Xác định các phương pháp và cách tiếp cận",
                    "Đánh giá và cải thiện các thực hành của team",
                    "Quản lý kỳ vọng của các bên liên quan"
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
                LevelNameVi = "Đảm bảo, tư vấn",
                Description = "Works under broad direction. Work is often self-initiated. Is accountable for actions and decisions taken in achieving significant results for a defined area.",
                DescriptionVi = "Làm việc dưới sự chỉ đạo rộng. Công việc thường tự khởi xướng. Chịu trách nhiệm về các hành động và quyết định để đạt được kết quả quan trọng cho một lĩnh vực xác định.",
                Autonomy = "Works under broad direction. Accountable for significant results. Influences strategy.",
                AutonomyVi = "Làm việc dưới sự chỉ đạo rộng. Chịu trách nhiệm về kết quả quan trọng. Ảnh hưởng đến chiến lược.",
                Influence = "Influences policy and strategy. Has significant influence across organisation.",
                InfluenceVi = "Ảnh hưởng đến chính sách và chiến lược. Có ảnh hưởng đáng kể trong tổ chức.",
                Complexity = "Performs highly complex work activities. Leads on the implementation of strategy.",
                ComplexityVi = "Thực hiện các hoạt động công việc rất phức tạp. Dẫn dắt việc triển khai chiến lược.",
                Knowledge = "Has a deep understanding of recognised industry knowledge. Develops new knowledge and extends industry body of knowledge.",
                KnowledgeVi = "Có hiểu biết sâu sắc về kiến thức ngành được công nhận. Phát triển kiến thức mới và mở rộng lĩnh vực kiến thức của ngành.",
                BusinessSkills = "Demonstrates leadership. Advises on the application of standards and methods. Ensures compliance.",
                BusinessSkillsVi = "Thể hiện khả năng lãnh đạo. Tư vấn về việc áp dụng các tiêu chuẩn và phương pháp. Đảm bảo tuân thủ.",
                BehavioralIndicators = new[]
                {
                    "Ensures quality and standards across area",
                    "Advises on complex technical decisions",
                    "Drives improvements at organisational level",
                    "Represents organisation externally",
                    "Develops organisational capabilities"
                },
                BehavioralIndicatorsVi = new[]
                {
                    "Đảm bảo chất lượng và tiêu chuẩn trong lĩnh vực",
                    "Tư vấn về các quyết định kỹ thuật phức tạp",
                    "Thúc đẩy cải tiến ở cấp tổ chức",
                    "Đại diện tổ chức với bên ngoài",
                    "Phát triển năng lực tổ chức"
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
                LevelNameVi = "Khởi xướng, ảnh hưởng",
                Description = "Has defined authority and accountability for actions and decisions within a significant area of work. Sets organisational strategy and advances industry knowledge.",
                DescriptionVi = "Có thẩm quyền và trách nhiệm xác định cho các hành động và quyết định trong một lĩnh vực công việc quan trọng. Đặt chiến lược tổ chức và nâng cao kiến thức ngành.",
                Autonomy = "Has defined authority for a significant area of work. Makes decisions critical to success.",
                AutonomyVi = "Có thẩm quyền xác định cho một lĩnh vực công việc quan trọng. Đưa ra các quyết định quan trọng cho sự thành công.",
                Influence = "Influences industry direction. Shapes policy and advances the profession.",
                InfluenceVi = "Ảnh hưởng đến hướng đi của ngành. Định hình chính sách và nâng cao nghề nghiệp.",
                Complexity = "Leads on initiating and implementing strategy. Addresses problems that impact the wider organisation.",
                ComplexityVi = "Dẫn dắt việc khởi xướng và triển khai chiến lược. Giải quyết các vấn đề ảnh hưởng đến tổ chức rộng hơn.",
                Knowledge = "Recognised as an expert. Has an extensive understanding and contributes to the evolution of knowledge.",
                KnowledgeVi = "Được công nhận là chuyên gia. Có hiểu biết rộng và đóng góp vào sự phát triển của kiến thức.",
                BusinessSkills = "Leads, manages and develops people. Develops strategic alliances. Communicates with authority at all levels.",
                BusinessSkillsVi = "Lãnh đạo, quản lý và phát triển con người. Phát triển các liên minh chiến lược. Giao tiếp có thẩm quyền ở mọi cấp độ.",
                BehavioralIndicators = new[]
                {
                    "Initiates strategic direction for the organisation",
                    "Influences industry standards and practices",
                    "Develops future leaders and experts",
                    "Drives innovation at scale",
                    "Builds strategic partnerships"
                },
                BehavioralIndicatorsVi = new[]
                {
                    "Khởi xướng định hướng chiến lược cho tổ chức",
                    "Ảnh hưởng đến các tiêu chuẩn và thực hành của ngành",
                    "Phát triển các nhà lãnh đạo và chuyên gia tương lai",
                    "Thúc đẩy đổi mới ở quy mô lớn",
                    "Xây dựng các quan hệ đối tác chiến lược"
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
                LevelNameVi = "Đặt chiến lược, truyền cảm hứng, huy động",
                Description = "Has authority and accountability for all aspects of a significant area of work including strategy, policy and operational responsibility. Sets direction, policy and strategy.",
                DescriptionVi = "Có thẩm quyền và trách nhiệm về tất cả các khía cạnh của một lĩnh vực công việc quan trọng bao gồm chiến lược, chính sách và trách nhiệm hoạt động. Đặt hướng đi, chính sách và chiến lược.",
                Autonomy = "The highest level of authority and responsibility. Accountable for overall success and policy.",
                AutonomyVi = "Cấp độ thẩm quyền và trách nhiệm cao nhất. Chịu trách nhiệm về sự thành công và chính sách tổng thể.",
                Influence = "Sets and advances the strategy of the organisation and/or industry. Highest levels of influence.",
                InfluenceVi = "Đặt và thúc đẩy chiến lược của tổ chức và/hoặc ngành. Mức độ ảnh hưởng cao nhất.",
                Complexity = "Leads on the formulation and implementation of strategy. Makes decisions critical to organisational success.",
                ComplexityVi = "Dẫn dắt việc xây dựng và triển khai chiến lược. Đưa ra các quyết định quan trọng cho sự thành công của tổ chức.",
                Knowledge = "Leads the advancement of knowledge. Applies the highest levels of industry knowledge and business acumen.",
                KnowledgeVi = "Dẫn dắt sự tiến bộ của kiến thức. Áp dụng mức độ cao nhất của kiến thức ngành và sự nhạy bén kinh doanh.",
                BusinessSkills = "Inspires others. Drives culture and values. Has outstanding leadership and communication skills.",
                BusinessSkillsVi = "Truyền cảm hứng cho người khác. Thúc đẩy văn hóa và giá trị. Có kỹ năng lãnh đạo và giao tiếp xuất sắc.",
                BehavioralIndicators = new[]
                {
                    "Sets strategic direction for the organisation/industry",
                    "Inspires and mobilises large-scale change",
                    "Shapes industry evolution and standards",
                    "Develops organisation-wide capabilities",
                    "Represents organisation at highest levels"
                },
                BehavioralIndicatorsVi = new[]
                {
                    "Đặt hướng chiến lược cho tổ chức/ngành",
                    "Truyền cảm hứng và huy động sự thay đổi quy mô lớn",
                    "Định hình sự phát triển và tiêu chuẩn của ngành",
                    "Phát triển năng lực toàn tổ chức",
                    "Đại diện tổ chức ở cấp cao nhất"
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
                        Description = $"[{skill.Name}] {attr.DescriptionVi}",
                        Autonomy = attr.AutonomyVi,
                        Influence = attr.InfluenceVi,
                        Complexity = attr.ComplexityVi,
                        Knowledge = attr.KnowledgeVi,
                        BusinessSkills = attr.BusinessSkillsVi,
                        BehavioralIndicators = System.Text.Json.JsonSerializer.Serialize(attr.BehavioralIndicatorsVi),
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
                LevelNameVi = a.Value.LevelNameVi,
                Description = a.Value.Description,
                DescriptionVi = a.Value.DescriptionVi,
                Autonomy = a.Value.Autonomy,
                AutonomyVi = a.Value.AutonomyVi,
                Influence = a.Value.Influence,
                InfluenceVi = a.Value.InfluenceVi,
                Complexity = a.Value.Complexity,
                ComplexityVi = a.Value.ComplexityVi,
                Knowledge = a.Value.Knowledge,
                KnowledgeVi = a.Value.KnowledgeVi,
                BusinessSkills = a.Value.BusinessSkills,
                BusinessSkillsVi = a.Value.BusinessSkillsVi,
                BehavioralIndicators = a.Value.BehavioralIndicators.ToList(),
                BehavioralIndicatorsVi = a.Value.BehavioralIndicatorsVi.ToList()
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
    public string LevelNameVi { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionVi { get; set; } = string.Empty;
    public string Autonomy { get; set; } = string.Empty;
    public string AutonomyVi { get; set; } = string.Empty;
    public string Influence { get; set; } = string.Empty;
    public string InfluenceVi { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty;
    public string ComplexityVi { get; set; } = string.Empty;
    public string Knowledge { get; set; } = string.Empty;
    public string KnowledgeVi { get; set; } = string.Empty;
    public string BusinessSkills { get; set; } = string.Empty;
    public string BusinessSkillsVi { get; set; } = string.Empty;
    public string[] BehavioralIndicators { get; set; } = Array.Empty<string>();
    public string[] BehavioralIndicatorsVi { get; set; } = Array.Empty<string>();
    public string[] EvidenceExamples { get; set; } = Array.Empty<string>();
}

/// <summary>
/// SFIA Level Reference DTO for API responses
/// </summary>
public class SfiaLevelReference
{
    public int Level { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public string LevelNameVi { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionVi { get; set; } = string.Empty;
    public string Autonomy { get; set; } = string.Empty;
    public string AutonomyVi { get; set; } = string.Empty;
    public string Influence { get; set; } = string.Empty;
    public string InfluenceVi { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty;
    public string ComplexityVi { get; set; } = string.Empty;
    public string Knowledge { get; set; } = string.Empty;
    public string KnowledgeVi { get; set; } = string.Empty;
    public string BusinessSkills { get; set; } = string.Empty;
    public string BusinessSkillsVi { get; set; } = string.Empty;
    public List<string> BehavioralIndicators { get; set; } = new();
    public List<string> BehavioralIndicatorsVi { get; set; } = new();
}
