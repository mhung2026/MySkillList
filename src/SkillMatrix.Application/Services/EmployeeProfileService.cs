using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Employee;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Entities.Learning;
using SkillMatrix.Domain.Enums;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Application.Services;

public class EmployeeProfileService : IEmployeeProfileService
{
    private readonly SkillMatrixDbContext _context;

    public EmployeeProfileService(SkillMatrixDbContext context)
    {
        _context = context;
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

        // Analyze gaps
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

            if (gapSize <= 0)
            {
                metCount++;
                continue;
            }

            // Get existing gap data for AI analysis
            existingGaps.TryGetValue(req.SkillId, out var existingGap);

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
                Priority = CalculateGapPriority(gapSize, req.IsMandatory).ToString(),
                IsMandatory = req.IsMandatory,
                AiAnalysis = existingGap?.AiAnalysis,
                AiRecommendation = existingGap?.AiRecommendation
            });
        }

        // Sort by priority and gap size
        gaps = gaps
            .OrderByDescending(g => g.Priority == "Critical")
            .ThenByDescending(g => g.Priority == "High")
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

        return new RecalculateGapsResultDto
        {
            Success = true,
            Message = $"Recalculated gaps for {targetRole.Name}",
            GapsCreated = created,
            GapsUpdated = updated,
            GapsResolved = resolved
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

        // Try to find matching learning resources
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

        // Create learning path
        var learningPath = new EmployeeLearningPath
        {
            EmployeeId = employeeId,
            SkillGapId = request.SkillGapId,
            TargetSkillId = targetSkillId,
            Title = $"Learning Path: {skill.Name} to Level {(int)targetLevel}",
            CurrentLevel = currentLevel,
            TargetLevel = targetLevel,
            Status = LearningPathStatus.Suggested,
            TargetCompletionDate = request.TargetCompletionDate,
            IsAiGenerated = false,
            ProgressPercentage = 0
        };

        _context.EmployeeLearningPaths.Add(learningPath);
        await _context.SaveChangesAsync();

        // Create learning path items
        var items = new List<LearningPathItem>();
        int order = 1;
        int totalHours = 0;

        if (matchedResources.Any())
        {
            // Use matched resources
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
            TargetCompletionDate = request.TargetCompletionDate,
            Items = items.Select(i => new LearningPathItemDto
            {
                Id = i.Id,
                DisplayOrder = i.DisplayOrder,
                Title = i.Title,
                Description = i.Description,
                ItemType = i.ItemType.ToString(),
                ResourceId = i.LearningResourceId,
                EstimatedHours = i.EstimatedHours,
                Status = i.Status.ToString()
            }).ToList(),
            Message = matchedResources.Any()
                ? "Learning path created with matched resources"
                : "Learning path created with suggested activities (no specific resources found)"
        };
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

    #endregion
}
