import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Card,
  Typography,
  Descriptions,
  Tag,
  Space,
  Button,
  Collapse,
  List,
  message,
  Modal,
  Form,
  Input,
  InputNumber,
  Select,
  Spin,
  Empty,
  Popconfirm,
  Radio,
  Checkbox,
} from 'antd';
import {
  ArrowLeftOutlined,
  PlusOutlined,
  RobotOutlined,
  EditOutlined,
  DeleteOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ThunderboltOutlined,
} from '@ant-design/icons';
import {
  getTestTemplate,
  createTestSection,
  updateTestSection,
  deleteTestSection,
  generateAiQuestions,
  createQuestion,
  updateQuestion,
  deleteQuestion,
} from '../../api/testTemplates';
import { getSkills } from '../../api/taxonomy';
import { systemEnumApi } from '../../api/systemEnums';
import type {
  TestSectionDto,
  CreateTestSectionDto,
  UpdateTestSectionDto,
  QuestionDto,
  CreateQuestionDto,
  UpdateQuestionDto,
  GenerateAiQuestionsRequest,
  SkillListDto,
  EnumDropdownItemDto,
} from '../../types';
import { ProficiencyLevel } from '../../types';
import QuestionFormModal from '../../components/QuestionFormModal';
import './TestTemplates.css';

const { Title, Text } = Typography;
const { Panel } = Collapse;

const proficiencyOptions = [
  { value: ProficiencyLevel.Follow, label: 'Level 1 - Follow' },
  { value: ProficiencyLevel.Assist, label: 'Level 2 - Assist' },
  { value: ProficiencyLevel.Apply, label: 'Level 3 - Apply' },
  { value: ProficiencyLevel.Enable, label: 'Level 4 - Enable' },
  { value: ProficiencyLevel.EnsureAdvise, label: 'Level 5 - Ensure/Advise' },
  { value: ProficiencyLevel.Initiate, label: 'Level 6 - Initiate' },
  { value: ProficiencyLevel.SetStrategy, label: 'Level 7 - Set Strategy' },
];

export default function TestTemplateDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [sectionModalOpen, setSectionModalOpen] = useState(false);
  const [editingSection, setEditingSection] = useState<TestSectionDto | null>(
    null
  );
  const [aiModalOpen, setAiModalOpen] = useState(false);
  const [selectedSectionId, setSelectedSectionId] = useState<string>('');
  const [questionModalOpen, setQuestionModalOpen] = useState(false);
  const [editingQuestion, setEditingQuestion] = useState<QuestionDto | null>(null);
  const [sectionForm] = Form.useForm();
  const [aiForm] = Form.useForm();

  // Fetch template detail
  const {
    data: template,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['testTemplate', id],
    queryFn: () => getTestTemplate(id!),
    enabled: !!id,
  });

  // Fetch skills for AI generation
  const { data: skillsData } = useQuery({
    queryKey: ['skills-dropdown'],
    queryFn: () => getSkills({ pageSize: 100 }),
  });

  // Fetch QuestionTypes from SystemEnum
  const { data: questionTypeOptions = [] } = useQuery({
    queryKey: ['enum-dropdown', 'QuestionType'],
    queryFn: () => systemEnumApi.getDropdown('QuestionType'),
    select: (data: EnumDropdownItemDto[]) =>
      data.map((item) => ({
        value: item.value,
        label: item.label,
      })),
  });

  // Fetch DifficultyLevel from SystemEnum
  const { data: difficultyOptions = [] } = useQuery({
    queryKey: ['enum-dropdown', 'DifficultyLevel'],
    queryFn: () => systemEnumApi.getDropdown('DifficultyLevel'),
    select: (data: EnumDropdownItemDto[]) =>
      data.map((item) => ({
        value: item.value,
        label: item.label,
      })),
  });

  // Mutations
  const createSectionMutation = useMutation({
    mutationFn: createTestSection,
    onSuccess: () => {
      message.success('Section created successfully');
      queryClient.invalidateQueries({ queryKey: ['testTemplate', id] });
      setSectionModalOpen(false);
      sectionForm.resetFields();
    },
    onError: () => message.error('Failed to create section'),
  });

  const updateSectionMutation = useMutation({
    mutationFn: ({
      sectionId,
      data,
    }: {
      sectionId: string;
      data: UpdateTestSectionDto;
    }) => updateTestSection(sectionId, data),
    onSuccess: () => {
      message.success('Section updated successfully');
      queryClient.invalidateQueries({ queryKey: ['testTemplate', id] });
      setSectionModalOpen(false);
      setEditingSection(null);
      sectionForm.resetFields();
    },
    onError: () => message.error('Failed to update section'),
  });

  const deleteSectionMutation = useMutation({
    mutationFn: deleteTestSection,
    onSuccess: () => {
      message.success('Section deleted successfully');
      queryClient.invalidateQueries({ queryKey: ['testTemplate', id] });
    },
    onError: () => message.error('Failed to delete section'),
  });

  const generateAiMutation = useMutation({
    mutationFn: generateAiQuestions,
    onSuccess: (data) => {
      message.success(`Generated ${data.length} questions successfully`);
      queryClient.invalidateQueries({ queryKey: ['testTemplate', id] });
      setAiModalOpen(false);
      aiForm.resetFields();
    },
    onError: () => message.error('Failed to generate questions'),
  });

  // Question mutations
  const createQuestionMutation = useMutation({
    mutationFn: createQuestion,
    onSuccess: () => {
      message.success('Question created successfully');
      queryClient.invalidateQueries({ queryKey: ['testTemplate', id] });
      setQuestionModalOpen(false);
      setEditingQuestion(null);
    },
    onError: () => message.error('Failed to create question'),
  });

  const updateQuestionMutation = useMutation({
    mutationFn: ({ questionId, data }: { questionId: string; data: UpdateQuestionDto }) =>
      updateQuestion(questionId, data),
    onSuccess: () => {
      message.success('Question updated successfully');
      queryClient.invalidateQueries({ queryKey: ['testTemplate', id] });
      setQuestionModalOpen(false);
      setEditingQuestion(null);
    },
    onError: () => message.error('Failed to update question'),
  });

  const deleteQuestionMutation = useMutation({
    mutationFn: deleteQuestion,
    onSuccess: () => {
      message.success('Question deleted successfully');
      queryClient.invalidateQueries({ queryKey: ['testTemplate', id] });
    },
    onError: () => message.error('Failed to delete question'),
  });

  const handleSectionSubmit = (values: CreateTestSectionDto) => {
    if (editingSection) {
      updateSectionMutation.mutate({
        sectionId: editingSection.id,
        data: values,
      });
    } else {
      createSectionMutation.mutate({
        ...values,
        testTemplateId: id!,
      });
    }
  };

  const handleEditSection = (section: TestSectionDto) => {
    setEditingSection(section);
    sectionForm.setFieldsValue({
      title: section.title,
      description: section.description,
      displayOrder: section.displayOrder,
      timeLimitMinutes: section.timeLimitMinutes,
    });
    setSectionModalOpen(true);
  };

  const handleOpenAiModal = (sectionId: string) => {
    setSelectedSectionId(sectionId);
    aiForm.setFieldsValue({
      questionCount: 5,
      language: 'en',
    });
    setAiModalOpen(true);
  };

  const handleAiGenerate = (values: Omit<GenerateAiQuestionsRequest, 'sectionId'>) => {
    // Get skill name and code if skillId is selected
    let skillName: string | undefined;
    let skillCode: string | undefined;

    if (values.skillId && skillsData?.data?.items) {
      const selectedSkill = skillsData.data.items.find(
        (s: SkillListDto) => s.id === values.skillId
      );
      if (selectedSkill) {
        skillName = selectedSkill.name;
        skillCode = selectedSkill.code;
      }
    }

    generateAiMutation.mutate({
      ...values,
      skillName,
      skillCode,
      sectionId: selectedSectionId,
    });
  };

  const handleOpenAddQuestion = (sectionId: string) => {
    setSelectedSectionId(sectionId);
    setEditingQuestion(null);
    setQuestionModalOpen(true);
  };

  const handleEditQuestion = (question: QuestionDto, sectionId: string) => {
    setSelectedSectionId(sectionId);
    setEditingQuestion(question);
    setQuestionModalOpen(true);
  };

  const handleQuestionSubmit = (values: CreateQuestionDto) => {
    if (editingQuestion) {
      updateQuestionMutation.mutate({
        questionId: editingQuestion.id,
        data: {
          ...values,
          isActive: editingQuestion.isActive,
        } as UpdateQuestionDto,
      });
    } else {
      createQuestionMutation.mutate(values);
    }
  };

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
      </div>
    );
  }

  if (error || !template) {
    return (
      <Card>
        <Empty description="Template not found" />
        <div style={{ textAlign: 'center', marginTop: 16 }}>
          <Button onClick={() => navigate('/tests/templates')}>
            Back to Templates
          </Button>
        </div>
      </Card>
    );
  }

  return (
    <div className="test-template-detail-container">
      <Space style={{ marginBottom: 16 }}>
        <Button
          icon={<ArrowLeftOutlined />}
          onClick={() => navigate('/tests/templates')}
        >
          Back
        </Button>
      </Space>

      {/* Template Info */}
      <Card className="template-info-card">
        <Space direction="vertical" style={{ width: '100%' }}>
          <div className="template-header">
            <div className="template-tags">
              <Title level={3} className="template-title">
                {template.title}
              </Title>
              {template.isAiGenerated && (
                <Tag color="purple" icon={<RobotOutlined />}>
                  AI Generated
                </Tag>
              )}
              <Tag color={template.isActive ? 'green' : 'red'}>
                {template.isActive ? 'Active' : 'Inactive'}
              </Tag>
            </div>
          </div>

          {template.description && (
            <Text type="secondary">{template.description}</Text>
          )}

          <Descriptions bordered size="small" column={4}>
            <Descriptions.Item label="Type">
              <Tag color="blue">{template.typeName}</Tag>
            </Descriptions.Item>
            <Descriptions.Item label="Target Skill">
              {template.targetSkillName || '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Time Limit">
              {template.timeLimitMinutes
                ? `${template.timeLimitMinutes} minutes`
                : 'No limit'}
            </Descriptions.Item>
            <Descriptions.Item label="Pass Score">
              {template.passingScore}%
            </Descriptions.Item>
            <Descriptions.Item label="Total Sections">
              {template.sectionCount}
            </Descriptions.Item>
            <Descriptions.Item label="Total Questions">
              {template.questionCount}
            </Descriptions.Item>
            <Descriptions.Item label="Randomized">
              {template.isRandomized ? 'Yes' : 'No'}
            </Descriptions.Item>
            <Descriptions.Item label="Requires Review">
              {template.requiresReview ? 'Yes' : 'No'}
            </Descriptions.Item>
          </Descriptions>
        </Space>
      </Card>

      {/* Sections */}
      <Card
        className="sections-card"
        title="Sections & Questions"
        extra={
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => {
              setEditingSection(null);
              sectionForm.resetFields();
              sectionForm.setFieldsValue({
                displayOrder: (template.sections?.length || 0) + 1,
              });
              setSectionModalOpen(true);
            }}
          >
            Add Section
          </Button>
        }
      >
        {template.sections?.length === 0 ? (
          <Empty description="No sections yet. Add a section to start adding questions." />
        ) : (
          <Collapse accordion>
            {template.sections?.map((section: TestSectionDto) => (
              <Panel
                header={
                  <Space>
                    <span>
                      {section.displayOrder}. {section.title}
                    </span>
                    <Tag>{section.questionCount} questions</Tag>
                    {section.timeLimitMinutes && (
                      <Tag color="orange">{section.timeLimitMinutes} min</Tag>
                    )}
                  </Space>
                }
                key={section.id}
                extra={
                  <Space onClick={(e) => e.stopPropagation()}>
                    <Button
                      type="primary"
                      size="small"
                      icon={<RobotOutlined />}
                      onClick={(e) => {
                        e.stopPropagation();
                        handleOpenAiModal(section.id);
                      }}
                    >
                      AI Generate
                    </Button>
                    <Button
                      type="link"
                      size="small"
                      icon={<EditOutlined />}
                      onClick={(e) => {
                        e.stopPropagation();
                        handleEditSection(section);
                      }}
                    />
                    <Popconfirm
                      title="Delete this section?"
                      description="All questions in this section will also be deleted."
                      onConfirm={(e) => {
                        e?.stopPropagation();
                        deleteSectionMutation.mutate(section.id);
                      }}
                      onCancel={(e) => e?.stopPropagation()}
                    >
                      <Button
                        type="link"
                        danger
                        size="small"
                        icon={<DeleteOutlined />}
                        onClick={(e) => e.stopPropagation()}
                      />
                    </Popconfirm>
                  </Space>
                }
              >
                {section.description && (
                  <Text type="secondary" style={{ display: 'block', marginBottom: 16 }}>
                    {section.description}
                  </Text>
                )}

                {section.questions?.length === 0 ? (
                  <Empty
                    description="No questions yet"
                    image={Empty.PRESENTED_IMAGE_SIMPLE}
                  >
                    <Space>
                      <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={() => handleOpenAddQuestion(section.id)}
                      >
                        Add Question
                      </Button>
                      <Button
                        icon={<RobotOutlined />}
                        onClick={() => handleOpenAiModal(section.id)}
                      >
                        Generate with AI
                      </Button>
                    </Space>
                  </Empty>
                ) : (
                  <>
                    <div style={{ marginBottom: 16 }}>
                      <Button
                        type="primary"
                        icon={<PlusOutlined />}
                        onClick={() => handleOpenAddQuestion(section.id)}
                      >
                        Add Question
                      </Button>
                    </div>
                    <List
                      dataSource={section.questions}
                      renderItem={(question: QuestionDto, index: number) => (
                        <List.Item
                          actions={[
                            <Button
                              key="edit"
                              type="link"
                              size="small"
                              icon={<EditOutlined />}
                              onClick={() => handleEditQuestion(question, section.id)}
                            >
                              Edit
                            </Button>,
                            <Popconfirm
                              key="delete"
                              title="Delete this question?"
                              description="This action cannot be undone."
                              onConfirm={() => deleteQuestionMutation.mutate(question.id)}
                            >
                              <Button
                                type="link"
                                danger
                                size="small"
                                icon={<DeleteOutlined />}
                              >
                                Delete
                              </Button>
                            </Popconfirm>,
                          ]}
                        >
                          <List.Item.Meta
                            avatar={
                              <div
                                style={{
                                  width: 32,
                                  height: 32,
                                  borderRadius: '50%',
                                  background: '#1890ff',
                                  color: '#fff',
                                  display: 'flex',
                                  alignItems: 'center',
                                  justifyContent: 'center',
                                  fontWeight: 'bold',
                                }}
                              >
                                {index + 1}
                              </div>
                            }
                            title={
                              <Space>
                                <span>{question.content}</span>
                                {question.isAiGenerated && (
                                  <Tag color="purple" icon={<RobotOutlined />}>
                                    AI
                                  </Tag>
                                )}
                              </Space>
                            }
                            description={
                              <Space wrap>
                                <Tag color="cyan">{question.typeName}</Tag>
                                <Tag color="orange">{question.difficultyName}</Tag>
                                {question.skillName && <Tag>{question.skillName}</Tag>}
                                <Tag color="geekblue">
                                  Level {question.targetLevel}
                                </Tag>
                                <Text type="secondary">
                                  {question.points} pts
                                </Text>
                              </Space>
                            }
                          />
                          {question.options?.length > 0 && (
                            <div style={{ marginLeft: 48 }}>
                              {question.options.map((opt) => (
                                <div key={opt.id} style={{ marginBottom: 4 }}>
                                  {opt.isCorrect ? (
                                    <CheckCircleOutlined
                                      style={{ color: '#52c41a', marginRight: 8 }}
                                    />
                                  ) : (
                                    <CloseCircleOutlined
                                      style={{ color: '#ff4d4f', marginRight: 8 }}
                                    />
                                  )}
                                  <Text
                                    style={{
                                      color: opt.isCorrect ? '#52c41a' : undefined,
                                    }}
                                  >
                                    {opt.content}
                                  </Text>
                                </div>
                              ))}
                            </div>
                          )}
                        </List.Item>
                      )}
                    />
                  </>
                )}
              </Panel>
            ))}
          </Collapse>
        )}
      </Card>

      {/* Section Modal */}
      <Modal
        title={editingSection ? 'Edit Section' : 'Create Section'}
        open={sectionModalOpen}
        onCancel={() => {
          setSectionModalOpen(false);
          setEditingSection(null);
          sectionForm.resetFields();
        }}
        footer={null}
      >
        <Form
          form={sectionForm}
          layout="vertical"
          onFinish={handleSectionSubmit}
        >
          <Form.Item
            name="title"
            label="Section Title"
            rules={[{ required: true, message: 'Please enter section title' }]}
          >
            <Input placeholder="e.g., C# Fundamentals" />
          </Form.Item>

          <Form.Item name="description" label="Description">
            <Input.TextArea rows={2} placeholder="Section description" />
          </Form.Item>

          <Space style={{ width: '100%' }}>
            <Form.Item
              name="displayOrder"
              label="Display Order"
              rules={[{ required: true }]}
              style={{ flex: 1 }}
            >
              <InputNumber min={1} style={{ width: '100%' }} />
            </Form.Item>

            <Form.Item
              name="timeLimitMinutes"
              label="Time Limit (min)"
              style={{ flex: 1 }}
            >
              <InputNumber min={1} style={{ width: '100%' }} />
            </Form.Item>
          </Space>

          <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
            <Space>
              <Button onClick={() => setSectionModalOpen(false)}>Cancel</Button>
              <Button
                type="primary"
                htmlType="submit"
                loading={
                  createSectionMutation.isPending ||
                  updateSectionMutation.isPending
                }
              >
                {editingSection ? 'Update' : 'Create'}
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>

      {/* AI Generation Modal */}
      <Modal
        title={
          <Space>
            <ThunderboltOutlined style={{ color: '#722ed1' }} />
            Generate Questions with AI
          </Space>
        }
        open={aiModalOpen}
        onCancel={() => {
          setAiModalOpen(false);
          aiForm.resetFields();
        }}
        footer={null}
        width={600}
      >
        <Form
          form={aiForm}
          layout="vertical"
          onFinish={handleAiGenerate}
          initialValues={{
            questionCount: 5,
            language: 'en',
          }}
        >
          {/* Required Fields */}
          <Form.Item
            name="questionTypes"
            label="Question Types"
            rules={[
              {
                required: true,
                message: 'Select at least one question type',
              },
            ]}
            tooltip="Required: Select the types of questions to generate"
          >
            <Checkbox.Group options={questionTypeOptions} />
          </Form.Item>

          <Form.Item
            name="language"
            label="Language"
            rules={[{ required: true, message: 'Please select a language' }]}
            tooltip="Required: Language for generated questions"
          >
            <Radio.Group>
              <Radio value="en">English</Radio>
              <Radio value="vi">Vietnamese</Radio>
            </Radio.Group>
          </Form.Item>

          <Form.Item
            name="questionCount"
            label="Number of Questions"
            rules={[{ required: true, message: 'Please enter number of questions' }]}
            tooltip="Required: How many questions to generate"
          >
            <InputNumber
              min={1}
              max={20}
              style={{ width: '100%' }}
              placeholder="5"
            />
          </Form.Item>

          {/* Optional Fields */}
          <Form.Item
            name="skillId"
            label="Skill"
            tooltip="Optional: Select a specific skill to focus questions on"
          >
            <Select
              placeholder="Select a skill (optional)"
              options={skillsData?.data?.items?.map((s: SkillListDto) => ({
                value: s.id,
                label: `${s.code} - ${s.name}`,
              }))}
              showSearch
              optionFilterProp="label"
              allowClear
            />
          </Form.Item>

          <Form.Item
            name="targetLevel"
            label="Target Proficiency Level"
            tooltip="Optional: Specify difficulty based on proficiency level"
          >
            <Select
              placeholder="Auto-detect (optional)"
              options={proficiencyOptions}
              allowClear
            />
          </Form.Item>

          <Form.Item
            name="difficulty"
            label="Difficulty"
            tooltip="Optional: Specific difficulty level"
          >
            <Select
              allowClear
              placeholder="Auto-detect based on level (optional)"
              options={difficultyOptions}
            />
          </Form.Item>

          <Form.Item
            name="additionalContext"
            label="Additional Context"
            tooltip="Optional: Provide extra context for AI"
          >
            <Input.TextArea
              rows={2}
              placeholder="e.g., Focus on .NET Core 8 features, include code examples..."
            />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
            <Space>
              <Button onClick={() => setAiModalOpen(false)}>Cancel</Button>
              <Button
                type="primary"
                htmlType="submit"
                loading={generateAiMutation.isPending}
                icon={<RobotOutlined />}
              >
                Generate Questions
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>

      {/* Question Form Modal */}
      <QuestionFormModal
        open={questionModalOpen}
        onCancel={() => {
          setQuestionModalOpen(false);
          setEditingQuestion(null);
        }}
        onSubmit={handleQuestionSubmit}
        loading={createQuestionMutation.isPending || updateQuestionMutation.isPending}
        editingQuestion={editingQuestion}
        sectionId={selectedSectionId}
        skills={skillsData?.data?.items}
        questionTypeOptions={questionTypeOptions}
        difficultyOptions={difficultyOptions}
      />
    </div>
  );
}
