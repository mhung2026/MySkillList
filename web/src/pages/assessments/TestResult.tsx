import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  Card,
  Button,
  Space,
  Typography,
  Tag,
  Progress,
  Row,
  Col,
  Statistic,
  Collapse,
  Table,
  Spin,
  Alert,
  Divider,
  Result,
} from 'antd';
import {
  TrophyOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ArrowLeftOutlined,
  FileTextOutlined,
  BarChartOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import type { SkillResultDto } from '../../types';
import { getAssessmentResult } from '../../api/assessments';
import './TestResult.css';

const { Title, Text, Paragraph } = Typography;

export default function TestResult() {
  const { assessmentId } = useParams<{ assessmentId: string }>();
  const navigate = useNavigate();

  const {
    data: result,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['assessmentResult', assessmentId],
    queryFn: () => getAssessmentResult(assessmentId!),
    enabled: !!assessmentId,
  });

  const getScoreColor = (percentage: number) => {
    if (percentage >= 80) return '#52c41a';
    if (percentage >= 60) return '#1890ff';
    if (percentage >= 40) return '#faad14';
    return '#ff4d4f';
  };

  const skillColumns: ColumnsType<SkillResultDto> = [
    {
      title: 'Skill',
      dataIndex: 'skillName',
      key: 'skillName',
      render: (text, record) => (
        <div className="skill-name-cell">
          <Text strong>{text}</Text>
          <Tag>{record.skillCode}</Tag>
        </div>
      ),
    },
    {
      title: 'Correct Answers',
      key: 'correct',
      align: 'center',
      render: (_, record) => (
        <Text>
          {record.correctAnswers}/{record.totalQuestions}
        </Text>
      ),
    },
    {
      title: 'Score',
      key: 'score',
      align: 'center',
      render: (_, record) => (
        <Text strong>
          {record.score}/{record.maxScore}
        </Text>
      ),
    },
    {
      title: 'Percentage',
      dataIndex: 'percentage',
      key: 'percentage',
      align: 'center',
      render: (value) => (
        <Progress
          percent={Math.round(value)}
          size="small"
          strokeColor={getScoreColor(value)}
          style={{ width: 100 }}
        />
      ),
    },
  ];

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: 100 }}>
        <Spin size="large" />
        <div style={{ marginTop: 16 }}>Loading results...</div>
      </div>
    );
  }

  if (error || !result) {
    return (
      <Alert
        type="error"
        message="Failed to load results"
        description="Please try again later or contact administrator."
        showIcon
        action={
          <Button onClick={() => navigate('/assessments')}>Go back</Button>
        }
      />
    );
  }

  return (
    <div className="test-result-container">
      {/* Back button */}
      <Button
        icon={<ArrowLeftOutlined />}
        style={{ marginBottom: 16 }}
        onClick={() => navigate('/assessments')}
      >
        Back to list
      </Button>

      {/* Result Header */}
      <Card className="result-header-card">
        <Result
          status={result.passed ? 'success' : 'warning'}
          icon={result.passed ? <TrophyOutlined /> : <FileTextOutlined />}
          title={
            <Space direction="vertical" size={0}>
              <Title level={2} className="result-title">
                {result.title}
              </Title>
              <Tag color={result.passed ? 'success' : 'error'} className="result-tag">
                {result.passed ? 'PASSED' : 'FAILED'}
              </Tag>
            </Space>
          }
          subTitle={
            <Space direction="vertical" size={8} className="result-subtitle">
              <Text type="secondary">
                Completed at: {new Date(result.completedAt).toLocaleString('en-US')}
              </Text>
              <Text type="secondary">
                Passing score: {result.passingScore}%
              </Text>
            </Space>
          }
        />

        {/* Score Display */}
        <Row gutter={[24, 24]} justify="center" className="score-cards">
          <Col xs={24} sm={8}>
            <Card className="score-card">
              <Statistic
                title="Score"
                value={result.totalScore}
                suffix={`/ ${result.maxScore}`}
                valueStyle={{ color: getScoreColor(result.percentage) }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={8}>
            <Card className="score-card progress-card">
              <Progress
                type="circle"
                percent={Math.round(result.percentage)}
                strokeColor={getScoreColor(result.percentage)}
                size={100}
              />
              <div className="progress-label">
                <Text strong>Correct Rate</Text>
              </div>
            </Card>
          </Col>
          <Col xs={24} sm={8}>
            <Card className="score-card">
              <Statistic
                title="Time Spent"
                value={result.totalTimeMinutes}
                suffix="min"
                prefix={<ClockCircleOutlined />}
              />
            </Card>
          </Col>
        </Row>
      </Card>

      {/* Statistics */}
      <Card title={<Space><BarChartOutlined /> Detailed Statistics</Space>} className="stats-grid">
        <Row gutter={[16, 16]}>
          <Col xs={12} sm={6}>
            <Card size="small" className="stat-card correct-bg">
              <Statistic
                title="Correct"
                value={result.correctAnswers}
                valueStyle={{ color: '#52c41a' }}
                prefix={<CheckCircleOutlined />}
              />
            </Card>
          </Col>
          <Col xs={12} sm={6}>
            <Card size="small" className="stat-card wrong-bg">
              <Statistic
                title="Wrong"
                value={result.wrongAnswers}
                valueStyle={{ color: '#ff4d4f' }}
                prefix={<CloseCircleOutlined />}
              />
            </Card>
          </Col>
          <Col xs={12} sm={6}>
            <Card size="small" className="stat-card skipped-bg">
              <Statistic
                title="Skipped"
                value={result.unansweredQuestions}
                valueStyle={{ color: '#faad14' }}
              />
            </Card>
          </Col>
          <Col xs={12} sm={6}>
            <Card size="small" className="stat-card review-bg">
              <Statistic
                title="Pending Review"
                value={result.pendingReviewQuestions}
                valueStyle={{ color: '#1890ff' }}
              />
            </Card>
          </Col>
        </Row>
      </Card>

      {/* Skill Breakdown */}
      {result.skillResults && result.skillResults.length > 0 && (
        <Card
          title={<Space><BarChartOutlined /> Results by Skill</Space>}
          className="skill-breakdown-card"
        >
          <Table
            columns={skillColumns}
            dataSource={result.skillResults}
            rowKey="skillId"
            pagination={false}
            size="small"
          />
        </Card>
      )}

      {/* Question Details */}
      {result.questionResults && result.questionResults.length > 0 && (
        <Card title={<Space><FileTextOutlined /> Question Details</Space>} className="question-details-card">
          <Collapse
            accordion
            items={result.questionResults.map((q) => ({
              key: q.questionId,
              label: (
                <div className="question-label">
                  <Tag color={q.isCorrect ? 'success' : q.isCorrect === false ? 'error' : 'processing'}>
                    {q.isCorrect ? <CheckCircleOutlined /> : q.isCorrect === false ? <CloseCircleOutlined /> : <ClockCircleOutlined />}
                  </Tag>
                  <Text strong>Question {q.questionNumber}</Text>
                  <Tag>{q.typeName}</Tag>
                  <Text type="secondary">{q.skillName}</Text>
                  <Tag color={q.isCorrect ? 'success' : 'default'}>
                    {q.pointsAwarded ?? 0}/{q.points} points
                  </Tag>
                </div>
              ),
              children: (
                <div>
                  {/* Question content */}
                  <Paragraph className="question-content-text">
                    <div dangerouslySetInnerHTML={{ __html: q.content }} />
                  </Paragraph>

                  {/* Code snippet */}
                  {q.codeSnippet && (
                    <pre className="question-code-block">
                      <code>{q.codeSnippet}</code>
                    </pre>
                  )}

                  <Divider />

                  {/* Options with results */}
                  {q.options && q.options.length > 0 && (
                    <div style={{ marginBottom: 16 }}>
                      <Text strong>Options:</Text>
                      <Space direction="vertical" style={{ width: '100%', marginTop: 8 }}>
                        {q.options.map((opt, idx) => {
                          let optionClass = 'question-option default';

                          if (opt.isCorrect && opt.wasSelected) {
                            optionClass = 'question-option correct-selected';
                          } else if (opt.isCorrect && !opt.wasSelected) {
                            optionClass = 'question-option correct-not-selected';
                          } else if (!opt.isCorrect && opt.wasSelected) {
                            optionClass = 'question-option wrong-selected';
                          }

                          return (
                            <div
                              key={opt.id}
                              className={optionClass}
                            >
                              <Space>
                                <Text strong>{String.fromCharCode(65 + idx)}.</Text>
                                <Text>{opt.content}</Text>
                                {opt.isCorrect && (
                                  <Tag color="success" icon={<CheckCircleOutlined />}>
                                    Correct answer
                                  </Tag>
                                )}
                                {opt.wasSelected && !opt.isCorrect && (
                                  <Tag color="error" icon={<CloseCircleOutlined />}>
                                    Your choice
                                  </Tag>
                                )}
                                {opt.wasSelected && opt.isCorrect && (
                                  <Tag color="success" icon={<CheckCircleOutlined />}>
                                    Correct choice
                                  </Tag>
                                )}
                              </Space>
                              {opt.explanation && (
                                <div style={{ marginTop: 4 }}>
                                  <Text type="secondary" italic>
                                    {opt.explanation}
                                  </Text>
                                </div>
                              )}
                            </div>
                          );
                        })}
                      </Space>
                    </div>
                  )}

                  {/* Text/Code answers */}
                  {q.userAnswer && (
                    <div style={{ marginBottom: 16 }}>
                      <Text strong>Your answer:</Text>
                      <div className="answer-block user-answer-block">
                        <Text>{q.userAnswer}</Text>
                      </div>
                    </div>
                  )}

                  {q.correctAnswer && (
                    <div style={{ marginBottom: 16 }}>
                      <Text strong style={{ color: '#52c41a' }}>
                        Correct answer:
                      </Text>
                      <div className="answer-block correct-answer-block">
                        <Text>{q.correctAnswer}</Text>
                      </div>
                    </div>
                  )}

                  {/* Explanation */}
                  {q.explanation && (
                    <Alert
                      type="info"
                      message="Explanation"
                      description={q.explanation}
                      showIcon
                    />
                  )}
                </div>
              ),
            }))}
          />
        </Card>
      )}
    </div>
  );
}
