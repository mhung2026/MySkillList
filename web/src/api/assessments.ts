import { apiClient } from './client';
import type {
  PagedResult,
  AvailableTestDto,
  AssessmentListDto,
  StartAssessmentRequest,
  StartAssessmentResponse,
  SubmitAnswerRequest,
  SubmitAnswerResponse,
  AssessmentResultDto,
} from '../types';

// Get available tests for employee
export const getAvailableTests = async (
  employeeId: string
): Promise<AvailableTestDto[]> => {
  const response = await apiClient.get<AvailableTestDto[]>(
    `/assessments/available/${employeeId}`
  );
  return response.data;
};

// Get employee's assessment history
export const getEmployeeAssessments = async (
  employeeId: string,
  pageNumber = 1,
  pageSize = 10
): Promise<PagedResult<AssessmentListDto>> => {
  const response = await apiClient.get<PagedResult<AssessmentListDto>>(
    `/assessments/employee/${employeeId}`,
    { params: { pageNumber, pageSize } }
  );
  return response.data;
};

// Start a new assessment
export const startAssessment = async (
  request: StartAssessmentRequest
): Promise<StartAssessmentResponse> => {
  const response = await apiClient.post<StartAssessmentResponse>(
    '/assessments/start',
    request
  );
  return response.data;
};

// Continue an in-progress assessment
export const continueAssessment = async (
  assessmentId: string
): Promise<StartAssessmentResponse> => {
  const response = await apiClient.get<StartAssessmentResponse>(
    `/assessments/${assessmentId}/continue`
  );
  return response.data;
};

// Submit answer for a question
export const submitAnswer = async (
  request: SubmitAnswerRequest
): Promise<SubmitAnswerResponse> => {
  const response = await apiClient.post<SubmitAnswerResponse>(
    '/assessments/answer',
    request
  );
  return response.data;
};

// Submit/complete the assessment
export const submitAssessment = async (
  assessmentId: string
): Promise<AssessmentResultDto> => {
  const response = await apiClient.post<AssessmentResultDto>(
    `/assessments/${assessmentId}/submit`
  );
  return response.data;
};

// Get assessment result
export const getAssessmentResult = async (
  assessmentId: string
): Promise<AssessmentResultDto> => {
  const response = await apiClient.get<AssessmentResultDto>(
    `/assessments/${assessmentId}/result`
  );
  return response.data;
};
