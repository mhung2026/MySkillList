using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Assessment;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Entities.Assessment;
using SkillMatrix.Domain.Enums;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Application.Services;

public class QuestionService : IQuestionService
{
    private readonly SkillMatrixDbContext _context;
    private readonly IAiQuestionGeneratorService _aiService;

    public QuestionService(SkillMatrixDbContext context, IAiQuestionGeneratorService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public async Task<PagedResult<QuestionListDto>> GetAllAsync(QuestionFilterRequest filter)
    {
        var query = _context.Questions
            .Include(q => q.Skill)
            .Include(q => q.Options)
            .AsQueryable();

        if (!filter.IncludeInactive)
        {
            query = query.Where(q => q.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(q => q.Content.ToLower().Contains(term));
        }

        if (filter.SkillId.HasValue)
        {
            query = query.Where(q => q.SkillId == filter.SkillId.Value);
        }

        if (filter.SectionId.HasValue)
        {
            query = query.Where(q => q.SectionId == filter.SectionId.Value);
        }

        if (filter.TargetLevel.HasValue)
        {
            query = query.Where(q => q.TargetLevel == filter.TargetLevel.Value);
        }

        if (filter.Type.HasValue)
        {
            query = query.Where(q => q.Type == filter.Type.Value);
        }

        if (filter.Difficulty.HasValue)
        {
            query = query.Where(q => q.Difficulty == filter.Difficulty.Value);
        }

        if (filter.IsAiGenerated.HasValue)
        {
            query = query.Where(q => q.IsAiGenerated == filter.IsAiGenerated.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(q => new QuestionListDto
            {
                Id = q.Id,
                SkillId = q.SkillId,
                SkillName = q.Skill!.Name,
                SkillCode = q.Skill.Code,
                TargetLevel = q.TargetLevel,
                TargetLevelName = q.TargetLevel.ToString(),
                Type = q.Type,
                TypeName = q.Type.ToString(),
                Content = q.Content,
                Points = q.Points,
                Difficulty = q.Difficulty,
                DifficultyName = q.Difficulty.ToString(),
                IsAiGenerated = q.IsAiGenerated,
                IsActive = q.IsActive,
                OptionCount = q.Options.Count
            })
            .ToListAsync();

        return new PagedResult<QuestionListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<QuestionDto?> GetByIdAsync(Guid id)
    {
        var question = await _context.Questions
            .Include(q => q.Skill)
            .Include(q => q.Section)
            .Include(q => q.Options.OrderBy(o => o.DisplayOrder))
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
            return null;

        return MapToDto(question);
    }

    public async Task<QuestionDto> CreateAsync(CreateQuestionDto dto)
    {
        var question = new Question
        {
            SectionId = dto.SectionId,
            SkillId = dto.SkillId,
            TargetLevel = dto.TargetLevel,
            Type = dto.Type,
            Content = dto.Content,
            CodeSnippet = dto.CodeSnippet,
            MediaUrl = dto.MediaUrl,
            Points = dto.Points,
            TimeLimitSeconds = dto.TimeLimitSeconds,
            Difficulty = dto.Difficulty,
            Tags = dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : null,
            GradingRubric = dto.GradingRubric,
            IsAiGenerated = false,
            IsActive = true
        };

        // Add options for multiple choice questions
        if (dto.Options.Any())
        {
            question.Options = dto.Options.Select((o, index) => new QuestionOption
            {
                Content = o.Content,
                IsCorrect = o.IsCorrect,
                DisplayOrder = o.DisplayOrder > 0 ? o.DisplayOrder : index + 1,
                Explanation = o.Explanation
            }).ToList();
        }

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(question.Id))!;
    }

    public async Task<QuestionDto?> UpdateAsync(Guid id, UpdateQuestionDto dto)
    {
        var question = await _context.Questions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
            return null;

        question.SectionId = dto.SectionId;
        question.SkillId = dto.SkillId;
        question.TargetLevel = dto.TargetLevel;
        question.Type = dto.Type;
        question.Content = dto.Content;
        question.CodeSnippet = dto.CodeSnippet;
        question.MediaUrl = dto.MediaUrl;
        question.Points = dto.Points;
        question.TimeLimitSeconds = dto.TimeLimitSeconds;
        question.Difficulty = dto.Difficulty;
        question.IsActive = dto.IsActive;
        question.Tags = dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : null;
        question.GradingRubric = dto.GradingRubric;

        // Update options - remove old ones and add new ones
        _context.QuestionOptions.RemoveRange(question.Options);

        if (dto.Options.Any())
        {
            question.Options = dto.Options.Select((o, index) => new QuestionOption
            {
                QuestionId = question.Id,
                Content = o.Content,
                IsCorrect = o.IsCorrect,
                DisplayOrder = o.DisplayOrder > 0 ? o.DisplayOrder : index + 1,
                Explanation = o.Explanation
            }).ToList();
        }

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
            return false;

        question.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleActiveAsync(Guid id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
            return false;

        question.IsActive = !question.IsActive;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<QuestionDto>> CreateBulkAsync(List<CreateQuestionDto> dtos)
    {
        var questions = dtos.Select(dto => new Question
        {
            SectionId = dto.SectionId,
            SkillId = dto.SkillId,
            TargetLevel = dto.TargetLevel,
            Type = dto.Type,
            Content = dto.Content,
            CodeSnippet = dto.CodeSnippet,
            MediaUrl = dto.MediaUrl,
            Points = dto.Points,
            TimeLimitSeconds = dto.TimeLimitSeconds,
            Difficulty = dto.Difficulty,
            Tags = dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : null,
            GradingRubric = dto.GradingRubric,
            IsAiGenerated = false,
            IsActive = true,
            Options = dto.Options.Select((o, index) => new QuestionOption
            {
                Content = o.Content,
                IsCorrect = o.IsCorrect,
                DisplayOrder = o.DisplayOrder > 0 ? o.DisplayOrder : index + 1,
                Explanation = o.Explanation
            }).ToList()
        }).ToList();

        _context.Questions.AddRange(questions);
        await _context.SaveChangesAsync();

        var createdQuestions = new List<QuestionDto>();
        foreach (var q in questions)
        {
            var result = await GetByIdAsync(q.Id);
            if (result != null)
                createdQuestions.Add(result);
        }

        return createdQuestions;
    }

    public async Task<List<QuestionDto>> CreateFromAiAsync(Guid sectionId, AiGenerateQuestionsRequest request)
    {
        // Generate questions using AI service
        var aiResponse = await _aiService.GenerateQuestionsAsync(request);

        if (!aiResponse.Success || !aiResponse.Questions.Any())
        {
            return new List<QuestionDto>();
        }

        // Convert AI-generated questions to entities
        var questions = aiResponse.Questions.Select((q, index) => new Question
        {
            SectionId = sectionId,
            SkillId = q.SkillId ?? request.SkillId,  // Nullable - can be null if no skill specified
            TargetLevel = q.TargetLevel ?? request.TargetLevel ?? ProficiencyLevel.Apply,
            Type = q.QuestionType,
            Content = q.Content,
            CodeSnippet = q.CodeSnippet,
            Points = q.SuggestedPoints,
            TimeLimitSeconds = q.SuggestedTimeSeconds,
            Difficulty = q.Difficulty ?? DifficultyLevel.Medium,
            Tags = q.Tags.Where(t => !string.IsNullOrWhiteSpace(t)).Any()
                ? JsonSerializer.Serialize(q.Tags.Where(t => !string.IsNullOrWhiteSpace(t)).ToList())
                : null,
            GradingRubric = q.GradingRubric,
            IsAiGenerated = true,
            AiPromptUsed = aiResponse.Metadata?.PromptUsed,
            IsActive = true,
            Options = q.Options.Select((o, i) => new QuestionOption
            {
                Content = o.Content,
                IsCorrect = o.IsCorrect,
                DisplayOrder = i + 1,
                Explanation = o.Explanation
            }).ToList()
        }).ToList();

        _context.Questions.AddRange(questions);

        // Mark the test template as AI-generated if this is the first AI question
        var section = await _context.TestSections
            .Include(s => s.TestTemplate)
            .FirstOrDefaultAsync(s => s.Id == sectionId);

        if (section?.TestTemplate != null && !section.TestTemplate.IsAiGenerated)
        {
            section.TestTemplate.IsAiGenerated = true;
        }

        await _context.SaveChangesAsync();

        var createdQuestions = new List<QuestionDto>();
        foreach (var q in questions)
        {
            var result = await GetByIdAsync(q.Id);
            if (result != null)
                createdQuestions.Add(result);
        }

        return createdQuestions;
    }

    private QuestionDto MapToDto(Question q)
    {
        return new QuestionDto
        {
            Id = q.Id,
            SectionId = q.SectionId,
            SectionTitle = q.Section?.Title,
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
