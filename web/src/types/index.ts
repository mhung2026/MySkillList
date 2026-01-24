// Common types
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

// Auth types
export enum UserRole {
  Employee = 1,
  TeamLead = 2,
  Manager = 3,
  Admin = 4,
}

export interface UserDto {
  id: string;
  email: string;
  fullName: string;
  avatarUrl?: string;
  role: UserRole;
  roleName: string;
  teamId?: string;
  teamName?: string;
  jobRoleId?: string;
  jobRoleName?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  message?: string;
  user?: UserDto;
  token?: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
  role?: UserRole;
  teamId?: string;
}

// Employment Status
export enum EmploymentStatus {
  Active = 1,
  OnLeave = 2,
  Resigned = 3,
  Terminated = 4,
}

// Employee Profile
export interface EmployeeProfileDto {
  id: string;
  email: string;
  fullName: string;
  avatarUrl?: string;
  role: UserRole;
  roleName: string;
  status: EmploymentStatus;
  statusName: string;
  teamId?: string;
  teamName?: string;
  jobRoleId?: string;
  jobRoleName?: string;
  managerId?: string;
  managerName?: string;
  joinDate?: string;
  yearsOfExperience: number;
  totalSkills: number;
  completedAssessments: number;
  averageSkillLevel: number;
  createdAt: string;
  updatedAt?: string;
}

export interface UpdateProfileRequest {
  fullName: string;
  avatarUrl?: string;
  joinDate?: string;
  yearsOfExperience: number;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface PagedRequest {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
  isActive?: boolean;
  includeInactive?: boolean;
}

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}

// Enums
export enum SkillCategory {
  Technical = 1,
  Professional = 2,
  Domain = 3,
  Leadership = 4,
  Tools = 5,
}

export enum SkillType {
  Core = 1,
  Specialty = 2,
  Adjacent = 3,
}

export enum ProficiencyLevel {
  None = 0,
  Follow = 1,
  Assist = 2,
  Apply = 3,
  Enable = 4,
  EnsureAdvise = 5,
  Initiate = 6,
  SetStrategy = 7,
}

// Skill Domain
export interface SkillDomainDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
  version: number;
  createdAt: string;
  updatedAt?: string;
  subcategoryCount: number;
  skillCount: number;
}

export interface SkillDomainListDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
  subcategoryCount: number;
  skillCount: number;
}

export interface CreateSkillDomainDto {
  code: string;
  name: string;
  description?: string;
  displayOrder: number;
}

export interface UpdateSkillDomainDto {
  code: string;
  name: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
}

export interface SkillDomainDropdownDto {
  id: string;
  code: string;
  name: string;
}

// Skill Subcategory
export interface SkillSubcategoryDto {
  id: string;
  skillDomainId: string;
  skillDomainName: string;
  code: string;
  name: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
  version: number;
  createdAt: string;
  updatedAt?: string;
  skillCount: number;
}

export interface SkillSubcategoryListDto {
  id: string;
  skillDomainId: string;
  skillDomainCode: string;
  skillDomainName: string;
  code: string;
  name: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
  skillCount: number;
}

export interface CreateSkillSubcategoryDto {
  skillDomainId: string;
  code: string;
  name: string;
  description?: string;
  displayOrder: number;
}

export interface UpdateSkillSubcategoryDto {
  skillDomainId: string;
  code: string;
  name: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
}

export interface SkillSubcategoryDropdownDto {
  id: string;
  code: string;
  name: string;
  fullName: string;
}

// Skill
export interface SkillDto {
  id: string;
  subcategoryId: string;
  subcategoryName: string;
  domainName: string;
  code: string;
  name: string;
  description?: string;
  category: SkillCategory;
  categoryName: string;
  skillType: SkillType;
  skillTypeName: string;
  displayOrder: number;
  isActive: boolean;
  isCompanySpecific: boolean;
  tags?: string[];
  applicableLevelsString?: string;
  applicableLevels?: number[];
  version: number;
  createdAt: string;
  updatedAt?: string;
  levelDefinitions: SkillLevelDefinitionDto[];
}

export interface SkillListDto {
  id: string;
  subcategoryId: string;
  subcategoryCode: string;
  subcategoryName: string;
  domainId: string;
  domainCode: string;
  domainName: string;
  code: string;
  name: string;
  description?: string;
  category: SkillCategory;
  categoryName: string;
  skillType: SkillType;
  skillTypeName: string;
  displayOrder: number;
  isActive: boolean;
  isCompanySpecific: boolean;
  applicableLevelsString?: string;
  employeeCount: number;
}

export interface CreateSkillDto {
  subcategoryId: string;
  code: string;
  name: string;
  description?: string;
  category: SkillCategory;
  skillType: SkillType;
  displayOrder: number;
  isCompanySpecific: boolean;
  tags?: string[];
}

export interface UpdateSkillDto {
  subcategoryId: string;
  code: string;
  name: string;
  description?: string;
  category: SkillCategory;
  skillType: SkillType;
  displayOrder: number;
  isActive: boolean;
  isCompanySpecific: boolean;
  tags?: string[];
}

export interface SkillLevelDefinitionDto {
  id: string;
  level: ProficiencyLevel;
  levelName: string;
  customLevelName?: string;
  displayLevelName: string;
  description: string;
  autonomy?: string;
  influence?: string;
  complexity?: string;
  businessSkills?: string;
  knowledge?: string;
  behavioralIndicators?: string[];
  evidenceExamples?: string[];
}

export interface CreateSkillLevelDefinitionDto {
  skillId: string;
  level: ProficiencyLevel;
  customLevelName?: string;
  description: string;
  autonomy?: string;
  influence?: string;
  complexity?: string;
  businessSkills?: string;
  knowledge?: string;
  behavioralIndicators?: string[];
  evidenceExamples?: string[];
}

// Proficiency Level Definition (database entity - extensible for various frameworks)
export interface LevelDefinitionDto {
  id: string;
  level: number;
  levelName: string;
  description?: string;
  autonomy?: string;
  influence?: string;
  complexity?: string;
  knowledge?: string;
  businessSkills?: string;
  behavioralIndicators?: string[];
  color?: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateLevelDefinitionDto {
  level: number;
  levelName: string;
  description?: string;
  autonomy?: string;
  influence?: string;
  complexity?: string;
  knowledge?: string;
  businessSkills?: string;
  behavioralIndicators?: string[];
  color?: string;
  displayOrder: number;
}

export interface UpdateLevelDefinitionDto {
  levelName: string;
  description?: string;
  autonomy?: string;
  influence?: string;
  complexity?: string;
  knowledge?: string;
  businessSkills?: string;
  behavioralIndicators?: string[];
  color?: string;
  displayOrder: number;
  isActive: boolean;
}

export interface SkillFilterRequest extends PagedRequest {
  domainId?: string;
  subcategoryId?: string;
  category?: number;
  skillType?: number;
  isCompanySpecific?: boolean;
}

// Enum values
export interface EnumValueDto {
  value: number;
  name: string;
  description?: string;
}

// Assessment/Test Enums
export enum AssessmentType {
  SelfAssessment = 1,
  ManagerAssessment = 2,
  PeerAssessment = 3,
  TechnicalTest = 4,
  Interview = 5,
  Custom = 6,
}

export enum QuestionType {
  MultipleChoice = 1,
  MultipleAnswer = 2,
  TrueFalse = 3,
  ShortAnswer = 4,
  LongAnswer = 5,
  CodingChallenge = 6,
  Scenario = 7,
  SituationalJudgment = 8,
  Rating = 9,
}

export enum DifficultyLevel {
  Easy = 1,
  Medium = 2,
  Hard = 3,
  Expert = 4,
}

// Test Template
export interface TestTemplateDto {
  id: string;
  title: string;
  description?: string;
  type: AssessmentType;
  typeName: string;
  targetJobRoleId?: string;
  targetJobRoleName?: string;
  targetSkillId?: string;
  targetSkillName?: string;
  timeLimitMinutes?: number;
  passingScore: number;
  isRandomized: boolean;
  maxQuestions?: number;
  isAiGenerated: boolean;
  requiresReview: boolean;
  isActive: boolean;
  version: number;
  createdAt: string;
  updatedAt?: string;
  sectionCount: number;
  questionCount: number;
  sections: TestSectionDto[];
}

export interface TestTemplateListDto {
  id: string;
  title: string;
  description?: string;
  type: AssessmentType;
  typeName: string;
  targetJobRoleName?: string;
  targetSkillName?: string;
  timeLimitMinutes?: number;
  passingScore: number;
  isAiGenerated: boolean;
  isActive: boolean;
  sectionCount: number;
  questionCount: number;
  assessmentCount: number;
}

export interface CreateTestTemplateDto {
  title: string;
  description?: string;
  type: AssessmentType;
  targetJobRoleId?: string;
  targetSkillId?: string;
  timeLimitMinutes?: number;
  passingScore: number;
  isRandomized: boolean;
  maxQuestions?: number;
  requiresReview: boolean;
}

export interface UpdateTestTemplateDto {
  title: string;
  description?: string;
  type: AssessmentType;
  targetJobRoleId?: string;
  targetSkillId?: string;
  timeLimitMinutes?: number;
  passingScore: number;
  isRandomized: boolean;
  maxQuestions?: number;
  requiresReview: boolean;
  isActive: boolean;
}

// Test Section
export interface TestSectionDto {
  id: string;
  testTemplateId: string;
  title: string;
  description?: string;
  displayOrder: number;
  timeLimitMinutes?: number;
  questionCount: number;
  questions: QuestionDto[];
}

export interface CreateTestSectionDto {
  testTemplateId: string;
  title: string;
  description?: string;
  displayOrder: number;
  timeLimitMinutes?: number;
}

export interface UpdateTestSectionDto {
  title: string;
  description?: string;
  displayOrder: number;
  timeLimitMinutes?: number;
}

// Question
export interface QuestionDto {
  id: string;
  sectionId?: string;
  sectionTitle?: string;
  skillId: string;
  skillName: string;
  skillCode: string;
  targetLevel: ProficiencyLevel;
  targetLevelName: string;
  type: QuestionType;
  typeName: string;
  content: string;
  codeSnippet?: string;
  mediaUrl?: string;
  points: number;
  timeLimitSeconds?: number;
  difficulty: DifficultyLevel;
  difficultyName: string;
  isAiGenerated: boolean;
  aiPromptUsed?: string;
  isActive: boolean;
  tags?: string[];
  options: QuestionOptionDto[];
  gradingRubric?: string;
}

export interface QuestionListDto {
  id: string;
  skillId: string;
  skillName: string;
  skillCode: string;
  targetLevel: ProficiencyLevel;
  targetLevelName: string;
  type: QuestionType;
  typeName: string;
  content: string;
  points: number;
  difficulty: DifficultyLevel;
  difficultyName: string;
  isAiGenerated: boolean;
  isActive: boolean;
  optionCount: number;
}

export interface CreateQuestionDto {
  sectionId?: string;
  skillId?: string;
  targetLevel: ProficiencyLevel;
  type: QuestionType;
  content: string;
  codeSnippet?: string;
  mediaUrl?: string;
  points: number;
  timeLimitSeconds?: number;
  difficulty: DifficultyLevel;
  tags?: string[];
  gradingRubric?: string;
  options: CreateQuestionOptionDto[];
}

export interface UpdateQuestionDto {
  sectionId?: string;
  skillId?: string;
  targetLevel: ProficiencyLevel;
  type: QuestionType;
  content: string;
  codeSnippet?: string;
  mediaUrl?: string;
  points: number;
  timeLimitSeconds?: number;
  difficulty: DifficultyLevel;
  tags?: string[];
  gradingRubric?: string;
  isActive: boolean;
  options: CreateQuestionOptionDto[];
}

export interface QuestionOptionDto {
  id: string;
  content: string;
  isCorrect: boolean;
  displayOrder: number;
  explanation?: string;
}

export interface CreateQuestionOptionDto {
  content: string;
  isCorrect: boolean;
  displayOrder: number;
  explanation?: string;
}

export interface QuestionFilterRequest extends PagedRequest {
  skillId?: string;
  sectionId?: string;
  targetLevel?: ProficiencyLevel;
  type?: QuestionType;
  difficulty?: DifficultyLevel;
  isAiGenerated?: boolean;
}

// AI Generation
export interface AiGenerateQuestionsRequest {
  questionTypes: QuestionType[];  // Required
  language: string;               // Required
  questionCount: number;          // Required
  skillId?: string;               // Optional
  targetLevel?: ProficiencyLevel; // Optional
  difficulty?: DifficultyLevel;   // Optional
  additionalContext?: string;     // Optional
  sectionId?: string;             // Optional
}

export interface AiGenerateQuestionsResponse {
  success: boolean;
  message?: string;
  error?: string;
  questions: AiGeneratedQuestion[];
  metadata: AiGenerationMetadata;
}

export interface AiGeneratedQuestion {
  content: string;
  codeSnippet?: string;
  type: QuestionType;
  difficulty: DifficultyLevel;
  suggestedPoints: number;
  suggestedTimeSeconds?: number;
  options: AiGeneratedOption[];
  explanation?: string;
  tags: string[];
  expectedAnswer?: string;
  gradingRubric?: string;
}

export interface AiGeneratedOption {
  content: string;
  isCorrect: boolean;
  explanation?: string;
}

export interface AiGenerationMetadata {
  model: string;
  tokensUsed: number;
  generationTimeMs: number;
  promptUsed: string;
  generatedAt: string;
}

export interface GenerateAiQuestionsRequest extends AiGenerateQuestionsRequest {
  sectionId: string;
}

// Assessment Status
export enum AssessmentStatus {
  Draft = 1,
  Pending = 2,
  InProgress = 3,
  Completed = 4,
  Reviewed = 5,
  Disputed = 6,
  Resolved = 7,
}

// Available Test
export interface AvailableTestDto {
  testTemplateId: string;
  title: string;
  description?: string;
  typeName: string;
  targetSkillName?: string;
  timeLimitMinutes?: number;
  questionCount: number;
  passingScore: number;
  hasAttempted: boolean;
  attemptCount: number;
  bestScore?: number;
}

// Assessment List
export interface AssessmentListDto {
  id: string;
  employeeId: string;
  employeeName: string;
  type: AssessmentType;
  typeName: string;
  status: AssessmentStatus;
  statusName: string;
  title?: string;
  testTemplateId?: string;
  testTemplateTitle?: string;
  score?: number;
  maxScore?: number;
  percentage?: number;
  startedAt?: string;
  completedAt?: string;
  createdAt: string;
}

// Start Assessment Request/Response
export interface StartAssessmentRequest {
  employeeId: string;
  testTemplateId: string;
  title?: string;
}

export interface StartAssessmentResponse {
  assessmentId: string;
  title: string;
  description?: string;
  timeLimitMinutes?: number;
  totalQuestions: number;
  totalPoints: number;
  startedAt: string;
  mustCompleteBy?: string;
  sections: TestSectionWithQuestionsDto[];
}

export interface TestSectionWithQuestionsDto {
  id: string;
  title: string;
  description?: string;
  displayOrder: number;
  timeLimitMinutes?: number;
  questions: QuestionForTestDto[];
}

export interface QuestionForTestDto {
  id: string;
  questionNumber: number;
  type: QuestionType;
  typeName: string;
  content: string;
  codeSnippet?: string;
  mediaUrl?: string;
  points: number;
  timeLimitSeconds?: number;
  skillName: string;
  options: OptionForTestDto[];
  // Existing answer (for continue assessment)
  selectedOptionIds?: string[];
  textResponse?: string;
  codeResponse?: string;
}

export interface OptionForTestDto {
  id: string;
  content: string;
  displayOrder: number;
}

// Submit Answer
export interface SubmitAnswerRequest {
  assessmentId: string;
  questionId: string;
  textResponse?: string;
  codeResponse?: string;
  selectedOptionIds?: string[];
  timeSpentSeconds?: number;
}

export interface SubmitAnswerResponse {
  success: boolean;
  responseId: string;
  isCorrect?: boolean;
  pointsAwarded?: number;
  feedback?: string;
}

// Assessment Result
export interface AssessmentResultDto {
  assessmentId: string;
  title: string;
  status: AssessmentStatus;
  statusName: string;
  totalScore: number;
  maxScore: number;
  percentage: number;
  passed: boolean;
  passingScore: number;
  startedAt: string;
  completedAt: string;
  totalTimeMinutes: number;
  totalQuestions: number;
  correctAnswers: number;
  wrongAnswers: number;
  unansweredQuestions: number;
  pendingReviewQuestions: number;
  skillResults: SkillResultDto[];
  questionResults: QuestionResultDto[];
}

export interface SkillResultDto {
  skillId: string;
  skillName: string;
  skillCode: string;
  correctAnswers: number;
  totalQuestions: number;
  score: number;
  maxScore: number;
  percentage: number;
}

export interface QuestionResultDto {
  questionId: string;
  questionNumber: number;
  content: string;
  codeSnippet?: string;
  type: QuestionType;
  typeName: string;
  skillName: string;
  points: number;
  userAnswer?: string;
  selectedOptionIds?: string[];
  correctAnswer?: string;
  correctOptionIds?: string[];
  isCorrect?: boolean;
  pointsAwarded?: number;
  explanation?: string;
  aiFeedback?: AiFeedbackDto;
  options: OptionResultDto[];
}

export interface AiFeedbackDto {
  feedback: string;
  strengthPoints: string[];
  improvementAreas: string[];
  detailedAnalysis?: string;
}

export interface OptionResultDto {
  id: string;
  content: string;
  isCorrect: boolean;
  wasSelected: boolean;
  explanation?: string;
}

// System Enum Configuration
export interface SystemEnumValueDto {
  id: string;
  enumType: string;
  value: number;
  code: string;
  name: string;
  description?: string;
  color?: string;
  icon?: string;
  displayOrder: number;
  isActive: boolean;
  isSystem: boolean;
  metadata?: string;
}

export interface CreateSystemEnumValueDto {
  enumType: string;
  value: number;
  code: string;
  name: string;
  description?: string;
  color?: string;
  icon?: string;
  displayOrder?: number;
  metadata?: string;
}

export interface UpdateSystemEnumValueDto {
  name: string;
  description?: string;
  color?: string;
  icon?: string;
  displayOrder?: number;
  metadata?: string;
}

export interface EnumTypeDto {
  enumType: string;
  displayName: string;
  description?: string;
  valueCount: number;
  values: SystemEnumValueDto[];
}

export interface EnumDropdownItemDto {
  value: number;
  code: string;
  label: string;
  color?: string;
  icon?: string;
}

export interface ReorderEnumValuesDto {
  enumType: string;
  orderedIds: string[];
}
