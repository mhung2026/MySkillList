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
      message.success('Bắt đầu làm bài thành công!');
      setStartModalVisible(false);
      navigate(`/assessments/take/${data.assessmentId}`);
    },
    onError: (error: Error) => {
      message.error(error.message || 'Không thể bắt đầu bài test');
    },
  });

  // Continue assessment mutation
  const continueMutation = useMutation({
    mutationFn: continueAssessment,
    onSuccess: (data) => {
      navigate(`/assessments/take/${data.assessmentId}`);
    },
    onError: (error: Error) => {
      message.error(error.message || 'Không thể tiếp tục bài test');
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
    const statusMap: Record<number, { color: string; text: string; icon: React.ReactNode }> = {
      0: { color: 'default', text: 'Scheduled', icon: <ClockCircleOutlined /> },
      1: { color: 'processing', text: 'In Progress', icon: <SyncOutlined spin /> },
      2: { color: 'success', text: 'Completed', icon: <CheckCircleOutlined /> },
      3: { color: 'warning', text: 'Pending Review', icon: <ClockCircleOutlined /> },
      4: { color: 'error', text: 'Failed', icon: <CloseCircleOutlined /> },
      5: { color: 'success', text: 'Passed', icon: <TrophyOutlined /> },
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
      title: 'Tên bài test',
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
      title: 'Loại',
      dataIndex: 'typeName',
      key: 'typeName',
      render: (text) => <Tag color="blue">{text}</Tag>,
    },
    {
      title: 'Số câu hỏi',
      dataIndex: 'questionCount',
      key: 'questionCount',
      align: 'center',
      render: (value) => <Text strong>{value}</Text>,
    },
    {
      title: 'Thời gian',
      dataIndex: 'timeLimitMinutes',
      key: 'timeLimitMinutes',
      align: 'center',
      render: (value) =>
        value ? (
          <Space>
            <ClockCircleOutlined />
            {value} phút
          </Space>
        ) : (
          <Text type="secondary">Không giới hạn</Text>
        ),
    },
    {
      title: 'Điểm đạt',
      dataIndex: 'passingScore',
      key: 'passingScore',
      align: 'center',
      render: (value) => <Text>{value}%</Text>,
    },
    {
      title: 'Đã làm',
      key: 'attempts',
      align: 'center',
      render: (_, record) => (
        <Space direction="vertical" size={0}>
          <Text>{record.attemptCount} lần</Text>
          {record.bestScore !== undefined && (
            <Text type="secondary">Tốt nhất: {record.bestScore}%</Text>
          )}
        </Space>
      ),
    },
    {
      title: 'Thao tác',
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
          Bắt đầu
        </Button>
      ),
    },
  ];

  // Columns for history
  const historyColumns: ColumnsType<AssessmentListDto> = [
    {
      title: 'Bài test',
      dataIndex: 'testTemplateTitle',
      key: 'testTemplateTitle',
      render: (text, record) => (
        <Space direction="vertical" size={0}>
          <Text strong>{text || record.title}</Text>
          <Text type="secondary" style={{ fontSize: 12 }}>
            {record.startedAt && new Date(record.startedAt).toLocaleString('vi-VN')}
          </Text>
        </Space>
      ),
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      render: (status) => getStatusTag(status),
    },
    {
      title: 'Điểm số',
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
      title: 'Thời gian hoàn thành',
      key: 'completedAt',
      render: (_, record) => {
        if (!record.completedAt) {
          return <Text type="secondary">Chưa hoàn thành</Text>;
        }
        return new Date(record.completedAt).toLocaleString('vi-VN');
      },
    },
    {
      title: 'Thao tác',
      key: 'actions',
      align: 'center',
      render: (_, record) => {
        if (record.status === 1) {
          // In Progress
          return (
            <Button
              type="primary"
              icon={<PlayCircleOutlined />}
              onClick={() => handleContinueTest(record.id)}
            >
              Tiếp tục
            </Button>
          );
        }
        if (record.status >= 2) {
          // Completed or later
          return (
            <Button icon={<FileTextOutlined />} onClick={() => handleViewResult(record.id)}>
              Xem kết quả
            </Button>
          );
        }
        return null;
      },
    },
  ];

  const inProgressAssessments = historyData?.items?.filter((a) => a.status === 1) || [];

  return (
    <div>
      <Title level={3}>
        <FileTextOutlined /> Bài kiểm tra kỹ năng
      </Title>

      {/* In Progress Alert */}
      {inProgressAssessments.length > 0 && (
        <Card
          style={{
            marginBottom: 16,
            background: '#fff7e6',
            borderColor: '#ffd591',
          }}
        >
          <Space direction="vertical" style={{ width: '100%' }}>
            <Text strong style={{ color: '#d46b08' }}>
              <SyncOutlined spin /> Bạn có {inProgressAssessments.length} bài test đang làm dở
            </Text>
            <Space wrap>
              {inProgressAssessments.map((assessment) => (
                <Button
                  key={assessment.id}
                  type="primary"
                  ghost
                  icon={<PlayCircleOutlined />}
                  onClick={() => handleContinueTest(assessment.id)}
                >
                  Tiếp tục: {assessment.testTemplateTitle || assessment.title}
                </Button>
              ))}
            </Space>
          </Space>
        </Card>
      )}

      <Tabs
        defaultActiveKey="available"
        items={[
          {
            key: 'available',
            label: (
              <span>
                <PlayCircleOutlined /> Bài test có thể làm
              </span>
            ),
            children: (
              <Card>
                {loadingTests ? (
                  <div style={{ textAlign: 'center', padding: 50 }}>
                    <Spin size="large" />
                  </div>
                ) : availableTests && availableTests.length > 0 ? (
                  <Table
                    columns={availableColumns}
                    dataSource={availableTests}
                    rowKey="testTemplateId"
                    pagination={false}
                  />
                ) : (
                  <Empty description="Không có bài test nào" />
                )}
              </Card>
            ),
          },
          {
            key: 'history',
            label: (
              <span>
                <HistoryOutlined /> Lịch sử làm bài
              </span>
            ),
            children: (
              <Card>
                {loadingHistory ? (
                  <div style={{ textAlign: 'center', padding: 50 }}>
                    <Spin size="large" />
                  </div>
                ) : historyData?.items && historyData.items.length > 0 ? (
                  <Table
                    columns={historyColumns}
                    dataSource={historyData.items}
                    rowKey="id"
                    pagination={{
                      total: historyData.totalCount,
                      pageSize: historyData.pageSize,
                      current: historyData.pageNumber,
                      showTotal: (total) => `Tổng ${total} bài`,
                    }}
                  />
                ) : (
                  <Empty description="Chưa có lịch sử làm bài" />
                )}
              </Card>
            ),
          },
        ]}
      />

      {/* Start Test Modal */}
      <Modal
        title="Bắt đầu làm bài test"
        open={startModalVisible}
        onCancel={() => setStartModalVisible(false)}
        onOk={handleStartTest}
        okText="Bắt đầu làm bài"
        cancelText="Hủy"
        confirmLoading={startMutation.isPending}
        width={600}
      >
        {selectedTest && (
          <div>
            <Descriptions column={1} bordered size="small">
              <Descriptions.Item label="Tên bài test">
                <Text strong>{selectedTest.title}</Text>
              </Descriptions.Item>
              {selectedTest.description && (
                <Descriptions.Item label="Mô tả">{selectedTest.description}</Descriptions.Item>
              )}
              <Descriptions.Item label="Loại">
                <Tag color="blue">{selectedTest.typeName}</Tag>
              </Descriptions.Item>
              {selectedTest.targetSkillName && (
                <Descriptions.Item label="Kỹ năng">
                  {selectedTest.targetSkillName}
                </Descriptions.Item>
              )}
            </Descriptions>

            <Row gutter={16} style={{ marginTop: 16 }}>
              <Col span={8}>
                <Statistic
                  title="Số câu hỏi"
                  value={selectedTest.questionCount}
                  prefix={<FileTextOutlined />}
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Thời gian"
                  value={selectedTest.timeLimitMinutes || 'Không giới hạn'}
                  suffix={selectedTest.timeLimitMinutes ? 'phút' : ''}
                  prefix={<ClockCircleOutlined />}
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Điểm đạt"
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
                Sau khi bắt đầu, bạn có thể tạm dừng và tiếp tục làm bài sau.
                {selectedTest.timeLimitMinutes &&
                  ` Thời gian làm bài tối đa là ${selectedTest.timeLimitMinutes} phút.`}
              </Text>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}
