import { apiClient } from './client';

// API Response wrapper
interface ApiResponse<T> {
  data: T;
  success: boolean;
  message: string | null;
  errors: string[] | null;
}

export interface DropdownItem {
  id: string;
  name: string;
  code?: string;
}

// Teams API
export async function getTeamsDropdown(): Promise<DropdownItem[]> {
  const response = await apiClient.get<ApiResponse<DropdownItem[]>>('/teams/dropdown');
  return response.data.data;
}

// Job Roles API
export async function getJobRolesDropdown(): Promise<DropdownItem[]> {
  const response = await apiClient.get<ApiResponse<DropdownItem[]>>('/jobroles/dropdown');
  return response.data.data;
}
