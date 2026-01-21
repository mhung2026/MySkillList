using SkillMatrix.Application.DTOs.Common;
using SkillMatrix.Application.DTOs.Taxonomy;

namespace SkillMatrix.Application.Interfaces;

public interface ISkillDomainService
{
    Task<PagedResult<SkillDomainListDto>> GetAllAsync(PagedRequest request);
    Task<SkillDomainDto?> GetByIdAsync(Guid id);
    Task<SkillDomainDto> CreateAsync(CreateSkillDomainDto dto);
    Task<SkillDomainDto> UpdateAsync(Guid id, UpdateSkillDomainDto dto);
    Task<bool> DeleteAsync(Guid id);  // Soft delete
    Task<bool> ToggleActiveAsync(Guid id);  // Activate/Deactivate
    Task<List<SkillDomainDropdownDto>> GetDropdownAsync();
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
}

public interface ISkillSubcategoryService
{
    Task<PagedResult<SkillSubcategoryListDto>> GetAllAsync(PagedRequest request, Guid? domainId = null);
    Task<SkillSubcategoryDto?> GetByIdAsync(Guid id);
    Task<SkillSubcategoryDto> CreateAsync(CreateSkillSubcategoryDto dto);
    Task<SkillSubcategoryDto> UpdateAsync(Guid id, UpdateSkillSubcategoryDto dto);
    Task<bool> DeleteAsync(Guid id);  // Soft delete
    Task<bool> ToggleActiveAsync(Guid id);  // Activate/Deactivate
    Task<List<SkillSubcategoryDropdownDto>> GetDropdownAsync(Guid? domainId = null);
    Task<bool> CodeExistsAsync(string code, Guid domainId, Guid? excludeId = null);
}

public interface ISkillService
{
    Task<PagedResult<SkillListDto>> GetAllAsync(SkillFilterRequest request);
    Task<SkillDto?> GetByIdAsync(Guid id);
    Task<SkillDto> CreateAsync(CreateSkillDto dto);
    Task<SkillDto> UpdateAsync(Guid id, UpdateSkillDto dto);
    Task<bool> DeleteAsync(Guid id);  // Soft delete
    Task<bool> ToggleActiveAsync(Guid id);  // Activate/Deactivate
    Task<List<SkillDropdownDto>> GetDropdownAsync(Guid? subcategoryId = null);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);

    // Level definitions
    Task<SkillLevelDefinitionDto> CreateLevelDefinitionAsync(CreateSkillLevelDefinitionDto dto);
    Task<SkillLevelDefinitionDto> UpdateLevelDefinitionAsync(Guid id, CreateSkillLevelDefinitionDto dto);
    Task<bool> DeleteLevelDefinitionAsync(Guid id);
}

public class SkillFilterRequest : PagedRequest
{
    public Guid? DomainId { get; set; }
    public Guid? SubcategoryId { get; set; }
    public int? Category { get; set; }  // SkillCategory enum value
    public int? SkillType { get; set; }  // SkillType enum value
    public bool? IsCompanySpecific { get; set; }
}
