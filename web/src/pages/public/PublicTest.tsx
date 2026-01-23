import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import {
  Card,
  Button,
  Radio,
  Checkbox,
  Input,
  Progress,
  Typography,
  Space,
  Spin,
  Result,
  message,
  Divider,
  Tag,
} from 'antd';
import {
  ClockCircleOutlined,
  ArrowLeftOutlined,
  ArrowRightOutlined,
  SendOutlined,
} from '@ant-design/icons';
import axios from 'axios';

const { Title, Text, Paragraph } = Typography;
const { TextArea } = Input;

const API_URL = import.meta.env.VITE_API_URL?.replace('/api', '') || '';

interface Question {
  id: string;
  content: string;
  type: string;
  options: { id: string; content: string }[];
  points: number;
  codeSnippet?: string;
}

interface Section {
  id: string;
  title: string;
  description?: string;
  questions: Question[];
}

interface TestData {
  assessmentId: string;
  title: string;
  description?: string;
  sections: Section[];
  timeLimitMinutes?: number;
  currentQuestionIndex?: number;
  status?: string;
  needsStart?: boolean;
}

interface TestResult {
  score: number;
  maxScore: number;
  percentage: number;
  passed: boolean;
  passingScore: number;
}

export default function PublicTest() {
  const { assessmentId } = useParams<{ assessmentId: string }>();

  const [loading, setLoading] = useState(true);
  const [testData, setTestData] = useState<TestData | null>(null);
  const [currentSectionIndex, setCurrentSectionIndex] = useState(0);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [answers, setAnswers] = useState<Record<string, any>>({});
  const [submitting, setSubmitting] = useState(false);
  const [completed, setCompleted] = useState(false);
  const [result, setResult] = useState<TestResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [timeLeft, setTimeLeft] = useState<number | null>(null);
  const [needsStart, setNeedsStart] = useState(false);
  const [starting, setStarting] = useState(false);

  // Fetch test data
  useEffect(() => {
    fetchTestData();
  }, [assessmentId]);

  // Timer
  useEffect(() => {
    if (timeLeft === null || timeLeft <= 0 || completed) return;

    const timer = setInterval(() => {
      setTimeLeft(prev => {
        if (prev === null || prev <= 1) {
          handleSubmitTest();
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(timer);
  }, [timeLeft, completed]);

  const fetchTestData = async () => {
    try {
      setLoading(true);
      const response = await axios.get(`${API_URL}/api/public/test/${assessmentId}`);
      const data = response.data;

      if (data.needsStart) {
        setNeedsStart(true);
        setTestData(data);
      } else {
        setTestData(data);
        if (data.timeLimitMinutes) {
          setTimeLeft(data.timeLimitMinutes * 60);
        }
      }
    } catch (err: any) {
      setError(err.response?.data?.error || 'Không thể tải bài test');
    } finally {
      setLoading(false);
    }
  };

  const handleStartTest = async () => {
    try {
      setStarting(true);
      const response = await axios.post(`${API_URL}/api/public/test/${assessmentId}/start`);
      setTestData(response.data);
      setNeedsStart(false);
      if (response.data.timeLimitMinutes) {
        setTimeLeft(response.data.timeLimitMinutes * 60);
      }
    } catch (err: any) {
      message.error(err.response?.data?.error || 'Không thể bắt đầu bài test');
    } finally {
      setStarting(false);
    }
  };

  const getCurrentQuestion = (): Question | null => {
    if (!testData?.sections?.length) return null;
    const section = testData.sections[currentSectionIndex];
    if (!section?.questions?.length) return null;
    return section.questions[currentQuestionIndex] || null;
  };

  const handleAnswer = async (questionId: string, answer: any) => {
    setAnswers(prev => ({ ...prev, [questionId]: answer }));

    // Submit answer to server
    try {
      const question = getCurrentQuestion();
      const payload: any = {
        questionId,
        timeSpentSeconds: 30, // TODO: Track actual time
      };

      if (question?.type === 'MultipleChoice' || question?.type === 'TrueFalse') {
        payload.selectedOptionIds = [answer];
      } else if (question?.type === 'MultipleAnswer') {
        payload.selectedOptionIds = answer;
      } else {
        payload.textResponse = answer;
      }

      await axios.post(`${API_URL}/api/public/test/${assessmentId}/answer`, payload);
    } catch (err) {
      console.error('Failed to save answer:', err);
    }
  };

  const handleNext = () => {
    const section = testData?.sections?.[currentSectionIndex];
    if (!section) return;

    if (currentQuestionIndex < section.questions.length - 1) {
      setCurrentQuestionIndex(prev => prev + 1);
    } else if (currentSectionIndex < (testData?.sections?.length || 0) - 1) {
      setCurrentSectionIndex(prev => prev + 1);
      setCurrentQuestionIndex(0);
    }
  };

  const handlePrev = () => {
    if (currentQuestionIndex > 0) {
      setCurrentQuestionIndex(prev => prev - 1);
    } else if (currentSectionIndex > 0) {
      setCurrentSectionIndex(prev => prev - 1);
      const prevSection = testData?.sections?.[currentSectionIndex - 1];
      setCurrentQuestionIndex((prevSection?.questions?.length || 1) - 1);
    }
  };

  const handleSubmitTest = async () => {
    try {
      setSubmitting(true);
      const response = await axios.post(`${API_URL}/api/public/test/${assessmentId}/submit`);
      setResult(response.data);
      setCompleted(true);
      message.success('Nộp bài thành công!');
    } catch (err: any) {
      message.error(err.response?.data?.error || 'Không thể nộp bài');
    } finally {
      setSubmitting(false);
    }
  };

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  const getTotalQuestions = () => {
    return testData?.sections?.reduce((acc, s) => acc + (s.questions?.length || 0), 0) || 0;
  };

  const getAnsweredCount = () => {
    return Object.keys(answers).length;
  };

  const getCurrentQuestionNumber = () => {
    let count = 0;
    for (let i = 0; i < currentSectionIndex; i++) {
      count += testData?.sections?.[i]?.questions?.length || 0;
    }
    return count + currentQuestionIndex + 1;
  };

  if (loading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh', background: '#f5f5f5' }}>
        <Spin size="large" tip="Đang tải bài test..." />
      </div>
    );
  }

  if (error) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh', background: '#f5f5f5' }}>
        <Result
          status="error"
          title="Không thể tải bài test"
          subTitle={error}
        />
      </div>
    );
  }

  if (needsStart) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh', background: '#f5f5f5' }}>
        <Card style={{ maxWidth: 500, textAlign: 'center' }}>
          <Title level={3}>{testData?.title || 'Bài kiểm tra'}</Title>
          <Paragraph>Bạn đã được mời làm bài kiểm tra này.</Paragraph>
          <Paragraph type="secondary">Nhấn nút bên dưới để bắt đầu.</Paragraph>
          <Button
            type="primary"
            size="large"
            loading={starting}
            onClick={handleStartTest}
          >
            Bắt đầu làm bài
          </Button>
        </Card>
      </div>
    );
  }

  if (completed && result) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh', background: '#f5f5f5' }}>
        <Card style={{ maxWidth: 500, textAlign: 'center' }}>
          <Result
            status={result.passed ? 'success' : 'warning'}
            title={result.passed ? 'Chúc mừng! Bạn đã hoàn thành bài test' : 'Bạn đã hoàn thành bài test'}
            subTitle={
              <Space direction="vertical">
                <Text>Điểm số: {result.score}/{result.maxScore}</Text>
                <Text>Tỷ lệ: {result.percentage.toFixed(1)}%</Text>
                <Text>Điểm đạt: {result.passingScore}%</Text>
                <Tag color={result.passed ? 'green' : 'red'}>
                  {result.passed ? 'ĐẠT' : 'CHƯA ĐẠT'}
                </Tag>
              </Space>
            }
          />
        </Card>
      </div>
    );
  }

  const currentQuestion = getCurrentQuestion();
  const section = testData?.sections?.[currentSectionIndex];
  const isLastQuestion =
    currentSectionIndex === (testData?.sections?.length || 0) - 1 &&
    currentQuestionIndex === (section?.questions?.length || 0) - 1;

  return (
    <div style={{ minHeight: '100vh', background: '#f5f5f5', padding: '20px' }}>
      <div style={{ maxWidth: 900, margin: '0 auto' }}>
        {/* Header */}
        <Card style={{ marginBottom: 16 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div>
              <Title level={4} style={{ margin: 0 }}>{testData?.title}</Title>
              <Text type="secondary">{section?.title}</Text>
            </div>
            <Space>
              {timeLeft !== null && (
                <Tag icon={<ClockCircleOutlined />} color={timeLeft < 300 ? 'red' : 'blue'}>
                  {formatTime(timeLeft)}
                </Tag>
              )}
              <Tag>
                Câu {getCurrentQuestionNumber()}/{getTotalQuestions()}
              </Tag>
              <Tag color="green">
                Đã trả lời: {getAnsweredCount()}/{getTotalQuestions()}
              </Tag>
            </Space>
          </div>
          <Progress
            percent={Math.round((getAnsweredCount() / getTotalQuestions()) * 100)}
            size="small"
            style={{ marginTop: 12 }}
          />
        </Card>

        {/* Question */}
        {currentQuestion && (
          <Card>
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              <div>
                <Tag color="blue">{currentQuestion.points} điểm</Tag>
                <Tag>{currentQuestion.type}</Tag>
              </div>

              <Title level={5}>{currentQuestion.content}</Title>

              {currentQuestion.codeSnippet && (
                <pre style={{ background: '#f5f5f5', padding: 16, borderRadius: 8, overflow: 'auto' }}>
                  {currentQuestion.codeSnippet}
                </pre>
              )}

              {/* Answer options based on type */}
              {(currentQuestion.type === 'MultipleChoice' || currentQuestion.type === 'TrueFalse') && (
                <Radio.Group
                  value={answers[currentQuestion.id]}
                  onChange={e => handleAnswer(currentQuestion.id, e.target.value)}
                  style={{ width: '100%' }}
                >
                  <Space direction="vertical" style={{ width: '100%' }}>
                    {currentQuestion.options.map(opt => (
                      <Radio key={opt.id} value={opt.id} style={{ padding: '8px 0' }}>
                        {opt.content}
                      </Radio>
                    ))}
                  </Space>
                </Radio.Group>
              )}

              {currentQuestion.type === 'MultipleAnswer' && (
                <Checkbox.Group
                  value={answers[currentQuestion.id] || []}
                  onChange={values => handleAnswer(currentQuestion.id, values)}
                  style={{ width: '100%' }}
                >
                  <Space direction="vertical" style={{ width: '100%' }}>
                    {currentQuestion.options.map(opt => (
                      <Checkbox key={opt.id} value={opt.id} style={{ padding: '8px 0' }}>
                        {opt.content}
                      </Checkbox>
                    ))}
                  </Space>
                </Checkbox.Group>
              )}

              {(currentQuestion.type === 'ShortAnswer' || currentQuestion.type === 'LongAnswer') && (
                <TextArea
                  value={answers[currentQuestion.id] || ''}
                  onChange={e => handleAnswer(currentQuestion.id, e.target.value)}
                  rows={currentQuestion.type === 'LongAnswer' ? 6 : 2}
                  placeholder="Nhập câu trả lời của bạn..."
                />
              )}

              {currentQuestion.type === 'CodingChallenge' && (
                <TextArea
                  value={answers[currentQuestion.id] || ''}
                  onChange={e => handleAnswer(currentQuestion.id, e.target.value)}
                  rows={10}
                  placeholder="Nhập code của bạn..."
                  style={{ fontFamily: 'monospace' }}
                />
              )}

              <Divider />

              {/* Navigation */}
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <Button
                  icon={<ArrowLeftOutlined />}
                  onClick={handlePrev}
                  disabled={currentSectionIndex === 0 && currentQuestionIndex === 0}
                >
                  Câu trước
                </Button>

                <Space>
                  {isLastQuestion ? (
                    <Button
                      type="primary"
                      icon={<SendOutlined />}
                      onClick={handleSubmitTest}
                      loading={submitting}
                    >
                      Nộp bài
                    </Button>
                  ) : (
                    <Button
                      type="primary"
                      icon={<ArrowRightOutlined />}
                      onClick={handleNext}
                    >
                      Câu tiếp
                    </Button>
                  )}
                </Space>
              </div>
            </Space>
          </Card>
        )}
      </div>
    </div>
  );
}
