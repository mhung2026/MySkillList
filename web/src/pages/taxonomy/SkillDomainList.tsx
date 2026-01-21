import { useState } from 'react';
import {
  Table,
  Button,
  Space,
  Input,
  Tag,
  Modal,
  Form,
  message,
  Popconfirm,
  Switch,
  Card,
  Typography,
  Tooltip,
  InputNumber,
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  SearchOutlined,
  ReloadOutlined,
  EyeInvisibleOutlined,
  EyeOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { skillDomainApi } from '../../api/taxonomy';
import type {
  SkillDomainListDto,
  CreateSkillDomainDto,
  UpdateSkillDomainDto,
} from '../../types';
import type { ColumnsType } from 'antd/es/table';

const { Title } = Typography;
const { TextArea } = Input;

export default function SkillDomainList() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [includeInactive, setIncludeInactive] = useState(false);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<SkillDomainListDto | null>(null);
  const [form] = Form.useForm();

  // Query
  const { data, isLoading, refetch } = useQuery({
    queryKey: ['skillDomains', { searchTerm, includeInactive, pageNumber, pageSize }],
    queryFn: () =>
      skillDomainApi.getAll({
        searchTerm,
        includeInactive,
        pageNumber,
        pageSize,
      }),
  });

  // Mutations
  const createMutation = useMutation({
    mutationFn: skillDomainApi.create,
    onSuccess: () => {
      message.success('Domain created successfully');
      queryClient.invalidateQueries({ queryKey: ['skillDomains'] });
      handleCloseModal();
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to create domain');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSkillDomainDto }) =>
      skillDomainApi.update(id, data),
    onSuccess: () => {
      message.success('Domain updated successfully');
      queryClient.invalidateQueries({ queryKey: ['skillDomains'] });
      handleCloseModal();
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to update domain');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: skillDomainApi.delete,
    onSuccess: () => {
      message.success('Domain deleted successfully');
      queryClient.invalidateQueries({ queryKey: ['skillDomains'] });
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to delete domain');
    },
  });

  const toggleActiveMutation = useMutation({
    mutationFn: skillDomainApi.toggleActive,
    onSuccess: () => {
      message.success('Status updated successfully');
      queryClient.invalidateQueries({ queryKey: ['skillDomains'] });
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to update status');
    },
  });

  const handleOpenModal = (item?: SkillDomainListDto) => {
    setEditingItem(item || null);
    if (item) {
      form.setFieldsValue({
        code: item.code,
        name: item.name,
        description: item.description,
        displayOrder: item.displayOrder,
        isActive: item.isActive,
      });
    } else {
      form.resetFields();
      form.setFieldsValue({ displayOrder: 0, isActive: true });
    }
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingItem(null);
    form.resetFields();
  };

  const handleSubmit = async (values: any) => {
    if (editingItem) {
      updateMutation.mutate({
        id: editingItem.id,
        data: values as UpdateSkillDomainDto,
      });
    } else {
      createMutation.mutate(values as CreateSkillDomainDto);
    }
  };

  const columns: ColumnsType<SkillDomainListDto> = [
    {
      title: 'Code',
      dataIndex: 'code',
      key: 'code',
      width: 100,
      sorter: true,
      render: (code: string) => <Tag color="blue">{code}</Tag>,
    },
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      sorter: true,
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
      width: 300,
    },
    {
      title: 'Order',
      dataIndex: 'displayOrder',
      key: 'displayOrder',
      width: 80,
      align: 'center',
    },
    {
      title: 'Subcategories',
      dataIndex: 'subcategoryCount',
      key: 'subcategoryCount',
      width: 120,
      align: 'center',
      render: (count: number) => <Tag>{count}</Tag>,
    },
    {
      title: 'Skills',
      dataIndex: 'skillCount',
      key: 'skillCount',
      width: 80,
      align: 'center',
      render: (count: number) => <Tag color="green">{count}</Tag>,
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 100,
      align: 'center',
      render: (isActive: boolean, record) => (
        <Tooltip title={isActive ? 'Click to deactivate' : 'Click to activate'}>
          <Switch
            checked={isActive}
            onChange={() => toggleActiveMutation.mutate(record.id)}
            checkedChildren="Active"
            unCheckedChildren="Inactive"
          />
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
              type="text"
              icon={<EditOutlined />}
              onClick={() => handleOpenModal(record)}
            />
          </Tooltip>
          <Popconfirm
            title="Delete this domain?"
            description="This will soft delete the domain. It can be restored later."
            onConfirm={() => deleteMutation.mutate(record.id)}
            okText="Delete"
            cancelText="Cancel"
          >
            <Tooltip title="Delete">
              <Button type="text" danger icon={<DeleteOutlined />} />
            </Tooltip>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <Card>
      <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Title level={4} style={{ margin: 0 }}>
          Skill Domains
        </Title>
        <Space>
          <Input
            placeholder="Search..."
            prefix={<SearchOutlined />}
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value);
              setPageNumber(1);
            }}
            style={{ width: 250 }}
            allowClear
          />
          <Tooltip title={includeInactive ? 'Showing all' : 'Showing active only'}>
            <Button
              icon={includeInactive ? <EyeOutlined /> : <EyeInvisibleOutlined />}
              onClick={() => {
                setIncludeInactive(!includeInactive);
                setPageNumber(1);
              }}
            >
              {includeInactive ? 'All' : 'Active'}
            </Button>
          </Tooltip>
          <Tooltip title="Refresh">
            <Button icon={<ReloadOutlined />} onClick={() => refetch()} />
          </Tooltip>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => handleOpenModal()}>
            Add Domain
          </Button>
        </Space>
      </div>

      <Table
        columns={columns}
        dataSource={data?.data?.items || []}
        rowKey="id"
        loading={isLoading}
        pagination={{
          current: pageNumber,
          pageSize,
          total: data?.data?.totalCount || 0,
          showSizeChanger: true,
          showTotal: (total) => `Total ${total} items`,
          onChange: (page, size) => {
            setPageNumber(page);
            setPageSize(size);
          },
        }}
        rowClassName={(record) => (!record.isActive ? 'inactive-row' : '')}
      />

      <Modal
        title={editingItem ? 'Edit Skill Domain' : 'Add Skill Domain'}
        open={isModalOpen}
        onCancel={handleCloseModal}
        footer={null}
        destroyOnClose
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Form.Item
            name="code"
            label="Code"
            rules={[
              { required: true, message: 'Please enter code' },
              { max: 20, message: 'Code must be max 20 characters' },
            ]}
          >
            <Input placeholder="e.g., DEV, ARCH, QUAL" style={{ textTransform: 'uppercase' }} />
          </Form.Item>

          <Form.Item
            name="name"
            label="Name"
            rules={[
              { required: true, message: 'Please enter name' },
              { max: 200, message: 'Name must be max 200 characters' },
            ]}
          >
            <Input placeholder="e.g., Development & Implementation" />
          </Form.Item>

          <Form.Item name="description" label="Description">
            <TextArea rows={3} placeholder="Description of this skill domain" />
          </Form.Item>

          <Form.Item name="displayOrder" label="Display Order">
            <InputNumber min={0} style={{ width: '100%' }} />
          </Form.Item>

          {editingItem && (
            <Form.Item name="isActive" label="Status" valuePropName="checked">
              <Switch checkedChildren="Active" unCheckedChildren="Inactive" />
            </Form.Item>
          )}

          <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
            <Space>
              <Button onClick={handleCloseModal}>Cancel</Button>
              <Button
                type="primary"
                htmlType="submit"
                loading={createMutation.isPending || updateMutation.isPending}
              >
                {editingItem ? 'Update' : 'Create'}
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>

      <style>{`
        .inactive-row {
          background-color: #fafafa;
          color: #999;
        }
        .inactive-row:hover > td {
          background-color: #f5f5f5 !important;
        }
      `}</style>
    </Card>
  );
}
