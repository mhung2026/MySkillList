import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Card,
  Typography,
  Space,
  Spin,
  Alert,
  Button,
  Row,
  Col,
  Statistic,
  Table,
  Tag,
  Modal,
  Form,
  InputNumber,
  DatePicker,
  Switch,
  message,
  Divider,
  Tabs,
} from 'antd';
import {
  ReloadOutlined,
  WarningOutlined,
  CheckCircleOutlined,
  RiseOutlined,
  BookOutlined,
  TrophyOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useAuth } from '../../contexts/AuthContext';
import {
  getGapAnalysis,
  recalculateGaps,
  bulkRecalculateGapsForAllEmployees,
  seedSampleData,
  createLearningPath,
  getLearningPaths,
  getLearningRecommendations,
  type SkillGapDetail,
  type CreateLearningPathRequest,
  type LearningRecommendation,
} from '../../api/employees';
import LearningPathRecommendations from '../../components/learning/LearningPathRecommendations';
import './SkillGapAnalysis.css';

const { Title, Text } = Typography;
const { TabPane } = Tabs;

export default function SkillGapAnalysis() {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const [selectedGap, setSelectedGap] = useState<SkillGapDetail | null>(null);
  const [learningPathModalVisible, setLearningPathModalVisible] = useState(false);
  const [form] = Form.useForm();

  const employeeId = user?.id || '';

  // Fetch gap analysis
  const {
    data: gapAnalysis,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['gapAnalysis', employeeId],
    queryFn: () => getGapAnalysis(employeeId),
    enabled: !!employeeId,
  });

  // Fetch learning recommendations
  const {
    data: learningRecommendations,
    isLoading: isLoadingRecommendations,
  } = useQuery({
    queryKey: ['learningRecommendations', employeeId],
    queryFn: () => getLearningRecommendations(employeeId),
    enabled: !!employeeId,
  });

  // Fetch learning paths
  const {
    data: learningPaths,
    isLoading: isLoadingPaths,
  } = useQuery({
    queryKey: ['learningPaths', employeeId],
    queryFn: () => getLearningPaths(employeeId),
    enabled: !!employeeId,
  });

  // Recalculate gaps mutation
  const recalculateMutation = useMutation({
    mutationFn: () => recalculateGaps(employeeId),
    onSuccess: (result) => {
      message.success(
        `Recalculated: ${result.gapsCreated} created, ${result.gapsUpdated} updated, ${result.gapsResolved} resolved`
      );
      queryClient.invalidateQueries({ queryKey: ['gapAnalysis', employeeId] });
      queryClient.invalidateQueries({ queryKey: ['learningRecommendations', employeeId] });
    },
    onError: () => {
      message.error('Failed to recalculate gaps');
    },
  });

  // Bulk recalculate gaps for all employees mutation
  const bulkRecalculateMutation = useMutation({
    mutationFn: bulkRecalculateGapsForAllEmployees,
    onSuccess: (result) => {
      message.success(
        `✅ Bulk Recalculation Complete!\n` +
        `Processed: ${result.employeesProcessed} employees\n` +
        `Total Gaps - Created: ${result.totalGapsCreated}, Updated: ${result.totalGapsUpdated}, Resolved: ${result.totalGapsResolved}`,
        10
      );
      // Refresh current employee's gaps
      queryClient.invalidateQueries({ queryKey: ['gapAnalysis', employeeId] });
      queryClient.invalidateQueries({ queryKey: ['learningRecommendations', employeeId] });
    },
    onError: () => {
      message.error('Failed to bulk recalculate gaps for all employees');
    },
  });

  // Seed sample data mutation
  const seedDataMutation = useMutation({
    mutationFn: seedSampleData,
    onSuccess: (result) => {
      message.success(
        `🎉 Database Seeded Successfully!\n` +
        `Skills: ${result.skillsCreated}, Roles: ${result.jobRolesCreated}, Teams: ${result.teamsCreated}\n` +
        `Employees: ${result.employeesCreated}, Gaps Created: ${result.gapsCreated}`,
        12
      );
      // Refresh all data
      queryClient.invalidateQueries({ queryKey: ['gapAnalysis'] });
      queryClient.invalidateQueries({ queryKey: ['learningRecommendations'] });
      window.location.reload(); // Full reload to refresh everything
    },
    onError: () => {
      message.error('Failed to seed database');
    },
  });

  // Create learning path mutation
  const createPathMutation = useMutation({
    mutationFn: (request: CreateLearningPathRequest) => createLearningPath(employeeId, request),
    onSuccess: (result) => {
      message.success(result.message || 'Learning path created successfully!');
      queryClient.invalidateQueries({ queryKey: ['learningPaths', employeeId] });
      setLearningPathModalVisible(false);
      form.resetFields();
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to create learning path');
    },
  });

  const handleCreateLearningPath = (gap: SkillGapDetail) => {
    setSelectedGap(gap);
    form.setFieldsValue({
      targetLevel: gap.expectedLevel || gap.requiredLevel,
      timeConstraintMonths: 6,
      useAiGeneration: true,
    });
    setLearningPathModalVisible(true);
  };

  const handleSubmitLearningPath = async () => {
    try {
      const values = await form.validateFields();
      if (!selectedGap) return;

      const request: CreateLearningPathRequest = {
        targetSkillId: selectedGap.skillId,
        targetLevel: values.targetLevel,
        targetCompletionDate: values.targetCompletionDate?.toISOString(),
        timeConstraintMonths: values.timeConstraintMonths,
        useAiGeneration: values.useAiGeneration,
      };

      createPathMutation.mutate(request);
    } catch {
      // Form validation failed
    }
  };

  const getPriorityColor = (priority: string) => {
    const colors: Record<string, string> = {
      Critical: 'red',
      High: 'orange',
      Medium: 'blue',
      Low: 'default',
      Met: 'green',
    };
    return colors[priority] || 'default';
  };

  // Render card view for mobile
  const renderSkillGapCard = (gap: SkillGapDetail) => {
    const cardClass = `skill-gap-card ${
      gap.isMet ? 'met' : gap.priority === 'Critical' ? 'critical' : gap.priority === 'High' ? 'high' : ''
    }`;

    return (
      <Card key={gap.skillId} className={cardClass}>
        <div className="skill-gap-card-header">
          <div className="skill-gap-card-title">
            <h4>
              {gap.isMet && <CheckCircleOutlined style={{ color: '#52c41a' }} />}
              <span style={{ color: gap.isMet ? '#52c41a' : undefined }}>{gap.skillName}</span>
            </h4>
            <div className="skill-gap-card-code">{gap.skillCode}</div>
          </div>
          <div className="skill-gap-card-action">
            <Button
              type="primary"
              size="small"
              icon={<BookOutlined />}
              onClick={() => handleCreateLearningPath(gap)}
            >
              Tạo lộ trình
            </Button>
          </div>
        </div>

        <div className="skill-gap-card-info">
          <div className="skill-gap-info-item">
            <span className="skill-gap-info-label">Current Level</span>
            <span className="skill-gap-info-value">
              <Tag color={gap.isMet ? 'green' : 'blue'}>
                L{gap.currentLevel}: {gap.currentLevelName}
              </Tag>
            </span>
          </div>

          <div className="skill-gap-info-item">
            <span className="skill-gap-info-label">Required Level</span>
            <span className="skill-gap-info-value">
              <Tag color={gap.isMet ? 'green' : 'orange'}>
                L{gap.requiredLevel}: {gap.requiredLevelName}
              </Tag>
            </span>
          </div>

          <div className="skill-gap-info-item">
            <span className="skill-gap-info-label">Gap</span>
            <span className="skill-gap-info-value">
              {gap.isMet ? (
                <Tag color="success" icon={<CheckCircleOutlined />}>
                  Met ✓
                </Tag>
              ) : (
                <Tag color={gap.gapSize >= 3 ? 'red' : gap.gapSize >= 2 ? 'orange' : 'default'}>
                  {gap.gapSize} {gap.gapSize === 1 ? 'level' : 'levels'}
                </Tag>
              )}
            </span>
          </div>

          <div className="skill-gap-info-item">
            <span className="skill-gap-info-label">Priority</span>
            <span className="skill-gap-info-value">
              <Tag color={getPriorityColor(gap.priority)} icon={gap.isMet ? <CheckCircleOutlined /> : undefined}>
                {gap.priority}
              </Tag>
            </span>
          </div>

          {gap.isMandatory && (
            <div className="skill-gap-info-item">
              <span className="skill-gap-info-label">Mandatory</span>
              <span className="skill-gap-info-value">
                <CheckCircleOutlined style={{ color: '#52c41a', fontSize: 18 }} />
              </span>
            </div>
          )}
        </div>

        {/* Expandable section for AI analysis */}
        {(gap.aiAnalysis || gap.aiRecommendation) && (
          <div className="skill-gap-card-expandable">
            <Space direction="vertical" style={{ width: '100%' }} size="small">
              {gap.aiAnalysis && (
                <Alert
                  message="AI Analysis"
                  description={gap.aiAnalysis}
                  type="info"
                  showIcon
                  style={{ fontSize: 13 }}
                />
              )}
              {gap.aiRecommendation && (
                <Alert
                  message="AI Recommendation"
                  description={gap.aiRecommendation}
                  type="success"
                  showIcon
                  icon={<RiseOutlined />}
                  style={{ fontSize: 13 }}
                />
              )}
            </Space>
          </div>
        )}
      </Card>
    );
  };

  const columns: ColumnsType<SkillGapDetail> = [
    {
      title: 'Skill',
      dataIndex: 'skillName',
      key: 'skillName',
      render: (text, record) => (
        <Space direction="vertical" size={0}>
          <Space>
            {record.isMet && <CheckCircleOutlined style={{ color: '#52c41a', fontSize: 16 }} />}
            <Text strong style={{ color: record.isMet ? '#52c41a' : undefined }}>{text}</Text>
          </Space>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {record.skillCode}
          </Text>
        </Space>
      ),
    },
    {
      title: 'Current',
      dataIndex: 'currentLevel',
      key: 'currentLevel',
      align: 'center',
      render: (level, record) => (
        <Tag color={record.isMet ? 'green' : 'blue'}>
          L{level}: {record.currentLevelName}
        </Tag>
      ),
    },
    {
      title: 'Required',
      dataIndex: 'requiredLevel',
      key: 'requiredLevel',
      align: 'center',
      render: (level, record) => (
        <Tag color={record.isMet ? 'green' : 'orange'}>
          L{level}: {record.requiredLevelName}
        </Tag>
      ),
    },
    {
      title: 'Gap',
      dataIndex: 'gapSize',
      key: 'gapSize',
      align: 'center',
      sorter: (a, b) => b.gapSize - a.gapSize,
      render: (gap, record) => {
        if (record.isMet) {
          return (
            <Tag color="success" icon={<CheckCircleOutlined />}>
              Met ✓
            </Tag>
          );
        }
        return (
          <Tag color={gap >= 3 ? 'red' : gap >= 2 ? 'orange' : 'default'}>
            {gap} {gap === 1 ? 'level' : 'levels'}
          </Tag>
        );
      },
    },
    {
      title: 'Priority',
      dataIndex: 'priority',
      key: 'priority',
      align: 'center',
      filters: [
        { text: 'Critical', value: 'Critical' },
        { text: 'High', value: 'High' },
        { text: 'Medium', value: 'Medium' },
        { text: 'Low', value: 'Low' },
        { text: 'Met ✓', value: 'Met' },
      ],
      onFilter: (value, record) => record.priority === value,
      render: (priority, record) => (
        <Tag color={getPriorityColor(priority)} icon={record.isMet ? <CheckCircleOutlined /> : undefined}>
          {priority}
        </Tag>
      ),
    },
    {
      title: 'Mandatory',
      dataIndex: 'isMandatory',
      key: 'isMandatory',
      align: 'center',
      render: (mandatory) => (mandatory ? <CheckCircleOutlined style={{ color: '#52c41a' }} /> : '-'),
    },
    {
      title: 'Actions',
      key: 'actions',
      align: 'center',
      render: (_, record) => (
        <Button
          type="primary"
          size="small"
          icon={<BookOutlined />}
          onClick={() => handleCreateLearningPath(record)}
        >
          Tạo lộ trình
        </Button>
      ),
    },
  ];

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: 100 }}>
        <Spin size="large" />
        <div style={{ marginTop: 16 }}>Loading gap analysis...</div>
      </div>
    );
  }

  if (error || !gapAnalysis) {
    return (
      <Card>
        <Alert
          message="Failed to load gap analysis"
          description="Please try again later or contact support."
          type="error"
          showIcon
        />
      </Card>
    );
  }

  return (
    <div className="skill-gap-analysis-container">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
        <Title level={3}>
          <TrophyOutlined /> Skill Gap Analysis
        </Title>
        <Space wrap>
          <Button
            icon={<ReloadOutlined />}
            onClick={() => recalculateMutation.mutate()}
            loading={recalculateMutation.isPending}
          >
            Recalculate My Gaps
          </Button>
          <Button
            type="primary"
            danger
            icon={<ReloadOutlined />}
            onClick={() => {
              Modal.confirm({
                title: 'Bulk Recalculate All Employee Gaps?',
                content: 'This will recalculate skill gaps for ALL employees in the system. This operation may take some time. Continue?',
                okText: 'Yes, Recalculate All',
                cancelText: 'Cancel',
                onOk: () => bulkRecalculateMutation.mutate(),
              });
            }}
            loading={bulkRecalculateMutation.isPending}
          >
            Recalculate All Employees
          </Button>
          <Button
            type="dashed"
            style={{ borderColor: '#52c41a', color: '#52c41a' }}
            icon={<BookOutlined />}
            onClick={() => {
              Modal.confirm({
                title: '🎲 Seed Sample Database?',
                content: (
                  <div>
                    <p>This will create sample data including:</p>
                    <ul>
                      <li>15 Skills (C#, React, SQL, etc.)</li>
                      <li>4 Job Roles (Junior, Mid, Senior, Tech Lead)</li>
                      <li>3 Teams</li>
                      <li>7 Employees with assigned skills</li>
                      <li>Skill gaps automatically calculated</li>
                    </ul>
                    <p><strong>Note:</strong> Only creates data if database is empty.</p>
                  </div>
                ),
                okText: 'Yes, Seed Database',
                cancelText: 'Cancel',
                onOk: () => seedDataMutation.mutate(),
                width: 550,
              });
            }}
            loading={seedDataMutation.isPending}
          >
            🎲 Seed Sample Data
          </Button>
        </Space>
      </div>

      {/* Employee Info */}
      <Card style={{ marginBottom: 24 }}>
        <Space direction="vertical" style={{ width: '100%' }}>
          <Title level={4}>{gapAnalysis.employee.fullName}</Title>
          <Space size="large" wrap>
            <Text>
              <strong>Current Role:</strong> {gapAnalysis.currentRole?.name || 'Not assigned'}
            </Text>
            <Text>
              <strong>Target Role:</strong> {gapAnalysis.targetRole.name}
            </Text>
            <Text>
              <strong>Team:</strong> {gapAnalysis.employee.team?.name || 'Not assigned'}
            </Text>
          </Space>
        </Space>
      </Card>

      {/* Summary Statistics */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Overall Readiness"
              value={gapAnalysis.summary.overallReadiness}
              precision={1}
              suffix="%"
              valueStyle={{ color: gapAnalysis.summary.overallReadiness >= 80 ? '#3f8600' : '#cf1322' }}
              prefix={
                gapAnalysis.summary.overallReadiness >= 80 ? (
                  <CheckCircleOutlined />
                ) : (
                  <WarningOutlined />
                )
              }
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Total Gaps"
              value={gapAnalysis.summary.totalGaps}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Critical Gaps"
              value={gapAnalysis.summary.criticalGaps}
              valueStyle={{ color: '#cf1322' }}
              prefix={<WarningOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Requirements Met"
              value={gapAnalysis.summary.metRequirements}
              valueStyle={{ color: '#3f8600' }}
              prefix={<CheckCircleOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {/* Tabs */}
      <Tabs defaultActiveKey="gaps">
        <TabPane tab="Skill Gaps" key="gaps">
          {/* Desktop table view */}
          <Card className="gaps-table-view">
            <Table
              columns={columns}
              dataSource={gapAnalysis.gaps}
              rowKey="skillId"
              pagination={{ pageSize: 10 }}
              expandable={{
                expandedRowRender: (record) => (
                  <Space direction="vertical" style={{ width: '100%' }}>
                    {record.aiAnalysis && (
                      <Alert
                        message="AI Analysis"
                        description={record.aiAnalysis}
                        type="info"
                        showIcon
                      />
                    )}
                    {record.aiRecommendation && (
                      <Alert
                        message="AI Recommendation"
                        description={record.aiRecommendation}
                        type="success"
                        showIcon
                        icon={<RiseOutlined />}
                      />
                    )}
                  </Space>
                ),
                rowExpandable: (record) => !!(record.aiAnalysis || record.aiRecommendation),
              }}
            />
          </Card>
          {/* Mobile card view */}
          <div className="gaps-card-view">
            {gapAnalysis.gaps.map(renderSkillGapCard)}
          </div>
        </TabPane>

        <TabPane tab={
          <span>
            <RiseOutlined /> AI Recommendations
          </span>
        } key="aiRecommendations">
          {isLoadingRecommendations ? (
            <Card>
              <div style={{ textAlign: 'center', padding: 40 }}>
                <Spin size="large" />
                <div style={{ marginTop: 16 }}>Loading recommendations...</div>
              </div>
            </Card>
          ) : learningRecommendations && learningRecommendations.length > 0 ? (
            <Space direction="vertical" size="large" style={{ width: '100%' }}>
              {/* Group recommendations by skill */}
              {Object.entries(
                learningRecommendations.reduce((acc, rec) => {
                  if (!acc[rec.skillId]) {
                    acc[rec.skillId] = {
                      skillName: rec.skillName,
                      recommendations: [],
                    };
                  }
                  acc[rec.skillId].recommendations.push(rec);
                  return acc;
                }, {} as Record<string, { skillName: string; recommendations: LearningRecommendation[] }>)
              ).map(([skillId, { skillName, recommendations }]) => (
                <Card
                  key={skillId}
                  title={
                    <Space>
                      <TrophyOutlined style={{ color: '#1890ff' }} />
                      <Text strong>{skillName}</Text>
                      <Tag color="purple">
                        {recommendations.length} {recommendations.length === 1 ? 'course' : 'courses'}
                      </Tag>
                    </Space>
                  }
                >
                  <Space direction="vertical" size="middle" style={{ width: '100%' }}>
                    {recommendations
                      .sort((a, b) => a.displayOrder - b.displayOrder)
                      .map((rec) => (
                        <Card key={rec.id} size="small" style={{ backgroundColor: '#fafafa' }}>
                          <Space direction="vertical" style={{ width: '100%' }}>
                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                              <Space>
                                <Tag color="blue">{rec.recommendationType}</Tag>
                                <Text strong>{rec.title}</Text>
                              </Space>
                              {rec.estimatedHours && (
                                <Tag icon={<BookOutlined />} color="green">
                                  {rec.estimatedHours}h
                                </Tag>
                              )}
                            </div>

                            {rec.description && (
                              <Text type="secondary" style={{ fontSize: 13 }}>
                                {rec.description.length > 300
                                  ? `${rec.description.substring(0, 300)}...`
                                  : rec.description}
                              </Text>
                            )}

                            {rec.rationale && (
                              <Alert
                                message="AI Rationale"
                                description={rec.rationale}
                                type="info"
                                showIcon
                                icon={<RiseOutlined />}
                              />
                            )}

                            {rec.url && (
                              <Button
                                type="primary"
                                icon={<BookOutlined />}
                                href={rec.url}
                                target="_blank"
                                rel="noopener noreferrer"
                              >
                                View Course on Coursera →
                              </Button>
                            )}
                          </Space>
                        </Card>
                      ))}
                  </Space>
                </Card>
              ))}
            </Space>
          ) : (
            <Card>
              <Alert
                message="Chưa có đề xuất khóa học"
                description="Hoàn thành một self-assessment để nhận đề xuất khóa học Coursera từ AI"
                type="info"
                showIcon
                icon={<RiseOutlined />}
              />
            </Card>
          )}
        </TabPane>

        <TabPane tab="Learning Path" key="learningPath">
          {isLoadingPaths ? (
            <Card>
              <div style={{ textAlign: 'center', padding: 40 }}>
                <Spin size="large" />
                <div style={{ marginTop: 16 }}>Loading learning paths...</div>
              </div>
            </Card>
          ) : learningPaths && learningPaths.length > 0 ? (
            <Space direction="vertical" size="large" style={{ width: '100%' }}>
              {learningPaths.map((path) => (
                <LearningPathRecommendations key={path.id} learningPath={path} />
              ))}
            </Space>
          ) : (
            <Card>
              <Alert
                message="Chưa có lộ trình học tập"
                description="Hãy chọn một skill gap và tạo lộ trình học tập với AI"
                type="info"
                showIcon
                icon={<BookOutlined />}
              />
            </Card>
          )}
        </TabPane>
      </Tabs>

      {/* Create Learning Path Modal */}
      <Modal
        title={
          <Space>
            <BookOutlined />
            <span>Tạo lộ trình học tập cho: {selectedGap?.skillName}</span>
          </Space>
        }
        open={learningPathModalVisible}
        onCancel={() => {
          setLearningPathModalVisible(false);
          form.resetFields();
        }}
        onOk={handleSubmitLearningPath}
        okText="Tạo lộ trình"
        cancelText="Hủy"
        confirmLoading={createPathMutation.isPending}
        width={600}
      >
        <Divider />
        {selectedGap && (
          <Alert
            message={
              <Space>
                <Text>Current: Level {selectedGap.currentLevel}</Text>
                <Text>→</Text>
                <Text>Target: Level {selectedGap.requiredLevel}</Text>
                <Text type="danger">(Gap: {selectedGap.gapSize} levels)</Text>
              </Space>
            }
            type="warning"
            showIcon
            style={{ marginBottom: 24 }}
          />
        )}

        <Form form={form} layout="vertical">
          <Form.Item
            name="targetLevel"
            label="Target Level"
            rules={[{ required: true, message: 'Please enter target level' }]}
          >
            <InputNumber min={1} max={7} style={{ width: '100%' }} />
          </Form.Item>

          <Form.Item name="timeConstraintMonths" label="Time Constraint (Months)">
            <InputNumber min={1} max={24} style={{ width: '100%' }} placeholder="6" />
          </Form.Item>

          <Form.Item name="targetCompletionDate" label="Target Completion Date">
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>

          <Form.Item
            name="useAiGeneration"
            label="Use AI to generate learning path"
            valuePropName="checked"
          >
            <Switch defaultChecked />
          </Form.Item>

          <Alert
            message="AI sẽ phân tích skill gap và đề xuất lộ trình học tập với các khóa học Coursera phù hợp"
            type="info"
            showIcon
            icon={<RiseOutlined />}
          />
        </Form>
      </Modal>
    </div>
  );
}
