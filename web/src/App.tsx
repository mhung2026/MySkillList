import { useState } from 'react';
import { BrowserRouter, Routes, Route, Navigate, Link, useLocation, useNavigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ConfigProvider, Layout, Menu, theme, Dropdown, Avatar, Space, Typography, Spin } from 'antd';
import {
  AppstoreOutlined,
  TagsOutlined,
  ApartmentOutlined,
  SettingOutlined,
  FileTextOutlined,
  RobotOutlined,
  FormOutlined,
  UserOutlined,
  LogoutOutlined,
  DashboardOutlined,
  OrderedListOutlined,
} from '@ant-design/icons';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import SkillDomainList from './pages/taxonomy/SkillDomainList';
import SkillSubcategoryList from './pages/taxonomy/SkillSubcategoryList';
import SkillList from './pages/taxonomy/SkillList';
import LevelDefinitionList from './pages/taxonomy/LevelDefinitionList';
import TestTemplateList from './pages/tests/TestTemplateList';
import TestTemplateDetail from './pages/tests/TestTemplateDetail';
import AvailableTests from './pages/assessments/AvailableTests';
import TakeTest from './pages/assessments/TakeTest';
import TestResult from './pages/assessments/TestResult';
import Login from './pages/auth/Login';
import Dashboard from './pages/dashboard/Dashboard';
import SystemEnumManagement from './pages/admin/SystemEnumManagement';
import { UserRole } from './types';

const { Header, Sider, Content } = Layout;
const { Text } = Typography;

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

// Protected Route wrapper
function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <Spin size="large" />
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}

function AppLayout() {
  const [collapsed, setCollapsed] = useState(false);
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const {
    token: { colorBgContainer, borderRadiusLG },
  } = theme.useToken();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  // Build menu based on user role
  const menuItems = [];

  // Dashboard - visible to Admin and Manager
  if (user?.role === UserRole.Admin || user?.role === UserRole.Manager) {
    menuItems.push({
      key: '/dashboard',
      icon: <DashboardOutlined />,
      label: <Link to="/dashboard">Dashboard</Link>,
    });
  }

  // Admin and Manager can access taxonomy and test management
  if (user?.role === UserRole.Admin || user?.role === UserRole.Manager) {
    menuItems.push(
      {
        key: 'taxonomy',
        icon: <ApartmentOutlined />,
        label: 'Skill Taxonomy',
        children: [
          {
            key: '/taxonomy/domains',
            icon: <AppstoreOutlined />,
            label: <Link to="/taxonomy/domains">Domains</Link>,
          },
          {
            key: '/taxonomy/subcategories',
            icon: <TagsOutlined />,
            label: <Link to="/taxonomy/subcategories">Subcategories</Link>,
          },
          {
            key: '/taxonomy/skills',
            icon: <SettingOutlined />,
            label: <Link to="/taxonomy/skills">Skills</Link>,
          },
          {
            key: '/taxonomy/level-definitions',
            icon: <OrderedListOutlined />,
            label: <Link to="/taxonomy/level-definitions">Level Definitions</Link>,
          },
        ],
      },
      {
        key: 'tests',
        icon: <FileTextOutlined />,
        label: 'Test Management',
        children: [
          {
            key: '/tests/templates',
            icon: <RobotOutlined />,
            label: <Link to="/tests/templates">Test Templates</Link>,
          },
        ],
      }
    );
  }

  // Admin only - System Configuration
  if (user?.role === UserRole.Admin) {
    menuItems.push({
      key: 'admin',
      icon: <SettingOutlined />,
      label: 'Administration',
      children: [
        {
          key: '/admin/enums',
          icon: <OrderedListOutlined />,
          label: <Link to="/admin/enums">System Enums</Link>,
        },
      ],
    });
  }

  // All users can access assessments
  menuItems.push({
    key: 'assessments',
    icon: <FormOutlined />,
    label: 'Assessments',
    children: [
      {
        key: '/assessments',
        icon: <UserOutlined />,
        label: <Link to="/assessments">My Tests</Link>,
      },
    ],
  });

  const userMenu = {
    items: [
      {
        key: 'profile',
        icon: <UserOutlined />,
        label: 'Profile',
      },
      {
        type: 'divider' as const,
      },
      {
        key: 'logout',
        icon: <LogoutOutlined />,
        label: 'Logout',
        onClick: handleLogout,
      },
    ],
  };

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider collapsible collapsed={collapsed} onCollapse={setCollapsed}>
        <div
          style={{
            height: 32,
            margin: 16,
            background: 'rgba(255, 255, 255, 0.2)',
            borderRadius: 6,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: '#fff',
            fontWeight: 'bold',
          }}
        >
          {collapsed ? 'SM' : 'Skill Matrix'}
        </div>
        <Menu
          theme="dark"
          defaultSelectedKeys={[location.pathname]}
          defaultOpenKeys={['taxonomy', 'tests', 'assessments', 'admin']}
          mode="inline"
          items={menuItems}
          selectedKeys={[location.pathname]}
        />
      </Sider>
      <Layout>
        <Header
          style={{
            padding: '0 16px',
            background: colorBgContainer,
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
          }}
        >
          <h2 style={{ margin: 0 }}>Skill Matrix Management</h2>
          <Dropdown menu={userMenu} placement="bottomRight">
            <Space style={{ cursor: 'pointer' }}>
              <Avatar icon={<UserOutlined />} src={user?.avatarUrl} />
              <div style={{ lineHeight: 1.2 }}>
                <Text strong>{user?.fullName}</Text>
                <br />
                <Text type="secondary" style={{ fontSize: 12 }}>
                  {user?.roleName}
                </Text>
              </div>
            </Space>
          </Dropdown>
        </Header>
        <Content style={{ margin: '16px' }}>
          <div
            style={{
              padding: 24,
              minHeight: 360,
              background: colorBgContainer,
              borderRadius: borderRadiusLG,
            }}
          >
            <Routes>
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
              <Route path="/dashboard" element={<Dashboard />} />
              <Route path="/taxonomy/domains" element={<SkillDomainList />} />
              <Route path="/taxonomy/subcategories" element={<SkillSubcategoryList />} />
              <Route path="/taxonomy/skills" element={<SkillList />} />
              <Route path="/taxonomy/level-definitions" element={<LevelDefinitionList />} />
              <Route path="/tests/templates" element={<TestTemplateList />} />
              <Route path="/tests/templates/:id" element={<TestTemplateDetail />} />
              <Route path="/assessments" element={<AvailableTests />} />
              <Route path="/assessments/take/:assessmentId" element={<TakeTest />} />
              <Route path="/assessments/result/:assessmentId" element={<TestResult />} />
              <Route path="/admin/enums" element={<SystemEnumManagement />} />
            </Routes>
          </div>
        </Content>
      </Layout>
    </Layout>
  );
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route
        path="/*"
        element={
          <ProtectedRoute>
            <AppLayout />
          </ProtectedRoute>
        }
      />
    </Routes>
  );
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ConfigProvider
        theme={{
          token: {
            colorPrimary: '#1890ff',
          },
        }}
      >
        <AuthProvider>
          <BrowserRouter>
            <AppRoutes />
          </BrowserRouter>
        </AuthProvider>
      </ConfigProvider>
    </QueryClientProvider>
  );
}
