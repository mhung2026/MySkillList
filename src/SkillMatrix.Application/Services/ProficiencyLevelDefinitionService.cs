using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Taxonomy;
using SkillMatrix.Domain.Entities.Taxonomy;
using SkillMatrix.Infrastructure.Persistence;
using System.Text.Json;

namespace SkillMatrix.Application.Services;

public interface IProficiencyLevelDefinitionService
{
    Task<List<ProficiencyLevelDefinitionDto>> GetAllAsync();
    Task<ProficiencyLevelDefinitionDto?> GetByIdAsync(Guid id);
    Task<ProficiencyLevelDefinitionDto?> GetByLevelAsync(int level);
    Task<ProficiencyLevelDefinitionDto> CreateAsync(CreateProficiencyLevelDefinitionDto dto);
    Task<ProficiencyLevelDefinitionDto> UpdateAsync(Guid id, UpdateProficiencyLevelDefinitionDto dto);
    Task DeleteAsync(Guid id);
    Task SeedDefaultLevelsAsync();
}

public class ProficiencyLevelDefinitionService : IProficiencyLevelDefinitionService
{
    private readonly SkillMatrixDbContext _context;

    public ProficiencyLevelDefinitionService(SkillMatrixDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProficiencyLevelDefinitionDto>> GetAllAsync()
    {
        var levels = await _context.ProficiencyLevelDefinitions
            .OrderBy(l => l.Level)
            .ToListAsync();

        return levels.Select(MapToDto).ToList();
    }

    public async Task<ProficiencyLevelDefinitionDto?> GetByIdAsync(Guid id)
    {
        var level = await _context.ProficiencyLevelDefinitions.FindAsync(id);
        return level == null ? null : MapToDto(level);
    }

    public async Task<ProficiencyLevelDefinitionDto?> GetByLevelAsync(int level)
    {
        var levelDef = await _context.ProficiencyLevelDefinitions
            .FirstOrDefaultAsync(l => l.Level == level);
        return levelDef == null ? null : MapToDto(levelDef);
    }

    public async Task<ProficiencyLevelDefinitionDto> CreateAsync(CreateProficiencyLevelDefinitionDto dto)
    {
        // Check if level already exists
        var existing = await _context.ProficiencyLevelDefinitions
            .FirstOrDefaultAsync(l => l.Level == dto.Level);
        if (existing != null)
        {
            throw new InvalidOperationException($"Level {dto.Level} already exists");
        }

        var entity = new ProficiencyLevelDefinition
        {
            Id = Guid.NewGuid(),
            Level = dto.Level,
            LevelName = dto.LevelName,
            Description = dto.Description,
            Autonomy = dto.Autonomy,
            Influence = dto.Influence,
            Complexity = dto.Complexity,
            Knowledge = dto.Knowledge,
            BusinessSkills = dto.BusinessSkills,
            BehavioralIndicators = dto.BehavioralIndicators != null
                ? JsonSerializer.Serialize(dto.BehavioralIndicators)
                : null,
            Color = dto.Color,
            DisplayOrder = dto.DisplayOrder > 0 ? dto.DisplayOrder : dto.Level,
            IsActive = true
        };

        _context.ProficiencyLevelDefinitions.Add(entity);
        await _context.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<ProficiencyLevelDefinitionDto> UpdateAsync(Guid id, UpdateProficiencyLevelDefinitionDto dto)
    {
        var entity = await _context.ProficiencyLevelDefinitions.FindAsync(id)
            ?? throw new KeyNotFoundException($"ProficiencyLevelDefinition with id {id} not found");

        entity.LevelName = dto.LevelName;
        entity.Description = dto.Description;
        entity.Autonomy = dto.Autonomy;
        entity.Influence = dto.Influence;
        entity.Complexity = dto.Complexity;
        entity.Knowledge = dto.Knowledge;
        entity.BusinessSkills = dto.BusinessSkills;
        entity.BehavioralIndicators = dto.BehavioralIndicators != null
            ? JsonSerializer.Serialize(dto.BehavioralIndicators)
            : null;
        entity.Color = dto.Color;
        entity.DisplayOrder = dto.DisplayOrder;
        entity.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.ProficiencyLevelDefinitions.FindAsync(id)
            ?? throw new KeyNotFoundException($"ProficiencyLevelDefinition with id {id} not found");

        entity.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task SeedDefaultLevelsAsync()
    {
        // Check if already seeded
        if (await _context.ProficiencyLevelDefinitions.AnyAsync())
            return;

        var defaultLevels = GetDefaultLevels();
        _context.ProficiencyLevelDefinitions.AddRange(defaultLevels);
        await _context.SaveChangesAsync();
    }

    private ProficiencyLevelDefinitionDto MapToDto(ProficiencyLevelDefinition entity)
    {
        return new ProficiencyLevelDefinitionDto
        {
            Id = entity.Id,
            Level = entity.Level,
            LevelName = entity.LevelName,
            Description = entity.Description,
            Autonomy = entity.Autonomy,
            Influence = entity.Influence,
            Complexity = entity.Complexity,
            Knowledge = entity.Knowledge,
            BusinessSkills = entity.BusinessSkills,
            BehavioralIndicators = !string.IsNullOrEmpty(entity.BehavioralIndicators)
                ? JsonSerializer.Deserialize<List<string>>(entity.BehavioralIndicators)
                : null,
            Color = entity.Color,
            DisplayOrder = entity.DisplayOrder,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static List<ProficiencyLevelDefinition> GetDefaultLevels()
    {
        var colors = new[] { "#87d068", "#52c41a", "#1890ff", "#722ed1", "#eb2f96", "#fa541c", "#f5222d" };

        return new List<ProficiencyLevelDefinition>
        {
            new ProficiencyLevelDefinition
            {
                Id = Guid.NewGuid(),
                Level = 1,
                LevelName = "Follow",
                Description = "Works under close direction. Uses limited discretion in resolving issues or enquiries.",
                Autonomy = "Works under close direction. Uses limited discretion.",
                Influence = "Minimal influence. May work alone or interact with immediate team.",
                Complexity = "Performs routine activities in a structured environment.",
                Knowledge = "Has a basic generic knowledge.",
                BusinessSkills = "Has sufficient communication skills.",
                Color = colors[0],
                DisplayOrder = 1,
                IsActive = true
            },
            new ProficiencyLevelDefinition
            {
                Id = Guid.NewGuid(),
                Level = 2,
                LevelName = "Assist",
                Description = "Works under routine direction. Uses limited discretion in resolving issues or enquiries.",
                Autonomy = "Works under routine direction within a clear framework.",
                Influence = "Interacts with and may influence immediate colleagues.",
                Complexity = "Performs a range of work activities in varied environments.",
                Knowledge = "Demonstrates application of essential generic knowledge.",
                BusinessSkills = "Plans and monitors own work.",
                Color = colors[1],
                DisplayOrder = 2,
                IsActive = true
            },
            new ProficiencyLevelDefinition
            {
                Id = Guid.NewGuid(),
                Level = 3,
                LevelName = "Apply",
                Description = "Works under general direction. Uses discretion in identifying and responding to complex issues.",
                Autonomy = "Works under general direction. Determines own approach.",
                Influence = "Interacts with and influences team members and stakeholders.",
                Complexity = "Performs a range of work, sometimes complex and non-routine.",
                Knowledge = "Demonstrates effective application of knowledge.",
                BusinessSkills = "Communicates effectively. Takes initiative.",
                Color = colors[2],
                DisplayOrder = 3,
                IsActive = true
            },
            new ProficiencyLevelDefinition
            {
                Id = Guid.NewGuid(),
                Level = 4,
                LevelName = "Enable",
                Description = "Works under broad direction. Work is often self-initiated. Fully responsible for technical objectives.",
                Autonomy = "Works under broad direction. Full accountability for own technical work.",
                Influence = "Influences team and specialist peers.",
                Complexity = "Work includes complex technical activities.",
                Knowledge = "Has a thorough understanding of industry knowledge.",
                BusinessSkills = "Communicates fluently. Facilitates collaboration.",
                Color = colors[3],
                DisplayOrder = 4,
                IsActive = true
            },
            new ProficiencyLevelDefinition
            {
                Id = Guid.NewGuid(),
                Level = 5,
                LevelName = "Ensure, advise",
                Description = "Works under broad direction. Accountable for significant results for a defined area.",
                Autonomy = "Works under broad direction. Accountable for significant results.",
                Influence = "Influences policy and strategy. Has significant influence.",
                Complexity = "Performs highly complex work activities.",
                Knowledge = "Has a deep understanding of recognised industry knowledge.",
                BusinessSkills = "Demonstrates leadership. Advises on standards.",
                Color = colors[4],
                DisplayOrder = 5,
                IsActive = true
            },
            new ProficiencyLevelDefinition
            {
                Id = Guid.NewGuid(),
                Level = 6,
                LevelName = "Initiate, influence",
                Description = "Has defined authority and accountability for actions and decisions within a significant area of work.",
                Autonomy = "Has defined authority for a significant area of work.",
                Influence = "Influences industry direction. Shapes policy.",
                Complexity = "Leads on initiating and implementing strategy.",
                Knowledge = "Recognised as an expert.",
                BusinessSkills = "Leads, manages and develops people.",
                Color = colors[5],
                DisplayOrder = 6,
                IsActive = true
            },
            new ProficiencyLevelDefinition
            {
                Id = Guid.NewGuid(),
                Level = 7,
                LevelName = "Set strategy, inspire, mobilise",
                Description = "Has authority and accountability for all aspects of a significant area of work including strategy.",
                Autonomy = "The highest level of authority and responsibility.",
                Influence = "Sets and advances the strategy of the organisation/industry.",
                Complexity = "Leads on the formulation and implementation of strategy.",
                Knowledge = "Leads the advancement of knowledge.",
                BusinessSkills = "Inspires others. Drives culture and values.",
                Color = colors[6],
                DisplayOrder = 7,
                IsActive = true
            }
        };
    }
}
