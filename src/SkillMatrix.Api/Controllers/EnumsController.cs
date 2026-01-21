using Microsoft.AspNetCore.Mvc;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Domain.Enums;
using SkillMatrix.Infrastructure.Persistence.SeedData;

namespace SkillMatrix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnumsController : ControllerBase
{
    /// <summary>
    /// Get all enum values for dropdowns
    /// </summary>
    [HttpGet]
    public ActionResult<ApiResponse<EnumValuesDto>> GetAll()
    {
        var result = new EnumValuesDto
        {
            SkillCategories = Enum.GetValues<SkillCategory>()
                .Select(e => new EnumValueDto { Value = (int)e, Name = e.ToString() })
                .ToList(),

            SkillTypes = Enum.GetValues<SkillType>()
                .Select(e => new EnumValueDto { Value = (int)e, Name = e.ToString() })
                .ToList(),

            ProficiencyLevels = Enum.GetValues<ProficiencyLevel>()
                .Select(e => new EnumValueDto
                {
                    Value = (int)e,
                    Name = e.ToString(),
                    Description = GetProficiencyLevelDescription(e)
                })
                .ToList(),

            AssessmentTypes = Enum.GetValues<AssessmentType>()
                .Select(e => new EnumValueDto { Value = (int)e, Name = e.ToString() })
                .ToList(),

            QuestionTypes = Enum.GetValues<QuestionType>()
                .Select(e => new EnumValueDto { Value = (int)e, Name = e.ToString() })
                .ToList(),

            LearningResourceTypes = Enum.GetValues<LearningResourceType>()
                .Select(e => new EnumValueDto { Value = (int)e, Name = e.ToString() })
                .ToList()
        };

        return Ok(ApiResponse<EnumValuesDto>.Ok(result));
    }

    [HttpGet("skill-categories")]
    public ActionResult<ApiResponse<List<EnumValueDto>>> GetSkillCategories()
    {
        var result = Enum.GetValues<SkillCategory>()
            .Select(e => new EnumValueDto { Value = (int)e, Name = e.ToString() })
            .ToList();
        return Ok(ApiResponse<List<EnumValueDto>>.Ok(result));
    }

    [HttpGet("skill-types")]
    public ActionResult<ApiResponse<List<EnumValueDto>>> GetSkillTypes()
    {
        var result = Enum.GetValues<SkillType>()
            .Select(e => new EnumValueDto
            {
                Value = (int)e,
                Name = e.ToString(),
                Description = GetSkillTypeDescription(e)
            })
            .ToList();
        return Ok(ApiResponse<List<EnumValueDto>>.Ok(result));
    }

    [HttpGet("proficiency-levels")]
    public ActionResult<ApiResponse<List<EnumValueDto>>> GetProficiencyLevels()
    {
        var result = Enum.GetValues<ProficiencyLevel>()
            .Select(e => new EnumValueDto
            {
                Value = (int)e,
                Name = e.ToString(),
                Description = GetProficiencyLevelDescription(e)
            })
            .ToList();
        return Ok(ApiResponse<List<EnumValueDto>>.Ok(result));
    }

    /// <summary>
    /// Get detailed SFIA 9 level definitions with Autonomy, Influence, Complexity, etc.
    /// </summary>
    [HttpGet("sfia-levels")]
    public ActionResult<ApiResponse<List<SfiaLevelReference>>> GetSfiaLevels()
    {
        var result = SfiaLevelDefinitionSeed.GetSfiaLevelReferences();
        return Ok(ApiResponse<List<SfiaLevelReference>>.Ok(result));
    }

    /// <summary>
    /// Get SFIA level definition by level number (1-7)
    /// </summary>
    [HttpGet("sfia-levels/{level:int}")]
    public ActionResult<ApiResponse<SfiaLevelReference>> GetSfiaLevel(int level)
    {
        if (level < 1 || level > 7)
            return BadRequest(ApiResponse<SfiaLevelReference>.Fail("Level must be between 1 and 7"));

        var profLevel = (ProficiencyLevel)level;
        if (SfiaLevelDefinitionSeed.SfiaLevelAttributes.Attributes.TryGetValue(profLevel, out var attr))
        {
            var result = new SfiaLevelReference
            {
                Level = level,
                LevelName = attr.LevelName,
                LevelNameVi = attr.LevelNameVi,
                Description = attr.Description,
                DescriptionVi = attr.DescriptionVi,
                Autonomy = attr.Autonomy,
                AutonomyVi = attr.AutonomyVi,
                Influence = attr.Influence,
                InfluenceVi = attr.InfluenceVi,
                Complexity = attr.Complexity,
                ComplexityVi = attr.ComplexityVi,
                Knowledge = attr.Knowledge,
                KnowledgeVi = attr.KnowledgeVi,
                BusinessSkills = attr.BusinessSkills,
                BusinessSkillsVi = attr.BusinessSkillsVi,
                BehavioralIndicators = attr.BehavioralIndicators.ToList(),
                BehavioralIndicatorsVi = attr.BehavioralIndicatorsVi.ToList()
            };
            return Ok(ApiResponse<SfiaLevelReference>.Ok(result));
        }

        return NotFound(ApiResponse<SfiaLevelReference>.Fail($"Level {level} not found"));
    }

    private static string GetProficiencyLevelDescription(ProficiencyLevel level) => level switch
    {
        ProficiencyLevel.None => "Không có kiến thức/kinh nghiệm",
        ProficiencyLevel.Follow => "Làm theo hướng dẫn, đang học",
        ProficiencyLevel.Assist => "Hỗ trợ người khác, phát triển kỹ năng",
        ProficiencyLevel.Apply => "Áp dụng độc lập, hiểu best practices",
        ProficiencyLevel.Enable => "Hỗ trợ người khác, đảm bảo chất lượng",
        ProficiencyLevel.EnsureAdvise => "Đảm bảo/Tư vấn ở cấp tổ chức",
        ProficiencyLevel.Initiate => "Khởi xướng, ảnh hưởng chiến lược",
        ProficiencyLevel.SetStrategy => "Định hướng chiến lược, dẫn đầu ngành",
        _ => ""
    };

    private static string GetSkillTypeDescription(SkillType type) => type switch
    {
        SkillType.Core => "Kỹ năng cốt lõi - Mọi người đều cần (Git, Communication...)",
        SkillType.Specialty => "Kỹ năng chuyên môn - Theo từng role",
        SkillType.Adjacent => "Kỹ năng liên quan - Nice to have, cross-functional",
        _ => ""
    };
}

public class EnumValuesDto
{
    public List<EnumValueDto> SkillCategories { get; set; } = new();
    public List<EnumValueDto> SkillTypes { get; set; } = new();
    public List<EnumValueDto> ProficiencyLevels { get; set; } = new();
    public List<EnumValueDto> AssessmentTypes { get; set; } = new();
    public List<EnumValueDto> QuestionTypes { get; set; } = new();
    public List<EnumValueDto> LearningResourceTypes { get; set; } = new();
}

public class EnumValueDto
{
    public int Value { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
