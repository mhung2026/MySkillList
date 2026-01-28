import { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate, Link, useLocation, useNavigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ConfigProvider, Layout, Menu, theme, Dropdown, Avatar, Space, Typography, Spin, Drawer, Button } from 'antd';
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
  TrophyOutlined,
  RiseOutlined,
  MenuOutlined,
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
import StartTest from './pages/assessments/StartTest';
import SelfAssessment from './pages/assessments/SelfAssessment';
import Login from './pages/auth/Login';
import Register from './pages/auth/Register';
import Dashboard from './pages/dashboard/Dashboard';
import SystemEnumManagement from './pages/admin/SystemEnumManagement';
import PublicTest from './pages/public/PublicTest';
import Profile from './pages/profile/Profile';
import SkillGapAnalysis from './pages/profile/SkillGapAnalysis';
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
  const [mobileDrawerVisible, setMobileDrawerVisible] = useState(false);
  const [isMobile, setIsMobile] = useState(window.innerWidth < 768);
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const {
    token: { colorBgContainer, borderRadiusLG },
  } = theme.useToken();

  // Detect mobile screen size
  useEffect(() => {
    const handleResize = () => {
      const mobile = window.innerWidth < 768;
      setIsMobile(mobile);
      if (!mobile) {
        setMobileDrawerVisible(false);
      }
    };

    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

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
      {
        key: '/assessments/self-assessment',
        icon: <TrophyOutlined />,
        label: <Link to="/assessments/self-assessment">Self Assessment</Link>,
      },
    ],
  });

  // Profile & Learning
  menuItems.push({
    key: 'profile',
    icon: <UserOutlined />,
    label: 'My Profile',
    children: [
      {
        key: '/profile',
        icon: <UserOutlined />,
        label: <Link to="/profile">Profile</Link>,
      },
      {
        key: '/profile/skill-gaps',
        icon: <RiseOutlined />,
        label: <Link to="/profile/skill-gaps">Skill Gaps & Learning</Link>,
      },
    ],
  });

  const userMenu = {
    items: [
      {
        key: 'profile',
        icon: <UserOutlined />,
        label: 'My Profile',
        onClick: () => navigate('/profile'),
      },
      {
        key: 'skill-gaps',
        icon: <RiseOutlined />,
        label: 'Skill Gaps & Learning',
        onClick: () => navigate('/profile/skill-gaps'),
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
      {!isMobile && (
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
          defaultOpenKeys={['taxonomy', 'tests', 'assessments', 'profile', 'admin']}
          mode="inline"
          items={menuItems}
          selectedKeys={[location.pathname]}
        />
      </Sider>
      )}
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
          <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
            {isMobile && (
              <Button
                type="text"
                icon={<MenuOutlined />}
                onClick={() => setMobileDrawerVisible(true)}
                style={{ fontSize: 20 }}
              />
            )}
            <h2 style={{ margin: 0, fontSize: isMobile ? 16 : 20 }}>
              {isMobile ? 'Skill Matrix' : 'Skill Matrix Management'}
            </h2>
          </div>
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
        <Content style={{ margin: isMobile ? '8px' : '16px' }}>
          <div
            style={{
              padding: isMobile ? 16 : 24,
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
              <Route path="/assessments/self-assessment" element={<SelfAssessment />} />
              <Route path="/assessments/start-test/:templateId" element={<StartTest />} />
              <Route path="/assessments/take/:assessmentId" element={<TakeTest />} />
              <Route path="/assessments/result/:assessmentId" element={<TestResult />} />
              <Route path="/profile" element={<Profile />} />
              <Route path="/profile/skill-gaps" element={<SkillGapAnalysis />} />
              <Route path="/admin/enums" element={<SystemEnumManagement />} />
            </Routes>
          </div>
        </Content>
      </Layout>

      {/* Mobile Drawer Menu */}
      <Drawer
        title={
          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <AppstoreOutlined />
            <span>Skill Matrix</span>
          </div>
        }
        placement="left"
        onClose={() => setMobileDrawerVisible(false)}
        open={mobileDrawerVisible}
        width={280}
        styles={{ body: { padding: 0 } }}
      >
        <Menu
          theme="light"
          mode="inline"
          items={menuItems}
          selectedKeys={[location.pathname]}
          defaultOpenKeys={['taxonomy', 'tests', 'assessments', 'profile', 'admin']}
          onClick={() => setMobileDrawerVisible(false)}
        />
      </Drawer>
    </Layout>
  );
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      {/* Public route - no authentication required */}
      <Route path="/test/:assessmentId" element={<PublicTest />} />
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
