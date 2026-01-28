import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import {
  Card,
  Table,
  Button,
  Space,
  Tag,
  Typography,
  message,
  Modal,
  Descriptions,
  Statistic,
  Row,
  Col,
  Tabs,
  Empty,
  Spin,
} from 'antd';
import {
  PlayCircleOutlined,
  HistoryOutlined,
  ClockCircleOutlined,
  TrophyOutlined,
  FileTextOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  SyncOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import type { AvailableTestDto, AssessmentListDto } from '../../types';
import {
  getAvailableTests,
  getEmployeeAssessments,
  startAssessment,
  continueAssessment,
} from '../../api/assessments';
import { useAuth } from '../../contexts/AuthContext';
import './AvailableTests.css';

const { Title, Text } = Typography;

export default function AvailableTests() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [selectedTest, setSelectedTest] = useState<AvailableTestDto | null>(null);
  const [startModalVisible, setStartModalVisible] = useState(false);

  const employeeId = user?.id || '';

  // Fetch available tests
  const {
    data: availableTests,
    isLoading: loadingTests,
  } = useQuery({
    queryKey: ['availableTests', employeeId],
    queryFn: () => getAvailableTests(employeeId),
    enabled: !!employeeId,
  });

  // Fetch assessment history
  const {
    data: historyData,
    isLoading: loadingHistory,
  } = useQuery({
    queryKey: ['assessmentHistory', employeeId],
    queryFn: () => getEmployeeAssessments(employeeId, 1, 50),
    enabled: !!employeeId,
  });

  // Start assessment mutation
  const startMutation = useMutation({
    mutationFn: startAssessment,
    onSuccess: (data) => {
      message.success('Test started successfully!');
      setStartModalVisible(false);
      navigate(`/assessments/take/${data.assessmentId}`);
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to start test');
    },
  });

  // Continue assessment mutation
  const continueMutation = useMutation({
    mutationFn: continueAssessment,
    onSuccess: (data) => {
      navigate(`/assessments/take/${data.assessmentId}`);
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to continue test');
    },
  });

  const handleStartTest = () => {
    if (selectedTest && employeeId) {
      startMutation.mutate({
        employeeId: employeeId,
        testTemplateId: selectedTest.testTemplateId,
      });
    }
  };

  const handleContinueTest = (assessmentId: string) => {
    continueMutation.mutate(assessmentId);
  };

  const handleViewResult = (assessmentId: string) => {
    navigate(`/assessments/result/${assessmentId}`);
  };

  const getStatusTag = (status: number) => {
    // Backend enum: Draft=1, Pending=2, InProgress=3, Completed=4, Reviewed=5, Disputed=6, Resolved=7
    const statusMap: Record<number, { color: string; text: string; icon: React.ReactNode }> = {
      1: { color: 'default', text: 'Draft', icon: <ClockCircleOutlined /> },
      2: { color: 'default', text: 'Pending', icon: <ClockCircleOutlined /> },
      3: { color: 'processing', text: 'In Progress', icon: <SyncOutlined /> },
      4: { color: 'warning', text: 'Pending Review', icon: <ClockCircleOutlined /> },
      5: { color: 'success', text: 'Reviewed', icon: <CheckCircleOutlined /> },
      6: { color: 'error', text: 'Disputed', icon: <CloseCircleOutlined /> },
      7: { color: 'success', text: 'Resolved', icon: <TrophyOutlined /> },
    };
    const config = statusMap[status] || { color: 'default', text: 'Unknown', icon: null };
    return (
      <Tag color={config.color} icon={config.icon}>
        {config.text}
      </Tag>
    );
  };

  // Columns for available tests
  const availableColumns: ColumnsType<AvailableTestDto> = [
    {
      title: 'Test Name',
      dataIndex: 'title',
      key: 'title',
      render: (text, record) => (
        <Space direction="vertical" size={0}>
          <Text strong>{text}</Text>
          {record.description && (
            <Text type="secondary" style={{ fontSize: 12 }}>
              {record.description}
            </Text>
          )}
        </Space>
      ),
    },
    {
      title: 'Type',
      dataIndex: 'typeName',
      key: 'typeName',
      render: (text) => <Tag color="blue">{text}</Tag>,
    },
    {
      title: 'Questions',
      dataIndex: 'questionCount',
      key: 'questionCount',
      align: 'center',
      render: (value) => <Text strong>{value}</Text>,
    },
    {
      title: 'Duration',
      dataIndex: 'timeLimitMinutes',
      key: 'timeLimitMinutes',
      align: 'center',
      render: (value) =>
        value ? (
          <Space>
            <ClockCircleOutlined />
            {value} min
          </Space>
        ) : (
          <Text type="secondary">No limit</Text>
        ),
    },
    {
      title: 'Passing Score',
      dataIndex: 'passingScore',
      key: 'passingScore',
      align: 'center',
      render: (value) => <Text>{value}%</Text>,
    },
    {
      title: 'Attempts',
      key: 'attempts',
      align: 'center',
      render: (_, record) => (
        <Space direction="vertical" size={0}>
          <Text>{record.attemptCount} times</Text>
          {record.bestScore !== undefined && (
            <Text type="secondary">Best: {record.bestScore}%</Text>
          )}
        </Space>
      ),
    },
    {
      title: 'Action',
      key: 'actions',
      align: 'center',
      render: (_, record) => (
        <Button
          type="primary"
          icon={<PlayCircleOutlined />}
          onClick={() => {
            setSelectedTest(record);
            setStartModalVisible(true);
          }}
        >
          Start
        </Button>
      ),
    },
  ];

  // Columns for history
  const historyColumns: ColumnsType<AssessmentListDto> = [
    {
      title: 'Test',
      dataIndex: 'testTemplateTitle',
      key: 'testTemplateTitle',
      render: (text, record) => (
        <Space direction="vertical" size={0}>
          <Text strong>{text || record.title}</Text>
          <Text type="secondary" style={{ fontSize: 12 }}>
            {record.startedAt && new Date(record.startedAt).toLocaleString('en-US')}
          </Text>
        </Space>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status) => getStatusTag(status),
    },
    {
      title: 'Score',
      key: 'score',
      align: 'center',
      render: (_, record) => {
        if (record.score === null || record.score === undefined) {
          return <Text type="secondary">-</Text>;
        }
        return (
          <Space direction="vertical" size={0} style={{ textAlign: 'center' }}>
            <Text strong style={{ fontSize: 16 }}>
              {record.score}/{record.maxScore}
            </Text>
            <Text type="secondary">({record.percentage?.toFixed(1)}%)</Text>
          </Space>
        );
      },
    },
    {
      title: 'Completed At',
      key: 'completedAt',
      render: (_, record) => {
        if (!record.completedAt) {
          return <Text type="secondary">Not completed</Text>;
        }
        return new Date(record.completedAt).toLocaleString('en-US');
      },
    },
    {
      title: 'Action',
      key: 'actions',
      align: 'center',
      render: (_, record) => {
        return (
          <Space>
            {record.status === 3 && (
              // In Progress (backend enum: InProgress=3)
              <Button
                type="primary"
                icon={<PlayCircleOutlined />}
                onClick={() => handleContinueTest(record.id)}
              >
                Continue
              </Button>
            )}
            {record.status >= 4 && (
              // Completed or later (backend enum: Completed=4, Reviewed=5, etc.)
              <Button icon={<FileTextOutlined />} onClick={() => handleViewResult(record.id)}>
                View Result
              </Button>
            )}
          </Space>
        );
      },
    },
  ];

  // Render card view for mobile
  const renderAvailableTestCard = (test: AvailableTestDto) => (
    <Card key={test.testTemplateId} className="test-card">
      <div className="test-card-header">
        <div className="test-card-title">
          <h3>{test.title}</h3>
          <Tag color="blue">{test.typeName}</Tag>
        </div>
        <div className="test-card-actions">
          <Button
            type="primary"
            icon={<PlayCircleOutlined />}
            onClick={() => {
              setSelectedTest(test);
              setStartModalVisible(true);
            }}
          >
            Start
          </Button>
        </div>
      </div>
      {test.description && (
        <Text type="secondary" style={{ fontSize: 13, display: 'block', marginTop: 8 }}>
          {test.description}
        </Text>
      )}
      <div className="test-card-info">
        <div className="test-info-item">
          <span className="test-info-label">Questions</span>
          <span className="test-info-value">
            <FileTextOutlined style={{ marginRight: 4 }} />
            {test.questionCount}
          </span>
        </div>
        <div className="test-info-item">
          <span className="test-info-label">Duration</span>
          <span className="test-info-value">
            {test.timeLimitMinutes ? (
              <>
                <ClockCircleOutlined style={{ marginRight: 4 }} />
                {test.timeLimitMinutes} min
              </>
            ) : (
              'No limit'
            )}
          </span>
        </div>
        <div className="test-info-item">
          <span className="test-info-label">Passing Score</span>
          <span className="test-info-value">
            <TrophyOutlined style={{ marginRight: 4 }} />
            {test.passingScore}%
          </span>
        </div>
        <div className="test-info-item">
          <span className="test-info-label">Attempts</span>
          <span className="test-info-value">
            {test.attemptCount} times
            {test.bestScore !== undefined && (
              <div style={{ fontSize: 12, color: 'rgba(0, 0, 0, 0.45)', fontWeight: 400 }}>
                Best: {test.bestScore}%
              </div>
            )}
          </span>
        </div>
      </div>
    </Card>
  );

  // Render history card for mobile
  const renderHistoryCard = (record: AssessmentListDto) => (
    <Card key={record.id} className="test-card">
      <div className="test-card-header">
        <div className="test-card-title">
          <h3>{record.testTemplateTitle || record.title}</h3>
          {getStatusTag(record.status)}
        </div>
        <div className="test-card-actions">
          {record.status === 3 && (
            <Button
              type="primary"
              icon={<PlayCircleOutlined />}
              onClick={() => handleContinueTest(record.id)}
            >
              Continue
            </Button>
          )}
          {record.status >= 4 && (
            <Button icon={<FileTextOutlined />} onClick={() => handleViewResult(record.id)}>
              View
            </Button>
          )}
        </div>
      </div>
      <div className="test-card-info">
        <div className="test-info-item">
          <span className="test-info-label">Started At</span>
          <span className="test-info-value">
            {record.startedAt && new Date(record.startedAt).toLocaleString('en-US')}
          </span>
        </div>
        {record.completedAt && (
          <div className="test-info-item">
            <span className="test-info-label">Completed At</span>
            <span className="test-info-value">
              {new Date(record.completedAt).toLocaleString('en-US')}
            </span>
          </div>
        )}
        {(record.score !== null && record.score !== undefined) && (
          <div className="test-info-item">
            <span className="test-info-label">Score</span>
            <span className="test-info-value">
              {record.score}/{record.maxScore}
              <div style={{ fontSize: 12, color: 'rgba(0, 0, 0, 0.45)', fontWeight: 400 }}>
                ({record.percentage?.toFixed(1)}%)
              </div>
            </span>
          </div>
        )}
      </div>
    </Card>
  );

  return (
    <div className="available-tests-container">
      <div className="page-header">
        <Title level={3}>
          <FileTextOutlined /> Skill Assessment Tests
        </Title>
      </div>

      <Tabs
        defaultActiveKey="available"
        className="tests-tabs"
        items={[
          {
            key: 'available',
            label: (
              <span>
                <PlayCircleOutlined /> Available Tests
              </span>
            ),
            children: (
              <>
                {loadingTests ? (
                  <div style={{ textAlign: 'center', padding: 50 }}>
                    <Spin size="large" />
                  </div>
                ) : availableTests && availableTests.length > 0 ? (
                  <>
                    {/* Desktop table view */}
                    <Card className="tests-table-view">
                      <Table
                        columns={availableColumns}
                        dataSource={availableTests}
                        rowKey="testTemplateId"
                        pagination={false}
                      />
                    </Card>
                    {/* Mobile card view */}
                    <div className="tests-card-view">
                      {availableTests.map(renderAvailableTestCard)}
                    </div>
                  </>
                ) : (
                  <Card>
                    <Empty description="No tests available" />
                  </Card>
                )}
              </>
            ),
          },
          {
            key: 'history',
            label: (
              <span>
                <HistoryOutlined /> Test History
              </span>
            ),
            children: (
              <>
                {loadingHistory ? (
                  <div style={{ textAlign: 'center', padding: 50 }}>
                    <Spin size="large" />
                  </div>
                ) : historyData?.items && historyData.items.length > 0 ? (
                  <>
                    {/* Desktop table view */}
                    <Card className="tests-table-view">
                      <Table
                        columns={historyColumns}
                        dataSource={historyData.items}
                        rowKey="id"
                        pagination={{
                          total: historyData.totalCount,
                          pageSize: historyData.pageSize,
                          current: historyData.pageNumber,
                          showTotal: (total) => `Total ${total} tests`,
                        }}
                      />
                    </Card>
                    {/* Mobile card view */}
                    <div className="tests-card-view">
                      {historyData.items.map(renderHistoryCard)}
                    </div>
                  </>
                ) : (
                  <Card>
                    <Empty description="No test history" />
                  </Card>
                )}
              </>
            ),
          },
        ]}
      />

      {/* Start Test Modal */}
      <Modal
        title="Start Test"
        open={startModalVisible}
        onCancel={() => setStartModalVisible(false)}
        onOk={handleStartTest}
        okText="Start Test"
        cancelText="Cancel"
        confirmLoading={startMutation.isPending}
        width={600}
      >
        {selectedTest && (
          <div>
            <Descriptions column={1} bordered size="small">
              <Descriptions.Item label="Test Name">
                <Text strong>{selectedTest.title}</Text>
              </Descriptions.Item>
              {selectedTest.description && (
                <Descriptions.Item label="Description">{selectedTest.description}</Descriptions.Item>
              )}
              <Descriptions.Item label="Type">
                <Tag color="blue">{selectedTest.typeName}</Tag>
              </Descriptions.Item>
              {selectedTest.targetSkillName && (
                <Descriptions.Item label="Skill">
                  {selectedTest.targetSkillName}
                </Descriptions.Item>
              )}
            </Descriptions>

            <Row gutter={16} style={{ marginTop: 16 }}>
              <Col span={8}>
                <Statistic
                  title="Questions"
                  value={selectedTest.questionCount}
                  prefix={<FileTextOutlined />}
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Duration"
                  value={selectedTest.timeLimitMinutes || 'No limit'}
                  suffix={selectedTest.timeLimitMinutes ? 'min' : ''}
                  prefix={<ClockCircleOutlined />}
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Passing Score"
                  value={selectedTest.passingScore}
                  suffix="%"
                  prefix={<TrophyOutlined />}
                />
              </Col>
            </Row>

            <div
              style={{
                marginTop: 16,
                padding: 12,
                background: '#f6ffed',
                border: '1px solid #b7eb8f',
                borderRadius: 6,
              }}
            >
              <Text type="secondary">
                <CheckCircleOutlined style={{ color: '#52c41a', marginRight: 8 }} />
                After starting, you can pause and continue the test later.
                {selectedTest.timeLimitMinutes &&
                  ` Maximum time limit is ${selectedTest.timeLimitMinutes} minutes.`}
              </Text>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}
