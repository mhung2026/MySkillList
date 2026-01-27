import { useState } from 'react';
import {
  Card,
  Typography,
  Space,
  Spin,
  Alert,
  Button,
  Divider,
  Tag,
  Row,
  Col,
  Collapse,
} from 'antd';
import {
  TrophyOutlined,
  ReloadOutlined,
  FileTextOutlined,
  CheckCircleOutlined,
  WarningOutlined,
  RiseOutlined,
  BarChartOutlined,
} from '@ant-design/icons';
import { useAuth } from '../../contexts/AuthContext';
import './SelfAssessment.css';

const { Title, Text, Paragraph } = Typography;
const { Panel } = Collapse;

interface AssessmentReport {
  skillName: string;
  inferredLevel: string;
  confidence: 'High' | 'Medium' | 'Low';
  behavioralEvidence: string[];
  observedLimitations: string[];
  developmentRecommendations: string[];
  levelAnalysis: {
    level: string;
    evidence: string;
    result: 'achieved' | 'not_achieved';
  }[];
}

interface SFIAAssessmentData {
  candidateName: string;
  assessmentDate: string;
  totalQuestions: number;
  skillsAssessed: string[];
  overallCapability: string;
  operationalReadiness: string;
  autonomyLevel: string;
  assessments: AssessmentReport[];
}

export default function SelfAssessment() {
  const { user } = useAuth();
  const [loading, setLoading] = useState(false);
  const [assessmentData, setAssessmentData] = useState<SFIAAssessmentData | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Mock data for demonstration
  const mockData: SFIAAssessmentData = {
    candidateName: user?.fullName || 'User',
    assessmentDate: new Date().toISOString(),
    totalQuestions: 24,
    skillsAssessed: ['Programming', 'System Design', 'Problem Solving'],
    overallCapability: 'SFIA Level 3',
    operationalReadiness: 'Can operate independently within clear scope',
    autonomyLevel: 'Autonomous within defined boundaries',
    assessments: [
      {
        skillName: 'Programming',
        inferredLevel: 'Level 3',
        confidence: 'High',
        behavioralEvidence: [
          'Consistently makes independent decisions within clear scope',
          'Minimal dependency on confirmation when documentation is adequate',
          'Prioritizes impact assessment before taking action',
        ],
        observedLimitations: [
          'Avoids decisions when scope is unclear',
          'Tends to wait for confirmation in high-risk situations',
          'Has not demonstrated ownership of overall quality',
        ],
        developmentRecommendations: [
          'Assigned tasks with broader scope',
          'Participation in design/priority decisions',
          'Practice reporting risks and proposing solutions',
        ],
        levelAnalysis: [
          { level: 'L1', evidence: 'Behavior requires supervision', result: 'not_achieved' },
          { level: 'L2', evidence: 'Follows instructions', result: 'achieved' },
          { level: 'L3', evidence: 'Autonomous within scope', result: 'achieved' },
          { level: 'L4', evidence: 'Responsible for approach', result: 'not_achieved' },
        ],
      },
      {
        skillName: 'System Design',
        inferredLevel: 'Level 2',
        confidence: 'Medium',
        behavioralEvidence: [
          'Follows established patterns and guidelines',
          'Seeks guidance when facing uncertainty',
          'Implements solutions based on clear requirements',
        ],
        observedLimitations: [
          'Limited experience with architectural decisions',
          'Requires guidance for complex system trade-offs',
          'Hesitant to propose alternative approaches',
        ],
        developmentRecommendations: [
          'Study system architecture patterns',
          'Participate in design reviews',
          'Practice evaluating design trade-offs',
        ],
        levelAnalysis: [
          { level: 'L1', evidence: 'Behavior requires supervision', result: 'not_achieved' },
          { level: 'L2', evidence: 'Follows instructions', result: 'achieved' },
          { level: 'L3', evidence: 'Autonomous within scope', result: 'not_achieved' },
        ],
      },
    ],
  };

  const handleGenerateReport = async () => {
    setLoading(true);
    setError(null);

    try {
      // TODO: Call actual API to fetch test history and generate report
      // For now, using mock data
      await new Promise(resolve => setTimeout(resolve, 2000)); // Simulate API call
      setAssessmentData(mockData);
    } catch (err) {
      setError('Failed to generate assessment report. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const getConfidenceColor = (confidence: string) => {
    switch (confidence) {
      case 'High': return 'green';
      case 'Medium': return 'orange';
      case 'Low': return 'red';
      default: return 'default';
    }
  };

  const getLevelColor = (level: string) => {
    const levelNum = parseInt(level.replace('Level ', ''));
    if (levelNum >= 5) return '#52c41a';
    if (levelNum >= 3) return '#1890ff';
    return '#faad14';
  };

  return (
    <div className="self-assessment-container">
      <div className="self-assessment-header">
        <Space align="center" size="large">
          <TrophyOutlined style={{ fontSize: 32, color: '#1890ff' }} />
          <div>
            <Title level={2} style={{ margin: 0 }}>
              Đánh Giá Năng Lực Cá Nhân
            </Title>
            <Text type="secondary">
              Phân tích năng lực dựa trên lịch sử làm bài test và chuẩn SFIA
            </Text>
          </div>
        </Space>
      </div>

      {!assessmentData && !loading && (
        <Card className="welcome-card">
          <Space direction="vertical" size="large" style={{ width: '100%', textAlign: 'center' }}>
            <FileTextOutlined style={{ fontSize: 64, color: '#1890ff' }} />
            <div>
              <Title level={4}>Chào mừng đến với Đánh Giá Năng Lực Cá Nhân</Title>
              <Paragraph type="secondary">
                Hệ thống sẽ phân tích lịch sử làm bài test của bạn và tạo ra báo cáo đánh giá năng lực
                theo chuẩn SFIA (Skills Framework for the Information Age).
              </Paragraph>
              <Paragraph type="secondary">
                Báo cáo sẽ bao gồm:
              </Paragraph>
              <ul style={{ textAlign: 'left', maxWidth: 500, margin: '0 auto' }}>
                <li>Cấp độ SFIA được suy luận cho từng kỹ năng</li>
                <li>Bằng chứng hành vi quan sát được</li>
                <li>Điểm mạnh và điểm cần cải thiện</li>
                <li>Khuyến nghị phát triển cụ thể</li>
              </ul>
            </div>
            <Button
              type="primary"
              size="large"
              icon={<FileTextOutlined />}
              onClick={handleGenerateReport}
            >
              Tạo Báo Cáo Đánh Giá
            </Button>
          </Space>
        </Card>
      )}

      {loading && (
        <Card>
          <div style={{ textAlign: 'center', padding: '60px 0' }}>
            <Spin size="large" />
            <div style={{ marginTop: 16 }}>
              <Text>Đang phân tích lịch sử test của bạn...</Text>
            </div>
          </div>
        </Card>
      )}

      {error && (
        <Alert
          message="Lỗi"
          description={error}
          type="error"
          showIcon
          closable
          onClose={() => setError(null)}
          style={{ marginBottom: 16 }}
        />
      )}

      {assessmentData && !loading && (
        <>
          {/* Overview Card */}
          <Card className="overview-card" style={{ marginBottom: 16 }}>
            <Row gutter={[16, 16]}>
              <Col xs={24} sm={12} md={6}>
                <Statistic
                  title="Ngày Đánh Giá"
                  value={new Date(assessmentData.assessmentDate).toLocaleDateString('vi-VN')}
                  prefix={<FileTextOutlined />}
                />
              </Col>
              <Col xs={24} sm={12} md={6}>
                <Statistic
                  title="Tổng Số Câu Hỏi"
                  value={assessmentData.totalQuestions}
                  prefix={<CheckCircleOutlined />}
                />
              </Col>
              <Col xs={24} sm={12} md={6}>
                <Statistic
                  title="Kỹ Năng Đánh Giá"
                  value={assessmentData.skillsAssessed.length}
                  prefix={<TrophyOutlined />}
                />
              </Col>
              <Col xs={24} sm={12} md={6}>
                <Button
                  icon={<ReloadOutlined />}
                  onClick={handleGenerateReport}
                  loading={loading}
                  block
                >
                  Làm Mới Báo Cáo
                </Button>
              </Col>
            </Row>
          </Card>

          {/* Executive Summary */}
          <Card
            title={
              <Space>
                <BarChartOutlined />
                <span>Tổng Quan Năng Lực</span>
              </Space>
            }
            style={{ marginBottom: 16 }}
            className="summary-card"
          >
            <Row gutter={[16, 16]}>
              <Col xs={24} md={8}>
                <Card bordered={false} className="metric-card">
                  <Statistic
                    title="Năng Lực Tổng Thể"
                    value={assessmentData.overallCapability}
                    valueStyle={{ color: '#1890ff', fontSize: 20 }}
                  />
                </Card>
              </Col>
              <Col xs={24} md={8}>
                <Card bordered={false} className="metric-card">
                  <div>
                    <Text type="secondary" style={{ display: 'block', marginBottom: 8 }}>
                      Mức Độ Sẵn Sàng
                    </Text>
                    <Text strong style={{ fontSize: 16 }}>
                      {assessmentData.operationalReadiness}
                    </Text>
                  </div>
                </Card>
              </Col>
              <Col xs={24} md={8}>
                <Card bordered={false} className="metric-card">
                  <div>
                    <Text type="secondary" style={{ display: 'block', marginBottom: 8 }}>
                      Mức Độ Tự Chủ
                    </Text>
                    <Text strong style={{ fontSize: 16 }}>
                      {assessmentData.autonomyLevel}
                    </Text>
                  </div>
                </Card>
              </Col>
            </Row>
          </Card>

          {/* Skills Assessment Details */}
          <Card
            title={
              <Space>
                <TrophyOutlined />
                <span>Đánh Giá Chi Tiết Theo Kỹ Năng</span>
              </Space>
            }
            className="skills-card"
          >
            <Collapse defaultActiveKey={[0]} className="assessment-collapse">
              {assessmentData.assessments.map((assessment, index) => (
                <Panel
                  key={index}
                  header={
                    <Space className="panel-header">
                      <Text strong style={{ fontSize: 16 }}>
                        {assessment.skillName}
                      </Text>
                      <Tag color={getLevelColor(assessment.inferredLevel)} style={{ fontSize: 14 }}>
                        {assessment.inferredLevel}
                      </Tag>
                      <Tag color={getConfidenceColor(assessment.confidence)}>
                        Độ tin cậy: {assessment.confidence}
                      </Tag>
                    </Space>
                  }
                >
                  <Space direction="vertical" size="large" style={{ width: '100%' }}>
                    {/* Level Analysis */}
                    <div>
                      <Title level={5}>
                        <CheckCircleOutlined /> Phân Tích Theo Cấp Độ SFIA
                      </Title>
                      <div className="level-analysis">
                        {assessment.levelAnalysis.map((level, idx) => (
                          <div key={idx} className="level-item">
                            <Space>
                              {level.result === 'achieved' ? (
                                <CheckCircleOutlined style={{ color: '#52c41a' }} />
                              ) : (
                                <CloseCircleOutlined style={{ color: '#d9d9d9' }} />
                              )}
                              <Text strong>{level.level}</Text>
                              <Text type="secondary">{level.evidence}</Text>
                              <Tag color={level.result === 'achieved' ? 'success' : 'default'}>
                                {level.result === 'achieved' ? 'Đạt' : 'Chưa đạt'}
                              </Tag>
                            </Space>
                          </div>
                        ))}
                      </div>
                      <Divider />
                      <Alert
                        message={`Kết luận: Level cao nhất đạt nhất quán là ${assessment.inferredLevel}`}
                        type="info"
                        showIcon
                      />
                    </div>

                    <Divider />

                    {/* Behavioral Evidence */}
                    <div>
                      <Title level={5}>
                        <CheckCircleOutlined style={{ color: '#52c41a' }} /> Bằng Chứng Hành Vi
                      </Title>
                      <Text type="secondary" style={{ display: 'block', marginBottom: 12 }}>
                        Các hành vi được quan sát xuyên suốt bài test:
                      </Text>
                      <ul className="evidence-list">
                        {assessment.behavioralEvidence.map((evidence, idx) => (
                          <li key={idx}>
                            <Text>{evidence}</Text>
                          </li>
                        ))}
                      </ul>
                    </div>

                    <Divider />

                    {/* Observed Limitations */}
                    <div>
                      <Title level={5}>
                        <WarningOutlined style={{ color: '#faad14' }} /> Giới Hạn Hiện Tại
                      </Title>
                      <Text type="secondary" style={{ display: 'block', marginBottom: 12 }}>
                        Các dấu hiệu cho thấy chưa đạt level cao hơn:
                      </Text>
                      <ul className="limitation-list">
                        {assessment.observedLimitations.map((limitation, idx) => (
                          <li key={idx}>
                            <Text type="warning">{limitation}</Text>
                          </li>
                        ))}
                      </ul>
                    </div>

                    <Divider />

                    {/* Development Recommendations */}
                    <div>
                      <Title level={5}>
                        <RiseOutlined style={{ color: '#1890ff' }} /> Khuyến Nghị Phát Triển
                      </Title>
                      <Text type="secondary" style={{ display: 'block', marginBottom: 12 }}>
                        Để tiến tới cấp độ cao hơn, đề xuất:
                      </Text>
                      <ul className="recommendation-list">
                        {assessment.developmentRecommendations.map((recommendation, idx) => (
                          <li key={idx}>
                            <Text strong>{recommendation}</Text>
                          </li>
                        ))}
                      </ul>
                    </div>
                  </Space>
                </Panel>
              ))}
            </Collapse>
          </Card>

          {/* Important Notes */}
          <Card style={{ marginTop: 16 }} className="notes-card">
            <Alert
              message="Lưu Ý Quan Trọng"
              description={
                <div>
                  <ul style={{ marginBottom: 0, paddingLeft: 20 }}>
                    <li>Báo cáo này phản ánh <strong>hành vi có khả năng xảy ra</strong>, không phải tiềm năng hay thái độ</li>
                    <li>Kết quả nên được dùng kết hợp với quan sát thực tế, review công việc, và phản hồi từ quản lý</li>
                    <li>Đánh giá KHÔNG đo lường tính cách, động lực, hoặc độ sâu kiến thức kỹ thuật</li>
                    <li>Đánh giá phản ánh <strong>hành vi dưới trách nhiệm</strong>, theo định nghĩa của SFIA</li>
                  </ul>
                </div>
              }
              type="info"
              showIcon
            />
          </Card>
        </>
      )}
    </div>
  );
}

function Statistic({ title, value, prefix }: { title: string; value: string | number; prefix?: React.ReactNode; valueStyle?: React.CSSProperties }) {
  return (
    <div className="custom-statistic">
      <div className="statistic-title">
        {prefix && <span style={{ marginRight: 8 }}>{prefix}</span>}
        <Text type="secondary">{title}</Text>
      </div>
      <div className="statistic-value">
        <Text strong style={{ fontSize: 20 }}>{value}</Text>
      </div>
    </div>
  );
}

function CloseCircleOutlined({ style }: { style?: React.CSSProperties }) {
  return <span style={style}>✗</span>;
}
