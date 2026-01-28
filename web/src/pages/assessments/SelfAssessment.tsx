import { useState, useEffect } from 'react';
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
  message,
} from 'antd';
import {
  TrophyOutlined,
  ReloadOutlined,
  FileTextOutlined,
  CheckCircleOutlined,
  WarningOutlined,
  RiseOutlined,
  BarChartOutlined,
  BookOutlined,
  LinkOutlined,
  SyncOutlined,
} from '@ant-design/icons';
import { useAuth } from '../../contexts/AuthContext';
import {
  getLearningRecommendations,
  recalculateGaps,
  getGapAnalysis,
  type LearningRecommendation
} from '../../api/employees';
import { getEmployeeAssessments, getAssessmentResult } from '../../api/assessments';
import { AssessmentStatus } from '../../types';
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
  const [recommendations, setRecommendations] = useState<LearningRecommendation[]>([]);
  const [recalculating, setRecalculating] = useState(false);

  // Fetch AI recommendations on mount
  useEffect(() => {
    if (user?.id) {
      fetchRecommendations();
    }
  }, [user?.id]);

  const fetchRecommendations = async () => {
    if (!user?.id) return;
    try {
      const data = await getLearningRecommendations(user.id);
      setRecommendations(data);
    } catch (err) {
      console.error('Failed to fetch recommendations:', err);
    }
  };

  const handleRecalculate = async () => {
    if (!user?.id) return;
    setRecalculating(true);
    try {
      await recalculateGaps(user.id);
      message.success('AI đang phân tích skill gaps và tạo recommendations...');
      // Wait a bit for AI processing, then refresh
      setTimeout(async () => {
        await fetchRecommendations();
        message.success('Đã cập nhật recommendations từ AI!');
        setRecalculating(false);
      }, 2000);
    } catch (err) {
      message.error('Không thể recalculate gaps');
      setRecalculating(false);
    }
  };

  const handleGenerateReport = async () => {
    setLoading(true);
    setError(null);

    try {
      // Fetch fresh gap analysis data
      if (!user?.id) {
        setError('User not found');
        return;
      }

      const freshGapData = await getGapAnalysis(user.id);
      await fetchRecommendations();

      // Fetch latest completed assessment to get real totalQuestions
      let totalQuestions = freshGapData?.gaps.length ? freshGapData.gaps.length * 3 : 0; // Default estimate
      try {
        const assessmentHistory = await getEmployeeAssessments(user.id, 1, 1);
        if (assessmentHistory.items.length > 0) {
          const latestAssessment = assessmentHistory.items[0];
          if (latestAssessment.status === AssessmentStatus.Completed && latestAssessment.id) {
            const assessmentResult = await getAssessmentResult(latestAssessment.id);
            totalQuestions = assessmentResult.totalQuestions;
          }
        }
      } catch (err) {
        console.error('Failed to fetch assessment history, using estimate:', err);
      }

      // Convert gap analysis to SFIA assessment format
      if (freshGapData && freshGapData.gaps.length > 0) {
        const assessmentReport: SFIAAssessmentData = {
          candidateName: freshGapData.employee.fullName,
          assessmentDate: new Date().toISOString(),
          totalQuestions: totalQuestions,
          skillsAssessed: freshGapData.gaps.map(g => g.skillName),
          overallCapability: `SFIA Level ${freshGapData.currentRole?.levelInHierarchy || 3}`,
          operationalReadiness: freshGapData.summary.overallReadiness >= 80
            ? 'Can operate independently within clear scope'
            : 'Requires guidance in some areas',
          autonomyLevel: freshGapData.summary.overallReadiness >= 80
            ? 'Autonomous within defined boundaries'
            : 'Growing autonomy with supervision',
          assessments: freshGapData.gaps.map(gap => ({
            skillName: gap.skillName,
            inferredLevel: `Level ${gap.currentLevel}`,
            confidence: gap.gapSize <= 1 ? 'High' : gap.gapSize <= 2 ? 'Medium' : 'Low',
            behavioralEvidence: gap.aiAnalysis ? [gap.aiAnalysis] : [
              `Currently operating at ${gap.currentLevelName}`,
              `Demonstrates competency at Level ${gap.currentLevel}`
            ],
            observedLimitations: gap.gapSize > 0 ? [
              `Gap of ${gap.gapSize} level${gap.gapSize > 1 ? 's' : ''} to reach ${gap.requiredLevelName}`,
              gap.isMandatory ? 'This is a mandatory skill for current role' : 'Optional skill for role advancement'
            ] : [],
            developmentRecommendations: gap.aiRecommendation ? [gap.aiRecommendation] : [
              `Target: ${gap.requiredLevelName}`,
              `Priority: ${gap.priority}`
            ],
            levelAnalysis: Array.from({ length: gap.requiredLevel }, (_, i) => {
              const targetLevel = i + 1;
              const isAchieved = targetLevel <= gap.currentLevel;
              const gapFromCurrent = targetLevel - gap.currentLevel;

              // Keep evidence simple and factual (AI analysis is shown separately in other sections)
              let evidence = '';
              if (isAchieved) {
                evidence = `Demonstrated competency at Level ${targetLevel}`;
              } else if (targetLevel === gap.requiredLevel) {
                // This is the required/target level
                if (gapFromCurrent === 1) {
                  evidence = `Target level - requires 1 level advancement`;
                } else {
                  evidence = `Target level - requires ${gapFromCurrent} levels advancement`;
                }
              } else if (gapFromCurrent === 1) {
                // Intermediate milestone - 1 level away
                evidence = `Intermediate milestone - 1 level from current`;
              } else {
                // Intermediate milestone - multiple levels away
                evidence = `Intermediate milestone - ${gapFromCurrent} levels from current`;
              }

              return {
                level: `L${targetLevel}`,
                evidence,
                result: (isAchieved ? 'achieved' : 'not_achieved') as 'achieved' | 'not_achieved'
              };
            })
          }))
        };
        setAssessmentData(assessmentReport);
      } else {
        // No gap analysis data available
        setError('No skill gap data found. Please ensure you have a job role assigned and skill gaps calculated.');
      }
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
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Button
                    icon={<ReloadOutlined />}
                    onClick={handleGenerateReport}
                    loading={loading}
                    block
                  >
                    Làm Mới Báo Cáo
                  </Button>
                  <Button
                    icon={<SyncOutlined spin={recalculating} />}
                    onClick={handleRecalculate}
                    loading={recalculating}
                    type="primary"
                    block
                  >
                    Recalculate AI Gaps
                  </Button>
                </Space>
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
                            <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                              <div style={{ display: 'flex', alignItems: 'center', flexWrap: 'wrap', gap: '8px' }}>
                                {level.result === 'achieved' ? (
                                  <CheckCircleOutlined style={{ color: '#52c41a' }} />
                                ) : (
                                  <CloseCircleOutlined style={{ color: '#d9d9d9' }} />
                                )}
                                <Text strong>{level.level}</Text>
                                <Tag color={level.result === 'achieved' ? 'success' : 'default'}>
                                  {level.result === 'achieved' ? 'Đạt' : 'Chưa đạt'}
                                </Tag>
                              </div>
                              <Text type="secondary" style={{ paddingLeft: '24px' }}>
                                {level.evidence}
                              </Text>
                            </div>
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

                    <Divider />

                    {/* AI-Generated Coursera Learning Resources */}
                    <div>
                      <Title level={5}>
                        <BookOutlined style={{ color: '#52c41a' }} /> Khóa học đề xuất từ AI
                      </Title>
                      <Text type="secondary" style={{ display: 'block', marginBottom: 12 }}>
                        Các khóa học được AI phân tích và gợi ý dựa trên skill gap của bạn:
                      </Text>
                      <Space direction="vertical" style={{ width: '100%' }} size="small">
                        {/* Real AI recommendations from backend */}
                        {recommendations.length > 0 ? (
                          recommendations
                            .filter(rec => rec.skillName.toLowerCase().includes(assessment.skillName.toLowerCase()))
                            .slice(0, 3)
                            .map((rec) => (
                              <Card key={rec.id} size="small" style={{ backgroundColor: '#f0f5ff' }}>
                                <Space direction="vertical" style={{ width: '100%' }} size="small">
                                  <div style={{ display: 'flex', justifyContent: 'space-between', flexWrap: 'wrap', gap: '8px', alignItems: 'flex-start' }}>
                                    <Text strong style={{ flex: 1, minWidth: '200px', wordBreak: 'break-word' }}>{rec.title}</Text>
                                    <Tag color={rec.recommendationType === 'Course' ? 'blue' : 'green'}>
                                      {rec.recommendationType}
                                    </Tag>
                                  </div>
                                  {rec.description && (
                                    <Text type="secondary" style={{ fontSize: '12px' }}>
                                      {rec.description}
                                    </Text>
                                  )}
                                  {rec.rationale && (
                                    <Text italic style={{ fontSize: '11px', color: '#8c8c8c' }}>
                                      💡 {rec.rationale}
                                    </Text>
                                  )}
                                  <Space wrap>
                                    {rec.estimatedHours && <Tag>~{rec.estimatedHours} hours</Tag>}
                                    <Tag color="purple">AI-Generated</Tag>
                                  </Space>
                                  {rec.url && (
                                    <Button
                                      type="link"
                                      icon={<LinkOutlined />}
                                      size="small"
                                      style={{ padding: 0 }}
                                      href={rec.url}
                                      target="_blank"
                                      rel="noopener noreferrer"
                                    >
                                      Xem khóa học trên Coursera →
                                    </Button>
                                  )}
                                </Space>
                              </Card>
                            ))
                        ) : (
                          <Card size="small" style={{ backgroundColor: '#f6f6f6' }}>
                            <Space direction="vertical" style={{ width: '100%', textAlign: 'center' }}>
                              <Text type="secondary">
                                Chưa có recommendations từ AI. Click "Recalculate AI Gaps" để tạo gợi ý học tập.
                              </Text>
                              <Button
                                type="primary"
                                icon={<SyncOutlined />}
                                onClick={handleRecalculate}
                                loading={recalculating}
                                size="small"
                              >
                                Tạo AI Recommendations
                              </Button>
                            </Space>
                          </Card>
                        )}

                        <Alert
                          message="Đề xuất từ AI"
                          description={
                            recommendations.length > 0
                              ? `Hệ thống AI đã phân tích ${recommendations.length} recommendations dựa trên skill gaps của bạn.`
                              : 'Hệ thống AI sẽ phân tích skill gap và gợi ý lộ trình học tập chi tiết với các khóa học Coursera phù hợp nhất.'
                          }
                          type="info"
                          showIcon
                          action={
                            <Button size="small" type="primary" href="/profile/skill-gaps">
                              Xem lộ trình đầy đủ
                            </Button>
                          }
                        />
                      </Space>
                    </div>
                  </Space>
                </Panel>
              ))}
            </Collapse>
          </Card>

          {/* All AI Learning Recommendations */}
          {recommendations.length > 0 && (
            <Card
              title={
                <Space>
                  <BookOutlined style={{ color: '#52c41a' }} />
                  <span>Tất Cả Khóa Học Đề Xuất Từ AI</span>
                  <Tag color="purple">{recommendations.length} courses</Tag>
                </Space>
              }
              style={{ marginTop: 16 }}
              extra={
                <Button
                  icon={<SyncOutlined spin={recalculating} />}
                  onClick={handleRecalculate}
                  loading={recalculating}
                  size="small"
                >
                  Refresh Recommendations
                </Button>
              }
            >
              <Space direction="vertical" size="middle" style={{ width: '100%' }}>
                <Alert
                  message="AI Coursera Recommendations"
                  description={`Hệ thống AI đã phân tích ${recommendations.length} khóa học Coursera phù hợp với skill gaps của bạn.`}
                  type="info"
                  showIcon
                  icon={<RiseOutlined />}
                />

                {/* Group recommendations by skill */}
                {Object.entries(
                  recommendations.reduce((acc, rec) => {
                    if (!acc[rec.skillId]) {
                      acc[rec.skillId] = {
                        skillName: rec.skillName,
                        recommendations: [],
                      };
                    }
                    acc[rec.skillId].recommendations.push(rec);
                    return acc;
                  }, {} as Record<string, { skillName: string; recommendations: LearningRecommendation[] }>)
                ).map(([skillId, { skillName, recommendations: skillRecs }]) => (
                  <Collapse key={skillId} defaultActiveKey={[skillId]}>
                    <Panel
                      header={
                        <Space>
                          <TrophyOutlined style={{ color: '#1890ff' }} />
                          <Text strong>{skillName}</Text>
                          <Tag color="blue">
                            {skillRecs.length} {skillRecs.length === 1 ? 'course' : 'courses'}
                          </Tag>
                        </Space>
                      }
                      key={skillId}
                    >
                      <Space direction="vertical" size="small" style={{ width: '100%' }}>
                        {skillRecs
                          .sort((a, b) => a.displayOrder - b.displayOrder)
                          .map((rec) => (
                            <Card key={rec.id} size="small" style={{ backgroundColor: '#f0f5ff' }}>
                              <Space direction="vertical" style={{ width: '100%' }} size="small">
                                <div style={{ display: 'flex', justifyContent: 'space-between', flexWrap: 'wrap', gap: '8px', alignItems: 'flex-start' }}>
                                  <Text strong style={{ flex: 1, minWidth: '200px', wordBreak: 'break-word' }}>{rec.title}</Text>
                                  <Space wrap size="small">
                                    <Tag color={rec.recommendationType === 'Course' ? 'blue' : 'green'}>
                                      {rec.recommendationType}
                                    </Tag>
                                    {rec.estimatedHours && (
                                      <Tag icon={<BookOutlined />} color="green">
                                        {rec.estimatedHours}h
                                      </Tag>
                                    )}
                                  </Space>
                                </div>

                                {rec.description && (
                                  <Text type="secondary" style={{ fontSize: '12px' }}>
                                    {rec.description.length > 200
                                      ? `${rec.description.substring(0, 200)}...`
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
                                    style={{ marginTop: 8 }}
                                  />
                                )}

                                {rec.url && (
                                  <Button
                                    type="primary"
                                    icon={<LinkOutlined />}
                                    href={rec.url}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    size="small"
                                  >
                                    Xem khóa học trên Coursera →
                                  </Button>
                                )}
                              </Space>
                            </Card>
                          ))}
                      </Space>
                    </Panel>
                  </Collapse>
                ))}

                <Button type="link" href="/profile/skill-gaps" block>
                  Xem lộ trình học tập đầy đủ →
                </Button>
              </Space>
            </Card>
          )}

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
