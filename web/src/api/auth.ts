import { apiClient } from './client';
import type { LoginRequest, LoginResponse, RegisterRequest, UserDto } from '../types';

export const login = async (request: LoginRequest): Promise<LoginResponse> => {
  const response = await apiClient.post<LoginResponse>('/auth/login', request);
  return response.data;
};

export const register = async (request: RegisterRequest): Promise<LoginResponse> => {
  const response = await apiClient.post<LoginResponse>('/auth/register', request);
  return response.data;
};

export const getCurrentUser = async (userId: string): Promise<UserDto> => {
  const response = await apiClient.get<UserDto>(`/auth/me/${userId}`);
  return response.data;
};

export const getAllUsers = async (): Promise<UserDto[]> => {
  const response = await apiClient.get<UserDto[]>('/auth/users');
  return response.data;
};

export const seedUsers = async (): Promise<void> => {
  await apiClient.post('/auth/seed');
};
