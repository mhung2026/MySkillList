import { apiClient } from './client';
import type {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  UserDto,
  EmployeeProfileDto,
  UpdateProfileRequest,
  ChangePasswordRequest,
} from '../types';

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

export const getProfile = async (userId: string): Promise<EmployeeProfileDto> => {
  const response = await apiClient.get<EmployeeProfileDto>(`/auth/profile/${userId}`);
  return response.data;
};

export const updateProfile = async (
  userId: string,
  request: UpdateProfileRequest
): Promise<EmployeeProfileDto> => {
  const response = await apiClient.post<EmployeeProfileDto>(`/auth/profile/${userId}/update`, request);
  return response.data;
};

export const changePassword = async (
  userId: string,
  request: ChangePasswordRequest
): Promise<void> => {
  await apiClient.post(`/auth/change-password/${userId}`, request);
};
