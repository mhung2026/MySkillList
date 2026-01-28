using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkillMatrix.Application.DTOs.Employee;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Application.Services.AI;
using SkillMatrix.Domain.Entities.Learning;
using SkillMatrix.Domain.Enums;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Application.Services;

public class EmployeeProfileService : IEmployeeProfileService
{
    private readonly SkillMatrixDbContext _context;
    private readonly IAiLearningPathService _aiLearningPathService;
    private readonly IAiSkillAnalyzerService _aiSkillAnalyzer;
    private readonly ILogger<EmployeeProfileService> _logger;

    public EmployeeProfileService(
        SkillMatrixDbContext context,
        IAiLearningPathService aiLearningPathService,
        IAiSkillAnalyzerService aiSkillAnalyzer,
        ILogger<EmployeeProfileService> logger)
    {
        _context = context;
        _aiLearningPathService = aiLearningPathService;
        _aiSkillAnalyzer = aiSkillAnalyzer;
        _logger = logger;
    }

    public async Task<SkillProfileDto?> GetSkillProfileAsync(Guid employeeId)
    {
        var employee = await _context.Employees
            .Include(e => e.Team)
            .Include(e => e.JobRole)
            .Include(e => e.Skills)
                .ThenInclude(s => s.Skill)
                    .ThenInclude(s => s.Subcategory)
                        .ThenInclude(sc => sc.SkillDomain)
            .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

        if (employee == null)
            return null;

        // Get recent assessments
        var recentAssessments = await _context.Assessments
            .Where(a => a.EmployeeId == employeeId && !a.IsDeleted)
            .OrderByDescending(a => a.CompletedAt ?? a.CreatedAt)
            .Take(5)
            .Select(a => new ProfileAssessmentDto
            {
                Id = a.Id,
                Title = a.Title ?? "Assessment",
                CompletedAt = a.CompletedAt,
                Score = a.Score,
                MaxScore = a.MaxScore,
                Percentage = a.Percentage,
                Status = a.Status.ToString()
            })
            .ToListAsync();

        // Build skill list
        var skills = employee.Skills
            .Where(es => !es.IsDeleted)
            .Select(es => new EmployeeSkillDetailDto
            {
                SkillId = es.SkillId,
                SkillName = es.Skill.Name,
                SkillCode = es.Skill.Code,
                Category = es.Skill.Category.ToString(),
                DomainName = es.Skill.Subcategory?.SkillDomain?.Name,
                CurrentLevel = (int)es.CurrentLevel,
                CurrentLevelName = es.CurrentLevel.ToString(),
                SelfAssessedLevel = es.SelfAssessedLevel.HasValue ? (int?)es.SelfAssessedLevel.Value : null,
                ManagerAssessedLevel = es.ManagerAssessedLevel.HasValue ? (int?)es.ManagerAssessedLevel.Value : null,
                TestValidatedLevel = es.TestValidatedLevel.HasValue ? (int?)es.TestValidatedLevel.Value : null,
                IsValidated = es.IsValidated,
                LastAssessedAt = es.LastAssessedAt
            })
            .OrderByDescending(s => s.CurrentLevel)
            .ToList();

        // Calculate summary
        var validatedCount = skills.Count(s => s.IsValidated);
        var avgLevel = skills.Any() ? skills.Average(s => s.CurrentLevel) : 0;
        var strengths = skills.Take(5).Select(s => s.SkillName).ToList();
        var improvementAreas = skills
            .Where(s => s.CurrentLevel < 3)
            .OrderBy(s => s.CurrentLevel)
            .Take(5)
            .Select(s => s.SkillName)
            .ToList();

        return new SkillProfileDto
        {
            Employee = new EmployeeBasicDto
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Email = employee.Email,
                JobRole = employee.JobRole != null ? new RoleBasicDto
                {
                    Id = employee.JobRole.Id,
                    Name = employee.JobRole.Name,
                    Code = employee.JobRole.Code,
                    LevelInHierarchy = employee.JobRole.LevelInHierarchy
                } : null,
                Team = employee.Team != null ? new TeamBasicDto
                {
                    Id = employee.Team.Id,
                    Name = employee.Team.Name
                } : null,
                YearsOfExperience = employee.YearsOfExperience
            },
            Skills = skills,
            Summary = new SkillProfileSummaryDto
            {
                TotalSkills = skills.Count,
                ValidatedSkills = validatedCount,
                AverageLevel = Math.Round(avgLevel, 2),
                Strengths = strengths,
                ImprovementAreas = improvementAreas
            },
            RecentAssessments = recentAssessments
        };
    }

    public async Task<GapAnalysisDto?> GetGapAnalysisAsync(Guid employeeId, Guid? targetRoleId = null)
    {
        var employee = await _context.Employees
            .Include(e => e.Team)
            .Include(e => e.JobRole)
            .Include(e => e.Skills)
                .ThenInclude(s => s.Skill)
            .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

        if (employee == null)
            return null;

        // Determine target role
        var roleId = targetRoleId ?? employee.JobRoleId;
        if (!roleId.HasValue)
        {
            return new GapAnalysisDto
            {
                Employee = MapToEmployeeBasicDto(employee),
                CurrentRole = null,
                TargetRole = new RoleBasicDto { Name = "No role assigned" },
                Gaps = new List<SkillGapDetailDto>(),
                Summary = new GapAnalysisSummaryDto { OverallReadiness = 0 }
            };
        }

        var targetRole = await _context.JobRoles
            .Include(r => r.SkillRequirements)
                .ThenInclude(sr => sr.Skill)
            .FirstOrDefaultAsync(r => r.Id == roleId.Value && !r.IsDeleted);

        if (targetRole == null)
        {
            return new GapAnalysisDto
            {
                Employee = MapToEmployeeBasicDto(employee),
                CurrentRole = employee.JobRole != null ? new RoleBasicDto
                {
                    Id = employee.JobRole.Id,
                    Name = employee.JobRole.Name,
                    Code = employee.JobRole.Code
                } : null,
                TargetRole = new RoleBasicDto { Name = "Role not found" },
                Gaps = new List<SkillGapDetailDto>(),
                Summary = new GapAnalysisSummaryDto { OverallReadiness = 0 }
            };
        }

        // Get existing gaps
        var existingGaps = await _context.SkillGaps
            .Where(g => g.EmployeeId == employeeId && !g.IsDeleted && g.ResolvedAt == null)
            .ToDictionaryAsync(g => g.SkillId);

        // Build employee skills lookup
        var employeeSkills = employee.Skills
            .Where(es => !es.IsDeleted)
            .ToDictionary(es => es.SkillId);

        // Analyze gaps - include both gaps and met requirements
        var gaps = new List<SkillGapDetailDto>();
        int metCount = 0;

        foreach (var req in targetRole.SkillRequirements.Where(r => !r.IsDeleted))
        {
            var currentLevel = ProficiencyLevel.None;
            if (employeeSkills.TryGetValue(req.SkillId, out var empSkill))
            {
                currentLevel = empSkill.CurrentLevel;
            }

            var gapSize = (int)req.MinimumLevel - (int)currentLevel;
            var isMet = gapSize <= 0;

            if (isMet)
            {
                metCount++;
                gapSize = 0; // Set to 0 for met requirements
            }

            // Get existing gap data for AI analysis (only for actual gaps)
            SkillGap? existingGap = null;
            if (!isMet)
            {
                existingGaps.TryGetValue(req.SkillId, out existingGap);
            }

            // Add all skills (both gaps and met requirements)
            gaps.Add(new SkillGapDetailDto
            {
                SkillId = req.SkillId,
                SkillName = req.Skill.Name,
                SkillCode = req.Skill.Code,
                CurrentLevel = (int)currentLevel,
                CurrentLevelName = currentLevel.ToString(),
                RequiredLevel = (int)req.MinimumLevel,
                RequiredLevelName = req.MinimumLevel.ToString(),
                ExpectedLevel = req.ExpectedLevel.HasValue ? (int?)req.ExpectedLevel.Value : null,
                GapSize = gapSize,
                Priority = isMet ? "Met" : CalculateGapPriority(gapSize, req.IsMandatory).ToString(),
                IsMandatory = req.IsMandatory,
                IsMet = isMet,
                AiAnalysis = existingGap?.AiAnalysis,
                AiRecommendation = existingGap?.AiRecommendation
            });
        }

        // Sort: Met requirements last, gaps first by priority
        gaps = gaps
            .OrderBy(g => g.IsMet) // False first (gaps), then True (met)
            .ThenByDescending(g => g.Priority == "Critical")
            .ThenByDescending(g => g.Priority == "High")
            .ThenByDescending(g => g.Priority == "Medium")
            .ThenByDescending(g => g.GapSize)
            .ToList();

        // Calculate readiness
        var totalRequirements = targetRole.SkillRequirements.Count(r => !r.IsDeleted);
        var readiness = totalRequirements > 0
            ? Math.Round((double)metCount / totalRequirements * 100, 1)
            : 100;

        return new GapAnalysisDto
        {
            Employee = MapToEmployeeBasicDto(employee),
            CurrentRole = employee.JobRole != null ? new RoleBasicDto
            {
                Id = employee.JobRole.Id,
                Name = employee.JobRole.Name,
                Code = employee.JobRole.Code,
                LevelInHierarchy = employee.JobRole.LevelInHierarchy
            } : null,
            TargetRole = new RoleBasicDto
            {
                Id = targetRole.Id,
                Name = targetRole.Name,
                Code = targetRole.Code,
                LevelInHierarchy = targetRole.LevelInHierarchy
            },
            Gaps = gaps,
            Summary = new GapAnalysisSummaryDto
            {
                TotalGaps = gaps.Count,
                CriticalGaps = gaps.Count(g => g.Priority == "Critical"),
                HighGaps = gaps.Count(g => g.Priority == "High"),
                MediumGaps = gaps.Count(g => g.Priority == "Medium"),
                LowGaps = gaps.Count(g => g.Priority == "Low"),
                MetRequirements = metCount,
                OverallReadiness = readiness
            }
        };
    }

    public async Task<RecalculateGapsResultDto> RecalculateGapsAsync(Guid employeeId, Guid? targetRoleId = null)
    {
        var employee = await _context.Employees
            .Include(e => e.JobRole)
                .ThenInclude(r => r!.SkillRequirements)
            .Include(e => e.Skills)
            .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

        if (employee == null)
        {
            return new RecalculateGapsResultDto
            {
                Success = false,
                Message = "Employee not found"
            };
        }

        var roleId = targetRoleId ?? employee.JobRoleId;
        if (!roleId.HasValue)
        {
            return new RecalculateGapsResultDto
            {
                Success = false,
                Message = "No role assigned to employee"
            };
        }

        var targetRole = targetRoleId.HasValue
            ? await _context.JobRoles
                .Include(r => r.SkillRequirements)
                .FirstOrDefaultAsync(r => r.Id == targetRoleId.Value && !r.IsDeleted)
            : employee.JobRole;

        if (targetRole == null)
        {
            return new RecalculateGapsResultDto
            {
                Success = false,
                Message = "Target role not found"
            };
        }

        // Get existing gaps
        var existingGaps = await _context.SkillGaps
            .Where(g => g.EmployeeId == employeeId && g.JobRoleId == roleId && !g.IsDeleted)
            .ToListAsync();

        var employeeSkills = employee.Skills
            .Where(es => !es.IsDeleted)
            .ToDictionary(es => es.SkillId);

        int created = 0, updated = 0, resolved = 0;

        foreach (var req in targetRole.SkillRequirements.Where(r => !r.IsDeleted))
        {
            var currentLevel = ProficiencyLevel.None;
            if (employeeSkills.TryGetValue(req.SkillId, out var empSkill))
            {
                currentLevel = empSkill.CurrentLevel;
            }

            var gapSize = (int)req.MinimumLevel - (int)currentLevel;
            var existingGap = existingGaps.FirstOrDefault(g => g.SkillId == req.SkillId);

            if (gapSize > 0)
            {
                if (existingGap == null)
                {
                    // Create new gap
                    var newGap = new SkillGap
                    {
                        EmployeeId = employeeId,
                        SkillId = req.SkillId,
                        JobRoleId = roleId,
                        CurrentLevel = currentLevel,
                        RequiredLevel = req.MinimumLevel,
                        GapSize = gapSize,
                        Priority = CalculateGapPriority(gapSize, req.IsMandatory),
                        IdentifiedAt = DateTime.UtcNow
                    };
                    _context.SkillGaps.Add(newGap);
                    created++;
                }
                else if (existingGap.ResolvedAt == null)
                {
                    // Update existing gap
                    existingGap.CurrentLevel = currentLevel;
                    existingGap.RequiredLevel = req.MinimumLevel;
                    existingGap.GapSize = gapSize;
                    existingGap.Priority = CalculateGapPriority(gapSize, req.IsMandatory);
                    existingGap.UpdatedAt = DateTime.UtcNow;
                    updated++;
                }
            }
            else if (existingGap != null && existingGap.ResolvedAt == null)
            {
                // Resolve gap (requirement met)
                existingGap.CurrentLevel = currentLevel;
                existingGap.GapSize = 0;
                existingGap.ResolvedAt = DateTime.UtcNow;
                existingGap.UpdatedAt = DateTime.UtcNow;
                resolved++;
            }
        }

        await _context.SaveChangesAsync();

        // Call AI to analyze gaps and generate recommendations
        try
        {
            await CallAiAnalysisForGapsAsync(employeeId, roleId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate AI recommendations for employee {EmployeeId}", employeeId);
            // Continue even if AI fails - gaps are still created
        }

        return new RecalculateGapsResultDto
        {
            Success = true,
            Message = $"Recalculated gaps for {targetRole.Name}",
            GapsCreated = created,
            GapsUpdated = updated,
            GapsResolved = resolved
        };
    }

    public async Task<BulkRecalculateGapsResultDto> BulkRecalculateGapsForAllEmployeesAsync()
    {
        _logger.LogInformation("Starting bulk gap recalculation for all employees");

        // Get all active employees with assigned roles
        var employees = await _context.Employees
            .Include(e => e.JobRole)
            .Where(e => !e.IsDeleted && e.JobRoleId.HasValue)
            .ToListAsync();

        if (!employees.Any())
        {
            return new BulkRecalculateGapsResultDto
            {
                Success = false,
                Message = "No employees with assigned roles found",
                EmployeesProcessed = 0
            };
        }

        var employeeResults = new List<EmployeeBulkGapResult>();
        int totalCreated = 0, totalUpdated = 0, totalResolved = 0;
        int employeesWithGaps = 0;

        foreach (var employee in employees)
        {
            try
            {
                _logger.LogInformation(
                    "Processing employee {EmployeeId} - {EmployeeName} ({RoleName})",
                    employee.Id, employee.FullName, employee.JobRole?.Name);

                var result = await RecalculateGapsAsync(employee.Id, employee.JobRoleId);

                if (result.Success)
                {
                    totalCreated += result.GapsCreated;
                    totalUpdated += result.GapsUpdated;
                    totalResolved += result.GapsResolved;

                    if (result.GapsCreated > 0 || result.GapsUpdated > 0)
                    {
                        employeesWithGaps++;
                    }

                    employeeResults.Add(new EmployeeBulkGapResult
                    {
                        EmployeeId = employee.Id,
                        EmployeeName = employee.FullName,
                        RoleName = employee.JobRole?.Name,
                        GapsCreated = result.GapsCreated,
                        GapsUpdated = result.GapsUpdated,
                        GapsResolved = result.GapsResolved
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to recalculate gaps for employee {EmployeeId} - {EmployeeName}",
                    employee.Id, employee.FullName);
                // Continue with next employee
            }
        }

        _logger.LogInformation(
            "Bulk gap recalculation completed. Processed: {EmployeeCount}, Total Gaps - Created: {Created}, Updated: {Updated}, Resolved: {Resolved}",
            employees.Count, totalCreated, totalUpdated, totalResolved);

        return new BulkRecalculateGapsResultDto
        {
            Success = true,
            Message = $"Successfully recalculated gaps for {employees.Count} employees",
            EmployeesProcessed = employees.Count,
            EmployeesWithGaps = employeesWithGaps,
            TotalGapsCreated = totalCreated,
            TotalGapsUpdated = totalUpdated,
            TotalGapsResolved = totalResolved,
            EmployeeResults = employeeResults
        };
    }

    public async Task<LearningPathDto> CreateLearningPathAsync(Guid employeeId, CreateLearningPathRequest request)
    {
        var employee = await _context.Employees
            .Include(e => e.Skills)
            .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

        if (employee == null)
        {
            throw new InvalidOperationException("Employee not found");
        }

        // Determine target skill
        Guid targetSkillId;
        ProficiencyLevel? currentLevel = null;
        SkillGap? skillGap = null;

        if (request.SkillGapId.HasValue)
        {
            skillGap = await _context.SkillGaps
                .Include(g => g.Skill)
                .FirstOrDefaultAsync(g => g.Id == request.SkillGapId.Value && !g.IsDeleted);

            if (skillGap == null)
                throw new InvalidOperationException("Skill gap not found");

            targetSkillId = skillGap.SkillId;
            currentLevel = skillGap.CurrentLevel;
        }
        else if (request.TargetSkillId.HasValue)
        {
            targetSkillId = request.TargetSkillId.Value;
            var empSkill = employee.Skills.FirstOrDefault(s => s.SkillId == targetSkillId && !s.IsDeleted);
            currentLevel = empSkill?.CurrentLevel;
        }
        else
        {
            throw new InvalidOperationException("Either SkillGapId or TargetSkillId is required");
        }

        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == targetSkillId && !s.IsDeleted);

        if (skill == null)
            throw new InvalidOperationException("Target skill not found");

        var targetLevel = (ProficiencyLevel)request.TargetLevel;

        // Use AI to generate personalized learning path with Coursera courses
        AiLearningPathResponse? aiPath = null;
        bool useAiPath = request.UseAiGeneration ?? true; // Default to using AI

        if (useAiPath)
        {
            try
            {
                _logger.LogInformation(
                    "Generating AI-powered learning path for employee {EmployeeId}, skill {SkillName}",
                    employeeId, skill.Name);

                var aiRequest = new AiLearningPathRequest
                {
                    EmployeeId = employee.Id,
                    EmployeeName = employee.FullName,
                    SkillId = skill.Id,
                    SkillName = skill.Name,
                    SkillCode = skill.Code,
                    SkillDescription = skill.Description,
                    CurrentLevel = (int)(currentLevel ?? ProficiencyLevel.None),
                    TargetLevel = (int)targetLevel,
                    TimeConstraintMonths = request.TimeConstraintMonths ?? 6,
                    Language = "vi"
                };

                aiPath = await _aiLearningPathService.GenerateLearningPathAsync(aiRequest);

                _logger.LogInformation(
                    "AI learning path generated successfully with {ItemCount} items",
                    aiPath.LearningItems.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to generate AI learning path, falling back to manual mode");
                // Fall back to manual mode if AI fails
                aiPath = null;
            }
        }

        // Create learning path entity
        var learningPath = new EmployeeLearningPath
        {
            EmployeeId = employeeId,
            SkillGapId = request.SkillGapId,
            TargetSkillId = targetSkillId,
            Title = aiPath?.PathTitle ?? $"Learning Path: {skill.Name} to Level {(int)targetLevel}",
            Description = aiPath?.PathDescription,
            CurrentLevel = currentLevel,
            TargetLevel = targetLevel,
            Status = LearningPathStatus.Suggested,
            TargetCompletionDate = request.TargetCompletionDate,
            IsAiGenerated = aiPath != null,
            AiRationale = aiPath?.AiRationale,
            ProgressPercentage = 0
        };

        _context.EmployeeLearningPaths.Add(learningPath);
        await _context.SaveChangesAsync();

        // Create learning path items
        var items = new List<LearningPathItem>();
        int totalHours = 0;

        if (aiPath != null && aiPath.LearningItems.Any())
        {
            // Use AI-generated items with Coursera courses
            foreach (var aiItem in aiPath.LearningItems)
            {
                var item = new LearningPathItem
                {
                    LearningPathId = learningPath.Id,
                    LearningResourceId = null, // Coursera courses are external
                    Title = aiItem.Title,
                    Description = aiItem.Description,
                    ItemType = MapItemType(aiItem.ItemType),
                    DisplayOrder = aiItem.Order,
                    EstimatedHours = aiItem.EstimatedHours,
                    TargetLevelAfter = (ProficiencyLevel)aiItem.TargetLevelAfter,
                    SuccessCriteria = aiItem.SuccessCriteria,
                    ExternalUrl = aiItem.CourseUrl,
                    Status = LearningItemStatus.NotStarted
                };
                items.Add(item);
                totalHours += aiItem.EstimatedHours;
            }

            totalHours = aiPath.EstimatedTotalHours;
        }
        else
        {
            // Fallback: Try to find matching learning resources from DB
            var matchedResources = await _context.LearningResourceSkills
                .Include(lrs => lrs.LearningResource)
                .Where(lrs =>
                    lrs.SkillId == targetSkillId &&
                    !lrs.IsDeleted &&
                    lrs.LearningResource.IsActive &&
                    (int)lrs.FromLevel <= (int)(currentLevel ?? ProficiencyLevel.None) &&
                    (int)lrs.ToLevel >= (int)targetLevel)
                .OrderBy(lrs => lrs.LearningResource.Difficulty)
                .ThenBy(lrs => lrs.LearningResource.EstimatedHours)
                .ToListAsync();

            int order = 1;
            if (matchedResources.Any())
            {
                foreach (var lrs in matchedResources)
                {
                    var item = new LearningPathItem
                    {
                        LearningPathId = learningPath.Id,
                        LearningResourceId = lrs.LearningResourceId,
                        Title = lrs.LearningResource.Title,
                        Description = lrs.LearningResource.Description,
                        ItemType = lrs.LearningResource.Type,
                        DisplayOrder = order++,
                        EstimatedHours = lrs.LearningResource.EstimatedHours,
                        Status = LearningItemStatus.NotStarted
                    };
                    items.Add(item);
                    totalHours += lrs.LearningResource.EstimatedHours ?? 0;
                }
            }
            else
            {
                // Generate mock items when no resources found
                items.Add(CreateMockItem(learningPath.Id, order++, $"Learn {skill.Name} Fundamentals", LearningResourceType.Course, 10));
                items.Add(CreateMockItem(learningPath.Id, order++, $"Practice {skill.Name} Exercises", LearningResourceType.Project, 15));
                items.Add(CreateMockItem(learningPath.Id, order++, $"Advanced {skill.Name} Techniques", LearningResourceType.Course, 15));
                totalHours = 40;
            }
        }

        _context.LearningPathItems.AddRange(items);
        learningPath.EstimatedTotalHours = totalHours;

        // Mark gap as addressed if applicable
        if (skillGap != null && !skillGap.IsAddressed)
        {
            skillGap.IsAddressed = true;
            skillGap.AddressedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return new LearningPathDto
        {
            Id = learningPath.Id,
            Status = learningPath.Status.ToString(),
            Title = learningPath.Title,
            Description = learningPath.Description,
            TargetSkill = new SkillBasicDto
            {
                Id = skill.Id,
                Name = skill.Name,
                Code = skill.Code
            },
            CurrentLevel = currentLevel.HasValue ? (int?)currentLevel.Value : null,
            CurrentLevelName = currentLevel?.ToString(),
            TargetLevel = (int)targetLevel,
            TargetLevelName = targetLevel.ToString(),
            EstimatedTotalHours = totalHours,
            EstimatedDurationWeeks = aiPath?.EstimatedDurationWeeks,
            TargetCompletionDate = request.TargetCompletionDate,
            IsAiGenerated = aiPath != null,
            AiRationale = aiPath?.AiRationale,
            KeySuccessFactors = aiPath?.KeySuccessFactors,
            PotentialChallenges = aiPath?.PotentialChallenges,
            Items = items.Select(i => new LearningPathItemDto
            {
                Id = i.Id,
                DisplayOrder = i.DisplayOrder,
                Title = i.Title,
                Description = i.Description,
                ItemType = i.ItemType.ToString(),
                ResourceId = i.LearningResourceId,
                ExternalUrl = i.ExternalUrl,
                EstimatedHours = i.EstimatedHours,
                TargetLevelAfter = i.TargetLevelAfter.HasValue ? (int?)i.TargetLevelAfter.Value : null,
                SuccessCriteria = i.SuccessCriteria,
                Status = i.Status.ToString()
            }).ToList(),
            Milestones = aiPath?.Milestones.Select(m => new LearningPathMilestoneDto
            {
                AfterItem = m.AfterItem,
                Description = m.Description,
                ExpectedLevel = m.ExpectedLevel
            }).ToList(),
            Message = aiPath != null
                ? "AI-powered learning path created with Coursera courses"
                : items.Any(i => i.LearningResourceId.HasValue)
                    ? "Learning path created with matched resources"
                    : "Learning path created with suggested activities"
        };
    }

    public async Task<List<LearningPathDto>> GetLearningPathsAsync(Guid employeeId)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

        if (employee == null)
        {
            return new List<LearningPathDto>();
        }

        var learningPaths = await _context.EmployeeLearningPaths
            .Include(lp => lp.TargetSkill)
            .Include(lp => lp.Items.OrderBy(i => i.DisplayOrder))
            .Where(lp => lp.EmployeeId == employeeId && !lp.IsDeleted)
            .OrderByDescending(lp => lp.CreatedAt)
            .ToListAsync();

        var result = new List<LearningPathDto>();

        foreach (var lp in learningPaths)
        {
            // Get current level for this skill
            var empSkill = await _context.EmployeeSkills
                .FirstOrDefaultAsync(es => es.EmployeeId == employeeId && es.SkillId == lp.TargetSkillId && !es.IsDeleted);

            var totalHours = lp.Items.Sum(i => i.EstimatedHours ?? 0);

            result.Add(new LearningPathDto
            {
                Id = lp.Id,
                Status = lp.Status.ToString(),
                Title = lp.Title,
                Description = lp.Description,
                TargetSkill = new SkillBasicDto
                {
                    Id = lp.TargetSkill.Id,
                    Name = lp.TargetSkill.Name,
                    Code = lp.TargetSkill.Code
                },
                CurrentLevel = empSkill?.CurrentLevel != null ? (int?)empSkill.CurrentLevel : null,
                CurrentLevelName = empSkill?.CurrentLevel.ToString(),
                TargetLevel = (int)lp.TargetLevel,
                TargetLevelName = lp.TargetLevel.ToString(),
                EstimatedTotalHours = totalHours,
                EstimatedDurationWeeks = null, // Not stored in database
                TargetCompletionDate = lp.TargetCompletionDate,
                IsAiGenerated = lp.IsAiGenerated,
                AiRationale = lp.AiRationale,
                KeySuccessFactors = null, // Not stored in database
                PotentialChallenges = null, // Not stored in database
                Items = lp.Items.Select(i => new LearningPathItemDto
                {
                    Id = i.Id,
                    DisplayOrder = i.DisplayOrder,
                    Title = i.Title,
                    Description = i.Description,
                    ItemType = i.ItemType.ToString(),
                    ResourceId = i.LearningResourceId,
                    ExternalUrl = i.ExternalUrl,
                    EstimatedHours = i.EstimatedHours,
                    TargetLevelAfter = i.TargetLevelAfter.HasValue ? (int?)i.TargetLevelAfter.Value : null,
                    SuccessCriteria = i.SuccessCriteria,
                    Status = i.Status.ToString()
                }).ToList(),
                Milestones = null // Not stored in database
            });
        }

        return result;
    }

    #region Helper Methods

    private EmployeeBasicDto MapToEmployeeBasicDto(Domain.Entities.Organization.Employee employee)
    {
        return new EmployeeBasicDto
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            JobRole = employee.JobRole != null ? new RoleBasicDto
            {
                Id = employee.JobRole.Id,
                Name = employee.JobRole.Name,
                Code = employee.JobRole.Code,
                LevelInHierarchy = employee.JobRole.LevelInHierarchy
            } : null,
            Team = employee.Team != null ? new TeamBasicDto
            {
                Id = employee.Team.Id,
                Name = employee.Team.Name
            } : null,
            YearsOfExperience = employee.YearsOfExperience
        };
    }

    private static GapPriority CalculateGapPriority(int gapSize, bool isMandatory)
    {
        return (gapSize, isMandatory) switch
        {
            ( >= 3, true) => GapPriority.Critical,
            ( >= 2, true) => GapPriority.High,
            ( >= 2, false) => GapPriority.Medium,
            _ => GapPriority.Low
        };
    }

    private static LearningPathItem CreateMockItem(Guid pathId, int order, string title, LearningResourceType type, int hours)
    {
        return new LearningPathItem
        {
            LearningPathId = pathId,
            LearningResourceId = null,
            Title = title,
            Description = $"Suggested learning activity: {title}",
            ItemType = type,
            DisplayOrder = order,
            EstimatedHours = hours,
            Status = LearningItemStatus.NotStarted
        };
    }

    private static LearningResourceType MapItemType(string itemType)
    {
        return itemType.ToLower() switch
        {
            "course" => LearningResourceType.Course,
            "book" => LearningResourceType.Book,
            "video" => LearningResourceType.Video,
            "project" => LearningResourceType.Project,
            "workshop" => LearningResourceType.Workshop,
            "certification" => LearningResourceType.Certification,
            "mentorship" => LearningResourceType.Mentorship,
            _ => LearningResourceType.Course
        };
    }

    /// <summary>
    /// Call AI service to analyze gaps and save recommendations to database
    /// </summary>
    private async Task CallAiAnalysisForGapsAsync(Guid employeeId, Guid roleId)
    {
        // Get employee and role info
        var employee = await _context.Employees
            .Include(e => e.JobRole)
            .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

        if (employee == null)
            return;

        // Get employee's current skills with gaps
        var gaps = await _context.SkillGaps
            .Include(g => g.Skill)
            .Where(g => g.EmployeeId == employeeId && g.JobRoleId == roleId && !g.IsDeleted && g.ResolvedAt == null)
            .ToListAsync();

        if (!gaps.Any())
            return; // No gaps to analyze

        // Prepare AI request
        var request = new DTOs.Assessment.AiAnalyzeSkillGapRequest
        {
            EmployeeId = employeeId,
            EmployeeName = employee.FullName,
            JobRoleId = roleId,
            JobRoleName = employee.JobRole?.Name ?? "Unknown Role",
            CurrentSkills = gaps.Select(g => new DTOs.Assessment.EmployeeSkillSnapshot
            {
                SkillId = g.SkillId,
                SkillName = g.Skill.Name,
                SkillCode = g.Skill.Code,
                CurrentLevel = g.CurrentLevel,
                RequiredLevel = g.RequiredLevel
            }).ToList()
        };

        // Call AI analyzer
        var aiResult = await _aiSkillAnalyzer.AnalyzeSkillGapsAsync(request);

        if (!aiResult.Success || !aiResult.Gaps.Any())
            return;

        // Update skill gaps with AI analysis
        foreach (var aiGap in aiResult.Gaps)
        {
            var gap = gaps.FirstOrDefault(g => g.SkillId == aiGap.SkillId);
            if (gap != null)
            {
                gap.AiAnalysis = aiGap.Analysis;
                gap.AiRecommendation = aiGap.Recommendation;
                gap.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Delete old recommendations for these gaps
        var existingRecommendations = await _context.LearningRecommendations
            .Where(lr => gaps.Select(g => g.Id).Contains(lr.SkillGapId) && !lr.IsDeleted)
            .ToListAsync();

        foreach (var old in existingRecommendations)
        {
            old.IsDeleted = true;
            old.DeletedAt = DateTime.UtcNow;
        }

        // Generate Coursera learning paths for each gap
        int totalRecommendations = 0;
        foreach (var gap in gaps)
        {
            try
            {
                _logger.LogInformation("Generating learning path for skill {SkillName}", gap.Skill.Name);

                var learningPathRequest = new AiLearningPathRequest
                {
                    EmployeeId = employeeId,
                    EmployeeName = employee.FullName,
                    SkillId = gap.SkillId,
                    SkillName = gap.Skill.Name,
                    SkillCode = gap.Skill.Code,
                    SkillDescription = gap.Skill.Description,
                    CurrentLevel = (int)gap.CurrentLevel,
                    TargetLevel = (int)gap.RequiredLevel,
                    TimeConstraintMonths = 6,
                    Language = "vn"
                };

                var learningPath = await _aiLearningPathService.GenerateLearningPathAsync(learningPathRequest);

                if (learningPath.LearningItems != null && learningPath.LearningItems.Any())
                {
                    int displayOrder = 0;
                    foreach (var item in learningPath.LearningItems.Take(3)) // Top 3 courses per skill
                    {
                        var recommendation = new LearningRecommendation
                        {
                            SkillGapId = gap.Id,
                            SkillId = gap.SkillId,
                            SkillName = gap.Skill.Name,
                            RecommendationType = item.ItemType ?? "Course",
                            Title = item.Title,
                            Description = item.Description,
                            Url = item.CourseUrl,
                            EstimatedHours = item.EstimatedHours,
                            Rationale = item.SuccessCriteria ?? learningPath.AiRationale ?? "AI-generated recommendation",
                            DisplayOrder = displayOrder++,
                            AiProvider = "Learning Path AI",
                            GeneratedAt = DateTime.UtcNow
                        };
                        _context.LearningRecommendations.Add(recommendation);
                        totalRecommendations++;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate learning path for skill {SkillName}", gap.Skill.Name);
                // Continue with next gap
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Generated {Count} Coursera recommendations for employee {EmployeeId}",
            totalRecommendations, employeeId);
    }

    #endregion
}
