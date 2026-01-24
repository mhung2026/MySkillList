import { useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import {
  Spin,
  message,
  Result,
  Button,
  Card,
  Typography,
  Descriptions,
  Tag,
  Space,
  Statistic,
  Row,
  Col,
  Alert,
} from 'antd';
import {
  PlayCircleOutlined,
  ClockCircleOutlined,
  FileTextOutlined,
  TrophyOutlined,
  CheckCircleOutlined,
  HomeOutlined,
} from '@ant-design/icons';
import { startAssessment } from '../../api/assessments';
import { getTestTemplate } from '../../api/testTemplates';
import { useAuth } from '../../contexts/AuthContext';

const { Title, Paragraph } = Typography;

export default function StartTest() {
  const { templateId } = useParams<{ templateId: string }>();
  const navigate = useNavigate();
  const { user, isAuthenticated, isLoading: authLoading } = useAuth();

  // Fetch test template info
  const {
    data: template,
    isLoading: templateLoading,
    error: templateError,
  } = useQuery({
    queryKey: ['testTemplate', templateId],
    queryFn: () => getTestTemplate(templateId!),
    enabled: !!templateId && isAuthenticated,
  });

  // Start assessment mutation
  const startMutation = useMutation({
    mutationFn: startAssessment,
    onSuccess: (data) => {
      message.success('Test started!');
      navigate(`/assessments/take/${data.assessmentId}`, { replace: true });
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to start test');
    },
  });

  // Redirect to login if not authenticated
  useEffect(() => {
    if (authLoading) return;

    if (!isAuthenticated) {
      const returnUrl = encodeURIComponent(window.location.pathname);
      navigate(`/login?returnUrl=${returnUrl}`, { replace: true });
    }
  }, [authLoading, isAuthenticated, navigate]);

  const handleStartTest = () => {
    if (user?.id && templateId) {
      startMutation.mutate({
        employeeId: user.id,
        testTemplateId: templateId,
      });
    }
  };

  // Loading states
  if (authLoading || templateLoading) {
    return (
      <div style={{ textAlign: 'center', padding: 100 }}>
        <Spin size="large" />
        <div style={{ marginTop: 16 }}>
          {authLoading ? 'Checking authentication...' : 'Loading test information...'}
        </div>
      </div>
    );
  }

  // Error state
  if (templateError || !template) {
    return (
      <Result
        status="error"
        title="Test Not Found"
        subTitle="The test does not exist or has been deleted."
        extra={[
          <Button
            type="primary"
            key="home"
            icon={<HomeOutlined />}
            onClick={() => navigate('/assessments')}
          >
            Go to Home
          </Button>,
        ]}
      />
    );
  }

  // Check if template is active
  if (!template.isActive) {
    return (
      <Result
        status="warning"
        title="Test Unavailable"
        subTitle="This test is currently inactive. Please contact the administrator."
        extra={[
          <Button
            type="primary"
            key="home"
            icon={<HomeOutlined />}
            onClick={() => navigate('/assessments')}
          >
            Go to Home
          </Button>,
        ]}
      />
    );
  }

  // Count total questions
  const totalQuestions = template.sections?.reduce(
    (sum, section) => sum + (section.questions?.length || 0),
    0
  ) || 0;

  return (
    <div style={{ maxWidth: 800, margin: '0 auto', padding: '24px 16px' }}>
      <Card>
        <Space direction="vertical" size="large" style={{ width: '100%' }}>
          {/* Header */}
          <div style={{ textAlign: 'center' }}>
            <Title level={2} style={{ marginBottom: 8 }}>
              <FileTextOutlined /> {template.title}
            </Title>
            {template.typeName && (
              <Tag color="blue" style={{ fontSize: 14 }}>
                {template.typeName}
              </Tag>
            )}
          </div>

          {/* Description */}
          {template.description && (
            <Paragraph style={{ textAlign: 'center', fontSize: 16 }}>
              {template.description}
            </Paragraph>
          )}

          {/* Statistics */}
          <Row gutter={[24, 24]} justify="center">
            <Col xs={12} sm={8}>
              <Card size="small" style={{ textAlign: 'center' }}>
                <Statistic
                  title="Questions"
                  value={totalQuestions}
                  prefix={<FileTextOutlined />}
                />
              </Card>
            </Col>
            <Col xs={12} sm={8}>
              <Card size="small" style={{ textAlign: 'center' }}>
                <Statistic
                  title="Duration"
                  value={template.timeLimitMinutes || 'No limit'}
                  suffix={template.timeLimitMinutes ? 'min' : ''}
                  prefix={<ClockCircleOutlined />}
                />
              </Card>
            </Col>
            <Col xs={12} sm={8}>
              <Card size="small" style={{ textAlign: 'center' }}>
                <Statistic
                  title="Passing Score"
                  value={template.passingScore}
                  suffix="%"
                  prefix={<TrophyOutlined />}
                />
              </Card>
            </Col>
          </Row>

          {/* Test details */}
          <Descriptions bordered size="small" column={1}>
            {template.targetSkillName && (
              <Descriptions.Item label="Target Skill">
                {template.targetSkillName}
              </Descriptions.Item>
            )}
            <Descriptions.Item label="Sections">
              {template.sections?.length || 0} section(s)
            </Descriptions.Item>
            {template.isRandomized && (
              <Descriptions.Item label="Randomized Questions">
                <Tag color="purple">Yes</Tag>
              </Descriptions.Item>
            )}
          </Descriptions>

          {/* Instructions */}
          <Alert
            type="info"
            showIcon
            icon={<CheckCircleOutlined />}
            message="Before You Start"
            description={
              <ul style={{ margin: '8px 0', paddingLeft: 20 }}>
                <li>You can pause and continue the test later.</li>
                {template.timeLimitMinutes && (
                  <li>
                    Maximum time limit is <strong>{template.timeLimitMinutes} minutes</strong>.
                  </li>
                )}
                <li>After submission, you cannot change your answers.</li>
                <li>
                  Passing score required: <strong>{template.passingScore}%</strong>
                </li>
              </ul>
            }
          />

          {/* Start button */}
          <div style={{ textAlign: 'center', paddingTop: 16 }}>
            <Space size="large">
              <Button
                size="large"
                icon={<HomeOutlined />}
                onClick={() => navigate('/assessments')}
              >
                Go to Home
              </Button>
              <Button
                type="primary"
                size="large"
                icon={<PlayCircleOutlined />}
                onClick={handleStartTest}
                loading={startMutation.isPending}
                style={{ minWidth: 200 }}
              >
                Start Test
              </Button>
            </Space>
          </div>

          {/* Error message */}
          {startMutation.isError && (
            <Alert
              type="error"
              showIcon
              message="Error"
              description={startMutation.error?.message || 'Failed to start test. Please try again.'}
              style={{ marginTop: 16 }}
            />
          )}
        </Space>
      </Card>
    </div>
  );
}
