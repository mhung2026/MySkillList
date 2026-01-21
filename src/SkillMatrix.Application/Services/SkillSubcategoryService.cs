using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Application.DTOs.Taxonomy;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Entities.Taxonomy;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Application.Services;

public class SkillSubcategoryService : ISkillSubcategoryService
{
    private readonly SkillMatrixDbContext _context;

    public SkillSubcategoryService(SkillMatrixDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SkillSubcategoryListDto>> GetAllAsync(PagedRequest request, Guid? domainId = null)
    {
        var query = _context.SkillSubcategories
            .Include(s => s.SkillDomain)
            .Where(s => s.IsCurrent)
            .AsQueryable();

        // Filter by domain
        if (domainId.HasValue)
        {
            query = query.Where(s => s.SkillDomainId == domainId.Value);
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
                s.SkillDomain.Name.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();

        // Sort
        query = request.SortBy?.ToLower() switch
        {
            "code" => request.SortDescending ? query.OrderByDescending(s => s.Code) : query.OrderBy(s => s.Code),
            "name" => request.SortDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
            "domain" => request.SortDescending
                ? query.OrderByDescending(s => s.SkillDomain.Name)
                : query.OrderBy(s => s.SkillDomain.Name),
            _ => query.OrderBy(s => s.SkillDomain.DisplayOrder).ThenBy(s => s.DisplayOrder)
        };

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SkillSubcategoryListDto
            {
                Id = s.Id,
                SkillDomainId = s.SkillDomainId,
                SkillDomainCode = s.SkillDomain.Code,
                SkillDomainName = s.SkillDomain.Name,
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                DisplayOrder = s.DisplayOrder,
                IsActive = s.IsActive,
                SkillCount = s.Skills.Count(sk => !sk.IsDeleted && sk.IsCurrent)
            })
            .ToListAsync();

        return new PagedResult<SkillSubcategoryListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<SkillSubcategoryDto?> GetByIdAsync(Guid id)
    {
        return await _context.SkillSubcategories
            .Include(s => s.SkillDomain)
            .Where(s => s.Id == id && s.IsCurrent)
            .Select(s => new SkillSubcategoryDto
            {
                Id = s.Id,
                SkillDomainId = s.SkillDomainId,
                SkillDomainName = s.SkillDomain.Name,
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                DisplayOrder = s.DisplayOrder,
                IsActive = s.IsActive,
                Version = s.Version,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                SkillCount = s.Skills.Count(sk => !sk.IsDeleted && sk.IsCurrent)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SkillSubcategoryDto> CreateAsync(CreateSkillSubcategoryDto dto)
    {
        var subcategory = new SkillSubcategory
        {
            Id = Guid.NewGuid(),
            SkillDomainId = dto.SkillDomainId,
            Code = dto.Code.ToUpper(),
            Name = dto.Name,
            Description = dto.Description,
            DisplayOrder = dto.DisplayOrder,
            IsActive = true,
            IsCurrent = true,
            Version = 1,
            EffectiveFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.SkillSubcategories.Add(subcategory);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(subcategory.Id))!;
    }

    public async Task<SkillSubcategoryDto> UpdateAsync(Guid id, UpdateSkillSubcategoryDto dto)
    {
        var subcategory = await _context.SkillSubcategories
            .FirstOrDefaultAsync(s => s.Id == id && s.IsCurrent);

        if (subcategory == null)
            throw new KeyNotFoundException($"SkillSubcategory with id {id} not found");

        subcategory.SkillDomainId = dto.SkillDomainId;
        subcategory.Code = dto.Code.ToUpper();
        subcategory.Name = dto.Name;
        subcategory.Description = dto.Description;
        subcategory.DisplayOrder = dto.DisplayOrder;
        subcategory.IsActive = dto.IsActive;
        subcategory.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var subcategory = await _context.SkillSubcategories
            .FirstOrDefaultAsync(s => s.Id == id && s.IsCurrent);

        if (subcategory == null)
            return false;

        subcategory.IsDeleted = true;
        subcategory.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleActiveAsync(Guid id)
    {
        var subcategory = await _context.SkillSubcategories
            .FirstOrDefaultAsync(s => s.Id == id && s.IsCurrent);

        if (subcategory == null)
            return false;

        subcategory.IsActive = !subcategory.IsActive;
        subcategory.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SkillSubcategoryDropdownDto>> GetDropdownAsync(Guid? domainId = null)
    {
        var query = _context.SkillSubcategories
            .Include(s => s.SkillDomain)
            .Where(s => s.IsCurrent && s.IsActive && s.SkillDomain.IsActive);

        if (domainId.HasValue)
        {
            query = query.Where(s => s.SkillDomainId == domainId.Value);
        }

        return await query
            .OrderBy(s => s.SkillDomain.DisplayOrder)
            .ThenBy(s => s.DisplayOrder)
            .Select(s => new SkillSubcategoryDropdownDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                FullName = s.SkillDomain.Name + " > " + s.Name
            })
            .ToListAsync();
    }

    public async Task<bool> CodeExistsAsync(string code, Guid domainId, Guid? excludeId = null)
    {
        var query = _context.SkillSubcategories
            .Where(s => s.Code.ToUpper() == code.ToUpper() &&
                        s.SkillDomainId == domainId &&
                        s.IsCurrent);

        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);

        return await query.AnyAsync();
    }
}
