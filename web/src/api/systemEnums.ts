import { apiClient } from './client';
import type {
  SystemEnumValueDto,
  CreateSystemEnumValueDto,
  UpdateSystemEnumValueDto,
  EnumTypeDto,
  EnumDropdownItemDto,
  ReorderEnumValuesDto,
} from '../types';

const BASE_URL = '/systemenums';

export const systemEnumApi = {
  // Get all enum types with value counts
  getTypes: async (): Promise<EnumTypeDto[]> => {
    const response = await apiClient.get<EnumTypeDto[]>(`${BASE_URL}/types`);
    return response.data;
  },

  // Get all values for a specific enum type
  getValuesByType: async (enumType: string, includeInactive = false): Promise<SystemEnumValueDto[]> => {
    const response = await apiClient.get<SystemEnumValueDto[]>(`${BASE_URL}/values/${enumType}`, {
      params: { includeInactive },
    });
    return response.data;
  },

  // Get dropdown values for a specific enum type
  getDropdown: async (enumType: string, language = 'en'): Promise<EnumDropdownItemDto[]> => {
    const response = await apiClient.get<EnumDropdownItemDto[]>(`${BASE_URL}/dropdown/${enumType}`, {
      params: { language },
    });
    return response.data;
  },

  // Get a single enum value by ID
  getById: async (id: string): Promise<SystemEnumValueDto> => {
    const response = await apiClient.get<SystemEnumValueDto>(`${BASE_URL}/${id}`);
    return response.data;
  },

  // Create a new enum value
  create: async (data: CreateSystemEnumValueDto): Promise<SystemEnumValueDto> => {
    const response = await apiClient.post<SystemEnumValueDto>(BASE_URL, data);
    return response.data;
  },

  // Update an enum value
  update: async (id: string, data: UpdateSystemEnumValueDto): Promise<SystemEnumValueDto> => {
    const response = await apiClient.put<SystemEnumValueDto>(`${BASE_URL}/${id}`, data);
    return response.data;
  },

  // Toggle active status
  toggleActive: async (id: string): Promise<void> => {
    await apiClient.patch(`${BASE_URL}/${id}/toggle-active`);
  },

  // Delete an enum value
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`${BASE_URL}/${id}`);
  },

  // Reorder enum values
  reorder: async (data: ReorderEnumValuesDto): Promise<void> => {
    await apiClient.post(`${BASE_URL}/reorder`, data);
  },

  // Seed default values
  seed: async (): Promise<void> => {
    await apiClient.post(`${BASE_URL}/seed`);
  },
};
