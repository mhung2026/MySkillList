using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Configuration;
using SkillMatrix.Domain.Entities.Configuration;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Application.Services;

public class SystemEnumService
{
    private readonly SkillMatrixDbContext _context;

    public SystemEnumService(SkillMatrixDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all enum types with value counts
    /// </summary>
    public async Task<List<EnumTypeDto>> GetAllEnumTypesAsync()
    {
        var groups = await _context.SystemEnumValues
            .GroupBy(e => e.EnumType)
            .Select(g => new EnumTypeDto
            {
                EnumType = g.Key,
                DisplayName = GetEnumTypeDisplayName(g.Key),
                Description = GetEnumTypeDescription(g.Key),
                ValueCount = g.Count()
            })
            .ToListAsync();

        // Add enum types that don't have values yet
        foreach (var enumType in SystemEnumTypes.All)
        {
            if (!groups.Any(g => g.EnumType == enumType))
            {
                groups.Add(new EnumTypeDto
                {
                    EnumType = enumType,
                    DisplayName = GetEnumTypeDisplayName(enumType),
                    Description = GetEnumTypeDescription(enumType),
                    ValueCount = 0
                });
            }
        }

        return groups.OrderBy(g => g.EnumType).ToList();
    }

    /// <summary>
    /// Get all values for a specific enum type
    /// </summary>
    public async Task<List<SystemEnumValueDto>> GetValuesByTypeAsync(string enumType, bool includeInactive = false)
    {
        var query = _context.SystemEnumValues
            .Where(e => e.EnumType == enumType);

        if (!includeInactive)
        {
            query = query.Where(e => e.IsActive);
        }

        return await query
            .OrderBy(e => e.DisplayOrder)
            .ThenBy(e => e.Value)
            .Select(e => MapToDto(e))
            .ToListAsync();
    }

    /// <summary>
    /// Get enum values for dropdown (simplified)
    /// </summary>
    public async Task<List<EnumDropdownItemDto>> GetDropdownAsync(string enumType, string language = "en")
    {
        return await _context.SystemEnumValues
            .Where(e => e.EnumType == enumType && e.IsActive)
            .OrderBy(e => e.DisplayOrder)
            .ThenBy(e => e.Value)
            .Select(e => new EnumDropdownItemDto
            {
                Value = e.Value,
                Code = e.Code,
                Label = language == "vi" && e.NameVi != null ? e.NameVi : e.Name,
                LabelVi = e.NameVi,
                Color = e.Color,
                Icon = e.Icon
            })
            .ToListAsync();
    }

    /// <summary>
    /// Get a single enum value by ID
    /// </summary>
    public async Task<SystemEnumValueDto?> GetByIdAsync(Guid id)
    {
        var entity = await _context.SystemEnumValues.FindAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    /// <summary>
    /// Get enum value by type and value
    /// </summary>
    public async Task<SystemEnumValueDto?> GetByTypeAndValueAsync(string enumType, int value)
    {
        var entity = await _context.SystemEnumValues
            .FirstOrDefaultAsync(e => e.EnumType == enumType && e.Value == value);
        return entity == null ? null : MapToDto(entity);
    }

    /// <summary>
    /// Get enum value by type and code
    /// </summary>
    public async Task<SystemEnumValueDto?> GetByTypeAndCodeAsync(string enumType, string code)
    {
        var entity = await _context.SystemEnumValues
            .FirstOrDefaultAsync(e => e.EnumType == enumType && e.Code == code);
        return entity == null ? null : MapToDto(entity);
    }

    /// <summary>
    /// Create a new enum value
    /// </summary>
    public async Task<SystemEnumValueDto> CreateAsync(CreateSystemEnumValueDto dto)
    {
        // Validate enum type
        if (!SystemEnumTypes.All.Contains(dto.EnumType))
        {
            throw new ArgumentException($"Invalid enum type: {dto.EnumType}");
        }

        // Check for duplicate value or code
        var exists = await _context.SystemEnumValues
            .AnyAsync(e => e.EnumType == dto.EnumType && (e.Value == dto.Value || e.Code == dto.Code));
        if (exists)
        {
            throw new InvalidOperationException($"Value or Code already exists for {dto.EnumType}");
        }

        // Auto display order if not specified
        var displayOrder = dto.DisplayOrder ?? await GetNextDisplayOrderAsync(dto.EnumType);

        var entity = new SystemEnumValue
        {
            EnumType = dto.EnumType,
            Value = dto.Value,
            Code = dto.Code,
            Name = dto.Name,
            NameVi = dto.NameVi,
            Description = dto.Description,
            DescriptionVi = dto.DescriptionVi,
            Color = dto.Color,
            Icon = dto.Icon,
            DisplayOrder = displayOrder,
            IsActive = true,
            IsSystem = false,
            Metadata = dto.Metadata
        };

        _context.SystemEnumValues.Add(entity);
        await _context.SaveChangesAsync();

        return MapToDto(entity);
    }

    /// <summary>
    /// Update an enum value
    /// </summary>
    public async Task<SystemEnumValueDto?> UpdateAsync(Guid id, UpdateSystemEnumValueDto dto)
    {
        var entity = await _context.SystemEnumValues.FindAsync(id);
        if (entity == null) return null;

        entity.Name = dto.Name;
        entity.NameVi = dto.NameVi;
        entity.Description = dto.Description;
        entity.DescriptionVi = dto.DescriptionVi;
        entity.Color = dto.Color;
        entity.Icon = dto.Icon;
        if (dto.DisplayOrder.HasValue)
        {
            entity.DisplayOrder = dto.DisplayOrder.Value;
        }
        entity.Metadata = dto.Metadata;

        await _context.SaveChangesAsync();
        return MapToDto(entity);
    }

    /// <summary>
    /// Toggle active status
    /// </summary>
    public async Task<bool> ToggleActiveAsync(Guid id)
    {
        var entity = await _context.SystemEnumValues.FindAsync(id);
        if (entity == null) return false;

        // System values cannot be deactivated
        if (entity.IsSystem && entity.IsActive)
        {
            throw new InvalidOperationException("System enum values cannot be deactivated");
        }

        entity.IsActive = !entity.IsActive;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Delete an enum value (soft delete)
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.SystemEnumValues.FindAsync(id);
        if (entity == null) return false;

        // System values cannot be deleted
        if (entity.IsSystem)
        {
            throw new InvalidOperationException("System enum values cannot be deleted");
        }

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Reorder enum values
    /// </summary>
    public async Task<bool> ReorderAsync(ReorderEnumValuesDto dto)
    {
        var values = await _context.SystemEnumValues
            .Where(e => e.EnumType == dto.EnumType && dto.OrderedIds.Contains(e.Id))
            .ToListAsync();

        for (int i = 0; i < dto.OrderedIds.Count; i++)
        {
            var entity = values.FirstOrDefault(v => v.Id == dto.OrderedIds[i]);
            if (entity != null)
            {
                entity.DisplayOrder = i + 1;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Seed default enum values for all types
    /// </summary>
    public async Task SeedDefaultValuesAsync()
    {
        // SkillCategory
        await SeedEnumTypeAsync(SystemEnumTypes.SkillCategory, new[]
        {
            (1, "Technical", "Technical", "Kỹ thuật", "Technical skills related to technology and development", "#1890ff", null as string),
            (2, "Professional", "Professional", "Chuyên nghiệp", "Soft skills and professional behaviors", "#52c41a", null),
            (3, "Domain", "Domain", "Chuyên ngành", "Domain-specific business knowledge", "#722ed1", null),
            (4, "Leadership", "Leadership", "Lãnh đạo", "Leadership and management skills", "#fa8c16", null),
            (5, "Tools", "Tools", "Công cụ", "Tools and platform proficiency", "#13c2c2", null)
        });

        // SkillType
        await SeedEnumTypeAsync(SystemEnumTypes.SkillType, new[]
        {
            (1, "Core", "Core", "Cốt lõi", "Core skills everyone needs", "#f5222d", null as string),
            (2, "Specialty", "Specialty", "Chuyên môn", "Role-specific deep skills", "#1890ff", null),
            (3, "Adjacent", "Adjacent", "Liên quan", "Nice to have, cross-functional skills", "#52c41a", null)
        });

        // AssessmentType
        await SeedEnumTypeAsync(SystemEnumTypes.AssessmentType, new[]
        {
            (1, "SelfAssessment", "Self Assessment", "Tự đánh giá", "Employee self-assessment", "#52c41a", "UserOutlined"),
            (2, "ManagerAssessment", "Manager Assessment", "Manager đánh giá", "Assessment by manager", "#1890ff", "TeamOutlined"),
            (3, "PeerAssessment", "Peer Assessment", "Đồng nghiệp đánh giá", "Assessment by peers", "#722ed1", "UsergroupAddOutlined"),
            (4, "Quiz", "Quiz", "Trắc nghiệm", "Multiple choice quiz", "#fa8c16", "FormOutlined"),
            (5, "CodingTest", "Coding Test", "Bài test code", "Coding challenge assessment", "#13c2c2", "CodeOutlined"),
            (6, "CaseStudy", "Case Study", "Bài tập tình huống", "Case study analysis", "#eb2f96", "FileTextOutlined"),
            (7, "RoleBasedTest", "Role-based Test", "Test theo vai trò", "Role-specific assessment", "#faad14", "SolutionOutlined"),
            (8, "SituationalJudgment", "Situational Judgment", "Đánh giá tình huống", "SJT assessment", "#2f54eb", "BulbOutlined")
        });

        // AssessmentStatus
        await SeedEnumTypeAsync(SystemEnumTypes.AssessmentStatus, new[]
        {
            (1, "Draft", "Draft", "Nháp", "Assessment is in draft", "#d9d9d9", null as string),
            (2, "Pending", "Pending", "Chờ xử lý", "Waiting to start", "#faad14", null),
            (3, "InProgress", "In Progress", "Đang làm", "Assessment in progress", "#1890ff", null),
            (4, "Completed", "Completed", "Hoàn thành", "Assessment completed", "#52c41a", null),
            (5, "Reviewed", "Reviewed", "Đã review", "Assessment reviewed", "#722ed1", null),
            (6, "Disputed", "Disputed", "Có phản hồi", "Employee disputed results", "#f5222d", null),
            (7, "Resolved", "Resolved", "Đã giải quyết", "Dispute resolved", "#13c2c2", null)
        });

        // QuestionType
        await SeedEnumTypeAsync(SystemEnumTypes.QuestionType, new[]
        {
            (1, "MultipleChoice", "Multiple Choice", "Trắc nghiệm một đáp án", "Single correct answer", "#1890ff", "CheckCircleOutlined"),
            (2, "MultipleAnswer", "Multiple Answer", "Trắc nghiệm nhiều đáp án", "Multiple correct answers", "#722ed1", "CheckSquareOutlined"),
            (3, "TrueFalse", "True/False", "Đúng/Sai", "True or false question", "#52c41a", "QuestionCircleOutlined"),
            (4, "ShortAnswer", "Short Answer", "Trả lời ngắn", "Brief text response", "#fa8c16", "EditOutlined"),
            (5, "LongAnswer", "Long Answer", "Tự luận", "Extended text response", "#eb2f96", "FileTextOutlined"),
            (6, "CodingChallenge", "Coding Challenge", "Viết code", "Code writing/completion", "#13c2c2", "CodeOutlined"),
            (7, "Scenario", "Scenario", "Tình huống", "Scenario-based question", "#faad14", "ApartmentOutlined"),
            (8, "SituationalJudgment", "Situational Judgment", "Đánh giá tình huống", "SJT question", "#2f54eb", "BulbOutlined")
        });

        // DifficultyLevel
        await SeedEnumTypeAsync(SystemEnumTypes.DifficultyLevel, new[]
        {
            (1, "Easy", "Easy", "Dễ", "Basic level", "#52c41a", null as string),
            (2, "Medium", "Medium", "Trung bình", "Intermediate level", "#faad14", null),
            (3, "Hard", "Hard", "Khó", "Advanced level", "#fa8c16", null),
            (4, "Expert", "Expert", "Chuyên gia", "Expert level", "#f5222d", null)
        });

        // GapPriority
        await SeedEnumTypeAsync(SystemEnumTypes.GapPriority, new[]
        {
            (1, "Low", "Low", "Thấp", "Low priority gap", "#52c41a", null as string),
            (2, "Medium", "Medium", "Trung bình", "Medium priority gap", "#faad14", null),
            (3, "High", "High", "Cao", "High priority gap", "#fa8c16", null),
            (4, "Critical", "Critical", "Nghiêm trọng", "Critical gap that needs immediate attention", "#f5222d", null)
        });

        // LearningResourceType
        await SeedEnumTypeAsync(SystemEnumTypes.LearningResourceType, new[]
        {
            (1, "Course", "Course", "Khóa học", "Online or offline course", "#1890ff", "PlayCircleOutlined"),
            (2, "Book", "Book", "Sách", "Book or ebook", "#722ed1", "BookOutlined"),
            (3, "Video", "Video", "Video", "Video tutorial", "#eb2f96", "VideoCameraOutlined"),
            (4, "Article", "Article", "Bài viết", "Article or blog post", "#52c41a", "FileTextOutlined"),
            (5, "Workshop", "Workshop", "Workshop", "Hands-on workshop", "#fa8c16", "TeamOutlined"),
            (6, "Certification", "Certification", "Chứng chỉ", "Certification program", "#13c2c2", "SafetyCertificateOutlined"),
            (7, "Project", "Project", "Dự án thực hành", "Hands-on project", "#faad14", "ProjectOutlined"),
            (8, "Mentorship", "Mentorship", "Mentorship", "One-on-one mentoring", "#2f54eb", "UserSwitchOutlined"),
            (9, "Seminar", "Seminar", "Hội thảo", "Internal knowledge sharing", "#f5222d", "NotificationOutlined")
        });

        // LearningPathStatus
        await SeedEnumTypeAsync(SystemEnumTypes.LearningPathStatus, new[]
        {
            (1, "Suggested", "Suggested", "Đề xuất", "AI suggested learning path", "#d9d9d9", null as string),
            (2, "Approved", "Approved", "Đã duyệt", "Manager approved", "#52c41a", null),
            (3, "InProgress", "In Progress", "Đang học", "Currently learning", "#1890ff", null),
            (4, "Completed", "Completed", "Hoàn thành", "Learning completed", "#722ed1", null),
            (5, "Paused", "Paused", "Tạm dừng", "Learning paused", "#faad14", null),
            (6, "Cancelled", "Cancelled", "Đã hủy", "Learning cancelled", "#f5222d", null)
        });

        // EmploymentStatus
        await SeedEnumTypeAsync(SystemEnumTypes.EmploymentStatus, new[]
        {
            (1, "Active", "Active", "Đang làm việc", "Currently employed", "#52c41a", null as string),
            (2, "OnLeave", "On Leave", "Nghỉ phép", "On leave", "#faad14", null),
            (3, "Resigned", "Resigned", "Đã nghỉ việc", "Resigned", "#d9d9d9", null),
            (4, "Terminated", "Terminated", "Đã chấm dứt", "Employment terminated", "#f5222d", null)
        });

        // UserRole
        await SeedEnumTypeAsync(SystemEnumTypes.UserRole, new[]
        {
            (1, "Employee", "Employee", "Nhân viên", "Regular employee", "#52c41a", "UserOutlined"),
            (2, "TeamLead", "Team Lead", "Trưởng nhóm", "Team lead with limited management", "#1890ff", "TeamOutlined"),
            (3, "Manager", "Manager", "Quản lý", "Department manager", "#722ed1", "CrownOutlined"),
            (4, "Admin", "Admin", "Quản trị viên", "System administrator", "#f5222d", "SettingOutlined")
        });

        // SjtEffectiveness
        await SeedEnumTypeAsync(SystemEnumTypes.SjtEffectiveness, new[]
        {
            (1, "MostEffective", "Most Effective", "Hiệu quả nhất", "Best possible response", "#52c41a", null as string),
            (2, "Effective", "Effective", "Hiệu quả", "Good response", "#1890ff", null),
            (3, "Ineffective", "Ineffective", "Không hiệu quả", "Poor response", "#faad14", null),
            (4, "CounterProductive", "Counter Productive", "Phản tác dụng", "Harmful response", "#f5222d", null)
        });
    }

    private async Task SeedEnumTypeAsync(string enumType, (int value, string code, string name, string nameVi, string desc, string color, string? icon)[] values)
    {
        var existingCount = await _context.SystemEnumValues.CountAsync(e => e.EnumType == enumType);
        if (existingCount > 0) return; // Already seeded

        var order = 1;
        foreach (var (value, code, name, nameVi, desc, color, icon) in values)
        {
            _context.SystemEnumValues.Add(new SystemEnumValue
            {
                EnumType = enumType,
                Value = value,
                Code = code,
                Name = name,
                NameVi = nameVi,
                Description = desc,
                DescriptionVi = desc,
                Color = color,
                Icon = icon,
                DisplayOrder = order++,
                IsActive = true,
                IsSystem = true
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task<int> GetNextDisplayOrderAsync(string enumType)
    {
        var max = await _context.SystemEnumValues
            .Where(e => e.EnumType == enumType)
            .MaxAsync(e => (int?)e.DisplayOrder) ?? 0;
        return max + 1;
    }

    private static SystemEnumValueDto MapToDto(SystemEnumValue entity)
    {
        return new SystemEnumValueDto
        {
            Id = entity.Id,
            EnumType = entity.EnumType,
            Value = entity.Value,
            Code = entity.Code,
            Name = entity.Name,
            NameVi = entity.NameVi,
            Description = entity.Description,
            DescriptionVi = entity.DescriptionVi,
            Color = entity.Color,
            Icon = entity.Icon,
            DisplayOrder = entity.DisplayOrder,
            IsActive = entity.IsActive,
            IsSystem = entity.IsSystem,
            Metadata = entity.Metadata
        };
    }

    private static string GetEnumTypeDisplayName(string enumType) => enumType switch
    {
        SystemEnumTypes.SkillCategory => "Skill Category",
        SystemEnumTypes.SkillType => "Skill Type",
        SystemEnumTypes.AssessmentType => "Assessment Type",
        SystemEnumTypes.AssessmentStatus => "Assessment Status",
        SystemEnumTypes.QuestionType => "Question Type",
        SystemEnumTypes.DifficultyLevel => "Difficulty Level",
        SystemEnumTypes.GapPriority => "Gap Priority",
        SystemEnumTypes.LearningResourceType => "Learning Resource Type",
        SystemEnumTypes.LearningPathStatus => "Learning Path Status",
        SystemEnumTypes.EmploymentStatus => "Employment Status",
        SystemEnumTypes.UserRole => "User Role",
        SystemEnumTypes.SjtEffectiveness => "SJT Effectiveness",
        _ => enumType
    };

    private static string GetEnumTypeDescription(string enumType) => enumType switch
    {
        SystemEnumTypes.SkillCategory => "Categories for classifying skills (Technical, Professional, etc.)",
        SystemEnumTypes.SkillType => "T-shaped model classification (Core, Specialty, Adjacent)",
        SystemEnumTypes.AssessmentType => "Types of assessments (Quiz, Coding Test, Case Study, etc.)",
        SystemEnumTypes.AssessmentStatus => "Status workflow for assessments",
        SystemEnumTypes.QuestionType => "Types of questions in assessments",
        SystemEnumTypes.DifficultyLevel => "Difficulty levels for questions",
        SystemEnumTypes.GapPriority => "Priority levels for skill gaps",
        SystemEnumTypes.LearningResourceType => "Types of learning resources",
        SystemEnumTypes.LearningPathStatus => "Status of learning paths",
        SystemEnumTypes.EmploymentStatus => "Employee employment status",
        SystemEnumTypes.UserRole => "System user roles",
        SystemEnumTypes.SjtEffectiveness => "Effectiveness levels for SJT responses",
        _ => string.Empty
    };
}
