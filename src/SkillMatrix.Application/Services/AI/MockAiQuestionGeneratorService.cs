using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Assessment;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Enums;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Application.Services.AI;

/// <summary>
/// Mock AI Service - trả về dữ liệu giả lập
/// Sẽ được thay thế bằng real AI service sau
/// </summary>
public class MockAiQuestionGeneratorService : IAiQuestionGeneratorService
{
    private readonly SkillMatrixDbContext _context;
    private readonly Random _random = new();

    public MockAiQuestionGeneratorService(SkillMatrixDbContext context)
    {
        _context = context;
    }

    public async Task<AiGenerateQuestionsResponse> GenerateQuestionsAsync(AiGenerateQuestionsRequest request)
    {
        var startTime = DateTime.UtcNow;

        // Get skill info if SkillId provided
        string skillName = request.SkillName ?? "General";
        string skillCode = "GEN";
        Guid? skillId = request.SkillId;

        if (request.SkillId.HasValue)
        {
            var skill = await _context.Skills
                .Include(s => s.Subcategory)
                .ThenInclude(sc => sc.SkillDomain)
                .FirstOrDefaultAsync(s => s.Id == request.SkillId.Value);

            if (skill != null)
            {
                skillName = skill.Name;
                skillCode = skill.Code;
                skillId = skill.Id;
            }
        }

        var questions = new List<AiGeneratedQuestion>();
        var isVietnamese = request.Language == "vi";

        for (int i = 0; i < request.QuestionCount; i++)
        {
            var question = GenerateQuestionByAssessmentType(
                request.AssessmentType,
                skillName,
                skillCode,
                skillId,
                request.TargetLevel,
                request.Difficulty,
                request.JobRole,
                isVietnamese,
                i + 1
            );

            questions.Add(question);
        }

        var endTime = DateTime.UtcNow;

        return new AiGenerateQuestionsResponse
        {
            Success = true,
            Message = $"Generated {questions.Count} questions for {request.AssessmentType}",
            Questions = questions,
            Metadata = new AiGenerationMetadata
            {
                Model = "mock-gpt-4",
                TokensUsed = _random.Next(500, 2000),
                GenerationTimeMs = (endTime - startTime).TotalMilliseconds + _random.Next(100, 500),
                PromptUsed = BuildMockPrompt(skillName, request),
                GeneratedAt = DateTime.UtcNow
            }
        };
    }

    public async Task<AiGradeAnswerResponse> GradeAnswerAsync(AiGradeAnswerRequest request)
    {
        await Task.Delay(100); // Simulate processing

        var hasKeywords = !string.IsNullOrEmpty(request.ExpectedAnswer) &&
                         request.StudentAnswer.Split(' ')
                             .Any(w => request.ExpectedAnswer.Contains(w, StringComparison.OrdinalIgnoreCase));

        var baseScore = hasKeywords ? 0.7 : 0.4;
        var randomVariation = _random.NextDouble() * 0.3;
        var percentage = Math.Min(1.0, baseScore + randomVariation);
        var pointsAwarded = (int)Math.Round(request.MaxPoints * percentage);

        return new AiGradeAnswerResponse
        {
            Success = true,
            PointsAwarded = pointsAwarded,
            MaxPoints = request.MaxPoints,
            Percentage = percentage * 100,
            Feedback = GenerateMockFeedback(percentage),
            StrengthPoints = GenerateMockStrengths(percentage),
            ImprovementAreas = GenerateMockImprovements(percentage),
            DetailedAnalysis = $"[Mock Analysis] Answer length: {request.StudentAnswer.Length} chars. " +
                              $"Keyword match: {(hasKeywords ? "Yes" : "No")}."
        };
    }

    private AiGeneratedQuestion GenerateQuestionByAssessmentType(
        AssessmentType assessmentType,
        string skillName,
        string skillCode,
        Guid? skillId,
        ProficiencyLevel? targetLevel,
        DifficultyLevel? difficulty,
        string? jobRole,
        bool isVietnamese,
        int index)
    {
        return assessmentType switch
        {
            AssessmentType.Quiz => GenerateQuizQuestion(skillName, skillCode, skillId, targetLevel, difficulty, isVietnamese, index),
            AssessmentType.CodingTest => GenerateCodingQuestion(skillName, skillCode, skillId, targetLevel, difficulty, isVietnamese, index),
            AssessmentType.CaseStudy => GenerateCaseStudyQuestion(skillName, skillCode, skillId, targetLevel, difficulty, isVietnamese, index),
            AssessmentType.RoleBasedTest => GenerateRoleBasedQuestion(skillName, skillCode, skillId, targetLevel, difficulty, jobRole, isVietnamese, index),
            AssessmentType.SituationalJudgment => GenerateSjtQuestion(skillName, skillCode, skillId, targetLevel, difficulty, jobRole, isVietnamese, index),
            _ => GenerateQuizQuestion(skillName, skillCode, skillId, targetLevel, difficulty, isVietnamese, index)
        };
    }

    private AiGeneratedQuestion GenerateQuizQuestion(
        string skillName, string skillCode, Guid? skillId,
        ProficiencyLevel? targetLevel, DifficultyLevel? difficulty,
        bool isVi, int index)
    {
        var templates = GetQuizTemplates(skillName, skillCode, targetLevel ?? ProficiencyLevel.Apply, isVi);
        var template = templates[_random.Next(templates.Count)];

        return new AiGeneratedQuestion
        {
            Content = $"{template.Question} (Q{index})",
            AssessmentType = AssessmentType.Quiz,
            QuestionType = QuestionType.MultipleChoice,
            Difficulty = difficulty,
            TargetLevel = targetLevel,
            SkillId = skillId,
            SkillName = skillName,
            SuggestedPoints = GetPointsForDifficulty(difficulty ?? DifficultyLevel.Medium),
            SuggestedTimeSeconds = 60,
            Tags = new List<string> { skillCode, "quiz" },
            Explanation = template.Explanation,
            Options = GenerateMockOptions(template.CorrectAnswer, isVi)
        };
    }

    private AiGeneratedQuestion GenerateCodingQuestion(
        string skillName, string skillCode, Guid? skillId,
        ProficiencyLevel? targetLevel, DifficultyLevel? difficulty,
        bool isVi, int index)
    {
        var templates = GetCodingTemplates(skillCode, isVi);
        var template = templates[_random.Next(templates.Count)];

        return new AiGeneratedQuestion
        {
            Content = $"{template.Question} (Q{index})",
            AssessmentType = AssessmentType.CodingTest,
            QuestionType = QuestionType.CodingChallenge,
            Difficulty = difficulty,
            TargetLevel = targetLevel,
            SkillId = skillId,
            SkillName = skillName,
            SuggestedPoints = GetPointsForDifficulty(difficulty ?? DifficultyLevel.Medium) * 2,
            SuggestedTimeSeconds = 900, // 15 minutes
            Tags = new List<string> { skillCode, "coding" },
            Explanation = template.Explanation,
            CodeSnippet = template.CodeSnippet,
            ExpectedOutput = template.CorrectAnswer,
            TestCases = new List<AiTestCase>
            {
                new() { Input = "[1, 2, 3]", ExpectedOutput = "6", Description = "Basic sum" },
                new() { Input = "[]", ExpectedOutput = "0", Description = "Empty array" },
                new() { Input = "[-1, 1]", ExpectedOutput = "0", Description = "Negative numbers", IsHidden = true }
            },
            GradingRubric = GenerateMockRubric(isVi)
        };
    }

    private AiGeneratedQuestion GenerateCaseStudyQuestion(
        string skillName, string skillCode, Guid? skillId,
        ProficiencyLevel? targetLevel, DifficultyLevel? difficulty,
        bool isVi, int index)
    {
        var scenario = isVi
            ? $"Công ty XYZ đang gặp vấn đề về hiệu suất hệ thống {skillName}. Hệ thống hiện tại có 10,000 users đồng thời và response time trung bình là 5 giây..."
            : $"Company XYZ is experiencing performance issues with their {skillName} system. The current system has 10,000 concurrent users and average response time is 5 seconds...";

        var question = isVi
            ? $"Phân tích tình huống trên và đề xuất giải pháp cải thiện. Hãy xem xét: architecture, caching, database optimization, và scaling strategies. (Q{index})"
            : $"Analyze the above situation and propose improvement solutions. Consider: architecture, caching, database optimization, and scaling strategies. (Q{index})";

        return new AiGeneratedQuestion
        {
            Content = question,
            AssessmentType = AssessmentType.CaseStudy,
            QuestionType = QuestionType.Scenario,
            Difficulty = difficulty,
            TargetLevel = targetLevel,
            SkillId = skillId,
            SkillName = skillName,
            SuggestedPoints = GetPointsForDifficulty(difficulty ?? DifficultyLevel.Hard) * 3,
            SuggestedTimeSeconds = 1800, // 30 minutes
            Tags = new List<string> { skillCode, "case-study", "analysis" },
            Scenario = scenario,
            Documents = new List<string>
            {
                isVi ? "Biểu đồ kiến trúc hiện tại" : "Current architecture diagram",
                isVi ? "Metrics dashboard screenshots" : "Metrics dashboard screenshots"
            },
            ExpectedAnswer = isVi
                ? "Giải pháp nên bao gồm: 1) Implement caching layer (Redis), 2) Database indexing và query optimization, 3) Horizontal scaling với load balancer..."
                : "Solution should include: 1) Implement caching layer (Redis), 2) Database indexing and query optimization, 3) Horizontal scaling with load balancer...",
            GradingRubric = GenerateCaseStudyRubric(isVi)
        };
    }

    private AiGeneratedQuestion GenerateRoleBasedQuestion(
        string skillName, string skillCode, Guid? skillId,
        ProficiencyLevel? targetLevel, DifficultyLevel? difficulty,
        string? jobRole, bool isVi, int index)
    {
        var role = jobRole ?? "Senior Developer";
        var roleContext = isVi
            ? $"Bạn là {role} trong một team 5 người. Team đang phát triển một hệ thống e-commerce quy mô lớn."
            : $"You are a {role} in a team of 5 people. The team is developing a large-scale e-commerce system.";

        var question = isVi
            ? $"Với vai trò {role}, bạn sẽ xử lý như thế nào khi junior developer liên tục submit code không đạt chuẩn? (Q{index})"
            : $"As a {role}, how would you handle a situation where a junior developer consistently submits code that doesn't meet standards? (Q{index})";

        return new AiGeneratedQuestion
        {
            Content = question,
            AssessmentType = AssessmentType.RoleBasedTest,
            QuestionType = QuestionType.LongAnswer,
            Difficulty = difficulty,
            TargetLevel = targetLevel,
            SkillId = skillId,
            SkillName = skillName,
            SuggestedPoints = GetPointsForDifficulty(difficulty ?? DifficultyLevel.Medium) * 2,
            SuggestedTimeSeconds = 600, // 10 minutes
            Tags = new List<string> { skillCode, "role-based", role.ToLower().Replace(" ", "-") },
            RoleContext = roleContext,
            Responsibilities = isVi
                ? new List<string> { "Code review", "Mentoring junior", "Architecture decisions", "Technical leadership" }
                : new List<string> { "Code review", "Mentoring juniors", "Architecture decisions", "Technical leadership" },
            ExpectedAnswer = isVi
                ? "Câu trả lời nên thể hiện: 1) Empathy và constructive feedback, 2) Pair programming sessions, 3) Clear coding guidelines, 4) Regular 1-on-1 meetings..."
                : "Answer should demonstrate: 1) Empathy and constructive feedback, 2) Pair programming sessions, 3) Clear coding guidelines, 4) Regular 1-on-1 meetings...",
            GradingRubric = GenerateRoleBasedRubric(isVi)
        };
    }

    private AiGeneratedQuestion GenerateSjtQuestion(
        string skillName, string skillCode, Guid? skillId,
        ProficiencyLevel? targetLevel, DifficultyLevel? difficulty,
        string? jobRole, bool isVi, int index)
    {
        var situation = isVi
            ? $"Bạn đang trong deadline quan trọng và phát hiện một bug critical trong production. Team lead đang nghỉ phép và không liên lạc được. (Q{index})"
            : $"You are in a critical deadline and discover a critical bug in production. The team lead is on leave and unreachable. (Q{index})";

        return new AiGeneratedQuestion
        {
            Content = isVi ? "Xếp hạng các phương án xử lý theo mức độ hiệu quả:" : "Rank the following responses by effectiveness:",
            AssessmentType = AssessmentType.SituationalJudgment,
            QuestionType = QuestionType.SituationalJudgment,
            Difficulty = difficulty,
            TargetLevel = targetLevel,
            SkillId = skillId,
            SkillName = skillName,
            SuggestedPoints = GetPointsForDifficulty(difficulty ?? DifficultyLevel.Medium),
            SuggestedTimeSeconds = 180, // 3 minutes
            Tags = new List<string> { skillCode, "sjt", "decision-making" },
            Situation = situation,
            ResponseOptions = new List<AiSjtResponseOption>
            {
                new()
                {
                    Content = isVi
                        ? "Tự fix bug và deploy ngay lập tức để đảm bảo deadline"
                        : "Fix the bug yourself and deploy immediately to meet the deadline",
                    Effectiveness = SjtEffectiveness.Ineffective,
                    Explanation = isVi
                        ? "Rủi ro cao, không có review, có thể gây thêm vấn đề"
                        : "High risk, no review, could cause more issues"
                },
                new()
                {
                    Content = isVi
                        ? "Escalate lên manager/senior khác, thông báo stakeholders về delay có thể xảy ra"
                        : "Escalate to another manager/senior, inform stakeholders about potential delay",
                    Effectiveness = SjtEffectiveness.MostEffective,
                    Explanation = isVi
                        ? "Đúng quy trình, có backup plan, transparent communication"
                        : "Follows process, has backup plan, transparent communication"
                },
                new()
                {
                    Content = isVi
                        ? "Chờ team lead quay lại để được hướng dẫn"
                        : "Wait for the team lead to return for guidance",
                    Effectiveness = SjtEffectiveness.CounterProductive,
                    Explanation = isVi
                        ? "Không chủ động, để vấn đề kéo dài"
                        : "Not proactive, lets the issue persist"
                },
                new()
                {
                    Content = isVi
                        ? "Fix bug, tạo PR và nhờ senior khác review trước khi deploy"
                        : "Fix the bug, create PR and ask another senior to review before deploying",
                    Effectiveness = SjtEffectiveness.Effective,
                    Explanation = isVi
                        ? "Chủ động nhưng vẫn có review, an toàn hơn"
                        : "Proactive but still has review, safer approach"
                }
            }
        };
    }

    private List<MockQuestionTemplate> GetQuizTemplates(string skillName, string skillCode, ProficiencyLevel level, bool isVi)
    {
        // C# Questions
        if (skillCode == "CSHP")
            return GetCSharpQuizTemplates(level, isVi);

        // .NET Questions
        if (skillCode == "NETC" || skillCode == "ASPN")
            return GetDotNetQuizTemplates(isVi);

        // Default
        return GetGenericQuizTemplates(skillName, isVi);
    }

    private List<MockQuestionTemplate> GetCSharpQuizTemplates(ProficiencyLevel level, bool isVi)
    {
        if (level <= ProficiencyLevel.Assist)
        {
            return new List<MockQuestionTemplate>
            {
                new()
                {
                    Question = isVi ? "Từ khóa 'var' trong C# dùng để làm gì?" : "What is the 'var' keyword used for in C#?",
                    CorrectAnswer = isVi ? "Khai báo biến với kiểu được suy luận tự động" : "Declare variables with implicitly inferred type",
                    Explanation = isVi ? "var cho phép compiler tự động xác định kiểu dữ liệu" : "var allows compiler to automatically determine type"
                },
                new()
                {
                    Question = isVi ? "Giá trị mặc định của bool trong C# là gì?" : "What is the default value of bool in C#?",
                    CorrectAnswer = "false",
                    Explanation = isVi ? "Bool mặc định là false" : "Bool defaults to false"
                }
            };
        }

        return new List<MockQuestionTemplate>
        {
            new()
            {
                Question = isVi ? "Sự khác biệt giữa abstract class và interface?" : "What's the difference between abstract class and interface?",
                CorrectAnswer = isVi
                    ? "Abstract class có thể có implementation, class chỉ kế thừa 1 abstract class nhưng implement nhiều interfaces"
                    : "Abstract class can have implementation, a class can inherit only one abstract class but implement multiple interfaces",
                Explanation = isVi ? "Đây là câu hỏi kinh điển về OOP" : "Classic OOP question"
            },
            new()
            {
                Question = isVi ? "Async/await trong C# hoạt động như thế nào?" : "How does async/await work in C#?",
                CorrectAnswer = isVi
                    ? "async đánh dấu method bất đồng bộ, await tạm dừng execution mà không block thread"
                    : "async marks a method as asynchronous, await pauses execution without blocking thread",
                Explanation = isVi ? "Async programming giúp cải thiện scalability" : "Async improves scalability"
            }
        };
    }

    private List<MockQuestionTemplate> GetDotNetQuizTemplates(bool isVi)
    {
        return new List<MockQuestionTemplate>
        {
            new()
            {
                Question = isVi
                    ? "Sự khác biệt giữa AddScoped, AddTransient và AddSingleton?"
                    : "What's the difference between AddScoped, AddTransient and AddSingleton?",
                CorrectAnswer = isVi
                    ? "Singleton: 1 instance toàn app. Scoped: 1 instance per request. Transient: new instance mỗi lần inject."
                    : "Singleton: 1 instance for entire app. Scoped: 1 instance per request. Transient: new instance each injection.",
                Explanation = isVi ? "Chọn đúng lifetime rất quan trọng" : "Choosing right lifetime is crucial"
            }
        };
    }

    private List<MockQuestionTemplate> GetGenericQuizTemplates(string skillName, bool isVi)
    {
        return new List<MockQuestionTemplate>
        {
            new()
            {
                Question = isVi ? $"Các khái niệm cơ bản của {skillName}?" : $"Basic concepts of {skillName}?",
                CorrectAnswer = isVi ? $"[Câu trả lời về {skillName}]" : $"[Answer about {skillName}]",
                Explanation = isVi ? "Kiểm tra kiến thức nền tảng" : "Tests foundational knowledge"
            }
        };
    }

    private List<MockQuestionTemplate> GetCodingTemplates(string skillCode, bool isVi)
    {
        return new List<MockQuestionTemplate>
        {
            new()
            {
                Question = isVi
                    ? "Viết function tính tổng các số trong array"
                    : "Write a function to calculate sum of numbers in array",
                CorrectAnswer = "Sum of all elements",
                CodeSnippet = skillCode switch
                {
                    "CSHP" => "public int Sum(int[] numbers)\n{\n    // Your code here\n}",
                    "RCTS" or "JSTS" => "function sum(numbers: number[]): number {\n    // Your code here\n}",
                    "PYTH" => "def sum_numbers(numbers: list[int]) -> int:\n    # Your code here\n    pass",
                    _ => "// Implement sum function"
                },
                Explanation = isVi ? "Bài tập cơ bản về array manipulation" : "Basic array manipulation exercise"
            }
        };
    }

    private List<AiGeneratedOption> GenerateMockOptions(string correctAnswer, bool isVi)
    {
        var options = new List<AiGeneratedOption>
        {
            new()
            {
                Content = correctAnswer,
                IsCorrect = true,
                Explanation = isVi ? "Đây là đáp án đúng" : "This is the correct answer"
            }
        };

        var wrongOptions = isVi
            ? new[] { "Đáp án không chính xác A", "Đáp án không chính xác B", "Đáp án không chính xác C" }
            : new[] { "Incorrect option A", "Incorrect option B", "Incorrect option C" };

        foreach (var wrong in wrongOptions)
        {
            options.Add(new AiGeneratedOption
            {
                Content = wrong,
                IsCorrect = false,
                Explanation = isVi ? "Đáp án này không đúng" : "This answer is incorrect"
            });
        }

        return options.OrderBy(_ => _random.Next()).ToList();
    }

    private string GenerateMockRubric(bool isVi)
    {
        return isVi
            ? "- 0 điểm: Không trả lời hoặc hoàn toàn sai\n- 1-2 điểm: Hiểu một phần\n- 3-4 điểm: Đúng, có giải thích\n- 5 điểm: Xuất sắc"
            : "- 0 points: No answer or completely wrong\n- 1-2 points: Partial understanding\n- 3-4 points: Correct with explanation\n- 5 points: Excellent";
    }

    private string GenerateCaseStudyRubric(bool isVi)
    {
        return isVi
            ? "- Problem Analysis (30%): Xác định đúng vấn đề\n- Solution Design (40%): Giải pháp khả thi\n- Trade-offs (20%): Đánh giá pros/cons\n- Communication (10%): Trình bày rõ ràng"
            : "- Problem Analysis (30%): Correctly identify issues\n- Solution Design (40%): Viable solutions\n- Trade-offs (20%): Evaluate pros/cons\n- Communication (10%): Clear presentation";
    }

    private string GenerateRoleBasedRubric(bool isVi)
    {
        return isVi
            ? "- Leadership (30%): Thể hiện khả năng lead\n- Communication (30%): Giao tiếp hiệu quả\n- Problem Solving (25%): Giải quyết vấn đề\n- Empathy (15%): Thấu hiểu team member"
            : "- Leadership (30%): Shows leadership ability\n- Communication (30%): Effective communication\n- Problem Solving (25%): Problem resolution\n- Empathy (15%): Understanding team members";
    }

    private int GetPointsForDifficulty(DifficultyLevel difficulty)
    {
        return difficulty switch
        {
            DifficultyLevel.Easy => 1,
            DifficultyLevel.Medium => 2,
            DifficultyLevel.Hard => 3,
            DifficultyLevel.Expert => 5,
            _ => 1
        };
    }

    private string BuildMockPrompt(string skillName, AiGenerateQuestionsRequest request)
    {
        return $"Generate {request.QuestionCount} {request.Language} questions for {request.AssessmentType}. " +
               $"Skill: {skillName}. Level: {request.TargetLevel?.ToString() ?? "Any"}. " +
               $"Difficulty: {request.Difficulty?.ToString() ?? "Any"}. " +
               $"Context: {request.AdditionalContext ?? "None"}";
    }

    private string GenerateMockFeedback(double percentage)
    {
        if (percentage >= 0.9) return "Excellent answer! Deep understanding demonstrated.";
        if (percentage >= 0.7) return "Good answer with correct core concepts.";
        if (percentage >= 0.5) return "Partial understanding. Some key points missing.";
        return "Answer needs improvement. Review core concepts.";
    }

    private List<string> GenerateMockStrengths(double percentage)
    {
        if (percentage >= 0.7)
            return new List<string> { "Clear explanation", "Good terminology usage" };
        return new List<string> { "Attempted to answer" };
    }

    private List<string> GenerateMockImprovements(double percentage)
    {
        if (percentage < 0.7)
            return new List<string> { "Add specific examples", "Explain reasoning", "Cover edge cases" };
        return new List<string> { "Could add more real-world examples" };
    }

    private class MockQuestionTemplate
    {
        public string Question { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public string? CodeSnippet { get; set; }
    }
}
