import { useState } from 'react';
import {
  Card,
  Table,
  Button,
  Space,
  Select,
  Typography,
  Tag,
  Modal,
  Form,
  Input,
  message,
  Popconfirm,
  Tooltip,
  Descriptions,
  Empty,
  Spin,
  Divider,
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  SaveOutlined,
  CloseOutlined,
  InfoCircleOutlined,
  ReloadOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { skillApi, skillLevelApi, skillDomainApi, skillSubcategoryApi, levelDefinitionApi } from '../../api/taxonomy';
import type { SkillLevelDefinitionDto, CreateSkillLevelDefinitionDto, ProficiencyLevel, LevelDefinitionDto } from '../../types';
import type { ColumnsType } from 'antd/es/table';

const { Title, Text } = Typography;
const { TextArea } = Input;

const levelColors: Record<number, string> = {
  1: '#87d068',  // Follow - green
  2: '#52c41a',  // Assist
  3: '#1890ff',  // Apply - blue
  4: '#722ed1',  // Enable - purple
  5: '#eb2f96',  // Ensure/Advise - pink
  6: '#fa541c',  // Initiate - orange
  7: '#f5222d',  // Set Strategy - red
};

export default function SkillLevelManagement() {
  const queryClient = useQueryClient();
  const [selectedDomainId, setSelectedDomainId] = useState<string | undefined>();
  const [selectedSubcategoryId, setSelectedSubcategoryId] = useState<string | undefined>();
  const [selectedSkillId, setSelectedSkillId] = useState<string | undefined>();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const [editingLevel, setEditingLevel] = useState<SkillLevelDefinitionDto | null>(null);
  const [selectedLevelForAdd, setSelectedLevelForAdd] = useState<number | null>(null);
  const [form] = Form.useForm();

  // Fetch dropdowns
  const { data: domainsData } = useQuery({
    queryKey: ['skillDomainsDropdown'],
    queryFn: skillDomainApi.getDropdown,
  });

  const { data: subcategoriesData } = useQuery({
    queryKey: ['skillSubcategoriesDropdown', selectedDomainId],
    queryFn: () => skillSubcategoryApi.getDropdown(selectedDomainId),
    enabled: !!selectedDomainId,
  });

  const { data: skillsData } = useQuery({
    queryKey: ['skills', { subcategoryId: selectedSubcategoryId, pageSize: 1000 }],
    queryFn: () => skillApi.getAll({ subcategoryId: selectedSubcategoryId, pageSize: 1000, pageNumber: 1 }),
    enabled: !!selectedSubcategoryId,
  });

  // Fetch selected skill details
  const { data: skillData, isLoading: skillLoading } = useQuery({
    queryKey: ['skill', selectedSkillId],
    queryFn: () => skillApi.getById(selectedSkillId!),
    enabled: !!selectedSkillId,
  });

  // Fetch level definitions from database
  const { data: levelDefinitionsData } = useQuery({
    queryKey: ['levelDefinitions'],
    queryFn: levelDefinitionApi.getAll,
  });

  const skill = skillData?.data;
  const levelDefinitions = skill?.levelDefinitions || [];
  const applicableLevels = skill?.applicableLevels || [];

  // Get levels that don't have definitions yet
  const undefinedLevels = applicableLevels.filter(
    level => !levelDefinitions.find(ld => ld.level === level)
  );

  const getLevelReference = (level: number): LevelDefinitionDto | undefined => {
    return levelDefinitionsData?.find(l => l.level === level);
  };

  // Mutations
  const createMutation = useMutation({
    mutationFn: skillLevelApi.create,
    onSuccess: () => {
      message.success('Level definition created successfully');
      queryClient.invalidateQueries({ queryKey: ['skill', selectedSkillId] });
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
      queryClient.invalidateQueries({ queryKey: ['skill', selectedSkillId] });
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
      queryClient.invalidateQueries({ queryKey: ['skill', selectedSkillId] });
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to delete level definition');
    },
  });

  const handleAddLevel = (level: number) => {
    const levelRef = getLevelReference(level);
    setIsCreating(true);
    setEditingLevel(null);
    setSelectedLevelForAdd(level);
    form.setFieldsValue({
      level,
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
    setSelectedLevelForAdd(null);
    form.setFieldsValue({
      level: definition.level,
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
    setSelectedLevelForAdd(null);
    form.resetFields();
  };

  const handleSubmit = (values: any) => {
    if (!selectedSkillId) return;

    const data: CreateSkillLevelDefinitionDto = {
      skillId: selectedSkillId,
      level: values.level as ProficiencyLevel,
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

  const columns: ColumnsType<SkillLevelDefinitionDto> = [
    {
      title: 'Level',
      dataIndex: 'level',
      key: 'level',
      width: 100,
      render: (level: number, record) => {
        const levelRef = getLevelReference(level);
        return (
          <Space direction="vertical" size={0}>
            <Tag color={levelColors[level]} style={{ fontSize: 14, padding: '2px 8px' }}>
              Level {level}
            </Tag>
            <Text type="secondary" style={{ fontSize: 12 }}>
              {levelRef?.levelName || record.levelName}
            </Text>
          </Space>
        );
      },
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
      render: (text: string) => (
        <Tooltip title={text}>
          <Text>{text || '-'}</Text>
        </Tooltip>
      ),
    },
    {
      title: 'Autonomy',
      dataIndex: 'autonomy',
      key: 'autonomy',
      width: 200,
      ellipsis: true,
      render: (text: string) => (
        <Tooltip title={text}>
          <Text>{text || '-'}</Text>
        </Tooltip>
      ),
    },
    {
      title: 'Complexity',
      dataIndex: 'complexity',
      key: 'complexity',
      width: 200,
      ellipsis: true,
      render: (text: string) => (
        <Tooltip title={text}>
          <Text>{text || '-'}</Text>
        </Tooltip>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      align: 'center',
      render: (_, record) => (
        <Space>
          <Tooltip title="Edit">
            <Button
              type="primary"
              size="small"
              icon={<EditOutlined />}
              onClick={() => handleEditLevel(record)}
            />
          </Tooltip>
          <Popconfirm
            title="Delete level definition"
            description="Are you sure you want to delete this level definition?"
            onConfirm={() => handleDeleteLevel(record.id)}
            okText="Delete"
            cancelText="Cancel"
            okButtonProps={{ danger: true }}
          >
            <Tooltip title="Delete">
              <Button
                size="small"
                danger
                icon={<DeleteOutlined />}
                loading={deleteMutation.isPending}
              />
            </Tooltip>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <Card>
      <div style={{ marginBottom: 16 }}>
        <Title level={4} style={{ margin: 0, marginBottom: 16 }}>
          Skill Level Definitions Management
        </Title>

        <Space wrap size="middle">
          <Select
            placeholder="Select Domain"
            style={{ width: 200 }}
            allowClear
            value={selectedDomainId}
            onChange={(value) => {
              setSelectedDomainId(value);
              setSelectedSubcategoryId(undefined);
              setSelectedSkillId(undefined);
            }}
            options={domainsData?.data?.map((d) => ({ label: d.name, value: d.id }))}
          />
          <Select
            placeholder="Select Subcategory"
            style={{ width: 250 }}
            allowClear
            value={selectedSubcategoryId}
            disabled={!selectedDomainId}
            onChange={(value) => {
              setSelectedSubcategoryId(value);
              setSelectedSkillId(undefined);
            }}
            options={subcategoriesData?.data?.map((s) => ({ label: s.fullName, value: s.id }))}
          />
          <Select
            placeholder="Select Skill"
            style={{ width: 300 }}
            allowClear
            showSearch
            optionFilterProp="label"
            value={selectedSkillId}
            disabled={!selectedSubcategoryId}
            onChange={setSelectedSkillId}
            options={skillsData?.data?.items?.map((s) => ({
              label: `${s.code} - ${s.name}`,
              value: s.id,
            }))}
          />
          {selectedSkillId && (
            <Button
              icon={<ReloadOutlined />}
              onClick={() => queryClient.invalidateQueries({ queryKey: ['skill', selectedSkillId] })}
            >
              Refresh
            </Button>
          )}
        </Space>
      </div>

      {!selectedSkillId ? (
        <Empty description="Please select a skill to manage its level definitions" />
      ) : skillLoading ? (
        <div style={{ textAlign: 'center', padding: 48 }}>
          <Spin size="large" />
        </div>
      ) : skill ? (
        <>
          {/* Skill Info */}
          <Card size="small" style={{ marginBottom: 16, background: '#fafafa' }}>
            <Descriptions column={4} size="small">
              <Descriptions.Item label="Skill Code">
                <Tag color="blue">{skill.code}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Name">
                <Text strong>{skill.name}</Text>
              </Descriptions.Item>
              <Descriptions.Item label="Category">
                <Tag>{skill.categoryName}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="Applicable Levels">
                <Space size={4}>
                  {applicableLevels.map(level => {
                    const hasDef = levelDefinitions.some(ld => ld.level === level);
                    return (
                      <Tag
                        key={level}
                        color={hasDef ? levelColors[level] : 'default'}
                      >
                        L{level}
                        {!hasDef && ' (-)'}
                      </Tag>
                    );
                  })}
                </Space>
              </Descriptions.Item>
            </Descriptions>
          </Card>

          {/* Add New Level Button */}
          {undefinedLevels.length > 0 && (
            <div style={{ marginBottom: 16 }}>
              <Space>
                <Text>Add level definition for:</Text>
                {undefinedLevels.map(level => {
                  const levelRef = getLevelReference(level);
                  return (
                    <Button
                      key={level}
                      type="primary"
                      size="small"
                      icon={<PlusOutlined />}
                      onClick={() => handleAddLevel(level)}
                    >
                      L{level} - {levelRef?.levelName || `Level ${level}`}
                    </Button>
                  );
                })}
              </Space>
            </div>
          )}

          {/* Level Definitions Table */}
          <Table
            columns={columns}
            dataSource={levelDefinitions}
            rowKey="id"
            size="small"
            pagination={false}
            expandable={{
              expandedRowRender: (record) => (
                <div style={{ padding: '8px 16px', background: '#fafafa' }}>
                  <Descriptions column={2} size="small" bordered>
                    <Descriptions.Item label="Influence" span={1}>
                      {record.influence || '-'}
                    </Descriptions.Item>
                    <Descriptions.Item label="Knowledge" span={1}>
                      {record.knowledge || '-'}
                    </Descriptions.Item>
                    <Descriptions.Item label="Business Skills" span={2}>
                      {record.businessSkills || '-'}
                    </Descriptions.Item>
                    {record.behavioralIndicators && record.behavioralIndicators.length > 0 && (
                      <Descriptions.Item label="Behavioral Indicators" span={2}>
                        <ul style={{ margin: 0, paddingLeft: 20 }}>
                          {record.behavioralIndicators.map((item, idx) => (
                            <li key={idx}>{item}</li>
                          ))}
                        </ul>
                      </Descriptions.Item>
                    )}
                    {record.evidenceExamples && record.evidenceExamples.length > 0 && (
                      <Descriptions.Item label="Evidence Examples" span={2}>
                        <ul style={{ margin: 0, paddingLeft: 20 }}>
                          {record.evidenceExamples.map((item, idx) => (
                            <li key={idx}>{item}</li>
                          ))}
                        </ul>
                      </Descriptions.Item>
                    )}
                  </Descriptions>
                </div>
              ),
            }}
            locale={{
              emptyText: (
                <Empty
                  description="No level definitions yet"
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                >
                  {undefinedLevels.length > 0 && (
                    <Button
                      type="primary"
                      icon={<PlusOutlined />}
                      onClick={() => handleAddLevel(undefinedLevels[0])}
                    >
                      Add First Level Definition
                    </Button>
                  )}
                </Empty>
              ),
            }}
          />
        </>
      ) : (
        <Empty description="Skill not found" />
      )}

      {/* Add/Edit Modal */}
      <Modal
        title={
          <Space>
            {isCreating ? 'Add Level Definition' : 'Edit Level Definition'}
            {(isCreating && selectedLevelForAdd) && (
              <Tag color={levelColors[selectedLevelForAdd]}>
                Level {selectedLevelForAdd} - {getLevelReference(selectedLevelForAdd)?.levelName}
              </Tag>
            )}
            {editingLevel && (
              <Tag color={levelColors[editingLevel.level]}>
                Level {editingLevel.level} - {editingLevel.levelName}
              </Tag>
            )}
            <Tooltip title="This content will be used to assess and guide employees to achieve this level">
              <InfoCircleOutlined style={{ color: '#1890ff' }} />
            </Tooltip>
          </Space>
        }
        open={isModalOpen}
        onCancel={handleCloseModal}
        footer={null}
        width={800}
        destroyOnClose
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Form.Item name="level" hidden>
            <Input />
          </Form.Item>

          <Form.Item
            name="description"
            label="Description"
            rules={[{ required: true, message: 'Please enter description' }]}
          >
            <TextArea rows={3} placeholder="Overall description of requirements for this level..." />
          </Form.Item>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
            <Form.Item name="autonomy" label="Autonomy">
              <TextArea rows={3} placeholder="Describe the level of autonomy required..." />
            </Form.Item>

            <Form.Item name="influence" label="Influence">
              <TextArea rows={3} placeholder="Describe the scope of influence..." />
            </Form.Item>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
            <Form.Item name="complexity" label="Complexity">
              <TextArea rows={3} placeholder="Describe the complexity of work handled..." />
            </Form.Item>

            <Form.Item name="knowledge" label="Knowledge">
              <TextArea rows={3} placeholder="Describe the required knowledge..." />
            </Form.Item>
          </div>

          <Form.Item name="businessSkills" label="Business Skills">
            <TextArea rows={2} placeholder="Required business skills..." />
          </Form.Item>

          <Divider />

          <Form.Item
            name="behavioralIndicators"
            label="Behavioral Indicators (one per line)"
          >
            <TextArea
              rows={4}
              placeholder="Example:&#10;Completes assigned tasks on time&#10;Reports progress clearly&#10;Follows coding standards&#10;..."
            />
          </Form.Item>

          <Form.Item
            name="evidenceExamples"
            label="Evidence Examples (one per line)"
          >
            <TextArea
              rows={4}
              placeholder="Example evidence:&#10;Completed authentication module&#10;Fixed 10 bugs in sprint&#10;Wrote unit tests for API&#10;..."
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
    </Card>
  );
}
