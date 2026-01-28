import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Card,
  Form,
  Input,
  Button,
  Space,
  Typography,
  message,
  Spin,
  Avatar,
  Row,
  Col,
  Statistic,
  Divider,
  DatePicker,
  InputNumber,
  Modal,
  Descriptions,
  Tag,
  Select,
} from 'antd';
import {
  UserOutlined,
  MailOutlined,
  TeamOutlined,
  CalendarOutlined,
  TrophyOutlined,
  EditOutlined,
  SaveOutlined,
  LockOutlined,
  CloseOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import { getProfile, updateProfile, changePassword } from '../../api/auth';
import { getTeamsDropdown, getJobRolesDropdown } from '../../api/organization';
import { useAuth } from '../../contexts/AuthContext';
import type { UpdateProfileRequest, ChangePasswordRequest, EmploymentStatus } from '../../types';

const { Title, Text } = Typography;

export default function Profile() {
  const { user, refreshUser } = useAuth();
  const queryClient = useQueryClient();
  const [isEditing, setIsEditing] = useState(false);
  const [passwordModalVisible, setPasswordModalVisible] = useState(false);
  const [form] = Form.useForm();
  const [passwordForm] = Form.useForm();

  const userId = user?.id || '';

  // Fetch profile
  const {
    data: profile,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['profile', userId],
    queryFn: () => getProfile(userId),
    enabled: !!userId,
  });

  // Fetch teams dropdown
  const { data: teams = [] } = useQuery({
    queryKey: ['teams-dropdown'],
    queryFn: getTeamsDropdown,
    enabled: isEditing,
  });

  // Fetch job roles dropdown
  const { data: jobRoles = [] } = useQuery({
    queryKey: ['jobroles-dropdown'],
    queryFn: getJobRolesDropdown,
    enabled: isEditing,
  });

  // Update profile mutation
  const updateMutation = useMutation({
    mutationFn: (data: UpdateProfileRequest) => updateProfile(userId, data),
    onSuccess: () => {
      message.success('Profile updated successfully!');
      queryClient.invalidateQueries({ queryKey: ['profile', userId] });
      refreshUser?.();
      setIsEditing(false);
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to update profile');
    },
  });

  // Change password mutation
  const changePasswordMutation = useMutation({
    mutationFn: (data: ChangePasswordRequest) => changePassword(userId, data),
    onSuccess: () => {
      message.success('Password changed successfully!');
      setPasswordModalVisible(false);
      passwordForm.resetFields();
    },
    onError: () => {
      message.error('Current password is incorrect');
    },
  });

  const handleEdit = () => {
    if (profile) {
      form.setFieldsValue({
        fullName: profile.fullName,
        avatarUrl: profile.avatarUrl,
        joinDate: profile.joinDate ? dayjs(profile.joinDate) : null,
        yearsOfExperience: profile.yearsOfExperience,
        teamId: profile.teamId,
        jobRoleId: profile.jobRoleId,
      });
      setIsEditing(true);
    }
  };

  const handleSave = async () => {
    try {
      const values = await form.validateFields();
      updateMutation.mutate({
        fullName: values.fullName,
        avatarUrl: values.avatarUrl || undefined,
        joinDate: values.joinDate ? values.joinDate.toISOString() : undefined,
        yearsOfExperience: values.yearsOfExperience || 0,
        teamId: values.teamId,
        jobRoleId: values.jobRoleId,
      });
    } catch {
      // Form validation failed
    }
  };

  const handleCancel = () => {
    setIsEditing(false);
    form.resetFields();
  };

  const handleChangePassword = async () => {
    try {
      const values = await passwordForm.validateFields();
      if (values.newPassword !== values.confirmPassword) {
        message.error('Passwords do not match');
        return;
      }
      changePasswordMutation.mutate({
        currentPassword: values.currentPassword,
        newPassword: values.newPassword,
      });
    } catch {
      // Form validation failed
    }
  };

  const getStatusColor = (status: EmploymentStatus) => {
    const colorMap: Record<number, string> = {
      1: 'green', // Active
      2: 'orange', // OnLeave
      3: 'red', // Resigned
      4: 'default', // Terminated
    };
    return colorMap[status] || 'default';
  };

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: 100 }}>
        <Spin size="large" />
        <div style={{ marginTop: 16 }}>Loading profile...</div>
      </div>
    );
  }

  if (error || !profile) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: 50 }}>
          <Text type="danger">Failed to load profile</Text>
        </div>
      </Card>
    );
  }

  return (
    <div style={{ maxWidth: 900, margin: '0 auto' }}>
      <Title level={3}>
        <UserOutlined /> My Profile
      </Title>

      {/* Profile Header Card */}
      <Card style={{ marginBottom: 24 }}>
        <Row gutter={24} align="middle">
          <Col>
            <Avatar
              size={100}
              src={profile.avatarUrl}
              icon={<UserOutlined />}
              style={{ backgroundColor: '#1890ff' }}
            />
          </Col>
          <Col flex={1}>
            <Space direction="vertical" size={4}>
              <Title level={4} style={{ margin: 0 }}>
                {profile.fullName}
              </Title>
              <Text type="secondary">
                <MailOutlined /> {profile.email}
              </Text>
              <Space>
                <Tag color="blue">{profile.roleName}</Tag>
                <Tag color={getStatusColor(profile.status)}>{profile.statusName}</Tag>
              </Space>
            </Space>
          </Col>
          <Col>
            {!isEditing ? (
              <Space>
                <Button icon={<EditOutlined />} onClick={handleEdit}>
                  Edit Profile
                </Button>
                <Button icon={<LockOutlined />} onClick={() => setPasswordModalVisible(true)}>
                  Change Password
                </Button>
              </Space>
            ) : (
              <Space>
                <Button
                  type="primary"
                  icon={<SaveOutlined />}
                  onClick={handleSave}
                  loading={updateMutation.isPending}
                >
                  Save
                </Button>
                <Button icon={<CloseOutlined />} onClick={handleCancel}>
                  Cancel
                </Button>
              </Space>
            )}
          </Col>
        </Row>
      </Card>

      {/* Statistics Cards */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={8}>
          <Card>
            <Statistic
              title="Total Skills"
              value={profile.totalSkills}
              prefix={<TrophyOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={8}>
          <Card>
            <Statistic
              title="Completed Assessments"
              value={profile.completedAssessments}
              prefix={<TrophyOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={8}>
          <Card>
            <Statistic
              title="Average Skill Level"
              value={profile.averageSkillLevel}
              precision={1}
              prefix={<TrophyOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {/* Profile Details */}
      <Card title="Profile Information">
        {isEditing ? (
          <Form form={form} layout="vertical">
            <Row gutter={[16, 0]}>
              <Col xs={24} md={12}>
                <Form.Item
                  name="fullName"
                  label="Full Name"
                  rules={[{ required: true, message: 'Please enter your name' }]}
                >
                  <Input prefix={<UserOutlined />} placeholder="Enter your full name" />
                </Form.Item>
              </Col>
              <Col xs={24} md={12}>
                <Form.Item name="avatarUrl" label="Avatar URL">
                  <Input placeholder="Enter avatar URL (optional)" />
                </Form.Item>
              </Col>
            </Row>
            <Row gutter={[16, 0]}>
              <Col xs={24} md={12}>
                <Form.Item name="joinDate" label="Join Date">
                  <DatePicker style={{ width: '100%' }} />
                </Form.Item>
              </Col>
              <Col xs={24} md={12}>
                <Form.Item name="yearsOfExperience" label="Years of Experience">
                  <InputNumber min={0} max={50} style={{ width: '100%' }} />
                </Form.Item>
              </Col>
            </Row>
            <Row gutter={[16, 0]}>
              <Col xs={24} md={12}>
                <Form.Item name="teamId" label="Team">
                  <Select
                    placeholder="Select team"
                    allowClear
                    showSearch
                    filterOption={(input, option) =>
                      (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
                    }
                    options={teams.map((t) => ({ label: t.name, value: t.id }))}
                  />
                </Form.Item>
              </Col>
              <Col xs={24} md={12}>
                <Form.Item name="jobRoleId" label="Job Role">
                  <Select
                    placeholder="Select job role"
                    allowClear
                    showSearch
                    filterOption={(input, option) =>
                      (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
                    }
                    options={jobRoles.map((jr) => ({
                      label: jr.code ? `${jr.code} - ${jr.name}` : jr.name,
                      value: jr.id,
                    }))}
                  />
                </Form.Item>
              </Col>
            </Row>
          </Form>
        ) : (
          <Descriptions column={{ xs: 1, sm: 2 }} bordered>
            <Descriptions.Item label="Full Name">
              <Text strong>{profile.fullName}</Text>
            </Descriptions.Item>
            <Descriptions.Item label="Email">
              <Text>{profile.email}</Text>
            </Descriptions.Item>
            <Descriptions.Item label="Role">
              <Tag color="blue">{profile.roleName}</Tag>
            </Descriptions.Item>
            <Descriptions.Item label="Status">
              <Tag color={getStatusColor(profile.status)}>{profile.statusName}</Tag>
            </Descriptions.Item>
            <Descriptions.Item label="Team">
              {profile.teamName ? (
                <Space>
                  <TeamOutlined />
                  {profile.teamName}
                </Space>
              ) : (
                <Text type="secondary">Not assigned</Text>
              )}
            </Descriptions.Item>
            <Descriptions.Item label="Job Role">
              {profile.jobRoleName || <Text type="secondary">Not assigned</Text>}
            </Descriptions.Item>
            <Descriptions.Item label="Manager">
              {profile.managerName || <Text type="secondary">Not assigned</Text>}
            </Descriptions.Item>
            <Descriptions.Item label="Join Date">
              {profile.joinDate ? (
                <Space>
                  <CalendarOutlined />
                  {dayjs(profile.joinDate).format('DD/MM/YYYY')}
                </Space>
              ) : (
                <Text type="secondary">Not set</Text>
              )}
            </Descriptions.Item>
            <Descriptions.Item label="Years of Experience">
              {profile.yearsOfExperience} year(s)
            </Descriptions.Item>
            <Descriptions.Item label="Account Created">
              {dayjs(profile.createdAt).format('DD/MM/YYYY HH:mm')}
            </Descriptions.Item>
          </Descriptions>
        )}
      </Card>

      {/* Change Password Modal */}
      <Modal
        title="Change Password"
        open={passwordModalVisible}
        onCancel={() => {
          setPasswordModalVisible(false);
          passwordForm.resetFields();
        }}
        onOk={handleChangePassword}
        okText="Change Password"
        confirmLoading={changePasswordMutation.isPending}
      >
        <Form form={passwordForm} layout="vertical">
          <Form.Item
            name="currentPassword"
            label="Current Password"
            rules={[{ required: true, message: 'Please enter current password' }]}
          >
            <Input.Password prefix={<LockOutlined />} placeholder="Enter current password" />
          </Form.Item>
          <Divider />
          <Form.Item
            name="newPassword"
            label="New Password"
            rules={[
              { required: true, message: 'Please enter new password' },
              { min: 6, message: 'Password must be at least 6 characters' },
            ]}
          >
            <Input.Password prefix={<LockOutlined />} placeholder="Enter new password" />
          </Form.Item>
          <Form.Item
            name="confirmPassword"
            label="Confirm New Password"
            rules={[
              { required: true, message: 'Please confirm new password' },
              { min: 6, message: 'Password must be at least 6 characters' },
            ]}
          >
            <Input.Password prefix={<LockOutlined />} placeholder="Confirm new password" />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
