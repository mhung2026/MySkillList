import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, Form, Input, Button, Typography, message, Space, Divider, Alert } from 'antd';
import { UserOutlined, LockOutlined, LoginOutlined } from '@ant-design/icons';
import { useAuth } from '../../contexts/AuthContext';
import type { LoginRequest } from '../../types';
import { seedUsers } from '../../api/auth';

const { Title, Text } = Typography;

export default function Login() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [loading, setLoading] = useState(false);
  const [seeding, setSeeding] = useState(false);

  const onFinish = async (values: LoginRequest) => {
    setLoading(true);
    const result = await login(values);
    setLoading(false);

    if (result.success) {
      message.success('Đăng nhập thành công!');
      navigate('/assessments');
    } else {
      message.error(result.message || 'Đăng nhập thất bại');
    }
  };

  const handleSeedUsers = async () => {
    setSeeding(true);
    try {
      await seedUsers();
      message.success('Đã tạo users mặc định');
    } catch {
      message.info('Users đã tồn tại');
    }
    setSeeding(false);
  };

  return (
    <div
      style={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      }}
    >
      <Card style={{ width: 400, boxShadow: '0 4px 12px rgba(0,0,0,0.15)' }}>
        <div style={{ textAlign: 'center', marginBottom: 24 }}>
          <Title level={2} style={{ margin: 0, color: '#1890ff' }}>
            Skill Matrix
          </Title>
          <Text type="secondary">Hệ thống quản lý kỹ năng</Text>
        </div>

        <Form name="login" onFinish={onFinish} layout="vertical" size="large">
          <Form.Item
            name="email"
            rules={[
              { required: true, message: 'Vui lòng nhập email' },
              { type: 'email', message: 'Email không hợp lệ' },
            ]}
          >
            <Input prefix={<UserOutlined />} placeholder="Email" />
          </Form.Item>

          <Form.Item
            name="password"
            rules={[{ required: true, message: 'Vui lòng nhập mật khẩu' }]}
          >
            <Input.Password prefix={<LockOutlined />} placeholder="Mật khẩu" />
          </Form.Item>

          <Form.Item>
            <Button type="primary" htmlType="submit" loading={loading} block icon={<LoginOutlined />}>
              Đăng nhập
            </Button>
          </Form.Item>
        </Form>

        <Divider>Tài khoản demo</Divider>

        <Alert
          type="info"
          message="Tài khoản có sẵn"
          description={
            <Space direction="vertical" size={4} style={{ fontSize: 12 }}>
              <Text code>admin@skillmatrix.com / admin123</Text>
              <Text code>manager@skillmatrix.com / manager123</Text>
              <Text code>employee1@skillmatrix.com / employee123</Text>
              <Text code>employee2@skillmatrix.com / employee123</Text>
            </Space>
          }
          style={{ marginBottom: 16 }}
        />

        <Button block onClick={handleSeedUsers} loading={seeding}>
          Tạo users demo (nếu chưa có)
        </Button>
      </Card>
    </div>
  );
}
