using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Assessment;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Enums;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Application.Services.AI;

/// <summary>
/// Mock AI Service - returns simulated data
/// Will be replaced by real AI service later
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

        // Use QuestionTypes if provided, otherwise fall back to AssessmentType-based generation
        if (request.QuestionTypes.Count > 0)
        {
            // Distribute questions across selected question types
            var questionsPerType = request.QuestionCount / request.QuestionTypes.Count;
            var remainder = request.QuestionCount % request.QuestionTypes.Count;
            var questionIndex = 1;

            for (int typeIndex = 0; typeIndex < request.QuestionTypes.Count; typeIndex++)
            {
                var questionType = request.QuestionTypes[typeIndex];
                var countForThisType = questionsPerType + (typeIndex < remainder ? 1 : 0);

                for (int i = 0; i < countForThisType; i++)
                {
                    var question = GenerateQuestionByQuestionType(
                        questionType,
                        skillName,
                        skillCode,
                        skillId,
                        request.TargetLevel,
                        request.Difficulty,
                        request.JobRole,
                        questionIndex++
                    );
                    questions.Add(question);
                }
            }
        }
        else
        {
            // Fall back to AssessmentType-based generation
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
                    i + 1
                );

                questions.Add(question);
            }
        }

        var endTime = DateTime.UtcNow;

        var questionTypesStr = request.QuestionTypes.Count > 0
            ? string.Join(", ", request.QuestionTypes)
            : request.AssessmentType.ToString();

        return new AiGenerateQuestionsResponse
        {
            Success = true,
            Message = $"Generated {questions.Count} questions for types: {questionTypesStr}",
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
                         request.SubmittedAnswer.Split(' ')
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
            DetailedAnalysis = $"[Mock Analysis] Answer length: {request.SubmittedAnswer.Length} chars. " +
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
        int index)
    {
        return assessmentType switch
        {
            AssessmentType.Quiz => GenerateQuizQuestion(skillName, skillCode, skillId, targetLevel, difficulty, index),
            AssessmentType.CodingTest => GenerateCodingQuestion(skillName, skillCode, skillId, targetLevel, difficulty, index),
            AssessmentType.CaseStudy => GenerateCaseStudyQuestion(skillName, skillCode, skillId, targetLevel, difficulty, index),
            AssessmentType.RoleBasedTest => GenerateRoleBasedQuestion(skillName, skillCode, skillId, targetLevel, difficulty, jobRole, index),
            AssessmentType.SituationalJudgment => GenerateSjtQuestion(skillName, skillCode, skillId, targetLevel, difficulty, jobRole, index),
            _ => GenerateQuizQuestion(skillName, skillCode, skillId, targetLevel, difficulty, index)
        };
    }

    private AiGeneratedQuestion GenerateQuestionByQuestionType(
        QuestionType questionType,
        string skillName,
        string skillCode,
        Guid? skillId,
        ProficiencyLevel? targetLevel,
        DifficultyLevel? difficulty,
        string? jobRole,
        int index)
    {
        return questionType switch
        {
            QuestionType.MultipleChoice => GenerateMultipleChoiceQuestion(skillName, skillCode, skillId, targetLevel, difficulty, index),
            QuestionType.MultipleAnswer => GenerateMultipleAnswerQuestion(skillName, skillCode, skillId, targetLevel, difficulty, index),
            QuestionType.TrueFalse => GenerateTrueFalseQuestion(skillName, skillCode, skillId, targetLevel, difficulty, index),
            QuestionType.ShortAnswer => GenerateShortAnswerQuestion(skillName, skillCode, skillId, targetLevel, difficulty, index),
            QuestionType.LongAnswer => GenerateLongAnswerQuestion(skillName, skillCode, skillId, targetLevel, difficulty, index),
            QuestionType.CodingChallenge => GenerateCodingQuestion(skillName, skillCode, skillId, targetLevel, difficulty, index),
            QuestionType.Scenario => GenerateCaseStudyQuestion(skillName, skillCode, skillId, targetLevel, difficulty, index),
            QuestionType.SituationalJudgment => GenerateSjtQuestion(skillName, skillCode, skillId, targetLevel, difficulty, jobRole, index),
            _ => GenerateMultipleChoiceQuestion(skillName, skillCode, skillId, targetLevel, difficulty, index)
        };
    }

    private AiGeneratedQuestion GenerateQuizQuestion(
        string skillName, string skillCode, Guid? skillId,
        ProficiencyLevel? targetLevel, DifficultyLevel? difficulty,
        int index)
    {
        var templates = GetQuizTemplates(skillName, skillCode, targetLevel ?? ProficiencyLevel.Apply);
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
            Options = GenerateMockOptions(template.CorrectAnswer)
        };
    }

    private AiGeneratedQuestion GenerateCodingQuestion(
        string skillName, string skillCode, Guid? skillId,
        ProficiencyLevel? targetLevel, DifficultyLevel? difficulty,
        int index)
    {
        var templates = GetCodingTemplates(skillCode);
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
            GradingRubric = GenerateMockRubric()
        };
    }

    private AiGeneratedQuestion GenerateCaseStudyQuestion(
        string skillName, string skillCode, Guid? skillId,
        ProficiencyLevel? targetLevel, DifficultyLevel? difficulty,
        int index)
    {
        var scenario = $"Company XYZ is experiencing performance issues with their {skillName} system. The current system has 10,000 concurrent users and average response time is 5 seconds...";

        var question = $"Analyze the above situation and propose improvement solutions. Consider: architecture, caching, database optimization, and scaling strategies. (Q{index})";

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
                "Current architecture diagram",
                "Metrics dashboard screenshots"
            },
            ExpectedAnswer = "Solution should include: 1) Implement caching layer (Redis), 2) Database indexing and query optimization, 3) Horizontal scaling with load balancer...",
            GradingRubric = GenerateCaseStudyRubric()
        };
    }

    private AiGeneratedQuestion GenerateRoleBasedQuestion(
        string skillName, string skillCode, Guid? skillId,
        ProficiencyLevel? targetLevel, DifficultyLevel? difficulty,
        string? jobRole, int index)
    {
        var role = jobRole ?? "Senior Developer";
        var roleContext = $"You are a {role} in a team of 5 people. The team is developing a large-scale e-commerce system.";

        var question = $"As a {role}, how would you handle a situation where a junior developer consistently submits code that doesn't meet standards? (Q{index})";

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
            Responsibilities = new List<string> { "Code review", "Mentoring juniors", "Architecture decisions", "Technical leadership" },
            ExpectedAnswer = "Answer should demonstrate: 1) Empathy and constructive feedback, 2) Pair programming sessions, 3) Clear coding guidelines, 4) Regular 1-on-1 meetings...",
            GradingRubric = GenerateRoleBasedRubric()
        };
    }

    private AiGeneratedQuestion GenerateSjtQuestion(
        string skillName, string skillCode, Guid? skillId,
        ProficiencyLevel? targetLevel, DifficultyLevel? difficulty,
        string? jobRole, int index)
    {
        var situation = $"You are in a critical deadline and discover a critical bug in production. The team lead is on leave and unreachable. (Q{index})";

        return new AiGeneratedQuestion
        {
            Content = "Rank the following responses by effectiveness:",
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
                    Content = "Fix the bug yourself and deploy immediately to meet the deadline",
                    Effectiveness = SjtEffectiveness.Ineffective,
                    Explanation = "High risk, no review, could cause more issues"
                },
                new()
                {
                    Content = "Escalate to another manager/senior, inform stakeholders about potential delay",
                    Effectiveness = SjtEffectiveness.MostEffective,
                    Explanation = "Follows process, has backup plan, transparent communication"
                },
                new()
                {
                    Content = "Wait for the team lead to return for guidance",
                    Effectiveness = SjtEffectiveness.CounterProductive,
                    Explanation = "Not proactive, lets the issue persist"
                },
                new()
                {
                    Content = "Fix the bug, create PR and ask another senior to review before deploying",
                    Effectiveness = SjtEffectiveness.Effective,
                    Explanation = "Proactive but still has review, safer approach"
                }
            }
        };
    }

    #region Question Type Generators

    private AiGeneratedQuestion GenerateMultipleChoiceQuestion(
        string skillName, string skillCode, Guid? skillId,
        ProficiencyLevel? targetLevel, DifficultyLevel? difficulty,
        int index)
    {
        var templates = GetQuizTemplates(skillName, skillCode, targetLevel ?? ProficiencyLevel.Apply);
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
            Tags = new List<string> { skillCode, "multiple-choice" },
            Explanation = template.Explanation,
            Options = GenerateMockOptions(template.CorrectAnswer)
        };
    }

    private AiGeneratedQuestion GenerateMultipleAnswerQuestion(
        string skillName, string skillCode, Guid? skillId,
        ProficiencyLevel? targetLevel, DifficultyLevel? difficulty,
        int index)
    {
        var questions = new[]
        {
            $"Which of the following are valid features of {skillName}? (Select all that apply)",
            $"Select all correct statements about {skillName}:",
            $"Which of these best practices apply to {skillName}? (Multiple answers)"
        };

        var options = new List<AiGeneratedOption>
        {
            new() { Content = "Feature A - Correct", IsCorrect = true, Explanation = "This is a valid feature" },
            new() { Content = "Feature B - Correct", IsCorrect = true, Explanation = "This is also valid" },
            new() { Content = "Feature C - Incorrect", IsCorrect = false, Explanation = "This is not applicable" },
            new() { Content = "Feature D - Incorrect", IsCorrect = false, Explanation = "This is incorrect" }
        };

        return new AiGeneratedQuestion
        {
            Content = $"{questions[_random.Next(questions.Length)]} (Q{index})",
            AssessmentType = AssessmentType.Quiz,
            QuestionType = QuestionType.MultipleAnswer,
            Difficulty = difficulty,
            TargetLevel = targetLevel,
            SkillId = skillId,
            SkillName = skillName,
            SuggestedPoints = GetPointsForDifficulty(difficulty ?? DifficultyLevel.Medium) + 1,
            SuggestedTimeSeconds = 90,
            Tags = new List<string> { skillCode, "multiple-answer" },
            Explanation = "Multiple correct answers exist for this question",
            Options = options.OrderBy(_ => _random.Next()).ToList()
        };
    }

    private AiGeneratedQuestion GenerateTrueFalseQuestion(
        string skillName, string skillCode, Guid? skillId,
        ProficiencyLevel? targetLevel, DifficultyLevel? difficulty,
        int index)
    {
        var statements = new[]
        {
            ($"{skillName} supports only synchronous operations.", false),
            ($"{skillName} can be used for enterprise applications.", true),
            ($"Performance optimization is not important in {skillName}.", false),
            ($"{skillName} follows industry best practices.", true)
        };

        var (statement, isTrue) = statements[_random.Next(statements.Length)];

        return new AiGeneratedQuestion
        {
            Content = $"True or False: {statement} (Q{index})",
            AssessmentType = AssessmentType.Quiz,
            QuestionType = QuestionType.TrueFalse,
            Difficulty = difficulty,
            TargetLevel = targetLevel,
            SkillId = skillId,
            SkillName = skillName,
            SuggestedPoints = GetPointsForDifficulty(difficulty ?? DifficultyLevel.Easy),
            SuggestedTimeSeconds = 30,
            Tags = new List<string> { skillCode, "true-false" },
            Explanation = isTrue ? "This statement is true" : "This statement is false",
            Options = new List<AiGeneratedOption>
            {
                new() { Content = "True", IsCorrect = isTrue, Explanation = isTrue ? "Correct!" : "The statement is actually false" },
                new() { Content = "False", IsCorrect = !isTrue, Explanation = !isTrue ? "Correct!" : "The statement is actually true" }
            }
        };
    }

    private AiGeneratedQuestion GenerateShortAnswerQuestion(
        string skillName, string skillCode, Guid? skillId,
        ProficiencyLevel? targetLevel, DifficultyLevel? difficulty,
        int index)
    {
        var questions = new[]
        {
            ($"What is the primary purpose of {skillName}?", "The primary purpose is to provide efficient and maintainable solutions for specific use cases."),
            ($"Name one key benefit of using {skillName}.", "Improved productivity, better code quality, or enhanced performance."),
            ($"What problem does {skillName} solve?", "It addresses common challenges in software development such as complexity management.")
        };

        var (question, answer) = questions[_random.Next(questions.Length)];

        return new AiGeneratedQuestion
        {
            Content = $"{question} (Q{index})",
            AssessmentType = AssessmentType.Quiz,
            QuestionType = QuestionType.ShortAnswer,
            Difficulty = difficulty,
            TargetLevel = targetLevel,
            SkillId = skillId,
            SkillName = skillName,
            SuggestedPoints = GetPointsForDifficulty(difficulty ?? DifficultyLevel.Medium),
            SuggestedTimeSeconds = 120,
            Tags = new List<string> { skillCode, "short-answer" },
            ExpectedAnswer = answer,
            GradingRubric = "- Full points: Answer covers the key concept\n- Partial: Mentions related concepts\n- Zero: Incorrect or irrelevant"
        };
    }

    private AiGeneratedQuestion GenerateLongAnswerQuestion(
        string skillName, string skillCode, Guid? skillId,
        ProficiencyLevel? targetLevel, DifficultyLevel? difficulty,
        int index)
    {
        var questions = new[]
        {
            $"Explain the architecture and design principles behind {skillName}. Include examples of when to use it.",
            $"Compare and contrast different approaches to implementing {skillName} in a large-scale application.",
            $"Describe how you would troubleshoot performance issues in a system using {skillName}."
        };

        return new AiGeneratedQuestion
        {
            Content = $"{questions[_random.Next(questions.Length)]} (Q{index})",
            AssessmentType = AssessmentType.Quiz,
            QuestionType = QuestionType.LongAnswer,
            Difficulty = difficulty,
            TargetLevel = targetLevel,
            SkillId = skillId,
            SkillName = skillName,
            SuggestedPoints = GetPointsForDifficulty(difficulty ?? DifficultyLevel.Hard) * 2,
            SuggestedTimeSeconds = 600,
            Tags = new List<string> { skillCode, "long-answer", "essay" },
            ExpectedAnswer = "A comprehensive answer should cover: 1) Core concepts, 2) Practical examples, 3) Trade-offs and considerations",
            GradingRubric = "- Depth (40%): Thorough explanation of concepts\n- Examples (30%): Relevant practical examples\n- Clarity (20%): Clear and organized writing\n- Accuracy (10%): Technical correctness"
        };
    }

    #endregion

    private List<MockQuestionTemplate> GetQuizTemplates(string skillName, string skillCode, ProficiencyLevel level)
    {
        // C# Questions
        if (skillCode == "CSHP")
            return GetCSharpQuizTemplates(level);

        // .NET Questions
        if (skillCode == "NETC" || skillCode == "ASPN")
            return GetDotNetQuizTemplates();

        // Default
        return GetGenericQuizTemplates(skillName);
    }

    private List<MockQuestionTemplate> GetCSharpQuizTemplates(ProficiencyLevel level)
    {
        if (level <= ProficiencyLevel.Assist)
        {
            return new List<MockQuestionTemplate>
            {
                new()
                {
                    Question = "What is the 'var' keyword used for in C#?",
                    CorrectAnswer = "Declare variables with implicitly inferred type",
                    Explanation = "var allows compiler to automatically determine type"
                },
                new()
                {
                    Question = "What is the default value of bool in C#?",
                    CorrectAnswer = "false",
                    Explanation = "Bool defaults to false"
                }
            };
        }

        return new List<MockQuestionTemplate>
        {
            new()
            {
                Question = "What's the difference between abstract class and interface?",
                CorrectAnswer = "Abstract class can have implementation, a class can inherit only one abstract class but implement multiple interfaces",
                Explanation = "Classic OOP question"
            },
            new()
            {
                Question = "How does async/await work in C#?",
                CorrectAnswer = "async marks a method as asynchronous, await pauses execution without blocking thread",
                Explanation = "Async improves scalability"
            }
        };
    }

    private List<MockQuestionTemplate> GetDotNetQuizTemplates()
    {
        return new List<MockQuestionTemplate>
        {
            new()
            {
                Question = "What's the difference between AddScoped, AddTransient and AddSingleton?",
                CorrectAnswer = "Singleton: 1 instance for entire app. Scoped: 1 instance per request. Transient: new instance each injection.",
                Explanation = "Choosing right lifetime is crucial"
            }
        };
    }

    private List<MockQuestionTemplate> GetGenericQuizTemplates(string skillName)
    {
        return new List<MockQuestionTemplate>
        {
            new()
            {
                Question = $"What are the basic concepts of {skillName}?",
                CorrectAnswer = $"[Answer about {skillName}]",
                Explanation = "Tests foundational knowledge"
            }
        };
    }

    private List<MockQuestionTemplate> GetCodingTemplates(string skillCode)
    {
        return new List<MockQuestionTemplate>
        {
            new()
            {
                Question = "Write a function to calculate sum of numbers in array",
                CorrectAnswer = "Sum of all elements",
                CodeSnippet = skillCode switch
                {
                    "CSHP" => "public int Sum(int[] numbers)\n{\n    // Your code here\n}",
                    "RCTS" or "JSTS" => "function sum(numbers: number[]): number {\n    // Your code here\n}",
                    "PYTH" => "def sum_numbers(numbers: list[int]) -> int:\n    # Your code here\n    pass",
                    _ => "// Implement sum function"
                },
                Explanation = "Basic array manipulation exercise"
            }
        };
    }

    private List<AiGeneratedOption> GenerateMockOptions(string correctAnswer)
    {
        var options = new List<AiGeneratedOption>
        {
            new()
            {
                Content = correctAnswer,
                IsCorrect = true,
                Explanation = "This is the correct answer"
            }
        };

        var wrongOptions = new[] { "Incorrect option A", "Incorrect option B", "Incorrect option C" };

        foreach (var wrong in wrongOptions)
        {
            options.Add(new AiGeneratedOption
            {
                Content = wrong,
                IsCorrect = false,
                Explanation = "This answer is incorrect"
            });
        }

        return options.OrderBy(_ => _random.Next()).ToList();
    }

    private string GenerateMockRubric()
    {
        return "- 0 points: No answer or completely wrong\n- 1-2 points: Partial understanding\n- 3-4 points: Correct with explanation\n- 5 points: Excellent";
    }

    private string GenerateCaseStudyRubric()
    {
        return "- Problem Analysis (30%): Correctly identify issues\n- Solution Design (40%): Viable solutions\n- Trade-offs (20%): Evaluate pros/cons\n- Communication (10%): Clear presentation";
    }

    private string GenerateRoleBasedRubric()
    {
        return "- Leadership (30%): Shows leadership ability\n- Communication (30%): Effective communication\n- Problem Solving (25%): Problem resolution\n- Empathy (15%): Understanding team members";
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
        var questionTypes = request.QuestionTypes.Count > 0
            ? string.Join(", ", request.QuestionTypes)
            : request.AssessmentType.ToString();

        return $"Generate {request.QuestionCount} {request.Language} questions. " +
               $"Question types: {questionTypes}. " +
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
