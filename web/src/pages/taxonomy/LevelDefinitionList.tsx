import { useState } from 'react';
import {
  Card,
  Table,
  Button,
  Space,
  Tag,
  Modal,
  Form,
  Input,
  InputNumber,
  message,
  Popconfirm,
  Typography,
  Switch,
  Descriptions,
  Collapse,
  ColorPicker,
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  ReloadOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { levelDefinitionApi } from '../../api/taxonomy';
import type { LevelDefinitionDto, CreateLevelDefinitionDto, UpdateLevelDefinitionDto } from '../../types';

const { Title, Text } = Typography;
const { TextArea } = Input;

export default function LevelDefinitionList() {
  const queryClient = useQueryClient();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingLevel, setEditingLevel] = useState<LevelDefinitionDto | null>(null);
  const [form] = Form.useForm();

  const { data: levels, isLoading, refetch } = useQuery({
    queryKey: ['levelDefinitions'],
    queryFn: levelDefinitionApi.getAll,
  });

  const createMutation = useMutation({
    mutationFn: levelDefinitionApi.create,
    onSuccess: () => {
      message.success('Level created successfully');
      queryClient.invalidateQueries({ queryKey: ['levelDefinitions'] });
      handleCloseModal();
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to create level');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateLevelDefinitionDto }) =>
      levelDefinitionApi.update(id, data),
    onSuccess: () => {
      message.success('Level updated successfully');
      queryClient.invalidateQueries({ queryKey: ['levelDefinitions'] });
      handleCloseModal();
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to update level');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: levelDefinitionApi.delete,
    onSuccess: () => {
      message.success('Level deleted successfully');
      queryClient.invalidateQueries({ queryKey: ['levelDefinitions'] });
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to delete level');
    },
  });

  const handleAdd = () => {
    setEditingLevel(null);
    form.resetFields();
    // Set default values
    const nextLevel = levels && levels.length > 0
      ? Math.max(...levels.map(l => l.level)) + 1
      : 1;
    form.setFieldsValue({
      level: nextLevel,
      displayOrder: nextLevel,
      color: '#1890ff',
    });
    setIsModalOpen(true);
  };

  const handleEdit = (record: LevelDefinitionDto) => {
    setEditingLevel(record);
    form.setFieldsValue({
      level: record.level,
      levelName: record.levelName,
      description: record.description,
      autonomy: record.autonomy,
      influence: record.influence,
      complexity: record.complexity,
      knowledge: record.knowledge,
      businessSkills: record.businessSkills,
      behavioralIndicators: record.behavioralIndicators?.join('\n'),
      color: record.color,
      displayOrder: record.displayOrder,
      isActive: record.isActive,
    });
    setIsModalOpen(true);
  };

  const handleDelete = (id: string) => {
    deleteMutation.mutate(id);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingLevel(null);
    form.resetFields();
  };

  const handleSubmit = (values: any) => {
    const data = {
      level: values.level,
      levelName: values.levelName,
      description: values.description,
      autonomy: values.autonomy,
      influence: values.influence,
      complexity: values.complexity,
      knowledge: values.knowledge,
      businessSkills: values.businessSkills,
      behavioralIndicators: values.behavioralIndicators
        ? values.behavioralIndicators.split('\n').filter((s: string) => s.trim())
        : undefined,
      color: typeof values.color === 'string' ? values.color : values.color?.toHexString?.() || values.color,
      displayOrder: values.displayOrder,
      isActive: values.isActive ?? true,
    };

    if (editingLevel) {
      updateMutation.mutate({ id: editingLevel.id, data: data as UpdateLevelDefinitionDto });
    } else {
      createMutation.mutate(data as CreateLevelDefinitionDto);
    }
  };

  const columns = [
    {
      title: 'Level',
      dataIndex: 'level',
      key: 'level',
      width: 80,
      render: (level: number, record: LevelDefinitionDto) => (
        <Tag color={record.color || '#1890ff'} style={{ fontSize: 14, padding: '4px 12px' }}>
          L{level}
        </Tag>
      ),
    },
    {
      title: 'Name',
      dataIndex: 'levelName',
      key: 'levelName',
      render: (name: string) => <Text strong>{name}</Text>,
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
      width: 400,
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 100,
      render: (isActive: boolean) => (
        <Tag color={isActive ? 'green' : 'red'}>
          {isActive ? 'Active' : 'Inactive'}
        </Tag>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 150,
      render: (_: any, record: LevelDefinitionDto) => (
        <Space>
          <Button
            type="primary"
            size="small"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
          >
            Edit
          </Button>
          <Popconfirm
            title="Delete this level?"
            description="This action cannot be undone."
            onConfirm={() => handleDelete(record.id)}
            okText="Delete"
            cancelText="Cancel"
            okButtonProps={{ danger: true }}
          >
            <Button
              size="small"
              danger
              icon={<DeleteOutlined />}
              loading={deleteMutation.isPending}
            />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  const expandedRowRender = (record: LevelDefinitionDto) => (
    <Descriptions column={1} size="small" bordered>
      <Descriptions.Item label="Autonomy">{record.autonomy || '-'}</Descriptions.Item>
      <Descriptions.Item label="Influence">{record.influence || '-'}</Descriptions.Item>
      <Descriptions.Item label="Complexity">{record.complexity || '-'}</Descriptions.Item>
      <Descriptions.Item label="Knowledge">{record.knowledge || '-'}</Descriptions.Item>
      <Descriptions.Item label="Business Skills">{record.businessSkills || '-'}</Descriptions.Item>
      {record.behavioralIndicators && record.behavioralIndicators.length > 0 && (
        <Descriptions.Item label="Behavioral Indicators">
          <ul style={{ margin: 0, paddingLeft: 20 }}>
            {record.behavioralIndicators.map((item, idx) => (
              <li key={idx}>{item}</li>
            ))}
          </ul>
        </Descriptions.Item>
      )}
    </Descriptions>
  );

  return (
    <div style={{ padding: 24 }}>
      <Card>
        <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Title level={4} style={{ margin: 0 }}>Level Definitions</Title>
          <Space>
            <Button icon={<ReloadOutlined />} onClick={() => refetch()}>
              Refresh
            </Button>
            <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>
              Add Level
            </Button>
          </Space>
        </div>

        <Table
          columns={columns}
          dataSource={levels || []}
          rowKey="id"
          loading={isLoading}
          expandable={{
            expandedRowRender,
            rowExpandable: () => true,
          }}
          pagination={false}
        />
      </Card>

      <Modal
        title={editingLevel ? `Edit Level ${editingLevel.level}` : 'Add New Level'}
        open={isModalOpen}
        onCancel={handleCloseModal}
        footer={null}
        width={700}
        destroyOnClose
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
            <Form.Item
              name="level"
              label="Level Number"
              rules={[{ required: true, message: 'Please enter level number' }]}
            >
              <InputNumber min={1} style={{ width: '100%' }} disabled={!!editingLevel} />
            </Form.Item>

            <Form.Item
              name="levelName"
              label="Level Name"
              rules={[{ required: true, message: 'Please enter level name' }]}
            >
              <Input placeholder="e.g., Follow, Assist, Apply, Enable" />
            </Form.Item>
          </div>

          <Form.Item name="description" label="Description">
            <TextArea rows={2} placeholder="General description of this level..." />
          </Form.Item>

          <Collapse
            items={[
              {
                key: 'details',
                label: 'Detailed Attributes (Optional)',
                children: (
                  <>
                    <Form.Item name="autonomy" label="Autonomy">
                      <TextArea rows={2} placeholder="Level of autonomy and direction needed..." />
                    </Form.Item>

                    <Form.Item name="influence" label="Influence">
                      <TextArea rows={2} placeholder="Scope of influence..." />
                    </Form.Item>

                    <Form.Item name="complexity" label="Complexity">
                      <TextArea rows={2} placeholder="Complexity of work handled..." />
                    </Form.Item>

                    <Form.Item name="knowledge" label="Knowledge">
                      <TextArea rows={2} placeholder="Required knowledge level..." />
                    </Form.Item>

                    <Form.Item name="businessSkills" label="Business Skills">
                      <TextArea rows={2} placeholder="Required business skills..." />
                    </Form.Item>

                    <Form.Item name="behavioralIndicators" label="Behavioral Indicators (one per line)">
                      <TextArea rows={4} placeholder="Enter behavioral indicators, one per line..." />
                    </Form.Item>
                  </>
                ),
              },
            ]}
            style={{ marginBottom: 16 }}
          />

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 16 }}>
            <Form.Item name="color" label="Display Color">
              <ColorPicker showText />
            </Form.Item>

            <Form.Item name="displayOrder" label="Display Order">
              <InputNumber min={1} style={{ width: '100%' }} />
            </Form.Item>

            {editingLevel && (
              <Form.Item name="isActive" label="Active" valuePropName="checked">
                <Switch />
              </Form.Item>
            )}
          </div>

          <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
            <Space>
              <Button onClick={handleCloseModal}>Cancel</Button>
              <Button
                type="primary"
                htmlType="submit"
                loading={createMutation.isPending || updateMutation.isPending}
              >
                {editingLevel ? 'Update' : 'Create'}
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
