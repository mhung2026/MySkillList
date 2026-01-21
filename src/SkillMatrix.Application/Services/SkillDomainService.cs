using Microsoft.EntityFrameworkCore;
using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Application.DTOs.Taxonomy;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Entities.Taxonomy;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Application.Services;

public class SkillDomainService : ISkillDomainService
{
    private readonly SkillMatrixDbContext _context;

    public SkillDomainService(SkillMatrixDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SkillDomainListDto>> GetAllAsync(PagedRequest request)
    {
        var query = _context.SkillDomains
            .Where(d => d.IsCurrent)
            .AsQueryable();

        // Filter by active status
        if (!request.IncludeInactive)
        {
            query = query.Where(d => d.IsActive);
        }
        else if (request.IsActive.HasValue)
        {
            query = query.Where(d => d.IsActive == request.IsActive.Value);
        }

        // Search
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(d =>
                d.Code.ToLower().Contains(term) ||
                d.Name.ToLower().Contains(term) ||
                (d.Description != null && d.Description.ToLower().Contains(term)));
        }

        // Count
        var totalCount = await query.CountAsync();

        // Sort
        query = request.SortBy?.ToLower() switch
        {
            "code" => request.SortDescending ? query.OrderByDescending(d => d.Code) : query.OrderBy(d => d.Code),
            "name" => request.SortDescending ? query.OrderByDescending(d => d.Name) : query.OrderBy(d => d.Name),
            "createdat" => request.SortDescending ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt),
            _ => query.OrderBy(d => d.DisplayOrder)
        };

        // Paginate
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new SkillDomainListDto
            {
                Id = d.Id,
                Code = d.Code,
                Name = d.Name,
                Description = d.Description,
                DisplayOrder = d.DisplayOrder,
                IsActive = d.IsActive,
                SubcategoryCount = d.Subcategories.Count(s => !s.IsDeleted && s.IsCurrent),
                SkillCount = d.Subcategories
                    .Where(s => !s.IsDeleted && s.IsCurrent)
                    .SelectMany(s => s.Skills)
                    .Count(sk => !sk.IsDeleted && sk.IsCurrent)
            })
            .ToListAsync();

        return new PagedResult<SkillDomainListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<SkillDomainDto?> GetByIdAsync(Guid id)
    {
        var domain = await _context.SkillDomains
            .Where(d => d.Id == id && d.IsCurrent)
            .Select(d => new SkillDomainDto
            {
                Id = d.Id,
                Code = d.Code,
                Name = d.Name,
                Description = d.Description,
                DisplayOrder = d.DisplayOrder,
                IsActive = d.IsActive,
                Version = d.Version,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                SubcategoryCount = d.Subcategories.Count(s => !s.IsDeleted && s.IsCurrent),
                SkillCount = d.Subcategories
                    .Where(s => !s.IsDeleted && s.IsCurrent)
                    .SelectMany(s => s.Skills)
                    .Count(sk => !sk.IsDeleted && sk.IsCurrent)
            })
            .FirstOrDefaultAsync();

        return domain;
    }

    public async Task<SkillDomainDto> CreateAsync(CreateSkillDomainDto dto)
    {
        var domain = new SkillDomain
        {
            Id = Guid.NewGuid(),
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

        _context.SkillDomains.Add(domain);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(domain.Id))!;
    }

    public async Task<SkillDomainDto> UpdateAsync(Guid id, UpdateSkillDomainDto dto)
    {
        var domain = await _context.SkillDomains
            .FirstOrDefaultAsync(d => d.Id == id && d.IsCurrent);

        if (domain == null)
            throw new KeyNotFoundException($"SkillDomain with id {id} not found");

        domain.Code = dto.Code.ToUpper();
        domain.Name = dto.Name;
        domain.Description = dto.Description;
        domain.DisplayOrder = dto.DisplayOrder;
        domain.IsActive = dto.IsActive;
        domain.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var domain = await _context.SkillDomains
            .FirstOrDefaultAsync(d => d.Id == id && d.IsCurrent);

        if (domain == null)
            return false;

        // Soft delete
        domain.IsDeleted = true;
        domain.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleActiveAsync(Guid id)
    {
        var domain = await _context.SkillDomains
            .FirstOrDefaultAsync(d => d.Id == id && d.IsCurrent);

        if (domain == null)
            return false;

        domain.IsActive = !domain.IsActive;
        domain.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SkillDomainDropdownDto>> GetDropdownAsync()
    {
        return await _context.SkillDomains
            .Where(d => d.IsCurrent && d.IsActive)
            .OrderBy(d => d.DisplayOrder)
            .Select(d => new SkillDomainDropdownDto
            {
                Id = d.Id,
                Code = d.Code,
                Name = d.Name
            })
            .ToListAsync();
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
    {
        var query = _context.SkillDomains
            .Where(d => d.Code.ToUpper() == code.ToUpper() && d.IsCurrent);

        if (excludeId.HasValue)
            query = query.Where(d => d.Id != excludeId.Value);

        return await query.AnyAsync();
    }
}
