using SkillMatrix.Application.DTOs.Assessment;
using SkillMatrix.Application.DTOs.Common;

namespace SkillMatrix.Application.Interfaces;

public interface ITestTemplateService
{
    Task<PagedResult<TestTemplateListDto>> GetAllAsync(int pageNumber, int pageSize, string? searchTerm = null, bool includeInactive = false);
    Task<TestTemplateDto?> GetByIdAsync(Guid id);
    Task<TestTemplateDto> CreateAsync(CreateTestTemplateDto dto);
    Task<TestTemplateDto?> UpdateAsync(Guid id, UpdateTestTemplateDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ToggleActiveAsync(Guid id);

    // Section operations
    Task<TestSectionDto> CreateSectionAsync(CreateTestSectionDto dto);
    Task<TestSectionDto?> UpdateSectionAsync(Guid id, UpdateTestSectionDto dto);
    Task<bool> DeleteSectionAsync(Guid id);
}

public interface IQuestionService
{
    Task<PagedResult<QuestionListDto>> GetAllAsync(QuestionFilterRequest filter);
    Task<QuestionDto?> GetByIdAsync(Guid id);
    Task<QuestionDto> CreateAsync(CreateQuestionDto dto);
    Task<QuestionDto?> UpdateAsync(Guid id, UpdateQuestionDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ToggleActiveAsync(Guid id);

    // Bulk operations for AI-generated questions
    Task<List<QuestionDto>> CreateBulkAsync(List<CreateQuestionDto> dtos);
    Task<List<QuestionDto>> CreateFromAiAsync(Guid sectionId, AiGenerateQuestionsRequest request);
}
