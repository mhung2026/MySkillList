using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Assessment;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Entities.Assessment;
using SkillMatrix.Domain.Enums;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Application.Services;

public class AssessmentService : IAssessmentService
{
    private readonly SkillMatrixDbContext _context;
    private readonly IAiQuestionGeneratorService _aiService;

    public AssessmentService(SkillMatrixDbContext context, IAiQuestionGeneratorService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public async Task<PagedResult<AssessmentListDto>> GetByEmployeeAsync(Guid employeeId, int pageNumber, int pageSize)
    {
        var query = _context.Assessments
            .Where(a => a.EmployeeId == employeeId)
            .Include(a => a.Employee)
            .Include(a => a.TestTemplate)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AssessmentListDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee.FullName,
                Type = a.Type,
                TypeName = a.Type.ToString(),
                Status = a.Status,
                StatusName = a.Status.ToString(),
                Title = a.Title,
                TestTemplateTitle = a.TestTemplate != null ? a.TestTemplate.Title : null,
                Score = a.Score,
                MaxScore = a.MaxScore,
                Percentage = a.Percentage,
                StartedAt = a.StartedAt,
                CompletedAt = a.CompletedAt,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<AssessmentListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AssessmentDto?> GetByIdAsync(Guid id)
    {
        var assessment = await _context.Assessments
            .Include(a => a.Employee)
            .Include(a => a.Assessor)
            .Include(a => a.TestTemplate)
            .Include(a => a.Responses)
                .ThenInclude(r => r.Question)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assessment == null)
            return null;

        return MapToDto(assessment);
    }

    public async Task<StartAssessmentResponse> StartAssessmentAsync(StartAssessmentRequest request)
    {
        // Get test template with all questions
        var template = await _context.TestTemplates
            .Include(t => t.Sections.OrderBy(s => s.DisplayOrder))
                .ThenInclude(s => s.Questions.Where(q => q.IsActive))
                    .ThenInclude(q => q.Options.OrderBy(o => o.DisplayOrder))
            .Include(t => t.Sections)
                .ThenInclude(s => s.Questions)
                    .ThenInclude(q => q.Skill)
            .FirstOrDefaultAsync(t => t.Id == request.TestTemplateId && t.IsActive);

        if (template == null)
            throw new InvalidOperationException("Test template not found or inactive");

        // Create new assessment
        var assessment = new Domain.Entities.Assessment.Assessment
        {
            EmployeeId = request.EmployeeId,
            Type = template.Type,
            Status = AssessmentStatus.InProgress,
            Title = request.Title ?? template.Title,
            Description = template.Description,
            TestTemplateId = template.Id,
            StartedAt = DateTime.UtcNow
        };

        _context.Assessments.Add(assessment);
        await _context.SaveChangesAsync();

        // Calculate totals
        var allQuestions = template.Sections
            .SelectMany(s => s.Questions.Where(q => q.IsActive))
            .ToList();

        var totalQuestions = allQuestions.Count;
        var totalPoints = allQuestions.Sum(q => q.Points);

        // Build response
        var questionNumber = 0;
        var sections = template.Sections.Select(s => new TestSectionWithQuestionsDto
        {
            Id = s.Id,
            Title = s.Title,
            Description = s.Description,
            DisplayOrder = s.DisplayOrder,
            TimeLimitMinutes = s.TimeLimitMinutes,
            Questions = s.Questions.Where(q => q.IsActive).Select(q => new QuestionForTestDto
            {
                Id = q.Id,
                QuestionNumber = ++questionNumber,
                Type = q.Type,
                TypeName = q.Type.ToString(),
                Content = q.Content,
                CodeSnippet = q.CodeSnippet,
                MediaUrl = q.MediaUrl,
                Points = q.Points,
                TimeLimitSeconds = q.TimeLimitSeconds,
                SkillName = q.Skill?.Name ?? string.Empty,
                Options = q.Options.Select(o => new OptionForTestDto
                {
                    Id = o.Id,
                    Content = o.Content,
                    DisplayOrder = o.DisplayOrder
                }).ToList()
            }).ToList()
        }).ToList();

        return new StartAssessmentResponse
        {
            AssessmentId = assessment.Id,
            Title = assessment.Title ?? template.Title,
            Description = template.Description,
            TimeLimitMinutes = template.TimeLimitMinutes,
            TotalQuestions = totalQuestions,
            TotalPoints = totalPoints,
            StartedAt = assessment.StartedAt!.Value,
            MustCompleteBy = template.TimeLimitMinutes.HasValue
                ? assessment.StartedAt!.Value.AddMinutes(template.TimeLimitMinutes.Value)
                : null,
            Sections = sections
        };
    }

    public async Task<StartAssessmentResponse?> GetInProgressAssessmentAsync(Guid assessmentId)
    {
        var assessment = await _context.Assessments
            .Include(a => a.TestTemplate)
                .ThenInclude(t => t!.Sections.OrderBy(s => s.DisplayOrder))
                    .ThenInclude(s => s.Questions.Where(q => q.IsActive))
                        .ThenInclude(q => q.Options.OrderBy(o => o.DisplayOrder))
            .Include(a => a.TestTemplate)
                .ThenInclude(t => t!.Sections)
                    .ThenInclude(s => s.Questions)
                        .ThenInclude(q => q.Skill)
            .Include(a => a.Responses)
            .FirstOrDefaultAsync(a => a.Id == assessmentId && a.Status == AssessmentStatus.InProgress);

        if (assessment?.TestTemplate == null)
            return null;

        var template = assessment.TestTemplate;
        var existingResponses = assessment.Responses.ToDictionary(r => r.QuestionId);

        var allQuestions = template.Sections
            .SelectMany(s => s.Questions.Where(q => q.IsActive))
            .ToList();

        var questionNumber = 0;
        var sections = template.Sections.Select(s => new TestSectionWithQuestionsDto
        {
            Id = s.Id,
            Title = s.Title,
            Description = s.Description,
            DisplayOrder = s.DisplayOrder,
            TimeLimitMinutes = s.TimeLimitMinutes,
            Questions = s.Questions.Where(q => q.IsActive).Select(q => new QuestionForTestDto
            {
                Id = q.Id,
                QuestionNumber = ++questionNumber,
                Type = q.Type,
                TypeName = q.Type.ToString(),
                Content = q.Content,
                CodeSnippet = q.CodeSnippet,
                MediaUrl = q.MediaUrl,
                Points = q.Points,
                TimeLimitSeconds = q.TimeLimitSeconds,
                SkillName = q.Skill?.Name ?? string.Empty,
                Options = q.Options.Select(o => new OptionForTestDto
                {
                    Id = o.Id,
                    Content = o.Content,
                    DisplayOrder = o.DisplayOrder
                }).ToList()
            }).ToList()
        }).ToList();

        return new StartAssessmentResponse
        {
            AssessmentId = assessment.Id,
            Title = assessment.Title ?? template.Title,
            Description = template.Description,
            TimeLimitMinutes = template.TimeLimitMinutes,
            TotalQuestions = allQuestions.Count,
            TotalPoints = allQuestions.Sum(q => q.Points),
            StartedAt = assessment.StartedAt!.Value,
            MustCompleteBy = template.TimeLimitMinutes.HasValue
                ? assessment.StartedAt!.Value.AddMinutes(template.TimeLimitMinutes.Value)
                : null,
            Sections = sections
        };
    }

    public async Task<SubmitAnswerResponse> SubmitAnswerAsync(SubmitAnswerRequest request)
    {
        var assessment = await _context.Assessments
            .FirstOrDefaultAsync(a => a.Id == request.AssessmentId && a.Status == AssessmentStatus.InProgress);

        if (assessment == null)
            throw new InvalidOperationException("Assessment not found or not in progress");

        var question = await _context.Questions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId);

        if (question == null)
            throw new InvalidOperationException("Question not found");

        // Check if already answered
        var existingResponse = await _context.AssessmentResponses
            .FirstOrDefaultAsync(r => r.AssessmentId == request.AssessmentId && r.QuestionId == request.QuestionId);

        AssessmentResponse response;
        if (existingResponse != null)
        {
            response = existingResponse;
        }
        else
        {
            response = new AssessmentResponse
            {
                AssessmentId = request.AssessmentId,
                QuestionId = request.QuestionId
            };
            _context.AssessmentResponses.Add(response);
        }

        // Update response
        response.TextResponse = request.TextResponse;
        response.CodeResponse = request.CodeResponse;
        response.SelectedOptions = request.SelectedOptionIds != null
            ? JsonSerializer.Serialize(request.SelectedOptionIds)
            : null;
        response.AnsweredAt = DateTime.UtcNow;
        response.TimeSpentSeconds = request.TimeSpentSeconds;

        // Auto-grade for multiple choice
        if (question.Type == QuestionType.MultipleChoice ||
            question.Type == QuestionType.MultipleAnswer ||
            question.Type == QuestionType.TrueFalse)
        {
            var correctOptionIds = question.Options
                .Where(o => o.IsCorrect)
                .Select(o => o.Id)
                .ToHashSet();

            var selectedIds = request.SelectedOptionIds?.ToHashSet() ?? new HashSet<Guid>();

            response.IsCorrect = correctOptionIds.SetEquals(selectedIds);
            response.PointsAwarded = response.IsCorrect == true ? question.Points : 0;
        }

        await _context.SaveChangesAsync();

        return new SubmitAnswerResponse
        {
            Success = true,
            ResponseId = response.Id,
            IsCorrect = response.IsCorrect,
            PointsAwarded = response.PointsAwarded,
            Feedback = response.IsCorrect.HasValue
                ? (response.IsCorrect.Value ? "Correct!" : "Incorrect")
                : "Answer saved. Will be graded later."
        };
    }

    public async Task<AssessmentResultDto> SubmitAssessmentAsync(Guid assessmentId)
    {
        var assessment = await _context.Assessments
            .Include(a => a.TestTemplate)
                .ThenInclude(t => t!.Sections)
                    .ThenInclude(s => s.Questions)
                        .ThenInclude(q => q.Options)
            .Include(a => a.TestTemplate)
                .ThenInclude(t => t!.Sections)
                    .ThenInclude(s => s.Questions)
                        .ThenInclude(q => q.Skill)
            .Include(a => a.Responses)
            .FirstOrDefaultAsync(a => a.Id == assessmentId);

        if (assessment == null)
            throw new InvalidOperationException("Assessment not found");

        if (assessment.Status != AssessmentStatus.InProgress)
            throw new InvalidOperationException("Assessment is not in progress");

        // Calculate scores
        var allQuestions = assessment.TestTemplate!.Sections
            .SelectMany(s => s.Questions.Where(q => q.IsActive))
            .ToList();

        var responses = assessment.Responses.ToDictionary(r => r.QuestionId);

        int totalScore = 0;
        int maxScore = 0;
        int correctAnswers = 0;
        int wrongAnswers = 0;
        int unanswered = 0;
        int pendingReview = 0;

        var questionResults = new List<QuestionResultDto>();
        var skillScores = new Dictionary<Guid, (string Name, string Code, int Correct, int Total, int Score, int MaxScore)>();

        var questionNumber = 0;
        foreach (var question in allQuestions)
        {
            questionNumber++;
            maxScore += question.Points;

            // Handle nullable SkillId - only track skill scores if skill is specified
            var skillId = question.SkillId;
            (string Name, string Code, int Correct, int Total, int Score, int MaxScore) skillData = ("", "", 0, 0, 0, 0);
            if (skillId.HasValue)
            {
                if (!skillScores.ContainsKey(skillId.Value))
                {
                    skillScores[skillId.Value] = (question.Skill?.Name ?? "", question.Skill?.Code ?? "", 0, 0, 0, 0);
                }
                skillData = skillScores[skillId.Value];
            }
            skillData.Total++;
            skillData.MaxScore += question.Points;

            responses.TryGetValue(question.Id, out var response);

            var questionResult = new QuestionResultDto
            {
                QuestionId = question.Id,
                QuestionNumber = questionNumber,
                Content = question.Content,
                CodeSnippet = question.CodeSnippet,
                Type = question.Type,
                TypeName = question.Type.ToString(),
                SkillName = question.Skill?.Name ?? "",
                Points = question.Points,
                Options = question.Options.Select(o => new OptionResultDto
                {
                    Id = o.Id,
                    Content = o.Content,
                    IsCorrect = o.IsCorrect,
                    WasSelected = response != null &&
                        !string.IsNullOrEmpty(response.SelectedOptions) &&
                        JsonSerializer.Deserialize<List<Guid>>(response.SelectedOptions)?.Contains(o.Id) == true,
                    Explanation = o.Explanation
                }).ToList()
            };

            if (response == null)
            {
                unanswered++;
                questionResult.IsCorrect = false;
                questionResult.PointsAwarded = 0;
            }
            else
            {
                questionResult.UserAnswer = response.TextResponse ?? response.CodeResponse;
                if (!string.IsNullOrEmpty(response.SelectedOptions))
                {
                    questionResult.SelectedOptionIds = JsonSerializer.Deserialize<List<Guid>>(response.SelectedOptions);
                }

                if (response.IsCorrect.HasValue)
                {
                    if (response.IsCorrect.Value)
                    {
                        correctAnswers++;
                        totalScore += response.PointsAwarded ?? question.Points;
                        skillData.Correct++;
                        skillData.Score += response.PointsAwarded ?? question.Points;
                    }
                    else
                    {
                        wrongAnswers++;
                    }
                    questionResult.IsCorrect = response.IsCorrect;
                    questionResult.PointsAwarded = response.PointsAwarded;
                }
                else
                {
                    pendingReview++;
                }
            }

            // Set correct answer info
            var correctOptions = question.Options.Where(o => o.IsCorrect).ToList();
            questionResult.CorrectOptionIds = correctOptions.Select(o => o.Id).ToList();
            questionResult.CorrectAnswer = string.Join(", ", correctOptions.Select(o => o.Content));

            // Get explanation from correct option
            questionResult.Explanation = correctOptions.FirstOrDefault()?.Explanation;

            // Update skill scores if skill is specified
            if (skillId.HasValue)
            {
                skillScores[skillId.Value] = skillData;
            }
            questionResults.Add(questionResult);
        }

        // Update assessment
        assessment.Status = pendingReview > 0 ? AssessmentStatus.Completed : AssessmentStatus.Reviewed;
        assessment.CompletedAt = DateTime.UtcNow;
        assessment.Score = totalScore;
        assessment.MaxScore = maxScore;
        assessment.Percentage = maxScore > 0 ? (double)totalScore / maxScore * 100 : 0;

        await _context.SaveChangesAsync();

        var passingScore = assessment.TestTemplate?.PassingScore ?? 70;

        return new AssessmentResultDto
        {
            AssessmentId = assessment.Id,
            Title = assessment.Title ?? "",
            Status = assessment.Status,
            StatusName = assessment.Status.ToString(),
            TotalScore = totalScore,
            MaxScore = maxScore,
            Percentage = assessment.Percentage ?? 0,
            Passed = assessment.Percentage >= passingScore,
            PassingScore = passingScore,
            StartedAt = assessment.StartedAt!.Value,
            CompletedAt = assessment.CompletedAt!.Value,
            TotalTimeMinutes = (int)(assessment.CompletedAt.Value - assessment.StartedAt.Value).TotalMinutes,
            TotalQuestions = allQuestions.Count,
            CorrectAnswers = correctAnswers,
            WrongAnswers = wrongAnswers,
            UnansweredQuestions = unanswered,
            PendingReviewQuestions = pendingReview,
            SkillResults = skillScores.Select(kv => new SkillResultDto
            {
                SkillId = kv.Key,
                SkillName = kv.Value.Name,
                SkillCode = kv.Value.Code,
                CorrectAnswers = kv.Value.Correct,
                TotalQuestions = kv.Value.Total,
                Score = kv.Value.Score,
                MaxScore = kv.Value.MaxScore,
                Percentage = kv.Value.MaxScore > 0 ? (double)kv.Value.Score / kv.Value.MaxScore * 100 : 0
            }).ToList(),
            QuestionResults = questionResults
        };
    }

    public async Task<AssessmentResultDto?> GetResultAsync(Guid assessmentId)
    {
        var assessment = await _context.Assessments
            .Include(a => a.TestTemplate)
                .ThenInclude(t => t!.Sections)
                    .ThenInclude(s => s.Questions)
                        .ThenInclude(q => q.Options)
            .Include(a => a.TestTemplate)
                .ThenInclude(t => t!.Sections)
                    .ThenInclude(s => s.Questions)
                        .ThenInclude(q => q.Skill)
            .Include(a => a.Responses)
            .FirstOrDefaultAsync(a => a.Id == assessmentId);

        if (assessment == null || assessment.Status == AssessmentStatus.InProgress)
            return null;

        // Reuse the same logic as SubmitAssessmentAsync for building results
        var allQuestions = assessment.TestTemplate!.Sections
            .SelectMany(s => s.Questions.Where(q => q.IsActive))
            .ToList();

        var responses = assessment.Responses.ToDictionary(r => r.QuestionId);
        var skillScores = new Dictionary<Guid, (string Name, string Code, int Correct, int Total, int Score, int MaxScore)>();
        var questionResults = new List<QuestionResultDto>();

        int correctAnswers = 0, wrongAnswers = 0, unanswered = 0, pendingReview = 0;
        var questionNumber = 0;

        foreach (var question in allQuestions)
        {
            questionNumber++;

            // Handle nullable SkillId - only track skill scores if skill is specified
            var skillId = question.SkillId;
            (string Name, string Code, int Correct, int Total, int Score, int MaxScore) skillData = ("", "", 0, 0, 0, 0);
            if (skillId.HasValue)
            {
                if (!skillScores.ContainsKey(skillId.Value))
                    skillScores[skillId.Value] = (question.Skill?.Name ?? "", question.Skill?.Code ?? "", 0, 0, 0, 0);
                skillData = skillScores[skillId.Value];
            }
            skillData.Total++;
            skillData.MaxScore += question.Points;

            responses.TryGetValue(question.Id, out var response);

            var questionResult = new QuestionResultDto
            {
                QuestionId = question.Id,
                QuestionNumber = questionNumber,
                Content = question.Content,
                CodeSnippet = question.CodeSnippet,
                Type = question.Type,
                TypeName = question.Type.ToString(),
                SkillName = question.Skill?.Name ?? "",
                Points = question.Points,
                Options = question.Options.Select(o => new OptionResultDto
                {
                    Id = o.Id,
                    Content = o.Content,
                    IsCorrect = o.IsCorrect,
                    WasSelected = response != null &&
                        !string.IsNullOrEmpty(response.SelectedOptions) &&
                        JsonSerializer.Deserialize<List<Guid>>(response.SelectedOptions)?.Contains(o.Id) == true,
                    Explanation = o.Explanation
                }).ToList()
            };

            if (response == null)
            {
                unanswered++;
            }
            else
            {
                questionResult.UserAnswer = response.TextResponse ?? response.CodeResponse;
                if (!string.IsNullOrEmpty(response.SelectedOptions))
                    questionResult.SelectedOptionIds = JsonSerializer.Deserialize<List<Guid>>(response.SelectedOptions);

                if (response.IsCorrect.HasValue)
                {
                    if (response.IsCorrect.Value) { correctAnswers++; skillData.Correct++; skillData.Score += response.PointsAwarded ?? 0; }
                    else wrongAnswers++;
                    questionResult.IsCorrect = response.IsCorrect;
                    questionResult.PointsAwarded = response.PointsAwarded;
                }
                else pendingReview++;
            }

            var correctOptions = question.Options.Where(o => o.IsCorrect).ToList();
            questionResult.CorrectOptionIds = correctOptions.Select(o => o.Id).ToList();
            questionResult.CorrectAnswer = string.Join(", ", correctOptions.Select(o => o.Content));
            questionResult.Explanation = correctOptions.FirstOrDefault()?.Explanation;

            // Update skill scores if skill is specified
            if (skillId.HasValue)
            {
                skillScores[skillId.Value] = skillData;
            }
            questionResults.Add(questionResult);
        }

        var passingScore = assessment.TestTemplate?.PassingScore ?? 70;

        return new AssessmentResultDto
        {
            AssessmentId = assessment.Id,
            Title = assessment.Title ?? "",
            Status = assessment.Status,
            StatusName = assessment.Status.ToString(),
            TotalScore = assessment.Score ?? 0,
            MaxScore = assessment.MaxScore ?? 0,
            Percentage = assessment.Percentage ?? 0,
            Passed = assessment.Percentage >= passingScore,
            PassingScore = passingScore,
            StartedAt = assessment.StartedAt ?? DateTime.MinValue,
            CompletedAt = assessment.CompletedAt ?? DateTime.MinValue,
            TotalTimeMinutes = assessment.StartedAt.HasValue && assessment.CompletedAt.HasValue
                ? (int)(assessment.CompletedAt.Value - assessment.StartedAt.Value).TotalMinutes
                : 0,
            TotalQuestions = allQuestions.Count,
            CorrectAnswers = correctAnswers,
            WrongAnswers = wrongAnswers,
            UnansweredQuestions = unanswered,
            PendingReviewQuestions = pendingReview,
            SkillResults = skillScores.Select(kv => new SkillResultDto
            {
                SkillId = kv.Key,
                SkillName = kv.Value.Name,
                SkillCode = kv.Value.Code,
                CorrectAnswers = kv.Value.Correct,
                TotalQuestions = kv.Value.Total,
                Score = kv.Value.Score,
                MaxScore = kv.Value.MaxScore,
                Percentage = kv.Value.MaxScore > 0 ? (double)kv.Value.Score / kv.Value.MaxScore * 100 : 0
            }).ToList(),
            QuestionResults = questionResults
        };
    }

    public async Task<List<AvailableTestDto>> GetAvailableTestsAsync(Guid employeeId)
    {
        var templates = await _context.TestTemplates
            .Where(t => t.IsActive)
            .Include(t => t.TargetSkill)
            .Include(t => t.Sections)
                .ThenInclude(s => s.Questions)
            .ToListAsync();

        var employeeAssessments = await _context.Assessments
            .Where(a => a.EmployeeId == employeeId && a.TestTemplateId != null)
            .GroupBy(a => a.TestTemplateId)
            .Select(g => new
            {
                TestTemplateId = g.Key,
                AttemptCount = g.Count(),
                BestScore = g.Max(a => a.Percentage)
            })
            .ToListAsync();

        var assessmentLookup = employeeAssessments.ToDictionary(a => a.TestTemplateId!.Value);

        return templates.Select(t =>
        {
            assessmentLookup.TryGetValue(t.Id, out var stats);
            return new AvailableTestDto
            {
                TestTemplateId = t.Id,
                Title = t.Title,
                Description = t.Description,
                TypeName = t.Type.ToString(),
                TargetSkillName = t.TargetSkill?.Name,
                TimeLimitMinutes = t.TimeLimitMinutes,
                QuestionCount = t.Sections.SelectMany(s => s.Questions.Where(q => q.IsActive)).Count(),
                PassingScore = t.PassingScore,
                HasAttempted = stats != null,
                AttemptCount = stats?.AttemptCount ?? 0,
                BestScore = stats?.BestScore
            };
        }).ToList();
    }

    private AssessmentDto MapToDto(Domain.Entities.Assessment.Assessment a)
    {
        return new AssessmentDto
        {
            Id = a.Id,
            EmployeeId = a.EmployeeId,
            EmployeeName = a.Employee?.FullName ?? "",
            AssessorId = a.AssessorId,
            AssessorName = a.Assessor?.FullName,
            Type = a.Type,
            TypeName = a.Type.ToString(),
            Status = a.Status,
            StatusName = a.Status.ToString(),
            Title = a.Title,
            Description = a.Description,
            ScheduledDate = a.ScheduledDate,
            StartedAt = a.StartedAt,
            CompletedAt = a.CompletedAt,
            TestTemplateId = a.TestTemplateId,
            TestTemplateTitle = a.TestTemplate?.Title,
            Score = a.Score,
            MaxScore = a.MaxScore,
            Percentage = a.Percentage,
            TimeLimitMinutes = a.TestTemplate?.TimeLimitMinutes,
            PassingScore = a.TestTemplate?.PassingScore,
            CreatedAt = a.CreatedAt,
            Responses = a.Responses.Select(r => new AssessmentResponseDto
            {
                Id = r.Id,
                QuestionId = r.QuestionId,
                QuestionContent = r.Question?.Content ?? "",
                QuestionType = r.Question?.Type ?? QuestionType.MultipleChoice,
                TextResponse = r.TextResponse,
                CodeResponse = r.CodeResponse,
                SelectedOptionIds = !string.IsNullOrEmpty(r.SelectedOptions)
                    ? JsonSerializer.Deserialize<List<Guid>>(r.SelectedOptions)
                    : null,
                IsCorrect = r.IsCorrect,
                PointsAwarded = r.PointsAwarded,
                MaxPoints = r.Question?.Points ?? 0,
                AnsweredAt = r.AnsweredAt,
                TimeSpentSeconds = r.TimeSpentSeconds
            }).ToList()
        };
    }
}
