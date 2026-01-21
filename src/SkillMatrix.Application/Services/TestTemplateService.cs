using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Assessment;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Entities.Assessment;
using SkillMatrix.Domain.Enums;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Application.Services;

public class TestTemplateService : ITestTemplateService
{
    private readonly SkillMatrixDbContext _context;

    public TestTemplateService(SkillMatrixDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TestTemplateListDto>> GetAllAsync(
        int pageNumber, int pageSize, string? searchTerm = null, bool includeInactive = false)
    {
        var query = _context.TestTemplates.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(term) ||
                (t.Description != null && t.Description.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Include(t => t.TargetJobRole)
            .Include(t => t.TargetSkill)
            .Include(t => t.Sections)
                .ThenInclude(s => s.Questions)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TestTemplateListDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Type = t.Type,
                TypeName = t.Type.ToString(),
                TargetJobRoleName = t.TargetJobRole != null ? t.TargetJobRole.Name : null,
                TargetSkillName = t.TargetSkill != null ? t.TargetSkill.Name : null,
                TimeLimitMinutes = t.TimeLimitMinutes,
                PassingScore = t.PassingScore,
                IsAiGenerated = t.IsAiGenerated,
                IsActive = t.IsActive,
                SectionCount = t.Sections.Count,
                QuestionCount = t.Sections.SelectMany(s => s.Questions).Count(),
                AssessmentCount = t.Assessments.Count
            })
            .ToListAsync();

        return new PagedResult<TestTemplateListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<TestTemplateDto?> GetByIdAsync(Guid id)
    {
        var template = await _context.TestTemplates
            .Include(t => t.TargetJobRole)
            .Include(t => t.TargetSkill)
            .Include(t => t.Sections.OrderBy(s => s.DisplayOrder))
                .ThenInclude(s => s.Questions)
                    .ThenInclude(q => q.Options.OrderBy(o => o.DisplayOrder))
            .Include(t => t.Sections)
                .ThenInclude(s => s.Questions)
                    .ThenInclude(q => q.Skill)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (template == null)
            return null;

        return MapToDto(template);
    }

    public async Task<TestTemplateDto> CreateAsync(CreateTestTemplateDto dto)
    {
        var template = new TestTemplate
        {
            Title = dto.Title,
            Description = dto.Description,
            Type = dto.Type,
            TargetJobRoleId = dto.TargetJobRoleId,
            TargetSkillId = dto.TargetSkillId,
            TimeLimitMinutes = dto.TimeLimitMinutes,
            PassingScore = dto.PassingScore,
            IsRandomized = dto.IsRandomized,
            MaxQuestions = dto.MaxQuestions,
            RequiresReview = dto.RequiresReview,
            IsActive = true,
            Version = 1
        };

        _context.TestTemplates.Add(template);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(template.Id))!;
    }

    public async Task<TestTemplateDto?> UpdateAsync(Guid id, UpdateTestTemplateDto dto)
    {
        var template = await _context.TestTemplates.FindAsync(id);
        if (template == null)
            return null;

        template.Title = dto.Title;
        template.Description = dto.Description;
        template.Type = dto.Type;
        template.TargetJobRoleId = dto.TargetJobRoleId;
        template.TargetSkillId = dto.TargetSkillId;
        template.TimeLimitMinutes = dto.TimeLimitMinutes;
        template.PassingScore = dto.PassingScore;
        template.IsRandomized = dto.IsRandomized;
        template.MaxQuestions = dto.MaxQuestions;
        template.RequiresReview = dto.RequiresReview;
        template.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var template = await _context.TestTemplates.FindAsync(id);
        if (template == null)
            return false;

        template.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleActiveAsync(Guid id)
    {
        var template = await _context.TestTemplates.FindAsync(id);
        if (template == null)
            return false;

        template.IsActive = !template.IsActive;
        await _context.SaveChangesAsync();
        return true;
    }

    // Section operations
    public async Task<TestSectionDto> CreateSectionAsync(CreateTestSectionDto dto)
    {
        var section = new TestSection
        {
            TestTemplateId = dto.TestTemplateId,
            Title = dto.Title,
            Description = dto.Description,
            DisplayOrder = dto.DisplayOrder,
            TimeLimitMinutes = dto.TimeLimitMinutes
        };

        _context.TestSections.Add(section);
        await _context.SaveChangesAsync();

        return new TestSectionDto
        {
            Id = section.Id,
            TestTemplateId = section.TestTemplateId,
            Title = section.Title,
            Description = section.Description,
            DisplayOrder = section.DisplayOrder,
            TimeLimitMinutes = section.TimeLimitMinutes,
            QuestionCount = 0,
            Questions = new List<QuestionDto>()
        };
    }

    public async Task<TestSectionDto?> UpdateSectionAsync(Guid id, UpdateTestSectionDto dto)
    {
        var section = await _context.TestSections
            .Include(s => s.Questions)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (section == null)
            return null;

        section.Title = dto.Title;
        section.Description = dto.Description;
        section.DisplayOrder = dto.DisplayOrder;
        section.TimeLimitMinutes = dto.TimeLimitMinutes;

        await _context.SaveChangesAsync();

        return new TestSectionDto
        {
            Id = section.Id,
            TestTemplateId = section.TestTemplateId,
            Title = section.Title,
            Description = section.Description,
            DisplayOrder = section.DisplayOrder,
            TimeLimitMinutes = section.TimeLimitMinutes,
            QuestionCount = section.Questions.Count
        };
    }

    public async Task<bool> DeleteSectionAsync(Guid id)
    {
        var section = await _context.TestSections.FindAsync(id);
        if (section == null)
            return false;

        _context.TestSections.Remove(section);
        await _context.SaveChangesAsync();
        return true;
    }

    private TestTemplateDto MapToDto(TestTemplate template)
    {
        return new TestTemplateDto
        {
            Id = template.Id,
            Title = template.Title,
            Description = template.Description,
            Type = template.Type,
            TypeName = template.Type.ToString(),
            TargetJobRoleId = template.TargetJobRoleId,
            TargetJobRoleName = template.TargetJobRole?.Name,
            TargetSkillId = template.TargetSkillId,
            TargetSkillName = template.TargetSkill?.Name,
            TimeLimitMinutes = template.TimeLimitMinutes,
            PassingScore = template.PassingScore,
            IsRandomized = template.IsRandomized,
            MaxQuestions = template.MaxQuestions,
            IsAiGenerated = template.IsAiGenerated,
            RequiresReview = template.RequiresReview,
            IsActive = template.IsActive,
            Version = template.Version,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            SectionCount = template.Sections.Count,
            QuestionCount = template.Sections.SelectMany(s => s.Questions).Count(),
            Sections = template.Sections.Select(s => new TestSectionDto
            {
                Id = s.Id,
                TestTemplateId = s.TestTemplateId,
                Title = s.Title,
                Description = s.Description,
                DisplayOrder = s.DisplayOrder,
                TimeLimitMinutes = s.TimeLimitMinutes,
                QuestionCount = s.Questions.Count,
                Questions = s.Questions.Select(q => MapQuestionToDto(q)).ToList()
            }).ToList()
        };
    }

    private QuestionDto MapQuestionToDto(Question q)
    {
        return new QuestionDto
        {
            Id = q.Id,
            SectionId = q.SectionId,
            SkillId = q.SkillId,
            SkillName = q.Skill?.Name ?? string.Empty,
            SkillCode = q.Skill?.Code ?? string.Empty,
            TargetLevel = q.TargetLevel,
            TargetLevelName = q.TargetLevel.ToString(),
            Type = q.Type,
            TypeName = q.Type.ToString(),
            Content = q.Content,
            CodeSnippet = q.CodeSnippet,
            MediaUrl = q.MediaUrl,
            Points = q.Points,
            TimeLimitSeconds = q.TimeLimitSeconds,
            Difficulty = q.Difficulty,
            DifficultyName = q.Difficulty.ToString(),
            IsAiGenerated = q.IsAiGenerated,
            AiPromptUsed = q.AiPromptUsed,
            IsActive = q.IsActive,
            Tags = !string.IsNullOrEmpty(q.Tags) ? JsonSerializer.Deserialize<List<string>>(q.Tags) : null,
            GradingRubric = q.GradingRubric,
            Options = q.Options.Select(o => new QuestionOptionDto
            {
                Id = o.Id,
                Content = o.Content,
                IsCorrect = o.IsCorrect,
                DisplayOrder = o.DisplayOrder,
                Explanation = o.Explanation
            }).ToList()
        };
    }
}
