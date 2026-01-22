using Microsoft.AspNetCore.Mvc;
using SkillMatrix.Application.DTOs.Assessment;
using SkillMatrix.Application.Interfaces;

namespace SkillMatrix.Api.Controllers;

/// <summary>
/// API endpoints for AI services (generate questions, analyze skills)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IAiQuestionGeneratorService _questionGenerator;
    private readonly IAiSkillAnalyzerService _skillAnalyzer;

    public AiController(
        IAiQuestionGeneratorService questionGenerator,
        IAiSkillAnalyzerService skillAnalyzer)
    {
        _questionGenerator = questionGenerator;
        _skillAnalyzer = skillAnalyzer;
    }

    /// <summary>
    /// Generate questions for a specific skill
    /// </summary>
    [HttpPost("generate-questions")]
    public async Task<ActionResult<AiGenerateQuestionsResponse>> GenerateQuestions(
        [FromBody] AiGenerateQuestionsRequest request)
    {
        if (request.SkillId == Guid.Empty)
        {
            return BadRequest(new AiGenerateQuestionsResponse
            {
                Success = false,
                Error = "SkillId is required"
            });
        }

        if (request.QuestionCount <= 0 || request.QuestionCount > 20)
        {
            return BadRequest(new AiGenerateQuestionsResponse
            {
                Success = false,
                Error = "QuestionCount must be between 1 and 20"
            });
        }

        var result = await _questionGenerator.GenerateQuestionsAsync(request);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Grade essay/coding answer
    /// </summary>
    [HttpPost("grade-answer")]
    public async Task<ActionResult<AiGradeAnswerResponse>> GradeAnswer(
        [FromBody] AiGradeAnswerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.StudentAnswer))
        {
            return BadRequest(new AiGradeAnswerResponse
            {
                Success = false,
                Feedback = "Student answer is required"
            });
        }

        var result = await _questionGenerator.GradeAnswerAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Analyze employee skill gaps
    /// </summary>
    [HttpPost("analyze-skill-gaps")]
    public async Task<ActionResult<AiAnalyzeSkillGapResponse>> AnalyzeSkillGaps(
        [FromBody] AiAnalyzeSkillGapRequest request)
    {
        if (request.EmployeeId == Guid.Empty)
        {
            return BadRequest(new AiAnalyzeSkillGapResponse
            {
                Success = false,
                OverallAssessment = "EmployeeId is required"
            });
        }

        var result = await _skillAnalyzer.AnalyzeSkillGapsAsync(request);
        return Ok(result);
    }
}
