import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Table,
  Button,
  Space,
  Input,
  Switch,
  message,
  Popconfirm,
  Tag,
  Typography,
  Card,
  Modal,
  Form,
  InputNumber,
  Select,
  Checkbox,
} from 'antd';
import {
  PlusOutlined,
  SearchOutlined,
  EditOutlined,
  DeleteOutlined,
  RobotOutlined,
  EyeOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { Link } from 'react-router-dom';
import {
  getTestTemplates,
  createTestTemplate,
  updateTestTemplate,
  deleteTestTemplate,
  toggleTestTemplateActive,
} from '../../api/testTemplates';
import { getSkills } from '../../api/taxonomy';
import type {
  TestTemplateListDto,
  CreateTestTemplateDto,
  UpdateTestTemplateDto,
  SkillListDto,
} from '../../types';
import { AssessmentType } from '../../types';

const { Title } = Typography;
const { Search } = Input;

const assessmentTypeOptions = [
  { value: AssessmentType.SelfAssessment, label: 'Self Assessment' },
  { value: AssessmentType.ManagerAssessment, label: 'Manager Assessment' },
  { value: AssessmentType.PeerAssessment, label: 'Peer Assessment' },
  { value: AssessmentType.TechnicalTest, label: 'Technical Test' },
  { value: AssessmentType.Interview, label: 'Interview' },
  { value: AssessmentType.Custom, label: 'Custom' },
];

export default function TestTemplateList() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [includeInactive, setIncludeInactive] = useState(false);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingTemplate, setEditingTemplate] =
    useState<TestTemplateListDto | null>(null);
  const [form] = Form.useForm();

  // Fetch test templates
  const { data, isLoading } = useQuery({
    queryKey: ['testTemplates', pageNumber, pageSize, searchTerm, includeInactive],
    queryFn: () =>
      getTestTemplates({ pageNumber, pageSize, searchTerm, includeInactive }),
  });

  // Fetch skills for dropdown
  const { data: skillsData } = useQuery({
    queryKey: ['skills-dropdown'],
    queryFn: () => getSkills({ pageSize: 100 }),
  });

  // Mutations
  const createMutation = useMutation({
    mutationFn: createTestTemplate,
    onSuccess: () => {
      message.success('Test template created successfully');
      queryClient.invalidateQueries({ queryKey: ['testTemplates'] });
      setModalOpen(false);
      form.resetFields();
    },
    onError: () => message.error('Failed to create test template'),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateTestTemplateDto }) =>
      updateTestTemplate(id, data),
    onSuccess: () => {
      message.success('Test template updated successfully');
      queryClient.invalidateQueries({ queryKey: ['testTemplates'] });
      setModalOpen(false);
      setEditingTemplate(null);
      form.resetFields();
    },
    onError: () => message.error('Failed to update test template'),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteTestTemplate,
    onSuccess: () => {
      message.success('Test template deleted successfully');
      queryClient.invalidateQueries({ queryKey: ['testTemplates'] });
    },
    onError: () => message.error('Failed to delete test template'),
  });

  const toggleActiveMutation = useMutation({
    mutationFn: toggleTestTemplateActive,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['testTemplates'] });
    },
    onError: () => message.error('Failed to toggle active status'),
  });

  const handleSubmit = (values: CreateTestTemplateDto) => {
    if (editingTemplate) {
      updateMutation.mutate({
        id: editingTemplate.id,
        data: { ...values, isActive: editingTemplate.isActive },
      });
    } else {
      createMutation.mutate(values);
    }
  };

  const handleEdit = (record: TestTemplateListDto) => {
    setEditingTemplate(record);
    form.setFieldsValue({
      title: record.title,
      description: record.description,
      type: record.type,
      timeLimitMinutes: record.timeLimitMinutes,
      passingScore: record.passingScore,
      isRandomized: false,
      requiresReview: true,
    });
    setModalOpen(true);
  };

  const columns: ColumnsType<TestTemplateListDto> = [
    {
      title: 'Title',
      dataIndex: 'title',
      key: 'title',
      render: (text, record) => (
        <Link to={`/tests/templates/${record.id}`}>
          <Space>
            {text}
            {record.isAiGenerated && (
              <Tag color="purple" icon={<RobotOutlined />}>
                AI
              </Tag>
            )}
          </Space>
        </Link>
      ),
    },
    {
      title: 'Type',
      dataIndex: 'typeName',
      key: 'typeName',
      render: (text) => <Tag color="blue">{text}</Tag>,
    },
    {
      title: 'Target',
      key: 'target',
      render: (_, record) =>
        record.targetSkillName || record.targetJobRoleName || '-',
    },
    {
      title: 'Questions',
      dataIndex: 'questionCount',
      key: 'questionCount',
      align: 'center',
    },
    {
      title: 'Sections',
      dataIndex: 'sectionCount',
      key: 'sectionCount',
      align: 'center',
    },
    {
      title: 'Time (min)',
      dataIndex: 'timeLimitMinutes',
      key: 'timeLimitMinutes',
      align: 'center',
      render: (value) => value || '-',
    },
    {
      title: 'Pass Score',
      dataIndex: 'passingScore',
      key: 'passingScore',
      align: 'center',
      render: (value) => `${value}%`,
    },
    {
      title: 'Active',
      dataIndex: 'isActive',
      key: 'isActive',
      align: 'center',
      render: (isActive, record) => (
        <Switch
          checked={isActive}
          onChange={() => toggleActiveMutation.mutate(record.id)}
          loading={toggleActiveMutation.isPending}
        />
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 150,
      render: (_, record) => (
        <Space>
          <Button
            type="link"
            icon={<EyeOutlined />}
            onClick={() => window.location.href = `/tests/templates/${record.id}`}
          />
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
          />
          <Popconfirm
            title="Are you sure you want to delete this template?"
            onConfirm={() => deleteMutation.mutate(record.id)}
            okText="Yes"
            cancelText="No"
          >
            <Button type="link" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <Card>
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            marginBottom: 16,
          }}
        >
          <Title level={4} style={{ margin: 0 }}>
            Test Templates
          </Title>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => {
              setEditingTemplate(null);
              form.resetFields();
              form.setFieldsValue({
                passingScore: 70,
                type: AssessmentType.TechnicalTest,
                isRandomized: false,
                requiresReview: true,
              });
              setModalOpen(true);
            }}
          >
            New Template
          </Button>
        </div>

        <Space style={{ marginBottom: 16 }}>
          <Search
            placeholder="Search templates..."
            allowClear
            prefix={<SearchOutlined />}
            style={{ width: 300 }}
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            onSearch={() => setPageNumber(1)}
          />
          <span>Show inactive:</span>
          <Switch
            checked={includeInactive}
            onChange={setIncludeInactive}
          />
        </Space>

        <Table
          columns={columns}
          dataSource={data?.items}
          rowKey="id"
          loading={isLoading}
          pagination={{
            current: pageNumber,
            pageSize: pageSize,
            total: data?.totalCount,
            showSizeChanger: true,
            showTotal: (total) => `Total ${total} templates`,
            onChange: (page, size) => {
              setPageNumber(page);
              setPageSize(size);
            },
          }}
        />
      </Card>

      <Modal
        title={editingTemplate ? 'Edit Test Template' : 'Create Test Template'}
        open={modalOpen}
        onCancel={() => {
          setModalOpen(false);
          setEditingTemplate(null);
          form.resetFields();
        }}
        footer={null}
        width={600}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          initialValues={{
            passingScore: 70,
            type: AssessmentType.TechnicalTest,
            isRandomized: false,
            requiresReview: true,
          }}
        >
          <Form.Item
            name="title"
            label="Title"
            rules={[{ required: true, message: 'Please enter title' }]}
          >
            <Input placeholder="Enter template title" />
          </Form.Item>

          <Form.Item name="description" label="Description">
            <Input.TextArea rows={3} placeholder="Enter description" />
          </Form.Item>

          <Form.Item
            name="type"
            label="Assessment Type"
            rules={[{ required: true }]}
          >
            <Select options={assessmentTypeOptions} />
          </Form.Item>

          <Form.Item name="targetSkillId" label="Target Skill">
            <Select
              allowClear
              placeholder="Select a skill (optional)"
              options={skillsData?.data?.items?.map((s: SkillListDto) => ({
                value: s.id,
                label: `${s.code} - ${s.name}`,
              }))}
              showSearch
              optionFilterProp="label"
            />
          </Form.Item>

          <Space style={{ width: '100%' }}>
            <Form.Item
              name="timeLimitMinutes"
              label="Time Limit (minutes)"
              style={{ flex: 1 }}
            >
              <InputNumber min={1} max={300} style={{ width: '100%' }} />
            </Form.Item>

            <Form.Item
              name="passingScore"
              label="Passing Score (%)"
              rules={[{ required: true }]}
              style={{ flex: 1 }}
            >
              <InputNumber min={0} max={100} style={{ width: '100%' }} />
            </Form.Item>
          </Space>

          <Form.Item
            name="maxQuestions"
            label="Max Questions (if randomized)"
          >
            <InputNumber min={1} style={{ width: '100%' }} />
          </Form.Item>

          <Space>
            <Form.Item name="isRandomized" valuePropName="checked">
              <Checkbox>Randomize Questions</Checkbox>
            </Form.Item>

            <Form.Item name="requiresReview" valuePropName="checked">
              <Checkbox>Requires Review</Checkbox>
            </Form.Item>
          </Space>

          <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
            <Space>
              <Button onClick={() => setModalOpen(false)}>Cancel</Button>
              <Button
                type="primary"
                htmlType="submit"
                loading={createMutation.isPending || updateMutation.isPending}
              >
                {editingTemplate ? 'Update' : 'Create'}
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
