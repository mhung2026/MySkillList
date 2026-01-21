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
      title: 'Kỹ năng',
      dataIndex: 'skillName',
      key: 'skillName',
      render: (text, record) => (
        <Space>
          <Text strong>{text}</Text>
          <Tag>{record.skillCode}</Tag>
        </Space>
      ),
    },
    {
      title: 'Số câu đúng',
      key: 'correct',
      align: 'center',
      render: (_, record) => (
        <Text>
          {record.correctAnswers}/{record.totalQuestions}
        </Text>
      ),
    },
    {
      title: 'Điểm',
      key: 'score',
      align: 'center',
      render: (_, record) => (
        <Text strong>
          {record.score}/{record.maxScore}
        </Text>
      ),
    },
    {
      title: 'Tỷ lệ',
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
        <div style={{ marginTop: 16 }}>Đang tải kết quả...</div>
      </div>
    );
  }

  if (error || !result) {
    return (
      <Alert
        type="error"
        message="Không thể tải kết quả"
        description="Vui lòng thử lại sau hoặc liên hệ quản trị viên."
        showIcon
        action={
          <Button onClick={() => navigate('/assessments')}>Quay lại</Button>
        }
      />
    );
  }

  return (
    <div>
      {/* Back button */}
      <Button
        icon={<ArrowLeftOutlined />}
        style={{ marginBottom: 16 }}
        onClick={() => navigate('/assessments')}
      >
        Quay lại danh sách
      </Button>

      {/* Result Header */}
      <Card style={{ marginBottom: 16 }}>
        <Result
          status={result.passed ? 'success' : 'warning'}
          icon={result.passed ? <TrophyOutlined /> : <FileTextOutlined />}
          title={
            <Space direction="vertical" size={0}>
              <Title level={2} style={{ margin: 0 }}>
                {result.title}
              </Title>
              <Tag color={result.passed ? 'success' : 'error'} style={{ fontSize: 16, padding: '4px 12px' }}>
                {result.passed ? 'ĐẠT' : 'CHƯA ĐẠT'}
              </Tag>
            </Space>
          }
          subTitle={
            <Space direction="vertical" size={8}>
              <Text type="secondary">
                Hoàn thành lúc: {new Date(result.completedAt).toLocaleString('vi-VN')}
              </Text>
              <Text type="secondary">
                Điểm cần đạt: {result.passingScore}%
              </Text>
            </Space>
          }
        />

        {/* Score Display */}
        <Row gutter={[24, 24]} justify="center" style={{ marginTop: 24 }}>
          <Col xs={24} sm={8}>
            <Card>
              <Statistic
                title="Điểm số"
                value={result.totalScore}
                suffix={`/ ${result.maxScore}`}
                valueStyle={{ color: getScoreColor(result.percentage), fontSize: 32 }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={8}>
            <Card>
              <div style={{ textAlign: 'center' }}>
                <Progress
                  type="circle"
                  percent={Math.round(result.percentage)}
                  strokeColor={getScoreColor(result.percentage)}
                  size={100}
                />
                <div style={{ marginTop: 8 }}>
                  <Text strong>Tỷ lệ đúng</Text>
                </div>
              </div>
            </Card>
          </Col>
          <Col xs={24} sm={8}>
            <Card>
              <Statistic
                title="Thời gian làm bài"
                value={result.totalTimeMinutes}
                suffix="phút"
                prefix={<ClockCircleOutlined />}
              />
            </Card>
          </Col>
        </Row>
      </Card>

      {/* Statistics */}
      <Card title={<Space><BarChartOutlined /> Thống kê chi tiết</Space>} style={{ marginBottom: 16 }}>
        <Row gutter={[16, 16]}>
          <Col xs={12} sm={6}>
            <Card size="small" style={{ background: '#f6ffed' }}>
              <Statistic
                title="Câu đúng"
                value={result.correctAnswers}
                valueStyle={{ color: '#52c41a' }}
                prefix={<CheckCircleOutlined />}
              />
            </Card>
          </Col>
          <Col xs={12} sm={6}>
            <Card size="small" style={{ background: '#fff2f0' }}>
              <Statistic
                title="Câu sai"
                value={result.wrongAnswers}
                valueStyle={{ color: '#ff4d4f' }}
                prefix={<CloseCircleOutlined />}
              />
            </Card>
          </Col>
          <Col xs={12} sm={6}>
            <Card size="small" style={{ background: '#fffbe6' }}>
              <Statistic
                title="Bỏ qua"
                value={result.unansweredQuestions}
                valueStyle={{ color: '#faad14' }}
              />
            </Card>
          </Col>
          <Col xs={12} sm={6}>
            <Card size="small" style={{ background: '#e6f7ff' }}>
              <Statistic
                title="Chờ chấm"
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
          title={<Space><BarChartOutlined /> Kết quả theo kỹ năng</Space>}
          style={{ marginBottom: 16 }}
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
        <Card title={<Space><FileTextOutlined /> Chi tiết từng câu hỏi</Space>}>
          <Collapse
            accordion
            items={result.questionResults.map((q) => ({
              key: q.questionId,
              label: (
                <Space>
                  <Tag color={q.isCorrect ? 'success' : q.isCorrect === false ? 'error' : 'processing'}>
                    {q.isCorrect ? <CheckCircleOutlined /> : q.isCorrect === false ? <CloseCircleOutlined /> : <ClockCircleOutlined />}
                  </Tag>
                  <Text strong>Câu {q.questionNumber}</Text>
                  <Tag>{q.typeName}</Tag>
                  <Text type="secondary">{q.skillName}</Text>
                  <Tag color={q.isCorrect ? 'success' : 'default'}>
                    {q.pointsAwarded ?? 0}/{q.points} điểm
                  </Tag>
                </Space>
              ),
              children: (
                <div>
                  {/* Question content */}
                  <Paragraph>
                    <div dangerouslySetInnerHTML={{ __html: q.content }} />
                  </Paragraph>

                  {/* Code snippet */}
                  {q.codeSnippet && (
                    <pre
                      style={{
                        background: '#f5f5f5',
                        padding: 12,
                        borderRadius: 6,
                        overflow: 'auto',
                      }}
                    >
                      <code>{q.codeSnippet}</code>
                    </pre>
                  )}

                  <Divider />

                  {/* Options with results */}
                  {q.options && q.options.length > 0 && (
                    <div style={{ marginBottom: 16 }}>
                      <Text strong>Các lựa chọn:</Text>
                      <Space direction="vertical" style={{ width: '100%', marginTop: 8 }}>
                        {q.options.map((opt, idx) => {
                          let bgColor = '#fafafa';
                          let textColor = undefined;

                          if (opt.isCorrect && opt.wasSelected) {
                            bgColor = '#f6ffed';
                          } else if (opt.isCorrect && !opt.wasSelected) {
                            bgColor = '#e6f7ff';
                          } else if (!opt.isCorrect && opt.wasSelected) {
                            bgColor = '#fff2f0';
                          }

                          return (
                            <div
                              key={opt.id}
                              style={{
                                padding: '8px 12px',
                                background: bgColor,
                                borderRadius: 6,
                                border: '1px solid #d9d9d9',
                              }}
                            >
                              <Space>
                                <Text strong>{String.fromCharCode(65 + idx)}.</Text>
                                <Text style={{ color: textColor }}>{opt.content}</Text>
                                {opt.isCorrect && (
                                  <Tag color="success" icon={<CheckCircleOutlined />}>
                                    Đáp án đúng
                                  </Tag>
                                )}
                                {opt.wasSelected && !opt.isCorrect && (
                                  <Tag color="error" icon={<CloseCircleOutlined />}>
                                    Bạn chọn
                                  </Tag>
                                )}
                                {opt.wasSelected && opt.isCorrect && (
                                  <Tag color="success" icon={<CheckCircleOutlined />}>
                                    Bạn chọn đúng
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
                      <Text strong>Câu trả lời của bạn:</Text>
                      <div
                        style={{
                          marginTop: 8,
                          padding: 12,
                          background: '#fafafa',
                          borderRadius: 6,
                        }}
                      >
                        <Text>{q.userAnswer}</Text>
                      </div>
                    </div>
                  )}

                  {q.correctAnswer && (
                    <div style={{ marginBottom: 16 }}>
                      <Text strong style={{ color: '#52c41a' }}>
                        Đáp án đúng:
                      </Text>
                      <div
                        style={{
                          marginTop: 8,
                          padding: 12,
                          background: '#f6ffed',
                          borderRadius: 6,
                        }}
                      >
                        <Text>{q.correctAnswer}</Text>
                      </div>
                    </div>
                  )}

                  {/* Explanation */}
                  {q.explanation && (
                    <Alert
                      type="info"
                      message="Giải thích"
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
