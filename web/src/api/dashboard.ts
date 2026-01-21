import { apiClient } from './client';

// DTOs
export interface TeamDistributionDto {
  teamId?: string;
  teamName: string;
  employeeCount: number;
  percentage: number;
}

export interface RoleDistributionDto {
  roleName: string;
  employeeCount: number;
  percentage: number;
}

export interface SkillPopularityDto {
  skillId: string;
  skillName: string;
  skillCode: string;
  domainName: string;
  employeeCount: number;
  averageLevel: number;
}

export interface ProficiencyDistributionDto {
  level: number;
  levelName: string;
  employeeSkillCount: number;
  percentage: number;
}

export interface RecentAssessmentDto {
  id: string;
  employeeName: string;
  testTitle: string;
  score?: number;
  maxScore?: number;
  percentage: number;
  passed: boolean;
  completedAt: string;
}

export interface DomainSkillCoverageDto {
  domainId: string;
  domainName: string;
  domainCode: string;
  totalSkills: number;
  employeesWithSkills: number;
  coveragePercentage: number;
}

export interface DashboardOverviewDto {
  totalEmployees: number;
  totalSkills: number;
  totalAssessments: number;
  totalTestTemplates: number;
  teamDistribution: TeamDistributionDto[];
  roleDistribution: RoleDistributionDto[];
  topSkills: SkillPopularityDto[];
  proficiencyDistribution: ProficiencyDistributionDto[];
  recentAssessments: RecentAssessmentDto[];
  domainSkillCoverage: DomainSkillCoverageDto[];
}

export interface EmployeeSkillItemDto {
  skillId: string;
  skillName: string;
  skillCode: string;
  domainName: string;
  currentLevel: number;
  levelName: string;
  isValidated: boolean;
  lastAssessedAt?: string;
}

export interface EmployeeSkillSummaryDto {
  employeeId: string;
  employeeName: string;
  teamName?: string;
  jobRoleName?: string;
  totalSkills: number;
  averageLevel: number;
  skills: EmployeeSkillItemDto[];
}

export interface SkillColumnDto {
  skillId: string;
  skillName: string;
  skillCode: string;
}

export interface EmployeeSkillRowDto {
  employeeId: string;
  employeeName: string;
  skillLevels: Record<string, number>;
}

export interface TeamSkillMatrixDto {
  teamId?: string;
  teamName: string;
  skills: SkillColumnDto[];
  employees: EmployeeSkillRowDto[];
}

// API functions
export const dashboardApi = {
  getOverview: async (): Promise<DashboardOverviewDto> => {
    const response = await apiClient.get('/dashboard/overview');
    return response.data;
  },

  getEmployeeSkills: async (teamId?: string): Promise<EmployeeSkillSummaryDto[]> => {
    const params = teamId ? { teamId } : {};
    const response = await apiClient.get('/dashboard/employees/skills', { params });
    return response.data;
  },

  getEmployeeSkillDetail: async (employeeId: string): Promise<EmployeeSkillSummaryDto> => {
    const response = await apiClient.get(`/dashboard/employees/${employeeId}/skills`);
    return response.data;
  },

  getSkillMatrix: async (teamId?: string): Promise<TeamSkillMatrixDto> => {
    const params = teamId ? { teamId } : {};
    const response = await apiClient.get('/dashboard/skill-matrix', { params });
    return response.data;
  },
};
