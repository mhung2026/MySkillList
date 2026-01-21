using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Assessment;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Entities.Assessment;
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

        // Get skill info
        var skill = await _context.Skills
            .Include(s => s.Subcategory)
            .ThenInclude(sc => sc.SkillDomain)
            .FirstOrDefaultAsync(s => s.Id == request.SkillId);

        if (skill == null)
        {
            return new AiGenerateQuestionsResponse
            {
                Success = false,
                Error = $"Skill with ID {request.SkillId} not found"
            };
        }

        var questions = new List<AiGeneratedQuestion>();
        var isVietnamese = request.Language == "vi";

        for (int i = 0; i < request.QuestionCount; i++)
        {
            var questionType = request.QuestionTypes[i % request.QuestionTypes.Count];
            var difficulty = request.Difficulty ?? GetDifficultyForLevel(request.TargetLevel);

            var question = GenerateMockQuestion(
                skill.Name,
                skill.Code,
                request.TargetLevel,
                questionType,
                difficulty,
                isVietnamese,
                i + 1
            );

            questions.Add(question);
        }

        var endTime = DateTime.UtcNow;

        return new AiGenerateQuestionsResponse
        {
            Success = true,
            Message = $"Generated {questions.Count} questions for {skill.Name}",
            Questions = questions,
            Metadata = new AiGenerationMetadata
            {
                Model = "mock-gpt-4",
                TokensUsed = _random.Next(500, 2000),
                GenerationTimeMs = (endTime - startTime).TotalMilliseconds + _random.Next(100, 500),
                PromptUsed = BuildMockPrompt(skill.Name, request),
                GeneratedAt = DateTime.UtcNow
            }
        };
    }

    public async Task<AiGradeAnswerResponse> GradeAnswerAsync(AiGradeAnswerRequest request)
    {
        await Task.Delay(100); // Simulate processing

        // Mock grading logic
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

    private AiGeneratedQuestion GenerateMockQuestion(
        string skillName,
        string skillCode,
        ProficiencyLevel level,
        QuestionType type,
        DifficultyLevel difficulty,
        bool isVietnamese,
        int index)
    {
        var templates = GetQuestionTemplates(skillName, skillCode, level, isVietnamese);
        var template = templates[_random.Next(templates.Count)];

        var question = new AiGeneratedQuestion
        {
            Content = $"{template.Question} (Q{index})",
            Type = type,
            Difficulty = difficulty,
            SuggestedPoints = GetPointsForDifficulty(difficulty),
            SuggestedTimeSeconds = GetTimeForDifficulty(difficulty, type),
            Tags = new List<string> { skillCode, level.ToString(), difficulty.ToString() },
            Explanation = template.Explanation
        };

        if (type == QuestionType.MultipleChoice || type == QuestionType.MultipleAnswer)
        {
            question.Options = GenerateMockOptions(template.CorrectAnswer, isVietnamese);
        }
        else if (type == QuestionType.CodingChallenge)
        {
            question.CodeSnippet = template.CodeSnippet;
            question.ExpectedAnswer = template.CorrectAnswer;
            question.GradingRubric = GenerateMockRubric(isVietnamese);
        }
        else
        {
            question.ExpectedAnswer = template.CorrectAnswer;
            question.GradingRubric = GenerateMockRubric(isVietnamese);
        }

        return question;
    }

    private List<MockQuestionTemplate> GetQuestionTemplates(
        string skillName, string skillCode, ProficiencyLevel level, bool isVi)
    {
        // C# Questions
        if (skillCode == "CSHP")
        {
            return GetCSharpQuestions(level, isVi);
        }
        // .NET Core Questions
        if (skillCode == "NETC" || skillCode == "ASPN")
        {
            return GetDotNetQuestions(level, isVi);
        }
        // React Questions
        if (skillCode == "RCTS")
        {
            return GetReactQuestions(level, isVi);
        }
        // SQL Questions
        if (skillCode == "SQLL" || skillCode == "PSQL")
        {
            return GetSqlQuestions(level, isVi);
        }
        // Default generic questions
        return GetGenericQuestions(skillName, level, isVi);
    }

    private List<MockQuestionTemplate> GetCSharpQuestions(ProficiencyLevel level, bool isVi)
    {
        var questions = new List<MockQuestionTemplate>();

        if (level <= ProficiencyLevel.Assist)
        {
            questions.AddRange(new[]
            {
                new MockQuestionTemplate
                {
                    Question = isVi
                        ? "Trong C#, từ khóa 'var' được sử dụng để làm gì?"
                        : "What is the 'var' keyword used for in C#?",
                    CorrectAnswer = isVi
                        ? "Khai báo biến với kiểu được suy luận tự động bởi compiler"
                        : "Declare variables with implicitly inferred type by the compiler",
                    Explanation = isVi
                        ? "var cho phép compiler tự động xác định kiểu dữ liệu dựa trên giá trị gán"
                        : "var allows the compiler to automatically determine the data type based on the assigned value"
                },
                new MockQuestionTemplate
                {
                    Question = isVi
                        ? "Sự khác biệt giữa 'string' và 'String' trong C# là gì?"
                        : "What is the difference between 'string' and 'String' in C#?",
                    CorrectAnswer = isVi
                        ? "Không có sự khác biệt, 'string' là alias của System.String"
                        : "No difference, 'string' is an alias for System.String",
                    Explanation = isVi
                        ? "string là keyword của C#, String là tên class trong .NET"
                        : "string is a C# keyword, String is the .NET class name"
                },
                new MockQuestionTemplate
                {
                    Question = isVi
                        ? "Giá trị mặc định của biến bool trong C# là gì?"
                        : "What is the default value of a bool variable in C#?",
                    CorrectAnswer = "false",
                    Explanation = isVi
                        ? "Tất cả value types có giá trị mặc định, bool mặc định là false"
                        : "All value types have default values, bool defaults to false"
                }
            });
        }
        else if (level <= ProficiencyLevel.Apply)
        {
            questions.AddRange(new[]
            {
                new MockQuestionTemplate
                {
                    Question = isVi
                        ? "Giải thích sự khác biệt giữa 'abstract class' và 'interface' trong C#?"
                        : "Explain the difference between 'abstract class' and 'interface' in C#?",
                    CorrectAnswer = isVi
                        ? "Abstract class có thể có implementation, interface thì không (trước C# 8). Class chỉ kế thừa 1 abstract class nhưng có thể implement nhiều interfaces."
                        : "Abstract class can have implementation, interface cannot (before C# 8). A class can inherit only one abstract class but implement multiple interfaces.",
                    Explanation = isVi
                        ? "Đây là câu hỏi kinh điển về OOP trong C#"
                        : "This is a classic OOP question in C#"
                },
                new MockQuestionTemplate
                {
                    Question = isVi
                        ? "LINQ trong C# là gì? Cho ví dụ sử dụng."
                        : "What is LINQ in C#? Provide an example.",
                    CorrectAnswer = isVi
                        ? "Language Integrated Query - cho phép query trực tiếp trên collections. VD: list.Where(x => x > 5).Select(x => x * 2)"
                        : "Language Integrated Query - allows querying directly on collections. Ex: list.Where(x => x > 5).Select(x => x * 2)",
                    Explanation = isVi
                        ? "LINQ là tính năng mạnh mẽ của C# để làm việc với data"
                        : "LINQ is a powerful C# feature for working with data",
                    CodeSnippet = "var result = numbers.Where(n => n > 5)\n                    .OrderBy(n => n)\n                    .Select(n => n * 2);"
                },
                new MockQuestionTemplate
                {
                    Question = isVi
                        ? "Async/await trong C# hoạt động như thế nào?"
                        : "How does async/await work in C#?",
                    CorrectAnswer = isVi
                        ? "async đánh dấu method là bất đồng bộ, await tạm dừng execution cho đến khi Task hoàn thành mà không block thread"
                        : "async marks a method as asynchronous, await pauses execution until the Task completes without blocking the thread",
                    Explanation = isVi
                        ? "Async programming giúp cải thiện scalability và responsiveness"
                        : "Async programming improves scalability and responsiveness"
                }
            });
        }
        else
        {
            questions.AddRange(new[]
            {
                new MockQuestionTemplate
                {
                    Question = isVi
                        ? "Giải thích về Garbage Collection trong .NET và các generation của nó?"
                        : "Explain Garbage Collection in .NET and its generations?",
                    CorrectAnswer = isVi
                        ? "GC tự động quản lý memory. Gen 0 cho short-lived objects, Gen 1 là buffer, Gen 2 cho long-lived objects. GC collect Gen 0 thường xuyên nhất."
                        : "GC automatically manages memory. Gen 0 for short-lived objects, Gen 1 is buffer, Gen 2 for long-lived objects. GC collects Gen 0 most frequently.",
                    Explanation = isVi
                        ? "Hiểu GC giúp tối ưu memory và performance"
                        : "Understanding GC helps optimize memory and performance"
                },
                new MockQuestionTemplate
                {
                    Question = isVi
                        ? "Khi nào nên sử dụng Span<T> thay vì Array trong C#?"
                        : "When should you use Span<T> instead of Array in C#?",
                    CorrectAnswer = isVi
                        ? "Span<T> dùng khi cần slice array mà không allocate memory mới, hoặc làm việc với stack-allocated memory. Giúp giảm GC pressure."
                        : "Span<T> is used when you need to slice arrays without allocating new memory, or work with stack-allocated memory. Helps reduce GC pressure.",
                    Explanation = isVi
                        ? "Span<T> là high-performance type cho memory manipulation"
                        : "Span<T> is a high-performance type for memory manipulation"
                }
            });
        }

        return questions;
    }

    private List<MockQuestionTemplate> GetDotNetQuestions(ProficiencyLevel level, bool isVi)
    {
        var questions = new List<MockQuestionTemplate>();

        questions.AddRange(new[]
        {
            new MockQuestionTemplate
            {
                Question = isVi
                    ? "Dependency Injection trong ASP.NET Core hoạt động như thế nào?"
                    : "How does Dependency Injection work in ASP.NET Core?",
                CorrectAnswer = isVi
                    ? "ASP.NET Core có built-in DI container. Đăng ký services trong Program.cs với AddScoped/AddTransient/AddSingleton, inject qua constructor."
                    : "ASP.NET Core has a built-in DI container. Register services in Program.cs with AddScoped/AddTransient/AddSingleton, inject via constructor.",
                Explanation = isVi
                    ? "DI là pattern quan trọng trong ASP.NET Core"
                    : "DI is an important pattern in ASP.NET Core"
            },
            new MockQuestionTemplate
            {
                Question = isVi
                    ? "Sự khác biệt giữa AddScoped, AddTransient và AddSingleton?"
                    : "What's the difference between AddScoped, AddTransient and AddSingleton?",
                CorrectAnswer = isVi
                    ? "Singleton: 1 instance toàn app. Scoped: 1 instance per request. Transient: new instance mỗi lần inject."
                    : "Singleton: 1 instance for entire app. Scoped: 1 instance per request. Transient: new instance each time injected.",
                Explanation = isVi
                    ? "Chọn đúng lifetime rất quan trọng để tránh bugs"
                    : "Choosing the right lifetime is crucial to avoid bugs"
            },
            new MockQuestionTemplate
            {
                Question = isVi
                    ? "Middleware trong ASP.NET Core là gì? Thứ tự có quan trọng không?"
                    : "What is Middleware in ASP.NET Core? Does order matter?",
                CorrectAnswer = isVi
                    ? "Middleware là component xử lý HTTP request/response. Thứ tự RẤT quan trọng vì chúng chạy theo pipeline order."
                    : "Middleware is a component that handles HTTP request/response. Order is VERY important as they run in pipeline order.",
                Explanation = isVi
                    ? "Understanding middleware pipeline is essential"
                    : "Understanding middleware pipeline is essential"
            }
        });

        return questions;
    }

    private List<MockQuestionTemplate> GetReactQuestions(ProficiencyLevel level, bool isVi)
    {
        return new List<MockQuestionTemplate>
        {
            new MockQuestionTemplate
            {
                Question = isVi
                    ? "Sự khác biệt giữa useState và useReducer trong React?"
                    : "What's the difference between useState and useReducer in React?",
                CorrectAnswer = isVi
                    ? "useState cho state đơn giản, useReducer cho state phức tạp với nhiều sub-values hoặc khi next state phụ thuộc previous state."
                    : "useState for simple state, useReducer for complex state with multiple sub-values or when next state depends on previous state.",
                Explanation = isVi
                    ? "Chọn đúng hook giúp code maintainable hơn"
                    : "Choosing the right hook makes code more maintainable"
            },
            new MockQuestionTemplate
            {
                Question = isVi
                    ? "useEffect cleanup function dùng để làm gì?"
                    : "What is the useEffect cleanup function used for?",
                CorrectAnswer = isVi
                    ? "Dọn dẹp side effects khi component unmount hoặc trước khi effect chạy lại (unsubscribe, clear timers, cancel requests)."
                    : "Clean up side effects when component unmounts or before effect re-runs (unsubscribe, clear timers, cancel requests).",
                Explanation = isVi
                    ? "Cleanup prevents memory leaks và bugs"
                    : "Cleanup prevents memory leaks and bugs"
            },
            new MockQuestionTemplate
            {
                Question = isVi
                    ? "Khi nào dùng useMemo và useCallback?"
                    : "When to use useMemo and useCallback?",
                CorrectAnswer = isVi
                    ? "useMemo để cache computed values, useCallback để cache function references. Dùng khi có expensive calculations hoặc pass functions to child components."
                    : "useMemo to cache computed values, useCallback to cache function references. Use for expensive calculations or passing functions to child components.",
                Explanation = isVi
                    ? "Optimization hooks giúp tránh unnecessary re-renders"
                    : "Optimization hooks help avoid unnecessary re-renders"
            }
        };
    }

    private List<MockQuestionTemplate> GetSqlQuestions(ProficiencyLevel level, bool isVi)
    {
        return new List<MockQuestionTemplate>
        {
            new MockQuestionTemplate
            {
                Question = isVi
                    ? "Sự khác biệt giữa INNER JOIN và LEFT JOIN?"
                    : "What's the difference between INNER JOIN and LEFT JOIN?",
                CorrectAnswer = isVi
                    ? "INNER JOIN chỉ trả về rows match ở cả 2 tables. LEFT JOIN trả về tất cả rows từ left table và matching rows từ right (NULL nếu không match)."
                    : "INNER JOIN returns only matching rows from both tables. LEFT JOIN returns all rows from left table and matching rows from right (NULL if no match).",
                Explanation = isVi
                    ? "Hiểu JOINs là foundation của SQL"
                    : "Understanding JOINs is the foundation of SQL"
            },
            new MockQuestionTemplate
            {
                Question = isVi
                    ? "Index trong database hoạt động như thế nào? Khi nào không nên dùng?"
                    : "How do database indexes work? When should you not use them?",
                CorrectAnswer = isVi
                    ? "Index giống mục lục sách, giúp tìm data nhanh hơn. Không nên dùng cho: tables nhỏ, columns ít được query, columns thường xuyên UPDATE."
                    : "Index works like a book's table of contents, helps find data faster. Don't use for: small tables, rarely queried columns, frequently UPDATEd columns.",
                Explanation = isVi
                    ? "Indexes improve read but slow down writes"
                    : "Indexes improve read but slow down writes"
            },
            new MockQuestionTemplate
            {
                Question = isVi
                    ? "Giải thích về Transaction và ACID properties?"
                    : "Explain Transaction and ACID properties?",
                CorrectAnswer = isVi
                    ? "Transaction là nhóm operations chạy như 1 unit. ACID: Atomicity (all or nothing), Consistency (valid state), Isolation (concurrent transactions independent), Durability (committed = permanent)."
                    : "Transaction is a group of operations that run as one unit. ACID: Atomicity (all or nothing), Consistency (valid state), Isolation (concurrent transactions independent), Durability (committed = permanent).",
                Explanation = isVi
                    ? "ACID ensures data integrity"
                    : "ACID ensures data integrity"
            }
        };
    }

    private List<MockQuestionTemplate> GetGenericQuestions(string skillName, ProficiencyLevel level, bool isVi)
    {
        return new List<MockQuestionTemplate>
        {
            new MockQuestionTemplate
            {
                Question = isVi
                    ? $"Hãy giải thích các khái niệm cơ bản của {skillName}?"
                    : $"Explain the basic concepts of {skillName}?",
                CorrectAnswer = isVi
                    ? $"[Câu trả lời mẫu về {skillName}]"
                    : $"[Sample answer about {skillName}]",
                Explanation = isVi
                    ? "Đây là câu hỏi kiểm tra kiến thức nền tảng"
                    : "This tests foundational knowledge"
            },
            new MockQuestionTemplate
            {
                Question = isVi
                    ? $"Khi nào bạn sẽ sử dụng {skillName} trong dự án thực tế?"
                    : $"When would you use {skillName} in a real project?",
                CorrectAnswer = isVi
                    ? $"[Câu trả lời về use cases của {skillName}]"
                    : $"[Answer about use cases of {skillName}]",
                Explanation = isVi
                    ? "Đánh giá khả năng áp dụng thực tế"
                    : "Evaluates practical application ability"
            },
            new MockQuestionTemplate
            {
                Question = isVi
                    ? $"Những best practices nào bạn nên tuân theo khi làm việc với {skillName}?"
                    : $"What best practices should you follow when working with {skillName}?",
                CorrectAnswer = isVi
                    ? $"[Các best practices cho {skillName}]"
                    : $"[Best practices for {skillName}]",
                Explanation = isVi
                    ? "Kiểm tra hiểu biết về standards và conventions"
                    : "Tests understanding of standards and conventions"
            }
        };
    }

    private List<AiGeneratedOption> GenerateMockOptions(string correctAnswer, bool isVi)
    {
        var options = new List<AiGeneratedOption>
        {
            new AiGeneratedOption
            {
                Content = correctAnswer,
                IsCorrect = true,
                Explanation = isVi ? "Đây là đáp án đúng" : "This is the correct answer"
            }
        };

        // Add wrong options
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

        // Shuffle options
        return options.OrderBy(_ => _random.Next()).ToList();
    }

    private string GenerateMockRubric(bool isVi)
    {
        return isVi
            ? "- 0 điểm: Không trả lời hoặc hoàn toàn sai\n- 1-2 điểm: Hiểu một phần, thiếu chi tiết\n- 3-4 điểm: Trả lời đúng, có giải thích\n- 5 điểm: Trả lời xuất sắc, có ví dụ thực tế"
            : "- 0 points: No answer or completely wrong\n- 1-2 points: Partial understanding, lacks detail\n- 3-4 points: Correct answer with explanation\n- 5 points: Excellent answer with real examples";
    }

    private DifficultyLevel GetDifficultyForLevel(ProficiencyLevel level)
    {
        return level switch
        {
            ProficiencyLevel.Follow or ProficiencyLevel.Assist => DifficultyLevel.Easy,
            ProficiencyLevel.Apply => DifficultyLevel.Medium,
            ProficiencyLevel.Enable => DifficultyLevel.Hard,
            _ => DifficultyLevel.Expert
        };
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

    private int GetTimeForDifficulty(DifficultyLevel difficulty, QuestionType type)
    {
        var baseTime = difficulty switch
        {
            DifficultyLevel.Easy => 60,
            DifficultyLevel.Medium => 120,
            DifficultyLevel.Hard => 180,
            DifficultyLevel.Expert => 300,
            _ => 60
        };

        // Coding challenges need more time
        if (type == QuestionType.CodingChallenge)
            baseTime *= 3;

        return baseTime;
    }

    private string BuildMockPrompt(string skillName, AiGenerateQuestionsRequest request)
    {
        return $"Generate {request.QuestionCount} {request.Language} questions for {skillName} " +
               $"at level {request.TargetLevel}. Types: {string.Join(", ", request.QuestionTypes)}. " +
               $"Additional context: {request.AdditionalContext ?? "None"}";
    }

    private string GenerateMockFeedback(double percentage)
    {
        if (percentage >= 0.9) return "Excellent answer! You demonstrated deep understanding.";
        if (percentage >= 0.7) return "Good answer with correct core concepts.";
        if (percentage >= 0.5) return "Partial understanding shown. Some key points missing.";
        return "Answer needs improvement. Review the core concepts.";
    }

    private List<string> GenerateMockStrengths(double percentage)
    {
        if (percentage >= 0.7)
            return new List<string> { "Clear explanation", "Good use of terminology" };
        return new List<string> { "Attempted to answer" };
    }

    private List<string> GenerateMockImprovements(double percentage)
    {
        if (percentage < 0.7)
            return new List<string> { "Add more specific examples", "Explain the reasoning", "Cover edge cases" };
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
