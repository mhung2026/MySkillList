import { apiClient } from './client';
import type {
  PagedResult,
  TestTemplateDto,
  TestTemplateListDto,
  CreateTestTemplateDto,
  UpdateTestTemplateDto,
  TestSectionDto,
  CreateTestSectionDto,
  UpdateTestSectionDto,
  QuestionDto,
  CreateQuestionDto,
  UpdateQuestionDto,
  GenerateAiQuestionsRequest,
  AiGenerateQuestionsResponse,
} from '../types';

// Test Templates
export const getTestTemplates = async (params: {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  includeInactive?: boolean;
}): Promise<PagedResult<TestTemplateListDto>> => {
  const response = await apiClient.get<PagedResult<TestTemplateListDto>>(
    '/testtemplates',
    { params }
  );
  return response.data;
};

export const getTestTemplate = async (id: string): Promise<TestTemplateDto> => {
  const response = await apiClient.get<TestTemplateDto>(`/testtemplates/${id}`);
  return response.data;
};

export const createTestTemplate = async (
  data: CreateTestTemplateDto
): Promise<TestTemplateDto> => {
  const response = await apiClient.post<TestTemplateDto>('/testtemplates', data);
  return response.data;
};

export const updateTestTemplate = async (
  id: string,
  data: UpdateTestTemplateDto
): Promise<TestTemplateDto> => {
  const response = await apiClient.put<TestTemplateDto>(
    `/testtemplates/${id}`,
    data
  );
  return response.data;
};

export const deleteTestTemplate = async (id: string): Promise<void> => {
  await apiClient.post(`/testtemplates/${id}/delete`);
};

export const toggleTestTemplateActive = async (id: string): Promise<void> => {
  await apiClient.post(`/testtemplates/${id}/toggle-active`);
};

// Test Sections
export const createTestSection = async (
  data: CreateTestSectionDto
): Promise<TestSectionDto> => {
  const response = await apiClient.post<TestSectionDto>(
    '/testtemplates/sections',
    data
  );
  return response.data;
};

export const updateTestSection = async (
  id: string,
  data: UpdateTestSectionDto
): Promise<TestSectionDto> => {
  const response = await apiClient.put<TestSectionDto>(
    `/testtemplates/sections/${id}`,
    data
  );
  return response.data;
};

export const deleteTestSection = async (id: string): Promise<void> => {
  await apiClient.post(`/testtemplates/sections/${id}/delete`);
};

// AI Generation
export const generateAiQuestions = async (
  data: GenerateAiQuestionsRequest
): Promise<QuestionDto[]> => {
  const response = await apiClient.post<QuestionDto[]>(
    '/questions/generate-ai',
    data
  );
  return response.data;
};

export const previewAiQuestions = async (
  data: Omit<GenerateAiQuestionsRequest, 'sectionId'>
): Promise<AiGenerateQuestionsResponse> => {
  const response = await apiClient.post<AiGenerateQuestionsResponse>(
    '/ai/generate-questions',
    data
  );
  return response.data;
};

// Questions CRUD
export const createQuestion = async (
  data: CreateQuestionDto
): Promise<QuestionDto> => {
  const response = await apiClient.post<QuestionDto>('/questions', data);
  return response.data;
};

export const updateQuestion = async (
  id: string,
  data: UpdateQuestionDto
): Promise<QuestionDto> => {
  const response = await apiClient.put<QuestionDto>(`/questions/${id}`, data);
  return response.data;
};

export const deleteQuestion = async (id: string): Promise<void> => {
  await apiClient.post(`/questions/${id}/delete`);
};

export const toggleQuestionActive = async (id: string): Promise<void> => {
  await apiClient.post(`/questions/${id}/toggle-active`);
};
