import { useState } from 'react';
import {
  Collapse,
  Typography,
  Descriptions,
  Tag,
  Button,
  Space,
  Modal,
  Form,
  Input,
  Select,
  Dropdown,
  message,
  Spin,
  Tooltip,
  Popconfirm,
  Divider,
} from 'antd';
import {
  EditOutlined,
  SaveOutlined,
  CloseOutlined,
  InfoCircleOutlined,
  DeleteOutlined,
  PlusOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { skillApi, skillLevelApi, levelDefinitionApi } from '../../api/taxonomy';
import type { SkillLevelDefinitionDto, CreateSkillLevelDefinitionDto, LevelDefinitionDto } from '../../types';

const { Text } = Typography;
const { TextArea } = Input;

// Default colors for proficiency levels, generates random color for levels > 7
const defaultLevelColors: Record<number, string> = {
  1: '#87d068',  // Follow - green
  2: '#52c41a',  // Assist
  3: '#1890ff',  // Apply - blue
  4: '#722ed1',  // Enable - purple
  5: '#eb2f96',  // Ensure/Advise - pink
  6: '#fa541c',  // Initiate - orange
  7: '#f5222d',  // Set Strategy - red
};

const getLevelColor = (level: number): string => {
  if (defaultLevelColors[level]) return defaultLevelColors[level];
  // Generate consistent color based on level number for custom levels
  const colors = ['#13c2c2', '#2f54eb', '#faad14', '#a0d911', '#eb2f96', '#722ed1', '#fa541c'];
  return colors[(level - 1) % colors.length];
};

interface SkillLevelEditorProps {
  skillId: string;
  skillName: string;
  applicableLevelsString?: string;
}

export default function SkillLevelEditor({ skillId, skillName, applicableLevelsString }: SkillLevelEditorProps) {
  const queryClient = useQueryClient();
  const [editingLevel, setEditingLevel] = useState<SkillLevelDefinitionDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const [form] = Form.useForm();

  // Parse applicable levels from applicableLevelsString
  const applicableLevels = applicableLevelsString
    ? applicableLevelsString.split(',').map(s => parseInt(s.trim())).filter(n => n >= 1)
    : [];

  // Fetch skill details with level definitions
  const { data: skillData, isLoading } = useQuery({
    queryKey: ['skill', skillId],
    queryFn: () => skillApi.getById(skillId),
    enabled: !!skillId,
  });

  // Fetch level definitions from database
  const { data: levelDefinitionsData } = useQuery({
    queryKey: ['levelDefinitions'],
    queryFn: levelDefinitionApi.getAll,
  });

  const createMutation = useMutation({
    mutationFn: skillLevelApi.create,
    onSuccess: () => {
      message.success('Level definition created successfully');
      queryClient.invalidateQueries({ queryKey: ['skill', skillId] });
      handleCloseModal();
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to create level definition');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: CreateSkillLevelDefinitionDto }) =>
      skillLevelApi.update(id, data),
    onSuccess: () => {
      message.success('Level definition updated successfully');
      queryClient.invalidateQueries({ queryKey: ['skill', skillId] });
      handleCloseModal();
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to update level definition');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: skillLevelApi.delete,
    onSuccess: () => {
      message.success('Level definition deleted successfully');
      queryClient.invalidateQueries({ queryKey: ['skill', skillId] });
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to delete level definition');
    },
  });

  const skill = skillData?.data;
  const levelDefinitions = skill?.levelDefinitions || [];

  // Get all levels to display: combine applicableLevels and any defined levels
  const definedLevels = levelDefinitions.map(ld => ld.level);
  const allDisplayLevels = [...new Set([...applicableLevels, ...definedLevels])].sort((a, b) => a - b);

  const getLevelReference = (level: number): LevelDefinitionDto | undefined => {
    return levelDefinitionsData?.find(l => l.level === level);
  };

  // Get color from level definition or use default
  const getLevelColorFromDb = (level: number): string => {
    const levelRef = getLevelReference(level);
    if (levelRef?.color) return levelRef.color;
    return getLevelColor(level);  // fallback to default color
  };

  const getLevelDefinition = (level: number) => {
    return levelDefinitions.find(ld => ld.level === level);
  };

  const handleAddLevel = (level: number) => {
    const levelRef = level > 0 ? getLevelReference(level) : null;
    setIsCreating(true);
    setEditingLevel(null);
    form.setFieldsValue({
      level: level > 0 ? level : undefined,
      customLevelName: '',  // Empty to use default level name
      description: levelRef?.description || '',
      autonomy: levelRef?.autonomy || '',
      influence: levelRef?.influence || '',
      complexity: levelRef?.complexity || '',
      businessSkills: levelRef?.businessSkills || '',
      knowledge: levelRef?.knowledge || '',
      behavioralIndicators: levelRef?.behavioralIndicators?.join('\n') || '',
      evidenceExamples: '',
    });
    setIsModalOpen(true);
  };

  const handleEditLevel = (definition: SkillLevelDefinitionDto) => {
    setIsCreating(false);
    setEditingLevel(definition);
    form.setFieldsValue({
      level: definition.level,
      customLevelName: definition.customLevelName || '',
      description: definition.description,
      autonomy: definition.autonomy,
      influence: definition.influence,
      complexity: definition.complexity,
      businessSkills: definition.businessSkills,
      knowledge: definition.knowledge,
      behavioralIndicators: definition.behavioralIndicators?.join('\n'),
      evidenceExamples: definition.evidenceExamples?.join('\n'),
    });
    setIsModalOpen(true);
  };

  const handleDeleteLevel = (id: string) => {
    deleteMutation.mutate(id);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingLevel(null);
    setIsCreating(false);
    form.resetFields();
  };

  const handleSubmit = (values: any) => {
    const data: CreateSkillLevelDefinitionDto = {
      skillId,
      level: values.level,
      customLevelName: values.customLevelName?.trim() || undefined,
      description: values.description,
      autonomy: values.autonomy,
      influence: values.influence,
      complexity: values.complexity,
      businessSkills: values.businessSkills,
      knowledge: values.knowledge,
      behavioralIndicators: values.behavioralIndicators
        ? values.behavioralIndicators.split('\n').filter((s: string) => s.trim())
        : undefined,
      evidenceExamples: values.evidenceExamples
        ? values.evidenceExamples.split('\n').filter((s: string) => s.trim())
        : undefined,
    };

    if (isCreating) {
      createMutation.mutate(data);
    } else if (editingLevel) {
      updateMutation.mutate({ id: editingLevel.id, data });
    }
  };

  if (isLoading) {
    return (
      <div style={{ padding: 24, textAlign: 'center' }}>
        <Spin />
      </div>
    );
  }

  // No need to check applicableLevels.length === 0 since any level can be added

  return (
    <div style={{ padding: '12px 24px', background: '#fafafa' }}>
      {/* Header with Add New Level */}
      <div style={{ marginBottom: 12, display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 8 }}>
        <div>
          <Text type="secondary">
            Proficiency Levels for <Text strong>{skillName}</Text>
          </Text>
          <div style={{ marginTop: 8 }}>
            {allDisplayLevels.length > 0 ? allDisplayLevels.map(level => {
              const levelRef = getLevelReference(level);
              const definition = getLevelDefinition(level);
              const displayName = definition?.customLevelName || levelRef?.levelName || `Level ${level}`;
              const isStandard = applicableLevels.includes(level);
              return (
                <Tag
                  key={level}
                  color={definition ? getLevelColorFromDb(level) : 'default'}
                  style={{ marginBottom: 4 }}
                >
                  L{level}: {displayName}
                  {!definition && ' (-)'}
                  {!isStandard && ' *'}
                </Tag>
              );
            }) : (
              <Text type="secondary">No levels defined yet. Click "Add Level" to create one.</Text>
            )}
          </div>
        </div>

        {/* Add Level Dropdown - select from undefined levels */}
        {(() => {
          // Get levels that haven't been defined for this skill
          const undefinedLevels = levelDefinitionsData
            ?.filter(l => l.isActive && !getLevelDefinition(l.level))
            .sort((a, b) => a.level - b.level) || [];

          if (undefinedLevels.length === 0) {
            return (
              <Tooltip title="All levels have been defined">
                <Button type="primary" icon={<PlusOutlined />} disabled>
                  Add Level
                </Button>
              </Tooltip>
            );
          }

          return (
            <Dropdown
              menu={{
                items: undefinedLevels.map(l => ({
                  key: l.level.toString(),
                  label: `L${l.level}: ${l.levelName}`,
                  onClick: () => handleAddLevel(l.level),
                })),
              }}
              trigger={['click']}
            >
              <Button type="primary" icon={<PlusOutlined />}>
                Add Level
              </Button>
            </Dropdown>
          );
        })()}
      </div>

      <Divider style={{ margin: '8px 0' }} />

      <Collapse
        accordion
        size="small"
        items={allDisplayLevels.map(level => {
          const definition = getLevelDefinition(level);
          const levelRef = getLevelReference(level);
          const defaultLevelName = levelRef?.levelName || `Level ${level}`;
          // Use custom name if available, otherwise use default
          const displayName = definition?.customLevelName || definition?.displayLevelName || defaultLevelName;
          const isStandardLevel = applicableLevels.includes(level);

          return {
            key: level.toString(),
            label: (
              <Space>
                <Tag color={getLevelColorFromDb(level)}>L{level}</Tag>
                <Text strong>{displayName}</Text>
                {definition?.customLevelName && (
                  <Text type="secondary" style={{ fontSize: 12 }}>({defaultLevelName})</Text>
                )}
                {!isStandardLevel && <Tag color="blue">Custom</Tag>}
                {!definition && <Tag color="warning">No definition</Tag>}
              </Space>
            ),
            extra: (
              <Space size="small" onClick={(e) => e.stopPropagation()}>
                {definition ? (
                  <>
                    <Button
                      type="primary"
                      size="small"
                      icon={<EditOutlined />}
                      onClick={(e) => {
                        e.stopPropagation();
                        handleEditLevel(definition);
                      }}
                    >
                      Edit
                    </Button>
                    <Popconfirm
                      title="Delete level definition"
                      description="Are you sure you want to delete this level definition?"
                      onConfirm={(e) => {
                        e?.stopPropagation();
                        handleDeleteLevel(definition.id);
                      }}
                      onCancel={(e) => e?.stopPropagation()}
                      okText="Delete"
                      cancelText="Cancel"
                      okButtonProps={{ danger: true }}
                    >
                      <Button
                        size="small"
                        danger
                        icon={<DeleteOutlined />}
                        onClick={(e) => e.stopPropagation()}
                        loading={deleteMutation.isPending}
                      >
                        Delete
                      </Button>
                    </Popconfirm>
                  </>
                ) : (
                  <Button
                    type="primary"
                    size="small"
                    icon={<PlusOutlined />}
                    onClick={(e) => {
                      e.stopPropagation();
                      handleAddLevel(level);
                    }}
                  >
                    Add
                  </Button>
                )}
              </Space>
            ),
            children: definition ? (
              <Descriptions column={1} size="small" bordered>
                <Descriptions.Item label="Description">
                  {definition.description || <Text type="secondary">-</Text>}
                </Descriptions.Item>
                <Descriptions.Item label="Autonomy">
                  {definition.autonomy || <Text type="secondary">-</Text>}
                </Descriptions.Item>
                <Descriptions.Item label="Influence">
                  {definition.influence || <Text type="secondary">-</Text>}
                </Descriptions.Item>
                <Descriptions.Item label="Complexity">
                  {definition.complexity || <Text type="secondary">-</Text>}
                </Descriptions.Item>
                <Descriptions.Item label="Knowledge">
                  {definition.knowledge || <Text type="secondary">-</Text>}
                </Descriptions.Item>
                <Descriptions.Item label="Business Skills">
                  {definition.businessSkills || <Text type="secondary">-</Text>}
                </Descriptions.Item>
                {definition.behavioralIndicators && definition.behavioralIndicators.length > 0 && (
                  <Descriptions.Item label="Behavioral Indicators">
                    <ul style={{ margin: 0, paddingLeft: 20 }}>
                      {definition.behavioralIndicators.map((item, idx) => (
                        <li key={idx}>{item}</li>
                      ))}
                    </ul>
                  </Descriptions.Item>
                )}
                {definition.evidenceExamples && definition.evidenceExamples.length > 0 && (
                  <Descriptions.Item label="Evidence Examples">
                    <ul style={{ margin: 0, paddingLeft: 20 }}>
                      {definition.evidenceExamples.map((item, idx) => (
                        <li key={idx}>{item}</li>
                      ))}
                    </ul>
                  </Descriptions.Item>
                )}
              </Descriptions>
            ) : (
              <div style={{ padding: 16, textAlign: 'center' }}>
                <Text type="secondary">
                  No detailed definition for this level yet.
                </Text>
                <br />
                <Button
                  type="primary"
                  size="small"
                  icon={<PlusOutlined />}
                  style={{ marginTop: 8 }}
                  onClick={() => handleAddLevel(level)}
                >
                  Add Definition
                </Button>
              </div>
            ),
          };
        })}
      />

      <Modal
        title={
          <Space>
            {isCreating ? 'Add Level Definition' : 'Edit Level Definition'}
            <Tooltip title="This content will be used to assess and guide employees to achieve this level">
              <InfoCircleOutlined style={{ color: '#1890ff' }} />
            </Tooltip>
          </Space>
        }
        open={isModalOpen}
        onCancel={handleCloseModal}
        footer={null}
        width={700}
        destroyOnClose
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          {/* Display selected level - read only */}
          <Form.Item
            name="level"
            label="Level"
            rules={[{ required: true, message: 'Level is required' }]}
          >
            <Select
              disabled
              options={levelDefinitionsData
                ?.filter(l => l.isActive)
                .map(l => ({
                  value: l.level,
                  label: `L${l.level}: ${l.levelName}`,
                }))}
            />
          </Form.Item>

          <Form.Item
            name="customLevelName"
            label="Custom Level Name"
            tooltip="Leave empty to use default level name (e.g., Enable, Apply). Enter a custom name to override."
          >
            <Input placeholder="e.g., Junior Developer, Senior Engineer (leave empty for default level name)" />
          </Form.Item>

          <Form.Item
            name="description"
            label="Description"
            rules={[{ required: true, message: 'Please enter description' }]}
          >
            <TextArea rows={2} placeholder="Overall description of requirements for this level..." />
          </Form.Item>

          <Form.Item name="autonomy" label="Autonomy">
            <TextArea rows={2} placeholder="Describe the level of autonomy required..." />
          </Form.Item>

          <Form.Item name="influence" label="Influence">
            <TextArea rows={2} placeholder="Describe the scope of influence..." />
          </Form.Item>

          <Form.Item name="complexity" label="Complexity">
            <TextArea rows={2} placeholder="Describe the complexity of work handled..." />
          </Form.Item>

          <Form.Item name="knowledge" label="Knowledge">
            <TextArea rows={2} placeholder="Describe the required knowledge..." />
          </Form.Item>

          <Form.Item name="businessSkills" label="Business Skills">
            <TextArea rows={2} placeholder="Required business skills..." />
          </Form.Item>

          <Form.Item
            name="behavioralIndicators"
            label="Behavioral Indicators (one per line)"
          >
            <TextArea
              rows={4}
              placeholder="Example:&#10;Completes assigned tasks on time&#10;Reports progress clearly&#10;..."
            />
          </Form.Item>

          <Form.Item
            name="evidenceExamples"
            label="Evidence Examples (one per line)"
          >
            <TextArea
              rows={4}
              placeholder="Example evidence:&#10;Completed authentication module&#10;Fixed 10 bugs in sprint&#10;..."
            />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
            <Space>
              <Button onClick={handleCloseModal} icon={<CloseOutlined />}>
                Cancel
              </Button>
              <Button
                type="primary"
                htmlType="submit"
                icon={<SaveOutlined />}
                loading={createMutation.isPending || updateMutation.isPending}
              >
                {isCreating ? 'Create' : 'Update'}
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
