import { useState, useEffect, useCallback, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import {
  Card,
  Button,
  Space,
  Typography,
  message,
  Modal,
  Progress,
  Radio,
  Checkbox,
  Input,
  Spin,
  Row,
  Col,
  Tag,
  Divider,
  Alert,
  Affix,
  Statistic,
  Rate,
} from 'antd';
import {
  ClockCircleOutlined,
  CheckCircleOutlined,
  LeftOutlined,
  RightOutlined,
  SendOutlined,
  ExclamationCircleOutlined,
  SaveOutlined,
  CheckOutlined,
} from '@ant-design/icons';
import type {
  QuestionForTestDto,
  SubmitAnswerRequest,
} from '../../types';
import { QuestionType } from '../../types';
import { continueAssessment, submitAnswer, submitAssessment } from '../../api/assessments';

const { Title, Text, Paragraph } = Typography;
const { TextArea } = Input;

interface UserAnswer {
  questionId: string;
  selectedOptionIds?: string[];
  textResponse?: string;
  codeResponse?: string;
  ratingValue?: number;
  isSaved: boolean;
  timeSpent: number;
}

export default function TakeTest() {
  const { assessmentId } = useParams<{ assessmentId: string }>();
  const navigate = useNavigate();

  const [currentSectionIndex, setCurrentSectionIndex] = useState(0);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [answers, setAnswers] = useState<Map<string, UserAnswer>>(new Map());
  const [timeRemaining, setTimeRemaining] = useState<number | null>(null);
  const [questionStartTime, setQuestionStartTime] = useState<number>(Date.now());
  const [submitModalVisible, setSubmitModalVisible] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const pendingSaveRef = useRef<Promise<void> | null>(null);
  const autoSubmitTriggeredRef = useRef(false);

  // Fetch assessment data
  const {
    data: assessmentData,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['assessment', assessmentId],
    queryFn: () => continueAssessment(assessmentId!),
    enabled: !!assessmentId,
  });

  // Submit answer mutation
  const answerMutation = useMutation({
    mutationFn: submitAnswer,
    onSuccess: (_, variables) => {
      setAnswers((prev) => {
        const newAnswers = new Map(prev);
        const existing = newAnswers.get(variables.questionId);
        if (existing) {
          newAnswers.set(variables.questionId, { ...existing, isSaved: true });
        }
        return newAnswers;
      });
    },
    onError: (error: Error) => {
      message.error(error.message || 'Failed to save answer');
    },
  });

  // Submit assessment mutation
  const submitMutation = useMutation({
    mutationFn: submitAssessment,
    onSuccess: () => {
      setSubmitModalVisible(false);
      message.success('Test submitted successfully!');
      // Small delay before navigate to prevent flash
      setTimeout(() => {
        navigate(`/assessments/result/${assessmentId}`);
      }, 300);
    },
    onError: (error: Error) => {
      setIsSubmitting(false);
      message.error(error.message || 'Failed to submit test');
    },
  });

  // Initialize timer
  useEffect(() => {
    if (assessmentData?.mustCompleteBy) {
      const endTime = new Date(assessmentData.mustCompleteBy).getTime();
      const updateTimer = () => {
        const now = Date.now();
        const remaining = Math.max(0, Math.floor((endTime - now) / 1000));
        setTimeRemaining(remaining);
        if (remaining === 0 && !autoSubmitTriggeredRef.current) {
          autoSubmitTriggeredRef.current = true;
          message.warning('Time is up! Auto-submitting...');
        }
      };
      updateTimer();
      const interval = setInterval(updateTimer, 1000);
      return () => clearInterval(interval);
    }
  }, [assessmentData?.mustCompleteBy]);

  // Auto-submit when time is up
  useEffect(() => {
    if (timeRemaining === 0 && autoSubmitTriggeredRef.current && !isSubmitting) {
      // Trigger auto-submit
      const doAutoSubmit = async () => {
        setIsSubmitting(true);
        try {
          // Get current question from state
          const section = assessmentData?.sections?.[currentSectionIndex];
          const question = section?.questions?.[currentQuestionIndex];

          // Save current answer first
          if (question && assessmentId) {
            const answer = answers.get(question.id);
            if (answer && !answer.isSaved) {
              const timeSpent = Math.floor((Date.now() - questionStartTime) / 1000) + (answer.timeSpent || 0);
              await new Promise<void>((resolve, reject) => {
                answerMutation.mutate({
                  assessmentId,
                  questionId: question.id,
                  selectedOptionIds: answer.selectedOptionIds,
                  textResponse: answer.textResponse,
                  codeResponse: answer.codeResponse,
                  timeSpentSeconds: timeSpent,
                }, {
                  onSuccess: () => resolve(),
                  onError: (err) => reject(err),
                });
              });
            }
          }
          // Wait a bit then submit
          await new Promise(resolve => setTimeout(resolve, 100));
          submitMutation.mutate(assessmentId!);
        } catch {
          setIsSubmitting(false);
        }
      };
      doAutoSubmit();
    }
  }, [timeRemaining, isSubmitting, assessmentData, currentSectionIndex, currentQuestionIndex, assessmentId, answers, questionStartTime, answerMutation, submitMutation]);

  // Load existing answers when assessment data is loaded (for continue assessment)
  useEffect(() => {
    if (!assessmentData?.sections) return;

    const existingAnswers = new Map<string, UserAnswer>();
    assessmentData.sections.forEach((section) => {
      section.questions.forEach((question) => {
        // Check if question has existing answer
        if (
          question.selectedOptionIds?.length ||
          question.textResponse ||
          question.codeResponse
        ) {
          existingAnswers.set(question.id, {
            questionId: question.id,
            selectedOptionIds: question.selectedOptionIds || [],
            textResponse: question.textResponse || '',
            codeResponse: question.codeResponse || '',
            isSaved: true, // Already saved in backend
            timeSpent: 0,
          });
        }
      });
    });

    if (existingAnswers.size > 0) {
      setAnswers(existingAnswers);
    }
  }, [assessmentData]);

  // Get all questions flat
  const getAllQuestions = useCallback((): QuestionForTestDto[] => {
    if (!assessmentData?.sections) return [];
    return assessmentData.sections.flatMap((s) => s.questions);
  }, [assessmentData]);

  const allQuestions = getAllQuestions();
  const currentSection = assessmentData?.sections?.[currentSectionIndex];
  const currentQuestion = currentSection?.questions?.[currentQuestionIndex];

  // Get global question index
  const getGlobalQuestionIndex = () => {
    if (!assessmentData?.sections) return 0;
    let index = 0;
    for (let i = 0; i < currentSectionIndex; i++) {
      index += assessmentData.sections[i].questions.length;
    }
    return index + currentQuestionIndex;
  };

  const globalIndex = getGlobalQuestionIndex();

  // Save current answer before navigation
  const saveCurrentAnswer = useCallback(async () => {
    if (!currentQuestion || !assessmentId) return;

    const answer = answers.get(currentQuestion.id);
    if (!answer || answer.isSaved) return;

    const timeSpent = Math.floor((Date.now() - questionStartTime) / 1000) + (answer.timeSpent || 0);

    const request: SubmitAnswerRequest = {
      assessmentId,
      questionId: currentQuestion.id,
      selectedOptionIds: answer.selectedOptionIds,
      textResponse: answer.textResponse,
      codeResponse: answer.codeResponse,
      timeSpentSeconds: timeSpent,
    };

    // Create promise and store ref
    const savePromise = new Promise<void>((resolve, reject) => {
      answerMutation.mutate(request, {
        onSuccess: () => resolve(),
        onError: (error) => reject(error),
      });
    });
    pendingSaveRef.current = savePromise;

    try {
      await savePromise;
    } catch {
      // Error handled by mutation
    }
  }, [currentQuestion, assessmentId, answers, questionStartTime, answerMutation]);

  // Update answer
  const updateAnswer = (
    questionId: string,
    update: Partial<Omit<UserAnswer, 'questionId' | 'isSaved'>>
  ) => {
    setAnswers((prev) => {
      const newAnswers = new Map(prev);
      const existing = newAnswers.get(questionId) || {
        questionId,
        isSaved: false,
        timeSpent: 0,
      };
      newAnswers.set(questionId, {
        ...existing,
        ...update,
        isSaved: false,
      });
      return newAnswers;
    });
  };

  // Navigation
  const goToQuestion = (sectionIdx: number, questionIdx: number) => {
    saveCurrentAnswer();
    setCurrentSectionIndex(sectionIdx);
    setCurrentQuestionIndex(questionIdx);
    setQuestionStartTime(Date.now());
  };

  const goNext = () => {
    if (!assessmentData?.sections) return;

    saveCurrentAnswer();

    if (currentQuestionIndex < currentSection!.questions.length - 1) {
      setCurrentQuestionIndex(currentQuestionIndex + 1);
    } else if (currentSectionIndex < assessmentData.sections.length - 1) {
      setCurrentSectionIndex(currentSectionIndex + 1);
      setCurrentQuestionIndex(0);
    }
    setQuestionStartTime(Date.now());
  };

  const goPrev = () => {
    saveCurrentAnswer();

    if (currentQuestionIndex > 0) {
      setCurrentQuestionIndex(currentQuestionIndex - 1);
    } else if (currentSectionIndex > 0) {
      const prevSectionIndex = currentSectionIndex - 1;
      const prevSection = assessmentData?.sections?.[prevSectionIndex];
      setCurrentSectionIndex(prevSectionIndex);
      setCurrentQuestionIndex(prevSection ? prevSection.questions.length - 1 : 0);
    }
    setQuestionStartTime(Date.now());
  };

  // Submit assessment
  const handleSubmitAssessment = async () => {
    if (isSubmitting) return;
    setIsSubmitting(true);

    try {
      // Wait for any pending save to complete
      await saveCurrentAnswer();
      if (pendingSaveRef.current) {
        await pendingSaveRef.current;
      }
      // Small delay to ensure save is processed
      await new Promise(resolve => setTimeout(resolve, 100));
      submitMutation.mutate(assessmentId!);
    } catch {
      setIsSubmitting(false);
    }
  };

  // Format time
  const formatTime = (seconds: number) => {
    const hrs = Math.floor(seconds / 3600);
    const mins = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;
    if (hrs > 0) {
      return `${hrs}:${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    }
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  // Check if question is answered
  const isQuestionAnswered = (questionId: string) => {
    const answer = answers.get(questionId);
    if (!answer) return false;
    return !!(
      answer.selectedOptionIds?.length ||
      answer.textResponse?.trim() ||
      answer.codeResponse?.trim() ||
      answer.ratingValue
    );
  };

  // Render question content
  const renderQuestionContent = () => {
    if (!currentQuestion) return null;

    const answer = answers.get(currentQuestion.id);

    switch (currentQuestion.type) {
      case QuestionType.MultipleChoice:
      case QuestionType.TrueFalse:
        return (
          <Radio.Group
            value={answer?.selectedOptionIds?.[0]}
            onChange={(e) =>
              updateAnswer(currentQuestion.id, { selectedOptionIds: [e.target.value] })
            }
            style={{ width: '100%' }}
          >
            <Space direction="vertical" style={{ width: '100%' }}>
              {currentQuestion.options.map((opt, idx) => (
                <Radio
                  key={opt.id}
                  value={opt.id}
                  style={{
                    padding: '12px 16px',
                    background: '#fafafa',
                    borderRadius: 8,
                    width: '100%',
                    marginRight: 0,
                  }}
                >
                  <Text>
                    <Text strong style={{ marginRight: 8 }}>
                      {String.fromCharCode(65 + idx)}.
                    </Text>
                    {opt.content}
                  </Text>
                </Radio>
              ))}
            </Space>
          </Radio.Group>
        );

      case QuestionType.MultipleAnswer:
        return (
          <Checkbox.Group
            value={answer?.selectedOptionIds || []}
            onChange={(values) =>
              updateAnswer(currentQuestion.id, { selectedOptionIds: values as string[] })
            }
            style={{ width: '100%' }}
          >
            <Space direction="vertical" style={{ width: '100%' }}>
              {currentQuestion.options.map((opt, idx) => (
                <Checkbox
                  key={opt.id}
                  value={opt.id}
                  style={{
                    padding: '12px 16px',
                    background: '#fafafa',
                    borderRadius: 8,
                    width: '100%',
                    marginLeft: 0,
                  }}
                >
                  <Text>
                    <Text strong style={{ marginRight: 8 }}>
                      {String.fromCharCode(65 + idx)}.
                    </Text>
                    {opt.content}
                  </Text>
                </Checkbox>
              ))}
            </Space>
          </Checkbox.Group>
        );

      case QuestionType.ShortAnswer:
        return (
          <Input
            placeholder="Enter your answer..."
            value={answer?.textResponse || ''}
            onChange={(e) => updateAnswer(currentQuestion.id, { textResponse: e.target.value })}
            size="large"
          />
        );

      case QuestionType.LongAnswer:
        return (
          <TextArea
            placeholder="Enter your detailed answer..."
            value={answer?.textResponse || ''}
            onChange={(e) => updateAnswer(currentQuestion.id, { textResponse: e.target.value })}
            rows={10}
            showCount
            maxLength={10000}
          />
        );

      case QuestionType.CodingChallenge:
        return (
          <div>
            <TextArea
              placeholder="Write your code here..."
              value={answer?.codeResponse || ''}
              onChange={(e) => updateAnswer(currentQuestion.id, { codeResponse: e.target.value })}
              rows={15}
              style={{ fontFamily: 'monospace' }}
            />
          </div>
        );

      case QuestionType.Scenario:
      case QuestionType.SituationalJudgment:
        // These types can have options (like multiple choice) or text response
        if (currentQuestion.options && currentQuestion.options.length > 0) {
          return (
            <Radio.Group
              value={answer?.selectedOptionIds?.[0]}
              onChange={(e) =>
                updateAnswer(currentQuestion.id, { selectedOptionIds: [e.target.value] })
              }
              style={{ width: '100%' }}
            >
              <Space direction="vertical" style={{ width: '100%' }}>
                {currentQuestion.options.map((opt, idx) => (
                  <Radio
                    key={opt.id}
                    value={opt.id}
                    style={{
                      padding: '12px 16px',
                      background: '#fafafa',
                      borderRadius: 8,
                      width: '100%',
                      marginRight: 0,
                    }}
                  >
                    <Text>
                      <Text strong style={{ marginRight: 8 }}>
                        {String.fromCharCode(65 + idx)}.
                      </Text>
                      {opt.content}
                    </Text>
                  </Radio>
                ))}
              </Space>
            </Radio.Group>
          );
        }
        // If no options, use text response
        return (
          <TextArea
            placeholder="Describe how you would handle this situation..."
            value={answer?.textResponse || ''}
            onChange={(e) => updateAnswer(currentQuestion.id, { textResponse: e.target.value })}
            rows={8}
            showCount
            maxLength={5000}
          />
        );

      case QuestionType.Rating:
        return (
          <div style={{ textAlign: 'center', padding: '20px 0' }}>
            <Rate
              value={answer?.ratingValue || 0}
              onChange={(value) => updateAnswer(currentQuestion.id, { ratingValue: value })}
              style={{ fontSize: 36 }}
              count={5}
            />
            <div style={{ marginTop: 12 }}>
              <Text type="secondary">
                {answer?.ratingValue === 1 && 'Very Poor'}
                {answer?.ratingValue === 2 && 'Poor'}
                {answer?.ratingValue === 3 && 'Average'}
                {answer?.ratingValue === 4 && 'Good'}
                {answer?.ratingValue === 5 && 'Excellent'}
                {!answer?.ratingValue && 'Select your rating'}
              </Text>
            </div>
          </div>
        );

      default:
        return <Text type="secondary">Question type not supported</Text>;
    }
  };

  // Render question navigator
  const renderQuestionNavigator = () => {
    if (!assessmentData?.sections) return null;

    return (
      <div>
        {assessmentData.sections.map((section, sIdx) => (
          <div key={section.id} style={{ marginBottom: 16 }}>
            <Text strong style={{ display: 'block', marginBottom: 8 }}>
              {section.title}
            </Text>
            <Space wrap size={[8, 8]}>
              {section.questions.map((q, qIdx) => {
                const isActive = sIdx === currentSectionIndex && qIdx === currentQuestionIndex;
                const isAnswered = isQuestionAnswered(q.id);
                const isSaved = answers.get(q.id)?.isSaved;

                return (
                  <Button
                    key={q.id}
                    size="small"
                    type={isActive ? 'primary' : 'default'}
                    style={{
                      width: 36,
                      height: 36,
                      background: isActive
                        ? undefined
                        : isAnswered
                          ? isSaved
                            ? '#52c41a'
                            : '#faad14'
                          : undefined,
                      color: isAnswered && !isActive ? '#fff' : undefined,
                      borderColor: isAnswered && !isActive ? 'transparent' : undefined,
                    }}
                    onClick={() => goToQuestion(sIdx, qIdx)}
                  >
                    {q.questionNumber}
                  </Button>
                );
              })}
            </Space>
          </div>
        ))}
      </div>
    );
  };

  // Calculate progress
  const answeredCount = Array.from(answers.values()).filter(
    (a) => a.selectedOptionIds?.length || a.textResponse?.trim() || a.codeResponse?.trim() || a.ratingValue
  ).length;

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: 100 }}>
        <Spin size="large" />
        <div style={{ marginTop: 16 }}>Loading test...</div>
      </div>
    );
  }

  // Show full screen loading when submitting
  if (isSubmitting || submitMutation.isPending) {
    return (
      <div style={{ textAlign: 'center', padding: 100 }}>
        <Spin size="large" />
        <div style={{ marginTop: 16 }}>Submitting your test...</div>
      </div>
    );
  }

  if (error || !assessmentData) {
    return (
      <Alert
        type="error"
        message="Failed to load test"
        description="Please try again later or contact administrator."
        showIcon
        action={
          <Button onClick={() => navigate('/assessments')}>Go back</Button>
        }
      />
    );
  }

  return (
    <div>
      {/* Header with timer */}
      <Affix offsetTop={0}>
        <Card size="small" style={{ marginBottom: 16 }}>
          <Row justify="space-between" align="middle">
            <Col>
              <Space>
                <Title level={4} style={{ margin: 0 }}>
                  {assessmentData.title}
                </Title>
                <Tag color="blue">
                  Question {globalIndex + 1}/{allQuestions.length}
                </Tag>
              </Space>
            </Col>
            <Col>
              <Space size="large">
                <Progress
                  type="circle"
                  percent={Math.round((answeredCount / allQuestions.length) * 100)}
                  size={50}
                  format={() => `${answeredCount}/${allQuestions.length}`}
                />
                {timeRemaining !== null && (
                  <div
                    style={{
                      background: timeRemaining < 300 ? '#ff4d4f' : '#1890ff',
                      color: '#fff',
                      padding: '8px 16px',
                      borderRadius: 8,
                    }}
                  >
                    <ClockCircleOutlined style={{ marginRight: 8 }} />
                    <Text strong style={{ color: '#fff', fontSize: 18 }}>
                      {formatTime(timeRemaining)}
                    </Text>
                  </div>
                )}
                <Button
                  type="primary"
                  danger
                  icon={<SendOutlined />}
                  onClick={() => setSubmitModalVisible(true)}
                >
                  Submit
                </Button>
              </Space>
            </Col>
          </Row>
        </Card>
      </Affix>

      <Row gutter={16}>
        {/* Question content */}
        <Col xs={24} lg={18}>
          <Card>
            {currentQuestion && (
              <div>
                {/* Question header */}
                <Space style={{ marginBottom: 16 }}>
                  <Tag color="purple">Question {currentQuestion.questionNumber}</Tag>
                  <Tag>{currentQuestion.typeName}</Tag>
                  <Tag color="gold">{currentQuestion.points} points</Tag>
                  <Tag color="cyan">{currentQuestion.skillName}</Tag>
                  {answers.get(currentQuestion.id)?.isSaved && (
                    <Tag color="success" icon={<CheckOutlined />}>
                      Saved
                    </Tag>
                  )}
                </Space>

                {/* Question content */}
                <Paragraph style={{ fontSize: 16 }}>
                  <div dangerouslySetInnerHTML={{ __html: currentQuestion.content }} />
                </Paragraph>

                {/* Code snippet */}
                {currentQuestion.codeSnippet && (
                  <pre
                    style={{
                      background: '#f5f5f5',
                      padding: 16,
                      borderRadius: 8,
                      overflow: 'auto',
                      marginBottom: 16,
                    }}
                  >
                    <code>{currentQuestion.codeSnippet}</code>
                  </pre>
                )}

                <Divider />

                {/* Answer section */}
                {renderQuestionContent()}

                {/* Navigation buttons */}
                <Divider />
                <Row justify="space-between">
                  <Col>
                    <Button
                      icon={<LeftOutlined />}
                      onClick={goPrev}
                      disabled={globalIndex === 0}
                    >
                      Previous
                    </Button>
                  </Col>
                  <Col>
                    <Space>
                      <Button
                        icon={<SaveOutlined />}
                        onClick={saveCurrentAnswer}
                        loading={answerMutation.isPending}
                      >
                        Save
                      </Button>
                      <Button
                        type="primary"
                        icon={<RightOutlined />}
                        onClick={goNext}
                        disabled={globalIndex === allQuestions.length - 1}
                      >
                        Next
                      </Button>
                    </Space>
                  </Col>
                </Row>
              </div>
            )}
          </Card>
        </Col>

        {/* Question navigator */}
        <Col xs={24} lg={6}>
          <Card title="Question List" size="small">
            {renderQuestionNavigator()}
            <Divider />
            <Space direction="vertical" size={4}>
              <Space>
                <div
                  style={{
                    width: 16,
                    height: 16,
                    background: '#52c41a',
                    borderRadius: 4,
                  }}
                />
                <Text type="secondary">Answered & Saved</Text>
              </Space>
              <Space>
                <div
                  style={{
                    width: 16,
                    height: 16,
                    background: '#faad14',
                    borderRadius: 4,
                  }}
                />
                <Text type="secondary">Answered (not saved)</Text>
              </Space>
              <Space>
                <div
                  style={{
                    width: 16,
                    height: 16,
                    background: '#fff',
                    border: '1px solid #d9d9d9',
                    borderRadius: 4,
                  }}
                />
                <Text type="secondary">Not answered</Text>
              </Space>
            </Space>
          </Card>
        </Col>
      </Row>

      {/* Submit Modal */}
      <Modal
        title={
          <Space>
            <ExclamationCircleOutlined style={{ color: '#faad14' }} />
            Confirm Submission
          </Space>
        }
        open={submitModalVisible}
        onCancel={() => setSubmitModalVisible(false)}
        onOk={handleSubmitAssessment}
        okText="Submit"
        cancelText="Continue"
        confirmLoading={isSubmitting || submitMutation.isPending}
        okButtonProps={{ danger: true }}
      >
        <div style={{ padding: '16px 0' }}>
          <Row gutter={[16, 16]}>
            <Col span={12}>
              <Card size="small">
                <Statistic
                  title="Answered"
                  value={answeredCount}
                  suffix={`/ ${allQuestions.length}`}
                  valueStyle={{ color: '#52c41a' }}
                  prefix={<CheckCircleOutlined />}
                />
              </Card>
            </Col>
            <Col span={12}>
              <Card size="small">
                <Statistic
                  title="Unanswered"
                  value={allQuestions.length - answeredCount}
                  suffix="questions"
                  valueStyle={{
                    color: allQuestions.length - answeredCount > 0 ? '#ff4d4f' : '#52c41a',
                  }}
                  prefix={<ExclamationCircleOutlined />}
                />
              </Card>
            </Col>
          </Row>

          {allQuestions.length - answeredCount > 0 && (
            <Alert
              type="warning"
              message={`You have ${allQuestions.length - answeredCount} unanswered questions. Are you sure you want to submit?`}
              style={{ marginTop: 16 }}
              showIcon
            />
          )}

          <Paragraph type="secondary" style={{ marginTop: 16 }}>
            After submission, you cannot change your answers.
          </Paragraph>
        </div>
      </Modal>
    </div>
  );
}
