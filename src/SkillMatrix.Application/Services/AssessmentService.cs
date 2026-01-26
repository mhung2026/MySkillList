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
                TestTemplateId = a.TestTemplateId,
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
        // Check for existing in-progress assessment for this employee and template
        var existingAssessment = await _context.Assessments
            .FirstOrDefaultAsync(a =>
                a.EmployeeId == request.EmployeeId &&
                a.TestTemplateId == request.TestTemplateId &&
                a.Status == AssessmentStatus.InProgress);

        if (existingAssessment != null)
        {
            // Return the existing in-progress assessment
            var result = await GetInProgressAssessmentAsync(existingAssessment.Id);
            if (result != null)
                return result;
        }

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

        // Check if time has expired - auto-submit if so
        if (template.TimeLimitMinutes.HasValue && assessment.StartedAt.HasValue)
        {
            var deadline = assessment.StartedAt.Value.AddMinutes(template.TimeLimitMinutes.Value);
            if (DateTime.UtcNow > deadline)
            {
                // Time expired - auto-submit the assessment
                await SubmitAssessmentAsync(assessmentId);
                return null; // Return null to indicate assessment was auto-submitted
            }
        }

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
            Questions = s.Questions.Where(q => q.IsActive).Select(q =>
            {
                existingResponses.TryGetValue(q.Id, out var existingResponse);
                return new QuestionForTestDto
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
                    }).ToList(),
                    // Populate existing answers
                    SelectedOptionIds = !string.IsNullOrEmpty(existingResponse?.SelectedOptions)
                        ? JsonSerializer.Deserialize<List<Guid>>(existingResponse.SelectedOptions)
                        : null,
                    TextResponse = existingResponse?.TextResponse,
                    CodeResponse = existingResponse?.CodeResponse
                };
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

    public async Task<StartAssessmentResponse?> StartExistingAssessmentAsync(Guid assessmentId)
    {
        // Get the existing assessment
        var assessment = await _context.Assessments
            .Include(a => a.TestTemplate)
                .ThenInclude(t => t!.Sections.OrderBy(s => s.DisplayOrder))
                    .ThenInclude(s => s.Questions.Where(q => q.IsActive))
                        .ThenInclude(q => q.Options.OrderBy(o => o.DisplayOrder))
            .Include(a => a.TestTemplate)
                .ThenInclude(t => t!.Sections)
                    .ThenInclude(s => s.Questions)
                        .ThenInclude(q => q.Skill)
            .FirstOrDefaultAsync(a => a.Id == assessmentId);

        if (assessment?.TestTemplate == null)
            return null;

        // If already in progress, return the in-progress data
        if (assessment.Status == AssessmentStatus.InProgress)
        {
            return await GetInProgressAssessmentAsync(assessmentId);
        }

        // If already completed, return null
        if (assessment.Status == AssessmentStatus.Completed ||
            assessment.Status == AssessmentStatus.Reviewed)
        {
            return null;
        }

        // Start the assessment - update status to InProgress
        assessment.Status = AssessmentStatus.InProgress;
        assessment.StartedAt = DateTime.UtcNow;
        assessment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var template = assessment.TestTemplate;

        // Build response
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

        // AI grading for essay questions
        var essayQuestionTypes = new[] { QuestionType.ShortAnswer, QuestionType.LongAnswer, QuestionType.CodingChallenge };

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

                // Create response record with 0 points for unanswered questions
                response = new AssessmentResponse
                {
                    AssessmentId = assessmentId,
                    QuestionId = question.Id,
                    PointsAwarded = 0,
                    IsCorrect = false,
                    AnsweredAt = DateTime.UtcNow
                };

                // If essay question left blank, add feedback
                if (essayQuestionTypes.Contains(question.Type))
                {
                    var blankFeedback = new AiFeedbackDto
                    {
                        Feedback = "Câu hỏi bị bỏ trống - 0 điểm.",
                        StrengthPoints = new List<string>(),
                        ImprovementAreas = new List<string> { "Bạn cần trả lời câu hỏi này để được chấm điểm." },
                        DetailedAnalysis = "Không có câu trả lời được gửi cho câu hỏi này."
                    };
                    questionResult.AiFeedback = blankFeedback;
                    response.AiGrading = JsonSerializer.Serialize(blankFeedback);
                }

                _context.AssessmentResponses.Add(response);
            }
            else
            {
                questionResult.UserAnswer = response.TextResponse ?? response.CodeResponse;
                if (!string.IsNullOrEmpty(response.SelectedOptions))
                {
                    questionResult.SelectedOptionIds = JsonSerializer.Deserialize<List<Guid>>(response.SelectedOptions);
                }

                // Check if this is an essay/text question that needs AI grading
                if (essayQuestionTypes.Contains(question.Type) && !string.IsNullOrEmpty(questionResult.UserAnswer))
                {
                    // Call AI to grade essay answer
                    try
                    {
                        var gradeRequest = new AiGradeAnswerRequest
                        {
                            QuestionId = question.Id,
                            QuestionContent = question.Content,
                            ExpectedAnswer = null, // Not stored in Question entity, AI uses GradingRubric
                            GradingRubric = question.GradingRubric,
                            StudentAnswer = questionResult.UserAnswer,
                            MaxPoints = question.Points
                        };

                        var gradeResult = await _aiService.GradeAnswerAsync(gradeRequest);

                        if (gradeResult.Success)
                        {
                            response.PointsAwarded = gradeResult.PointsAwarded;
                            response.IsCorrect = gradeResult.PointsAwarded >= question.Points * 0.7;
                            // Store full AI feedback as JSON
                            var aiFeedbackDto = new AiFeedbackDto
                            {
                                Feedback = gradeResult.Feedback,
                                StrengthPoints = gradeResult.StrengthPoints,
                                ImprovementAreas = gradeResult.ImprovementAreas,
                                DetailedAnalysis = gradeResult.DetailedAnalysis
                            };
                            response.AiGrading = JsonSerializer.Serialize(aiFeedbackDto);

                            questionResult.PointsAwarded = gradeResult.PointsAwarded;
                            questionResult.IsCorrect = response.IsCorrect;
                            questionResult.AiFeedback = new AiFeedbackDto
                            {
                                Feedback = gradeResult.Feedback,
                                StrengthPoints = gradeResult.StrengthPoints,
                                ImprovementAreas = gradeResult.ImprovementAreas,
                                DetailedAnalysis = gradeResult.DetailedAnalysis
                            };

                            if (response.IsCorrect == true)
                            {
                                correctAnswers++;
                                totalScore += gradeResult.PointsAwarded;
                                skillData.Correct++;
                                skillData.Score += gradeResult.PointsAwarded;
                            }
                            else
                            {
                                wrongAnswers++;
                                totalScore += gradeResult.PointsAwarded; // Add partial score
                                skillData.Score += gradeResult.PointsAwarded;
                            }
                        }
                        else
                        {
                            // AI grading failed, mark as pending review
                            pendingReview++;
                            var failedFeedback = new AiFeedbackDto
                            {
                                Feedback = "Không thể chấm điểm tự động. Cần người đánh giá.",
                                ImprovementAreas = new List<string> { gradeResult.Feedback }
                            };
                            questionResult.AiFeedback = failedFeedback;
                            response.AiGrading = JsonSerializer.Serialize(failedFeedback);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error and mark as pending review
                        pendingReview++;
                        var errorFeedback = new AiFeedbackDto
                        {
                            Feedback = "Lỗi khi chấm điểm tự động.",
                            ImprovementAreas = new List<string> { ex.Message }
                        };
                        questionResult.AiFeedback = errorFeedback;
                        response.AiGrading = JsonSerializer.Serialize(errorFeedback);
                    }
                }
                else if (response.IsCorrect.HasValue)
                {
                    // Multiple choice or already graded
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
                else if (essayQuestionTypes.Contains(question.Type) && string.IsNullOrEmpty(questionResult.UserAnswer))
                {
                    // Essay question left blank - 0 points
                    response.PointsAwarded = 0;
                    response.IsCorrect = false;
                    response.AiGrading = JsonSerializer.Serialize(new AiFeedbackDto
                    {
                        Feedback = "Câu hỏi bị bỏ trống - 0 điểm.",
                        ImprovementAreas = new List<string> { "Bạn cần trả lời câu hỏi này để được chấm điểm." }
                    });

                    questionResult.PointsAwarded = 0;
                    questionResult.IsCorrect = false;
                    questionResult.AiFeedback = new AiFeedbackDto
                    {
                        Feedback = "Câu hỏi bị bỏ trống - 0 điểm.",
                        ImprovementAreas = new List<string> { "Bạn cần trả lời câu hỏi này để được chấm điểm." }
                    };
                    wrongAnswers++;
                }
                else if (!essayQuestionTypes.Contains(question.Type))
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

                // Parse AI feedback from database
                if (!string.IsNullOrEmpty(response.AiGrading))
                {
                    try
                    {
                        questionResult.AiFeedback = JsonSerializer.Deserialize<AiFeedbackDto>(response.AiGrading);
                    }
                    catch
                    {
                        // If not valid JSON, create simple feedback object
                        questionResult.AiFeedback = new AiFeedbackDto { Feedback = response.AiGrading };
                    }
                }
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
