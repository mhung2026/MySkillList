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
  Checkbox,
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  SearchOutlined,
  ReloadOutlined,
  EyeInvisibleOutlined,
  EyeOutlined,
  StarOutlined,
  DownOutlined,
  RightOutlined,
} from '@ant-design/icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { skillApi, skillDomainApi, skillSubcategoryApi, enumApi } from '../../api/taxonomy';
import { SkillLevelEditor } from '../../components/skill';
import type {
  SkillListDto,
  CreateSkillDto,
  UpdateSkillDto,
  SkillCategory,
  SkillType,
} from '../../types';
import type { ColumnsType } from 'antd/es/table';

const { Title } = Typography;
const { TextArea } = Input;

const categoryColors: Record<number, string> = {
  1: 'blue',      // Technical
  2: 'green',     // Professional
  3: 'orange',    // Domain
  4: 'purple',    // Leadership
  5: 'cyan',      // Tools
};

const skillTypeColors: Record<number, string> = {
  1: 'gold',      // Core
  2: 'geekblue',  // Specialty
  3: 'lime',      // Adjacent
};

export default function SkillList() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [includeInactive, setIncludeInactive] = useState(false);
  const [selectedDomainId, setSelectedDomainId] = useState<string | undefined>();
  const [selectedSubcategoryId, setSelectedSubcategoryId] = useState<string | undefined>();
  const [selectedCategory, setSelectedCategory] = useState<number | undefined>();
  const [selectedSkillType, setSelectedSkillType] = useState<number | undefined>();
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<SkillListDto | null>(null);
  const [form] = Form.useForm();

  // Queries
  const { data, isLoading, refetch } = useQuery({
    queryKey: ['skills', {
      searchTerm,
      includeInactive,
      selectedDomainId,
      selectedSubcategoryId,
      selectedCategory,
      selectedSkillType,
      pageNumber,
      pageSize,
    }],
    queryFn: () =>
      skillApi.getAll({
        searchTerm,
        includeInactive,
        domainId: selectedDomainId,
        subcategoryId: selectedSubcategoryId,
        category: selectedCategory,
        skillType: selectedSkillType,
        pageNumber,
        pageSize,
      }),
  });

  const { data: domainsData } = useQuery({
    queryKey: ['skillDomainsDropdown'],
    queryFn: skillDomainApi.getDropdown,
  });

  const { data: subcategoriesData } = useQuery({
    queryKey: ['skillSubcategoriesDropdown', selectedDomainId],
    queryFn: () => skillSubcategoryApi.getDropdown(selectedDomainId),
  });

  const { data: categoriesData } = useQuery({
    queryKey: ['skillCategories'],
    queryFn: enumApi.getSkillCategories,
  });

  const { data: typesData } = useQuery({
    queryKey: ['skillTypes'],
    queryFn: enumApi.getSkillTypes,
  });

  // Mutations
  const createMutation = useMutation({
    mutationFn: skillApi.create,
    onSuccess: () => {
      message.success('Skill created successfully');
      queryClient.invalidateQueries({ queryKey: ['skills'] });
      handleCloseModal();
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to create skill');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSkillDto }) =>
      skillApi.update(id, data),
    onSuccess: () => {
      message.success('Skill updated successfully');
      queryClient.invalidateQueries({ queryKey: ['skills'] });
      handleCloseModal();
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to update skill');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: skillApi.delete,
    onSuccess: () => {
      message.success('Skill deleted successfully');
      queryClient.invalidateQueries({ queryKey: ['skills'] });
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to delete skill');
    },
  });

  const toggleActiveMutation = useMutation({
    mutationFn: skillApi.toggleActive,
    onSuccess: () => {
      message.success('Status updated successfully');
      queryClient.invalidateQueries({ queryKey: ['skills'] });
    },
    onError: (error: any) => {
      message.error(error.response?.data?.message || 'Failed to update status');
    },
  });

  const handleOpenModal = (item?: SkillListDto) => {
    setEditingItem(item || null);
    if (item) {
      form.setFieldsValue({
        subcategoryId: item.subcategoryId,
        code: item.code,
        name: item.name,
        description: item.description,
        category: item.category,
        skillType: item.skillType,
        displayOrder: item.displayOrder,
        isActive: item.isActive,
        isCompanySpecific: item.isCompanySpecific,
      });
    } else {
      form.resetFields();
      form.setFieldsValue({
        displayOrder: 0,
        isActive: true,
        isCompanySpecific: false,
        subcategoryId: selectedSubcategoryId,
        category: 1,
        skillType: 2,
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
        data: values as UpdateSkillDto,
      });
    } else {
      createMutation.mutate(values as CreateSkillDto);
    }
  };

  const columns: ColumnsType<SkillListDto> = [
    {
      title: 'Domain',
      key: 'domain',
      width: 100,
      render: (_, record) => (
        <Tooltip title={record.domainName}>
          <Tag color="purple">{record.domainCode}</Tag>
        </Tooltip>
      ),
    },
    {
      title: 'Subcategory',
      key: 'subcategory',
      width: 120,
      render: (_, record) => (
        <Tooltip title={record.subcategoryName}>
          <Tag>{record.subcategoryCode}</Tag>
        </Tooltip>
      ),
    },
    {
      title: 'Code',
      dataIndex: 'code',
      key: 'code',
      width: 80,
      render: (code: string) => <Tag color="blue">{code}</Tag>,
    },
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      render: (name: string, record) => (
        <Space>
          {name}
          {record.isCompanySpecific && (
            <Tooltip title="Company Specific">
              <StarOutlined style={{ color: '#faad14' }} />
            </Tooltip>
          )}
        </Space>
      ),
    },
    {
      title: 'Category',
      dataIndex: 'category',
      key: 'category',
      width: 110,
      render: (category: SkillCategory, record) => (
        <Tag color={categoryColors[category]}>{record.categoryName}</Tag>
      ),
    },
    {
      title: 'Type',
      dataIndex: 'skillType',
      key: 'skillType',
      width: 100,
      render: (type: SkillType, record) => (
        <Tag color={skillTypeColors[type]}>{record.skillTypeName}</Tag>
      ),
    },
    {
      title: 'Applicable Levels',
      dataIndex: 'applicableLevelsString',
      key: 'applicableLevels',
      width: 120,
      render: (levels: string | undefined) => {
        if (!levels) return <Tag color="default">-</Tag>;
        const levelList = levels.split(',').map(s => s.trim());
        return (
          <Space size={2} wrap>
            {levelList.map(level => (
              <Tag key={level} color="blue" style={{ margin: 0 }}>
                L{level}
              </Tag>
            ))}
          </Space>
        );
      },
    },
    {
      title: 'Employees',
      dataIndex: 'employeeCount',
      key: 'employeeCount',
      width: 90,
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
          size="small"
        />
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 100,
      align: 'center',
      render: (_, record) => (
        <Space>
          <Tooltip title="Edit">
            <Button
              type="text"
              size="small"
              icon={<EditOutlined />}
              onClick={() => handleOpenModal(record)}
            />
          </Tooltip>
          <Popconfirm
            title="Delete this skill?"
            onConfirm={() => deleteMutation.mutate(record.id)}
            okText="Delete"
            cancelText="Cancel"
          >
            <Tooltip title="Delete">
              <Button type="text" size="small" danger icon={<DeleteOutlined />} />
            </Tooltip>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <Card>
      <div style={{ marginBottom: 16 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
          <Title level={4} style={{ margin: 0 }}>Skills</Title>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => handleOpenModal()}>
            Add Skill
          </Button>
        </div>

        <Space wrap>
          <Select
            placeholder="Domain"
            style={{ width: 150 }}
            allowClear
            value={selectedDomainId}
            onChange={(value) => {
              setSelectedDomainId(value);
              setSelectedSubcategoryId(undefined);
              setPageNumber(1);
            }}
            options={domainsData?.data?.map((d) => ({ label: d.name, value: d.id }))}
          />
          <Select
            placeholder="Subcategory"
            style={{ width: 180 }}
            allowClear
            value={selectedSubcategoryId}
            onChange={(value) => {
              setSelectedSubcategoryId(value);
              setPageNumber(1);
            }}
            options={subcategoriesData?.data?.map((s) => ({ label: s.fullName, value: s.id }))}
          />
          <Select
            placeholder="Category"
            style={{ width: 130 }}
            allowClear
            value={selectedCategory}
            onChange={(value) => {
              setSelectedCategory(value);
              setPageNumber(1);
            }}
            options={categoriesData?.data?.map((c) => ({ label: c.name, value: c.value }))}
          />
          <Select
            placeholder="Type"
            style={{ width: 120 }}
            allowClear
            value={selectedSkillType}
            onChange={(value) => {
              setSelectedSkillType(value);
              setPageNumber(1);
            }}
            options={typesData?.data?.map((t) => ({ label: t.name, value: t.value }))}
          />
          <Input
            placeholder="Search..."
            prefix={<SearchOutlined />}
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value);
              setPageNumber(1);
            }}
            style={{ width: 180 }}
            allowClear
          />
          <Tooltip title={includeInactive ? 'Showing all' : 'Showing active only'}>
            <Button
              icon={includeInactive ? <EyeOutlined /> : <EyeInvisibleOutlined />}
              onClick={() => {
                setIncludeInactive(!includeInactive);
                setPageNumber(1);
              }}
            />
          </Tooltip>
          <Tooltip title="Refresh">
            <Button icon={<ReloadOutlined />} onClick={() => refetch()} />
          </Tooltip>
        </Space>
      </div>

      <Table
        columns={columns}
        dataSource={data?.data?.items || []}
        rowKey="id"
        loading={isLoading}
        size="small"
        expandable={{
          expandedRowRender: (record) => (
            <SkillLevelEditor
              skillId={record.id}
              skillName={record.name}
              applicableLevelsString={record.applicableLevelsString}
            />
          ),
          expandIcon: ({ expanded, onExpand, record }) =>
            expanded ? (
              <DownOutlined
                style={{ cursor: 'pointer', marginRight: 8 }}
                onClick={(e) => onExpand(record, e)}
              />
            ) : (
              <RightOutlined
                style={{ cursor: 'pointer', marginRight: 8 }}
                onClick={(e) => onExpand(record, e)}
              />
            ),
          rowExpandable: () => true,
        }}
        pagination={{
          current: pageNumber,
          pageSize,
          total: data?.data?.totalCount || 0,
          showSizeChanger: true,
          showTotal: (total) => `Total ${total} skills`,
          onChange: (page, size) => {
            setPageNumber(page);
            setPageSize(size);
          },
        }}
        rowClassName={(record) => (!record.isActive ? 'inactive-row' : '')}
      />

      <Modal
        title={editingItem ? 'Edit Skill' : 'Add Skill'}
        open={isModalOpen}
        onCancel={handleCloseModal}
        footer={null}
        destroyOnClose
        width={600}
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Form.Item
            name="subcategoryId"
            label="Subcategory"
            rules={[{ required: true, message: 'Please select subcategory' }]}
          >
            <Select
              placeholder="Select subcategory"
              showSearch
              optionFilterProp="label"
              options={subcategoriesData?.data?.map((s) => ({
                label: s.fullName,
                value: s.id,
              }))}
            />
          </Form.Item>

          <Space style={{ width: '100%' }} size="middle">
            <Form.Item
              name="code"
              label="Code"
              rules={[
                { required: true, message: 'Please enter code' },
                { max: 20, message: 'Max 20 characters' },
              ]}
              style={{ flex: 1 }}
            >
              <Input placeholder="e.g., CSHP, RCTS" style={{ textTransform: 'uppercase' }} />
            </Form.Item>

            <Form.Item
              name="displayOrder"
              label="Order"
              style={{ width: 100 }}
            >
              <InputNumber min={0} style={{ width: '100%' }} />
            </Form.Item>
          </Space>

          <Form.Item
            name="name"
            label="Name"
            rules={[
              { required: true, message: 'Please enter name' },
              { max: 200, message: 'Max 200 characters' },
            ]}
          >
            <Input placeholder="e.g., C#, React" />
          </Form.Item>

          <Form.Item name="description" label="Description">
            <TextArea rows={2} placeholder="Description of this skill" />
          </Form.Item>

          <Space style={{ width: '100%' }} size="middle">
            <Form.Item
              name="category"
              label="Category"
              rules={[{ required: true, message: 'Please select category' }]}
              style={{ flex: 1 }}
            >
              <Select
                placeholder="Select category"
                options={categoriesData?.data?.map((c) => ({ label: c.name, value: c.value }))}
              />
            </Form.Item>

            <Form.Item
              name="skillType"
              label="Skill Type"
              rules={[{ required: true, message: 'Please select type' }]}
              style={{ flex: 1 }}
            >
              <Select
                placeholder="Select type"
                options={typesData?.data?.map((t) => ({
                  label: `${t.name} - ${t.description}`,
                  value: t.value,
                }))}
              />
            </Form.Item>
          </Space>

          <Space>
            <Form.Item name="isCompanySpecific" valuePropName="checked">
              <Checkbox>Company Specific</Checkbox>
            </Form.Item>

            {editingItem && (
              <Form.Item name="isActive" valuePropName="checked">
                <Checkbox>Active</Checkbox>
              </Form.Item>
            )}
          </Space>

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
