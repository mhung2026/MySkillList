using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Application.DTOs.Taxonomy;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Entities.Taxonomy;
using SkillMatrix.Domain.Enums;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Application.Services;

public class SkillService : ISkillService
{
    private readonly SkillMatrixDbContext _context;

    public SkillService(SkillMatrixDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SkillListDto>> GetAllAsync(SkillFilterRequest request)
    {
        var query = _context.Skills
            .Include(s => s.Subcategory)
            .ThenInclude(sc => sc.SkillDomain)
            .Where(s => s.IsCurrent)
            .AsQueryable();

        // Filter by domain
        if (request.DomainId.HasValue)
        {
            query = query.Where(s => s.Subcategory.SkillDomainId == request.DomainId.Value);
        }

        // Filter by subcategory
        if (request.SubcategoryId.HasValue)
        {
            query = query.Where(s => s.SubcategoryId == request.SubcategoryId.Value);
        }

        // Filter by category
        if (request.Category.HasValue)
        {
            query = query.Where(s => s.Category == (SkillCategory)request.Category.Value);
        }

        // Filter by skill type
        if (request.SkillType.HasValue)
        {
            query = query.Where(s => s.SkillType == (Domain.Enums.SkillType)request.SkillType.Value);
        }

        // Filter by company specific
        if (request.IsCompanySpecific.HasValue)
        {
            query = query.Where(s => s.IsCompanySpecific == request.IsCompanySpecific.Value);
        }

        // Filter by active status
        if (!request.IncludeInactive)
        {
            query = query.Where(s => s.IsActive);
        }
        else if (request.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == request.IsActive.Value);
        }

        // Search
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(s =>
                s.Code.ToLower().Contains(term) ||
                s.Name.ToLower().Contains(term) ||
                (s.Description != null && s.Description.ToLower().Contains(term)) ||
                s.Subcategory.Name.ToLower().Contains(term) ||
                s.Subcategory.SkillDomain.Name.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();

        // Sort
        query = request.SortBy?.ToLower() switch
        {
            "code" => request.SortDescending ? query.OrderByDescending(s => s.Code) : query.OrderBy(s => s.Code),
            "name" => request.SortDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
            "category" => request.SortDescending ? query.OrderByDescending(s => s.Category) : query.OrderBy(s => s.Category),
            "type" => request.SortDescending ? query.OrderByDescending(s => s.SkillType) : query.OrderBy(s => s.SkillType),
            "domain" => request.SortDescending
                ? query.OrderByDescending(s => s.Subcategory.SkillDomain.Name)
                : query.OrderBy(s => s.Subcategory.SkillDomain.Name),
            _ => query.OrderBy(s => s.Subcategory.SkillDomain.DisplayOrder)
                      .ThenBy(s => s.Subcategory.DisplayOrder)
                      .ThenBy(s => s.DisplayOrder)
        };

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SkillListDto
            {
                Id = s.Id,
                SubcategoryId = s.SubcategoryId,
                SubcategoryCode = s.Subcategory.Code,
                SubcategoryName = s.Subcategory.Name,
                DomainId = s.Subcategory.SkillDomainId,
                DomainCode = s.Subcategory.SkillDomain.Code,
                DomainName = s.Subcategory.SkillDomain.Name,
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                Category = s.Category,
                CategoryName = s.Category.ToString(),
                SkillType = s.SkillType,
                SkillTypeName = s.SkillType.ToString(),
                DisplayOrder = s.DisplayOrder,
                IsActive = s.IsActive,
                IsCompanySpecific = s.IsCompanySpecific,
                ApplicableLevelsString = s.ApplicableLevels,
                EmployeeCount = s.EmployeeSkills.Count(es => !es.IsDeleted)
            })
            .ToListAsync();

        return new PagedResult<SkillListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<SkillDto?> GetByIdAsync(Guid id)
    {
        var skill = await _context.Skills
            .Include(s => s.Subcategory)
            .ThenInclude(sc => sc.SkillDomain)
            .Include(s => s.LevelDefinitions)
            .Where(s => s.Id == id && s.IsCurrent)
            .FirstOrDefaultAsync();

        if (skill == null) return null;

        return new SkillDto
        {
            Id = skill.Id,
            SubcategoryId = skill.SubcategoryId,
            SubcategoryName = skill.Subcategory.Name,
            DomainName = skill.Subcategory.SkillDomain.Name,
            Code = skill.Code,
            Name = skill.Name,
            Description = skill.Description,
            Category = skill.Category,
            CategoryName = skill.Category.ToString(),
            SkillType = skill.SkillType,
            SkillTypeName = skill.SkillType.ToString(),
            DisplayOrder = skill.DisplayOrder,
            IsActive = skill.IsActive,
            IsCompanySpecific = skill.IsCompanySpecific,
            Tags = string.IsNullOrEmpty(skill.Tags)
                ? null
                : JsonSerializer.Deserialize<List<string>>(skill.Tags),
            ApplicableLevelsString = skill.ApplicableLevels,
            ApplicableLevels = ParseApplicableLevels(skill.ApplicableLevels),
            Version = skill.Version,
            CreatedAt = skill.CreatedAt,
            UpdatedAt = skill.UpdatedAt,
            LevelDefinitions = skill.LevelDefinitions
                .Where(ld => !ld.IsDeleted)
                .OrderBy(ld => ld.Level)
                .Select(ld => new SkillLevelDefinitionDto
                {
                    Id = ld.Id,
                    Level = ld.Level,
                    LevelName = ld.Level.ToString(),
                    CustomLevelName = ld.CustomLevelName,
                    Description = ld.Description,
                    Autonomy = ld.Autonomy,
                    Influence = ld.Influence,
                    Complexity = ld.Complexity,
                    BusinessSkills = ld.BusinessSkills,
                    Knowledge = ld.Knowledge,
                    BehavioralIndicators = string.IsNullOrEmpty(ld.BehavioralIndicators)
                        ? null
                        : JsonSerializer.Deserialize<List<string>>(ld.BehavioralIndicators),
                    EvidenceExamples = string.IsNullOrEmpty(ld.EvidenceExamples)
                        ? null
                        : JsonSerializer.Deserialize<List<string>>(ld.EvidenceExamples)
                })
                .ToList()
        };
    }

    public async Task<SkillDto> CreateAsync(CreateSkillDto dto)
    {
        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            SubcategoryId = dto.SubcategoryId,
            Code = dto.Code.ToUpper(),
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            SkillType = dto.SkillType,
            DisplayOrder = dto.DisplayOrder,
            IsCompanySpecific = dto.IsCompanySpecific,
            Tags = dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : null,
            IsActive = true,
            IsCurrent = true,
            Version = 1,
            EffectiveFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(skill.Id))!;
    }

    public async Task<SkillDto> UpdateAsync(Guid id, UpdateSkillDto dto)
    {
        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == id && s.IsCurrent);

        if (skill == null)
            throw new KeyNotFoundException($"Skill with id {id} not found");

        skill.SubcategoryId = dto.SubcategoryId;
        skill.Code = dto.Code.ToUpper();
        skill.Name = dto.Name;
        skill.Description = dto.Description;
        skill.Category = dto.Category;
        skill.SkillType = dto.SkillType;
        skill.DisplayOrder = dto.DisplayOrder;
        skill.IsActive = dto.IsActive;
        skill.IsCompanySpecific = dto.IsCompanySpecific;
        skill.Tags = dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : null;
        skill.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == id && s.IsCurrent);

        if (skill == null)
            return false;

        skill.IsDeleted = true;
        skill.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleActiveAsync(Guid id)
    {
        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == id && s.IsCurrent);

        if (skill == null)
            return false;

        skill.IsActive = !skill.IsActive;
        skill.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SkillDropdownDto>> GetDropdownAsync(Guid? subcategoryId = null)
    {
        var query = _context.Skills
            .Include(s => s.Subcategory)
            .ThenInclude(sc => sc.SkillDomain)
            .Where(s => s.IsCurrent && s.IsActive &&
                        s.Subcategory.IsActive && s.Subcategory.SkillDomain.IsActive);

        if (subcategoryId.HasValue)
        {
            query = query.Where(s => s.SubcategoryId == subcategoryId.Value);
        }

        return await query
            .OrderBy(s => s.Subcategory.SkillDomain.DisplayOrder)
            .ThenBy(s => s.Subcategory.DisplayOrder)
            .ThenBy(s => s.DisplayOrder)
            .Select(s => new SkillDropdownDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                FullPath = s.Subcategory.SkillDomain.Name + " > " + s.Subcategory.Name + " > " + s.Name,
                Category = s.Category,
                SkillType = s.SkillType
            })
            .ToListAsync();
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
    {
        var query = _context.Skills
            .Where(s => s.Code.ToUpper() == code.ToUpper() && s.IsCurrent);

        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<SkillLevelDefinitionDto> CreateLevelDefinitionAsync(CreateSkillLevelDefinitionDto dto)
    {
        var definition = new SkillLevelDefinition
        {
            Id = Guid.NewGuid(),
            SkillId = dto.SkillId,
            Level = dto.Level,
            CustomLevelName = dto.CustomLevelName,
            Description = dto.Description,
            Autonomy = dto.Autonomy,
            Influence = dto.Influence,
            Complexity = dto.Complexity,
            BusinessSkills = dto.BusinessSkills,
            Knowledge = dto.Knowledge,
            BehavioralIndicators = dto.BehavioralIndicators != null
                ? JsonSerializer.Serialize(dto.BehavioralIndicators)
                : null,
            EvidenceExamples = dto.EvidenceExamples != null
                ? JsonSerializer.Serialize(dto.EvidenceExamples)
                : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.SkillLevelDefinitions.Add(definition);
        await _context.SaveChangesAsync();

        return MapToLevelDefinitionDto(definition);
    }

    public async Task<SkillLevelDefinitionDto> UpdateLevelDefinitionAsync(Guid id, CreateSkillLevelDefinitionDto dto)
    {
        var definition = await _context.SkillLevelDefinitions
            .FirstOrDefaultAsync(ld => ld.Id == id && !ld.IsDeleted);

        if (definition == null)
            throw new KeyNotFoundException($"SkillLevelDefinition with id {id} not found");

        definition.Level = dto.Level;
        definition.CustomLevelName = dto.CustomLevelName;
        definition.Description = dto.Description;
        definition.Autonomy = dto.Autonomy;
        definition.Influence = dto.Influence;
        definition.Complexity = dto.Complexity;
        definition.BusinessSkills = dto.BusinessSkills;
        definition.Knowledge = dto.Knowledge;
        definition.BehavioralIndicators = dto.BehavioralIndicators != null
            ? JsonSerializer.Serialize(dto.BehavioralIndicators)
            : null;
        definition.EvidenceExamples = dto.EvidenceExamples != null
            ? JsonSerializer.Serialize(dto.EvidenceExamples)
            : null;
        definition.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToLevelDefinitionDto(definition);
    }

    public async Task<bool> DeleteLevelDefinitionAsync(Guid id)
    {
        var definition = await _context.SkillLevelDefinitions
            .FirstOrDefaultAsync(ld => ld.Id == id && !ld.IsDeleted);

        if (definition == null)
            return false;

        definition.IsDeleted = true;
        definition.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private static SkillLevelDefinitionDto MapToLevelDefinitionDto(SkillLevelDefinition ld)
    {
        return new SkillLevelDefinitionDto
        {
            Id = ld.Id,
            Level = ld.Level,
            LevelName = ld.Level.ToString(),
            CustomLevelName = ld.CustomLevelName,
            Description = ld.Description,
            Autonomy = ld.Autonomy,
            Influence = ld.Influence,
            Complexity = ld.Complexity,
            BusinessSkills = ld.BusinessSkills,
            Knowledge = ld.Knowledge,
            BehavioralIndicators = string.IsNullOrEmpty(ld.BehavioralIndicators)
                ? null
                : JsonSerializer.Deserialize<List<string>>(ld.BehavioralIndicators),
            EvidenceExamples = string.IsNullOrEmpty(ld.EvidenceExamples)
                ? null
                : JsonSerializer.Deserialize<List<string>>(ld.EvidenceExamples)
        };
    }

    private static List<int>? ParseApplicableLevels(string? applicableLevels)
    {
        if (string.IsNullOrWhiteSpace(applicableLevels))
            return null;

        return applicableLevels
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => int.TryParse(s, out _))
            .Select(int.Parse)
            .Where(l => l >= 1)
            .OrderBy(l => l)
            .ToList();
    }
}
