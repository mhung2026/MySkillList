import { apiClient } from './client';
import type {
  ApiResponse,
  PagedResult,
  PagedRequest,
  SkillDomainDto,
  SkillDomainListDto,
  CreateSkillDomainDto,
  UpdateSkillDomainDto,
  SkillDomainDropdownDto,
  SkillSubcategoryDto,
  SkillSubcategoryListDto,
  CreateSkillSubcategoryDto,
  UpdateSkillSubcategoryDto,
  SkillSubcategoryDropdownDto,
  SkillDto,
  SkillListDto,
  CreateSkillDto,
  UpdateSkillDto,
  SkillFilterRequest,
  EnumValueDto,
  SkillLevelDefinitionDto,
  CreateSkillLevelDefinitionDto,
  LevelDefinitionDto,
  CreateLevelDefinitionDto,
  UpdateLevelDefinitionDto,
} from '../types';

// Skill Domains API
export const skillDomainApi = {
  getAll: async (params: PagedRequest) => {
    const response = await apiClient.get<ApiResponse<PagedResult<SkillDomainListDto>>>(
      '/skilldomains',
      { params }
    );
    return response.data;
  },

  getById: async (id: string) => {
    const response = await apiClient.get<ApiResponse<SkillDomainDto>>(`/skilldomains/${id}`);
    return response.data;
  },

  create: async (data: CreateSkillDomainDto) => {
    const response = await apiClient.post<ApiResponse<SkillDomainDto>>('/skilldomains', data);
    return response.data;
  },

  update: async (id: string, data: UpdateSkillDomainDto) => {
    const response = await apiClient.post<ApiResponse<SkillDomainDto>>(`/skilldomains/${id}/update`, data);
    return response.data;
  },

  delete: async (id: string) => {
    const response = await apiClient.post<ApiResponse<boolean>>(`/skilldomains/${id}/delete`);
    return response.data;
  },

  toggleActive: async (id: string) => {
    const response = await apiClient.post<ApiResponse<boolean>>(`/skilldomains/${id}/toggle-active`);
    return response.data;
  },

  getDropdown: async () => {
    const response = await apiClient.get<ApiResponse<SkillDomainDropdownDto[]>>('/skilldomains/dropdown');
    return response.data;
  },

  checkCode: async (code: string, excludeId?: string) => {
    const response = await apiClient.get<ApiResponse<boolean>>('/skilldomains/check-code', {
      params: { code, excludeId },
    });
    return response.data;
  },
};

// Skill Subcategories API
export const skillSubcategoryApi = {
  getAll: async (params: PagedRequest & { domainId?: string }) => {
    const response = await apiClient.get<ApiResponse<PagedResult<SkillSubcategoryListDto>>>(
      '/skillsubcategories',
      { params }
    );
    return response.data;
  },

  getById: async (id: string) => {
    const response = await apiClient.get<ApiResponse<SkillSubcategoryDto>>(`/skillsubcategories/${id}`);
    return response.data;
  },

  create: async (data: CreateSkillSubcategoryDto) => {
    const response = await apiClient.post<ApiResponse<SkillSubcategoryDto>>('/skillsubcategories', data);
    return response.data;
  },

  update: async (id: string, data: UpdateSkillSubcategoryDto) => {
    const response = await apiClient.post<ApiResponse<SkillSubcategoryDto>>(`/skillsubcategories/${id}/update`, data);
    return response.data;
  },

  delete: async (id: string) => {
    const response = await apiClient.post<ApiResponse<boolean>>(`/skillsubcategories/${id}/delete`);
    return response.data;
  },

  toggleActive: async (id: string) => {
    const response = await apiClient.post<ApiResponse<boolean>>(`/skillsubcategories/${id}/toggle-active`);
    return response.data;
  },

  getDropdown: async (domainId?: string) => {
    const response = await apiClient.get<ApiResponse<SkillSubcategoryDropdownDto[]>>(
      '/skillsubcategories/dropdown',
      { params: { domainId } }
    );
    return response.data;
  },
};

// Skills API
export const skillApi = {
  getAll: async (params: SkillFilterRequest) => {
    const response = await apiClient.get<ApiResponse<PagedResult<SkillListDto>>>('/skills', { params });
    return response.data;
  },

  getById: async (id: string) => {
    const response = await apiClient.get<ApiResponse<SkillDto>>(`/skills/${id}`);
    return response.data;
  },

  create: async (data: CreateSkillDto) => {
    const response = await apiClient.post<ApiResponse<SkillDto>>('/skills', data);
    return response.data;
  },

  update: async (id: string, data: UpdateSkillDto) => {
    const response = await apiClient.post<ApiResponse<SkillDto>>(`/skills/${id}/update`, data);
    return response.data;
  },

  delete: async (id: string) => {
    const response = await apiClient.post<ApiResponse<boolean>>(`/skills/${id}/delete`);
    return response.data;
  },

  toggleActive: async (id: string) => {
    const response = await apiClient.post<ApiResponse<boolean>>(`/skills/${id}/toggle-active`);
    return response.data;
  },

  getDropdown: async (subcategoryId?: string) => {
    const response = await apiClient.get<ApiResponse<SkillDomainDropdownDto[]>>('/skills/dropdown', {
      params: { subcategoryId },
    });
    return response.data;
  },
};

// Helper function for skills dropdown
export const getSkills = async (params: SkillFilterRequest) => {
  return skillApi.getAll(params);
};

// Enums API
export const enumApi = {
  getSkillCategories: async () => {
    const response = await apiClient.get<ApiResponse<EnumValueDto[]>>('/enums/skill-categories');
    return response.data;
  },

  getSkillTypes: async () => {
    const response = await apiClient.get<ApiResponse<EnumValueDto[]>>('/enums/skill-types');
    return response.data;
  },

  getProficiencyLevels: async () => {
    const response = await apiClient.get<ApiResponse<EnumValueDto[]>>('/enums/proficiency-levels');
    return response.data;
  },

};

// Skill Level Definitions API
export const skillLevelApi = {
  create: async (data: CreateSkillLevelDefinitionDto) => {
    const response = await apiClient.post<ApiResponse<SkillLevelDefinitionDto>>(
      `/skills/${data.skillId}/level-definitions`,
      data
    );
    return response.data;
  },

  update: async (id: string, data: CreateSkillLevelDefinitionDto) => {
    const response = await apiClient.post<ApiResponse<SkillLevelDefinitionDto>>(
      `/skills/level-definitions/${id}/update`,
      data
    );
    return response.data;
  },

  delete: async (id: string) => {
    const response = await apiClient.post<ApiResponse<boolean>>(`/skills/level-definitions/${id}/delete`);
    return response.data;
  },
};

// Level Definitions API (Global Proficiency Level Definitions)
export const levelDefinitionApi = {
  getAll: async () => {
    const response = await apiClient.get<LevelDefinitionDto[]>('/leveldefinitions');
    return response.data;
  },

  getById: async (id: string) => {
    const response = await apiClient.get<LevelDefinitionDto>(`/leveldefinitions/${id}`);
    return response.data;
  },

  getByLevel: async (level: number) => {
    const response = await apiClient.get<LevelDefinitionDto>(`/leveldefinitions/by-level/${level}`);
    return response.data;
  },

  create: async (data: CreateLevelDefinitionDto) => {
    const response = await apiClient.post<LevelDefinitionDto>('/leveldefinitions', data);
    return response.data;
  },

  update: async (id: string, data: UpdateLevelDefinitionDto) => {
    const response = await apiClient.post<LevelDefinitionDto>(`/leveldefinitions/${id}/update`, data);
    return response.data;
  },

  delete: async (id: string) => {
    const response = await apiClient.post(`/leveldefinitions/${id}/delete`);
    return response.data;
  },

  seed: async () => {
    const response = await apiClient.post('/leveldefinitions/seed');
    return response.data;
  },
};
