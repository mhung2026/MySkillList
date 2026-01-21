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
  Select,
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
import { skillDomainApi, skillSubcategoryApi } from '../../api/taxonomy';
import type {
  SkillSubcategoryListDto,
  CreateSkillSubcategoryDto,
  UpdateSkillSubcategoryDto,
} from '../../types';
import type { ColumnsType } from 'antd/es/table';

const { Title } = Typography;
const { TextArea } = Input;

export default function SkillSubcategoryList() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [includeInactive, setIncludeInactive] = useState(false);
  const [selectedDomainId, setSelectedDomainId] = useState<string | undefined>();
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<SkillSubcategoryListDto | null>(null);
  const [form] = Form.useForm();

  // Queries
  const { data, isLoading, refetch } = useQuery({
    queryKey: ['skillSubcategories', { searchTerm, includeInactive, selectedDomainId, pageNumber, pageSize }],
    queryFn: () =>
      skillSubcategoryApi.getAll({
        searchTerm,
        includeInactive,
        domainId: selectedDomainId,
        pageNumber,
        pageSize,
      }),
  });

  const { data: domainsData } = useQuery({
    queryKey: ['skillDomainsDropdown'],
    queryFn: skillDomainApi.getDropdown,
  });

  // Mutations
  const createMutation = useMutation({
    mutationFn: skillSubcategoryApi.create,
    onSuccess: () => {
      message.success('Subcategory created successfully');
      queryClient.invalidateQueries({ queryKey: ['skillSubcategories'] });
      handleCloseModal();
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to create subcategory');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSkillSubcategoryDto }) =>
      skillSubcategoryApi.update(id, data),
    onSuccess: () => {
      message.success('Subcategory updated successfully');
      queryClient.invalidateQueries({ queryKey: ['skillSubcategories'] });
      handleCloseModal();
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to update subcategory');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: skillSubcategoryApi.delete,
    onSuccess: () => {
      message.success('Subcategory deleted successfully');
      queryClient.invalidateQueries({ queryKey: ['skillSubcategories'] });
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to delete subcategory');
    },
  });

  const toggleActiveMutation = useMutation({
    mutationFn: skillSubcategoryApi.toggleActive,
    onSuccess: () => {
      message.success('Status updated successfully');
      queryClient.invalidateQueries({ queryKey: ['skillSubcategories'] });
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to update status');
    },
  });

  const handleOpenModal = (item?: SkillSubcategoryListDto) => {
    setEditingItem(item || null);
    if (item) {
      form.setFieldsValue({
        skillDomainId: item.skillDomainId,
        code: item.code,
        name: item.name,
        description: item.description,
        displayOrder: item.displayOrder,
        isActive: item.isActive,
      });
    } else {
      form.resetFields();
      form.setFieldsValue({
        displayOrder: 0,
        isActive: true,
        skillDomainId: selectedDomainId,
      });
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
        data: values as UpdateSkillSubcategoryDto,
      });
    } else {
      createMutation.mutate(values as CreateSkillSubcategoryDto);
    }
  };

  const columns: ColumnsType<SkillSubcategoryListDto> = [
    {
      title: 'Domain',
      key: 'domain',
      width: 150,
      render: (_, record) => (
        <Tag color="purple">{record.skillDomainCode}</Tag>
      ),
      filters: domainsData?.data?.map((d) => ({ text: d.name, value: d.id })),
      onFilter: (value, record) => record.skillDomainId === value,
    },
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
      width: 250,
    },
    {
      title: 'Order',
      dataIndex: 'displayOrder',
      key: 'displayOrder',
      width: 80,
      align: 'center',
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
        <Switch
          checked={isActive}
          onChange={() => toggleActiveMutation.mutate(record.id)}
          checkedChildren="Active"
          unCheckedChildren="Inactive"
        />
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
            title="Delete this subcategory?"
            description="This will soft delete the subcategory."
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
          Skill Subcategories
        </Title>
        <Space>
          <Select
            placeholder="Filter by Domain"
            style={{ width: 200 }}
            allowClear
            value={selectedDomainId}
            onChange={(value) => {
              setSelectedDomainId(value);
              setPageNumber(1);
            }}
            options={domainsData?.data?.map((d) => ({ label: d.name, value: d.id }))}
          />
          <Input
            placeholder="Search..."
            prefix={<SearchOutlined />}
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value);
              setPageNumber(1);
            }}
            style={{ width: 200 }}
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
            Add Subcategory
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
        title={editingItem ? 'Edit Skill Subcategory' : 'Add Skill Subcategory'}
        open={isModalOpen}
        onCancel={handleCloseModal}
        footer={null}
        destroyOnClose
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Form.Item
            name="skillDomainId"
            label="Domain"
            rules={[{ required: true, message: 'Please select domain' }]}
          >
            <Select
              placeholder="Select domain"
              options={domainsData?.data?.map((d) => ({ label: `[${d.code}] ${d.name}`, value: d.id }))}
            />
          </Form.Item>

          <Form.Item
            name="code"
            label="Code"
            rules={[
              { required: true, message: 'Please enter code' },
              { max: 20, message: 'Code must be max 20 characters' },
            ]}
          >
            <Input placeholder="e.g., PROG, FRMW, TEST" style={{ textTransform: 'uppercase' }} />
          </Form.Item>

          <Form.Item
            name="name"
            label="Name"
            rules={[
              { required: true, message: 'Please enter name' },
              { max: 200, message: 'Name must be max 200 characters' },
            ]}
          >
            <Input placeholder="e.g., Programming Languages" />
          </Form.Item>

          <Form.Item name="description" label="Description">
            <TextArea rows={3} placeholder="Description of this subcategory" />
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
      `}</style>
    </Card>
  );
}
