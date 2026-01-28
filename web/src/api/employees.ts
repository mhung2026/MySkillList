import { apiClient } from './client';

// API Response wrapper
interface ApiResponse<T> {
  data: T;
  success: boolean;
  message: string | null;
  errors: string[] | null;
}

// Types
export interface SkillGapDetail {
  skillId: string;
  skillName: string;
  skillCode: string;
  currentLevel: number;
  currentLevelName: string;
  requiredLevel: number;
  requiredLevelName: string;
  expectedLevel?: number;
  gapSize: number;
  priority: string;
  isMandatory: boolean;
  isMet: boolean; // True if current level meets or exceeds required level
  aiAnalysis?: string;
  aiRecommendation?: string;
}

export interface GapAnalysisSummary {
  totalGaps: number;
  criticalGaps: number;
  highGaps: number;
  mediumGaps: number;
  lowGaps: number;
  metRequirements: number;
  overallReadiness: number;
}

export interface GapAnalysisResponse {
  employee: {
    id: string;
    fullName: string;
    email: string;
    jobRole?: {
      id: string;
      name: string;
      code: string;
      levelInHierarchy?: number;
    };
    team?: {
      id: string;
      name: string;
    };
    yearsOfExperience?: number;
  };
  currentRole?: {
    id: string;
    name: string;
    code: string;
    levelInHierarchy?: number;
  };
  targetRole: {
    id: string;
    name: string;
    code: string;
    levelInHierarchy?: number;
  };
  gaps: SkillGapDetail[];
  summary: GapAnalysisSummary;
}

export interface LearningPathItem {
  id: string;
  displayOrder: number;
  title: string;
  description?: string;
  itemType: string;
  resourceId?: string;
  externalUrl?: string;
  estimatedHours?: number;
  targetLevelAfter?: number;
  successCriteria?: string;
  status: string;
}

export interface LearningPathMilestone {
  afterItem: number;
  description: string;
  expectedLevel: number;
}

export interface LearningPathResponse {
  id: string;
  status: string;
  title: string;
  description?: string;
  targetSkill: {
    id: string;
    name: string;
    code: string;
  };
  currentLevel?: number;
  currentLevelName?: string;
  targetLevel: number;
  targetLevelName: string;
  estimatedTotalHours?: number;
  estimatedDurationWeeks?: number;
  targetCompletionDate?: string;
  isAiGenerated: boolean;
  aiRationale?: string;
  keySuccessFactors?: string[];
  potentialChallenges?: string[];
  items: LearningPathItem[];
  milestones?: LearningPathMilestone[];
  message?: string;
}

export interface CreateLearningPathRequest {
  skillGapId?: string;
  targetSkillId?: string;
  targetLevel: number;
  targetCompletionDate?: string;
  timeConstraintMonths?: number;
  useAiGeneration?: boolean;
}

export interface RecalculateGapsResult {
  success: boolean;
  message: string;
  gapsCreated: number;
  gapsUpdated: number;
  gapsResolved: number;
}

export interface EmployeeBulkGapResult {
  employeeId: string;
  employeeName: string;
  roleName?: string;
  gapsCreated: number;
  gapsUpdated: number;
  gapsResolved: number;
}

export interface BulkRecalculateGapsResult {
  success: boolean;
  message: string;
  employeesProcessed: number;
  employeesWithGaps: number;
  totalGapsCreated: number;
  totalGapsUpdated: number;
  totalGapsResolved: number;
  employeeResults: EmployeeBulkGapResult[];
}

export interface LearningRecommendation {
  id: string;
  skillGapId: string;
  skillId: string;
  skillName: string;
  recommendationType: string;  // "Course", "Project", "Mentorship"
  title: string;
  description?: string;
  url?: string;  // Coursera course URL
  estimatedHours?: number;
  rationale: string;
  displayOrder: number;
  generatedAt: string;
}

// API Functions
export async function getGapAnalysis(employeeId: string, targetRoleId?: string) {
  const params = targetRoleId ? { targetRoleId } : {};
  const response = await apiClient.get<ApiResponse<GapAnalysisResponse>>(
    `/employees/${employeeId}/gap-analysis`,
    { params }
  );
  return response.data.data;
}

export async function recalculateGaps(employeeId: string, targetRoleId?: string) {
  const response = await apiClient.post<ApiResponse<RecalculateGapsResult>>(
    `/employees/${employeeId}/gap-analysis/recalculate`,
    { targetRoleId }
  );
  return response.data.data;
}

export async function bulkRecalculateGapsForAllEmployees() {
  const response = await apiClient.post<ApiResponse<BulkRecalculateGapsResult>>(
    '/employees/gap-analysis/recalculate-all'
  );
  return response.data.data;
}

export async function getLearningPaths(employeeId: string) {
  const response = await apiClient.get<ApiResponse<LearningPathResponse[]>>(
    `/employees/${employeeId}/learning-paths`
  );
  return response.data.data || [];
}

export async function createLearningPath(
  employeeId: string,
  request: CreateLearningPathRequest
) {
  const response = await apiClient.post<ApiResponse<LearningPathResponse>>(
    `/employees/${employeeId}/learning-path`,
    request
  );
  return response.data.data;
}

export async function getLearningRecommendations(employeeId: string) {
  const response = await apiClient.get<ApiResponse<LearningRecommendation[]>>(
    `/employees/${employeeId}/learning-recommendations`
  );
  return response.data.data || [];
}

// Admin APIs
export interface SeedDatabaseResult {
  success: boolean;
  message: string;
  skillsCreated: number;
  jobRolesCreated: number;
  teamsCreated: number;
  employeesCreated: number;
  gapsCreated: number;
  gapsUpdated: number;
}

export async function seedSampleData() {
  const response = await apiClient.post<ApiResponse<SeedDatabaseResult>>(
    '/admin/seed-sample-data'
  );
  return response.data.data;
}
