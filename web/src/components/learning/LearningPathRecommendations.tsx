import { Card, Typography, Space, Tag, Button, Empty, Timeline, Alert, Collapse, Progress } from 'antd';
import {
  BookOutlined,
  LinkOutlined,
  ClockCircleOutlined,
  TrophyOutlined,
  CheckCircleOutlined,
  BulbOutlined,
  WarningOutlined,
  RiseOutlined,
} from '@ant-design/icons';
import type { LearningPathResponse } from '../../api/employees';

const { Title, Text, Paragraph } = Typography;
const { Panel } = Collapse;

interface LearningPathRecommendationsProps {
  learningPath: LearningPathResponse;
  compact?: boolean;
}

export default function LearningPathRecommendations({
  learningPath,
  compact = false,
}: LearningPathRecommendationsProps) {
  if (!learningPath || !learningPath.items || learningPath.items.length === 0) {
    return (
      <Empty
        description="Chưa có lộ trình học tập"
        image={Empty.PRESENTED_IMAGE_SIMPLE}
      />
    );
  }

  const getItemTypeColor = (type: string) => {
    const colors: Record<string, string> = {
      Course: 'blue',
      Book: 'green',
      Video: 'purple',
      Project: 'orange',
      Workshop: 'cyan',
      Certification: 'gold',
      Mentorship: 'magenta',
    };
    return colors[type] || 'default';
  };

  const getStatusColor = (status: string) => {
    const colors: Record<string, string> = {
      NotStarted: 'default',
      InProgress: 'processing',
      Completed: 'success',
      Skipped: 'warning',
    };
    return colors[status] || 'default';
  };

  const calculateProgress = () => {
    const completed = learningPath.items.filter((i) => i.status === 'Completed').length;
    return (completed / learningPath.items.length) * 100;
  };

  return (
    <div>
      {/* Header */}
      <Card>
        <Space direction="vertical" style={{ width: '100%' }} size="middle">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <div>
              <Title level={4} style={{ margin: 0 }}>
                <BookOutlined style={{ color: '#1890ff' }} /> {learningPath.title}
              </Title>
              {learningPath.isAiGenerated && (
                <Tag color="purple" style={{ marginTop: 8 }}>
                  AI-Generated with Coursera
                </Tag>
              )}
            </div>
            <Tag color="blue">{learningPath.status}</Tag>
          </div>

          {learningPath.description && (
            <Paragraph type="secondary">{learningPath.description}</Paragraph>
          )}

          {/* Statistics */}
          <Space size="large" wrap>
            <Space>
              <ClockCircleOutlined />
              <Text strong>{learningPath.estimatedTotalHours || 0} giờ</Text>
            </Space>
            {learningPath.estimatedDurationWeeks && (
              <Space>
                <Text strong>{learningPath.estimatedDurationWeeks} tuần</Text>
              </Space>
            )}
            <Space>
              <TrophyOutlined />
              <Text>
                Level {learningPath.currentLevel || 0} → {learningPath.targetLevel}
              </Text>
            </Space>
          </Space>

          {/* Progress */}
          <div>
            <Text type="secondary">Tiến độ hoàn thành:</Text>
            <Progress percent={Math.round(calculateProgress())} status="active" />
          </div>
        </Space>
      </Card>

      {/* AI Insights */}
      {learningPath.isAiGenerated && !compact && (
        <Collapse style={{ marginTop: 16 }} defaultActiveKey={['rationale']}>
          <Panel
            header={
              <Space>
                <BulbOutlined style={{ color: '#faad14' }} />
                <Text strong>AI Analysis & Recommendations</Text>
              </Space>
            }
            key="rationale"
          >
            {learningPath.aiRationale && (
              <Alert
                message="Lý do AI đề xuất lộ trình này"
                description={learningPath.aiRationale}
                type="info"
                showIcon
                icon={<BulbOutlined />}
                style={{ marginBottom: 16 }}
              />
            )}

            {learningPath.keySuccessFactors && learningPath.keySuccessFactors.length > 0 && (
              <div style={{ marginBottom: 16 }}>
                <Title level={5}>
                  <CheckCircleOutlined style={{ color: '#52c41a' }} /> Yếu tố thành công
                </Title>
                <ul>
                  {learningPath.keySuccessFactors.map((factor, idx) => (
                    <li key={idx}>
                      <Text>{factor}</Text>
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {learningPath.potentialChallenges && learningPath.potentialChallenges.length > 0 && (
              <div>
                <Title level={5}>
                  <WarningOutlined style={{ color: '#faad14' }} /> Thách thức tiềm ẩn
                </Title>
                <ul>
                  {learningPath.potentialChallenges.map((challenge, idx) => (
                    <li key={idx}>
                      <Text type="warning">{challenge}</Text>
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </Panel>
        </Collapse>
      )}

      {/* Learning Items */}
      <Card title="Lộ trình học tập chi tiết" style={{ marginTop: 16 }}>
        <Timeline>
          {learningPath.items.map((item, idx) => {
            // Check if there's a milestone after this item
            const milestone = learningPath.milestones?.find((m) => m.afterItem === idx + 1);

            return (
              <div key={item.id}>
                <Timeline.Item
                  color={item.status === 'Completed' ? 'green' : 'blue'}
                  dot={
                    item.status === 'Completed' ? (
                      <CheckCircleOutlined style={{ fontSize: '16px' }} />
                    ) : undefined
                  }
                >
                  <Card size="small" style={{ marginBottom: 8 }}>
                    <Space direction="vertical" style={{ width: '100%' }}>
                      {/* Header */}
                      <div
                        style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}
                      >
                        <Space>
                          <Tag color={getItemTypeColor(item.itemType)}>{item.itemType}</Tag>
                          <Text strong>{item.title}</Text>
                        </Space>
                        <Tag color={getStatusColor(item.status)}>{item.status}</Tag>
                      </div>

                      {/* Description */}
                      {item.description && <Text type="secondary">{item.description}</Text>}

                      {/* Metadata */}
                      <Space wrap>
                        {item.estimatedHours && (
                          <Tag icon={<ClockCircleOutlined />}>{item.estimatedHours}h</Tag>
                        )}
                        {item.targetLevelAfter !== undefined && (
                          <Tag icon={<TrophyOutlined />} color="gold">
                            Target: Level {item.targetLevelAfter}
                          </Tag>
                        )}
                      </Space>

                      {/* Success Criteria */}
                      {item.successCriteria && !compact && (
                        <Alert
                          message="Tiêu chí thành công"
                          description={item.successCriteria}
                          type="success"
                          showIcon
                          icon={<CheckCircleOutlined />}
                          style={{ marginTop: 8 }}
                        />
                      )}

                      {/* External Link (Coursera) */}
                      {item.externalUrl && (
                        <Button
                          type="primary"
                          icon={<LinkOutlined />}
                          href={item.externalUrl}
                          target="_blank"
                          rel="noopener noreferrer"
                          style={{ marginTop: 8 }}
                        >
                          Xem khóa học trên Coursera →
                        </Button>
                      )}
                    </Space>
                  </Card>
                </Timeline.Item>

                {/* Milestone */}
                {milestone && !compact && (
                  <Timeline.Item
                    color="purple"
                    dot={<RiseOutlined style={{ fontSize: '16px' }} />}
                  >
                    <Alert
                      message={`Milestone: ${milestone.description}`}
                      description={`Expected Level: ${milestone.expectedLevel}`}
                      type="info"
                      showIcon
                      style={{ marginBottom: 8 }}
                    />
                  </Timeline.Item>
                )}
              </div>
            );
          })}
        </Timeline>
      </Card>
    </div>
  );
}
