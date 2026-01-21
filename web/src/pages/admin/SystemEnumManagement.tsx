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
  Tabs,
  Tooltip,
  Badge,
  ColorPicker,
  Select,
  Spin,
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  ReloadOutlined,
  SettingOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  LockOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { systemEnumApi } from '../../api/systemEnums';
import type {
  SystemEnumValueDto,
  CreateSystemEnumValueDto,
  UpdateSystemEnumValueDto,
  EnumTypeDto,
} from '../../types';
import type { ColumnsType } from 'antd/es/table';

const { Title, Text } = Typography;
const { TextArea } = Input;

// Icon options for enum values
const iconOptions = [
  'CheckCircleOutlined', 'CloseCircleOutlined', 'UserOutlined', 'TeamOutlined',
  'CodeOutlined', 'FileTextOutlined', 'FormOutlined', 'BulbOutlined',
  'SettingOutlined', 'SafetyCertificateOutlined', 'BookOutlined', 'VideoCameraOutlined',
  'PlayCircleOutlined', 'ProjectOutlined', 'NotificationOutlined', 'CrownOutlined',
  'QuestionCircleOutlined', 'EditOutlined', 'ApartmentOutlined', 'SolutionOutlined',
  'UsergroupAddOutlined', 'UserSwitchOutlined', 'CheckSquareOutlined',
];

export default function SystemEnumManagement() {
  const [selectedEnumType, setSelectedEnumType] = useState<string | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingValue, setEditingValue] = useState<SystemEnumValueDto | null>(null);
  const [form] = Form.useForm();
  const queryClient = useQueryClient();

  // Fetch all enum types
  const { data: enumTypes, isLoading: typesLoading } = useQuery({
    queryKey: ['systemEnumTypes'],
    queryFn: systemEnumApi.getTypes,
  });

  // Fetch values for selected enum type
  const { data: enumValues, isLoading: valuesLoading } = useQuery({
    queryKey: ['systemEnumValues', selectedEnumType],
    queryFn: () => systemEnumApi.getValuesByType(selectedEnumType!, true),
    enabled: !!selectedEnumType,
  });

  // Mutations
  const createMutation = useMutation({
    mutationFn: systemEnumApi.create,
    onSuccess: () => {
      message.success('Enum value created successfully');
      queryClient.invalidateQueries({ queryKey: ['systemEnumValues', selectedEnumType] });
      queryClient.invalidateQueries({ queryKey: ['systemEnumTypes'] });
      handleCloseModal();
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to create enum value');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSystemEnumValueDto }) =>
      systemEnumApi.update(id, data),
    onSuccess: () => {
      message.success('Enum value updated successfully');
      queryClient.invalidateQueries({ queryKey: ['systemEnumValues', selectedEnumType] });
      handleCloseModal();
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to update enum value');
    },
  });

  const toggleActiveMutation = useMutation({
    mutationFn: systemEnumApi.toggleActive,
    onSuccess: () => {
      message.success('Status updated successfully');
      queryClient.invalidateQueries({ queryKey: ['systemEnumValues', selectedEnumType] });
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to update status');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: systemEnumApi.delete,
    onSuccess: () => {
      message.success('Enum value deleted successfully');
      queryClient.invalidateQueries({ queryKey: ['systemEnumValues', selectedEnumType] });
      queryClient.invalidateQueries({ queryKey: ['systemEnumTypes'] });
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to delete enum value');
    },
  });

  const handleOpenCreateModal = () => {
    setEditingValue(null);
    const nextValue = enumValues ? Math.max(...enumValues.map(v => v.value), 0) + 1 : 1;
    const nextOrder = enumValues ? Math.max(...enumValues.map(v => v.displayOrder), 0) + 1 : 1;
    form.setFieldsValue({
      enumType: selectedEnumType,
      value: nextValue,
      code: '',
      name: '',
      nameVi: '',
      description: '',
      descriptionVi: '',
      color: '#1890ff',
      icon: null,
      displayOrder: nextOrder,
    });
    setIsModalOpen(true);
  };

  const handleOpenEditModal = (record: SystemEnumValueDto) => {
    setEditingValue(record);
    form.setFieldsValue({
      name: record.name,
      nameVi: record.nameVi || '',
      description: record.description || '',
      descriptionVi: record.descriptionVi || '',
      color: record.color || '#1890ff',
      icon: record.icon,
      displayOrder: record.displayOrder,
    });
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingValue(null);
    form.resetFields();
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      const colorValue = typeof values.color === 'object' ? values.color.toHexString() : values.color;

      if (editingValue) {
        // Update
        const updateData: UpdateSystemEnumValueDto = {
          name: values.name,
          nameVi: values.nameVi || undefined,
          description: values.description || undefined,
          descriptionVi: values.descriptionVi || undefined,
          color: colorValue || undefined,
          icon: values.icon || undefined,
          displayOrder: values.displayOrder,
        };
        await updateMutation.mutateAsync({ id: editingValue.id, data: updateData });
      } else {
        // Create
        const createData: CreateSystemEnumValueDto = {
          enumType: values.enumType,
          value: values.value,
          code: values.code,
          name: values.name,
          nameVi: values.nameVi || undefined,
          description: values.description || undefined,
          descriptionVi: values.descriptionVi || undefined,
          color: colorValue || undefined,
          icon: values.icon || undefined,
          displayOrder: values.displayOrder,
        };
        await createMutation.mutateAsync(createData);
      }
    } catch (error) {
      // Form validation error
    }
  };

  const columns: ColumnsType<SystemEnumValueDto> = [
    {
      title: 'Value',
      dataIndex: 'value',
      key: 'value',
      width: 70,
      render: (value: number) => <Tag>{value}</Tag>,
    },
    {
      title: 'Code',
      dataIndex: 'code',
      key: 'code',
      width: 150,
      render: (code: string, record) => (
        <Space>
          <Text strong>{code}</Text>
          {record.isSystem && (
            <Tooltip title="System value - cannot be deleted">
              <LockOutlined style={{ color: '#faad14' }} />
            </Tooltip>
          )}
        </Space>
      ),
    },
    {
      title: 'Name (EN)',
      dataIndex: 'name',
      key: 'name',
      render: (name: string, record) => (
        <Space>
          {record.color && (
            <div
              style={{
                width: 16,
                height: 16,
                backgroundColor: record.color,
                borderRadius: 4,
                display: 'inline-block',
              }}
            />
          )}
          <span>{name}</span>
        </Space>
      ),
    },
    {
      title: 'Name (VI)',
      dataIndex: 'nameVi',
      key: 'nameVi',
      render: (nameVi?: string) => nameVi || <Text type="secondary">-</Text>,
    },
    {
      title: 'Order',
      dataIndex: 'displayOrder',
      key: 'displayOrder',
      width: 80,
      sorter: (a, b) => a.displayOrder - b.displayOrder,
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 100,
      render: (isActive: boolean) =>
        isActive ? (
          <Badge status="success" text="Active" />
        ) : (
          <Badge status="default" text="Inactive" />
        ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 150,
      render: (_, record) => (
        <Space>
          <Tooltip title="Edit">
            <Button
              type="text"
              icon={<EditOutlined />}
              onClick={() => handleOpenEditModal(record)}
            />
          </Tooltip>
          <Tooltip title={record.isActive ? 'Deactivate' : 'Activate'}>
            <Button
              type="text"
              icon={record.isActive ? <CloseCircleOutlined /> : <CheckCircleOutlined />}
              onClick={() => toggleActiveMutation.mutate(record.id)}
              disabled={record.isSystem && record.isActive}
            />
          </Tooltip>
          {!record.isSystem && (
            <Popconfirm
              title="Delete this enum value?"
              description="This action cannot be undone."
              onConfirm={() => deleteMutation.mutate(record.id)}
              okText="Yes"
              cancelText="No"
            >
              <Tooltip title="Delete">
                <Button type="text" danger icon={<DeleteOutlined />} />
              </Tooltip>
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  const tabItems = enumTypes?.map((et: EnumTypeDto) => ({
    key: et.enumType,
    label: (
      <Space>
        {et.displayName}
        <Badge count={et.valueCount} style={{ backgroundColor: '#52c41a' }} />
      </Space>
    ),
  }));

  return (
    <Card>
      <Space direction="vertical" style={{ width: '100%' }} size="large">
        <Space style={{ justifyContent: 'space-between', width: '100%' }}>
          <Space>
            <SettingOutlined style={{ fontSize: 24 }} />
            <Title level={4} style={{ margin: 0 }}>
              System Enum Configuration
            </Title>
          </Space>
          <Button
            icon={<ReloadOutlined />}
            onClick={() => {
              queryClient.invalidateQueries({ queryKey: ['systemEnumTypes'] });
              if (selectedEnumType) {
                queryClient.invalidateQueries({ queryKey: ['systemEnumValues', selectedEnumType] });
              }
            }}
          >
            Refresh
          </Button>
        </Space>

        <Text type="secondary">
          Configure system enumerations. System values (marked with lock icon) cannot be deleted but can be edited.
        </Text>

        {typesLoading ? (
          <Spin />
        ) : (
          <Tabs
            activeKey={selectedEnumType || undefined}
            onChange={setSelectedEnumType}
            items={tabItems}
          />
        )}

        {selectedEnumType && (
          <>
            <Space style={{ justifyContent: 'space-between', width: '100%' }}>
              <Text>
                <strong>{enumTypes?.find(e => e.enumType === selectedEnumType)?.description}</strong>
              </Text>
              <Button type="primary" icon={<PlusOutlined />} onClick={handleOpenCreateModal}>
                Add Value
              </Button>
            </Space>

            <Table
              columns={columns}
              dataSource={enumValues}
              rowKey="id"
              loading={valuesLoading}
              pagination={false}
              size="small"
            />
          </>
        )}

        {!selectedEnumType && !typesLoading && (
          <div style={{ textAlign: 'center', padding: 40 }}>
            <Text type="secondary">Select an enum type from the tabs above to manage its values</Text>
          </div>
        )}
      </Space>

      {/* Create/Edit Modal */}
      <Modal
        title={editingValue ? `Edit: ${editingValue.code}` : 'Add New Enum Value'}
        open={isModalOpen}
        onCancel={handleCloseModal}
        onOk={handleSubmit}
        confirmLoading={createMutation.isPending || updateMutation.isPending}
        width={600}
      >
        <Form form={form} layout="vertical">
          {!editingValue && (
            <>
              <Form.Item name="enumType" label="Enum Type" hidden>
                <Input />
              </Form.Item>

              <Space style={{ width: '100%' }} align="start">
                <Form.Item
                  name="value"
                  label="Value (Number)"
                  rules={[{ required: true, message: 'Please enter value' }]}
                  style={{ width: 150 }}
                >
                  <InputNumber min={1} style={{ width: '100%' }} />
                </Form.Item>

                <Form.Item
                  name="code"
                  label="Code"
                  rules={[
                    { required: true, message: 'Please enter code' },
                    { pattern: /^[A-Za-z][A-Za-z0-9]*$/, message: 'Code must start with letter, alphanumeric only' },
                  ]}
                  style={{ flex: 1 }}
                >
                  <Input placeholder="e.g., NewCategory" />
                </Form.Item>
              </Space>
            </>
          )}

          <Space style={{ width: '100%' }} align="start">
            <Form.Item
              name="name"
              label="Name (English)"
              rules={[{ required: true, message: 'Please enter name' }]}
              style={{ flex: 1 }}
            >
              <Input placeholder="Display name in English" />
            </Form.Item>

            <Form.Item name="nameVi" label="Name (Vietnamese)" style={{ flex: 1 }}>
              <Input placeholder="Tên hiển thị tiếng Việt" />
            </Form.Item>
          </Space>

          <Form.Item name="description" label="Description (English)">
            <TextArea rows={2} placeholder="Description in English" />
          </Form.Item>

          <Form.Item name="descriptionVi" label="Description (Vietnamese)">
            <TextArea rows={2} placeholder="Mô tả tiếng Việt" />
          </Form.Item>

          <Space style={{ width: '100%' }} align="start">
            <Form.Item name="color" label="Color" style={{ width: 120 }}>
              <ColorPicker format="hex" />
            </Form.Item>

            <Form.Item name="icon" label="Icon" style={{ flex: 1 }}>
              <Select
                allowClear
                placeholder="Select icon"
                options={iconOptions.map(icon => ({ value: icon, label: icon }))}
              />
            </Form.Item>

            <Form.Item
              name="displayOrder"
              label="Display Order"
              rules={[{ required: true, message: 'Please enter order' }]}
              style={{ width: 120 }}
            >
              <InputNumber min={1} style={{ width: '100%' }} />
            </Form.Item>
          </Space>
        </Form>
      </Modal>
    </Card>
  );
}
