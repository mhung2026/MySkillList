import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Card,
  Row,
  Col,
  Statistic,
  Table,
  Tag,
  Progress,
  Select,
  Spin,
  Typography,
  Space,
  Tooltip,
  Badge,
} from 'antd';
import {
  TeamOutlined,
  ToolOutlined,
  FileTextOutlined,
  CheckCircleOutlined,
  UserOutlined,
  TrophyOutlined,
} from '@ant-design/icons';
import {
  dashboardApi,
} from '../../api/dashboard';
import type {
  DashboardOverviewDto,
  EmployeeSkillSummaryDto,
  TeamSkillMatrixDto,
} from '../../api/dashboard';

const { Title, Text } = Typography;

// Color for proficiency levels
const levelColors: Record<number, string> = {
  1: '#ff4d4f',
  2: '#ff7a45',
  3: '#ffa940',
  4: '#fadb14',
  5: '#73d13d',
  6: '#36cfc9',
  7: '#1890ff',
};

const levelNames: Record<number, string> = {
  1: 'Follow',
  2: 'Assist',
  3: 'Apply',
  4: 'Enable',
  5: 'Ensure/Advise',
  6: 'Initiate/Influence',
  7: 'Set Strategy',
};

export default function Dashboard() {
  const [selectedTeamId, setSelectedTeamId] = useState<string | undefined>(undefined);

  // Fetch overview data
  const { data: overview, isLoading: loadingOverview } = useQuery<DashboardOverviewDto>({
    queryKey: ['dashboard-overview'],
    queryFn: dashboardApi.getOverview,
  });

  // Fetch employee skills
  const { data: employeeSkills, isLoading: loadingEmployeeSkills } = useQuery<EmployeeSkillSummaryDto[]>({
    queryKey: ['employee-skills', selectedTeamId],
    queryFn: () => dashboardApi.getEmployeeSkills(selectedTeamId),
  });

  // Fetch skill matrix
  const { data: skillMatrix, isLoading: loadingSkillMatrix } = useQuery<TeamSkillMatrixDto>({
    queryKey: ['skill-matrix', selectedTeamId],
    queryFn: () => dashboardApi.getSkillMatrix(selectedTeamId),
  });

  if (loadingOverview) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 100 }}>
        <Spin size="large" />
      </div>
    );
  }

  // Build team options from overview data
  const teamOptions = overview?.teamDistribution?.map((t) => ({
    value: t.teamId || '',
    label: t.teamName,
  })) || [];

  // Employee skills table columns
  const employeeColumns = [
    {
      title: 'Employee',
      dataIndex: 'employeeName',
      key: 'employeeName',
      width: 200,
      render: (name: string, record: EmployeeSkillSummaryDto) => (
        <Space>
          <UserOutlined />
          <div>
            <div>{name}</div>
            <Text type="secondary" style={{ fontSize: 12 }}>
              {record.teamName || 'No team'} | {record.jobRoleName || 'No role'}
            </Text>
          </div>
        </Space>
      ),
    },
    {
      title: 'Skills',
      dataIndex: 'totalSkills',
      key: 'totalSkills',
      width: 100,
      align: 'center' as const,
      render: (count: number) => <Badge count={count} showZero color="#1890ff" />,
    },
    {
      title: 'Avg Level',
      dataIndex: 'averageLevel',
      key: 'averageLevel',
      width: 120,
      align: 'center' as const,
      render: (level: number) => (
        <Tag color={levelColors[Math.round(level)] || '#999'}>
          {level.toFixed(1)}
        </Tag>
      ),
    },
    {
      title: 'Skills',
      key: 'skills',
      render: (_: unknown, record: EmployeeSkillSummaryDto) => (
        <Space wrap size={[4, 4]}>
          {record.skills.slice(0, 5).map((skill) => (
            <Tooltip
              key={skill.skillId}
              title={`${skill.skillName} - ${levelNames[skill.currentLevel] || `Level ${skill.currentLevel}`}`}
            >
              <Tag
                color={levelColors[skill.currentLevel]}
                style={{ cursor: 'pointer' }}
              >
                {skill.skillCode}
              </Tag>
            </Tooltip>
          ))}
          {record.skills.length > 5 && (
            <Tag>+{record.skills.length - 5} more</Tag>
          )}
        </Space>
      ),
    },
  ];

  // Skill matrix columns
  const matrixColumns = skillMatrix
    ? [
        {
          title: 'Employee',
          dataIndex: 'employeeName',
          key: 'employeeName',
          fixed: 'left' as const,
          width: 150,
        },
        ...skillMatrix.skills.map((skill) => ({
          title: (
            <Tooltip title={skill.skillName}>
              <span>{skill.skillCode}</span>
            </Tooltip>
          ),
          dataIndex: ['skillLevels', skill.skillId],
          key: skill.skillId,
          width: 70,
          align: 'center' as const,
          render: (level: number | undefined) =>
            level ? (
              <Tag color={levelColors[level]} style={{ margin: 0 }}>
                {level}
              </Tag>
            ) : (
              <span style={{ color: '#ccc' }}>-</span>
            ),
        })),
      ]
    : [];

  return (
    <div>
      <Title level={3}>Dashboard - Employee Skills Overview</Title>

      {/* Statistics Cards */}
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Employees"
              value={overview?.totalEmployees || 0}
              prefix={<TeamOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Skills"
              value={overview?.totalSkills || 0}
              prefix={<ToolOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Completed Assessments"
              value={overview?.totalAssessments || 0}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Test Templates"
              value={overview?.totalTestTemplates || 0}
              prefix={<FileTextOutlined />}
              valueStyle={{ color: '#fa8c16' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Distribution Charts */}
      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col xs={24} lg={12}>
          <Card title="Employee Distribution by Team">
            {overview?.teamDistribution?.map((team) => (
              <div key={team.teamId || 'none'} style={{ marginBottom: 12 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                  <Text>{team.teamName}</Text>
                  <Text type="secondary">
                    {team.employeeCount} ({team.percentage}%)
                  </Text>
                </div>
                <Progress
                  percent={team.percentage}
                  showInfo={false}
                  strokeColor="#1890ff"
                />
              </div>
            ))}
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card title="Proficiency Level Distribution">
            {overview?.proficiencyDistribution?.map((level) => (
              <div key={level.level} style={{ marginBottom: 12 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                  <Space>
                    <Tag color={levelColors[level.level]}>L{level.level}</Tag>
                    <Text>{levelNames[level.level] || level.levelName}</Text>
                  </Space>
                  <Text type="secondary">
                    {level.employeeSkillCount} ({level.percentage}%)
                  </Text>
                </div>
                <Progress
                  percent={level.percentage}
                  showInfo={false}
                  strokeColor={levelColors[level.level]}
                />
              </div>
            ))}
          </Card>
        </Col>
      </Row>

      {/* Top Skills */}
      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col xs={24} lg={12}>
          <Card
            title={
              <Space>
                <TrophyOutlined style={{ color: '#faad14' }} />
                Top 10 Popular Skills
              </Space>
            }
          >
            <Table
              dataSource={overview?.topSkills}
              rowKey="skillId"
              pagination={false}
              size="small"
              columns={[
                {
                  title: '#',
                  key: 'rank',
                  width: 40,
                  render: (_, __, index) => index + 1,
                },
                {
                  title: 'Skill',
                  dataIndex: 'skillName',
                  key: 'skillName',
                  render: (name: string, record) => (
                    <div>
                      <div>{name}</div>
                      <Text type="secondary" style={{ fontSize: 11 }}>
                        {record.domainName}
                      </Text>
                    </div>
                  ),
                },
                {
                  title: 'Employees',
                  dataIndex: 'employeeCount',
                  key: 'employeeCount',
                  width: 80,
                  align: 'center' as const,
                },
                {
                  title: 'Avg Level',
                  dataIndex: 'averageLevel',
                  key: 'averageLevel',
                  width: 80,
                  align: 'center' as const,
                  render: (level: number) => (
                    <Tag color={levelColors[Math.round(level)]}>
                      {level.toFixed(1)}
                    </Tag>
                  ),
                },
              ]}
            />
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card title="Domain Skill Coverage">
            {overview?.domainSkillCoverage?.map((domain) => (
              <div key={domain.domainId} style={{ marginBottom: 12 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                  <Text strong>{domain.domainName}</Text>
                  <Text type="secondary">
                    {domain.totalSkills} skills | {domain.employeesWithSkills} records
                  </Text>
                </div>
                <Progress
                  percent={domain.coveragePercentage}
                  size="small"
                  format={(percent) => `${percent}%`}
                />
              </div>
            ))}
          </Card>
        </Col>
      </Row>

      {/* Recent Assessments */}
      <Card title="Recent Assessments" style={{ marginTop: 16 }}>
        <Table
          dataSource={overview?.recentAssessments}
          rowKey="id"
          pagination={false}
          size="small"
          columns={[
            {
              title: 'Employee',
              dataIndex: 'employeeName',
              key: 'employeeName',
            },
            {
              title: 'Test',
              dataIndex: 'testTitle',
              key: 'testTitle',
            },
            {
              title: 'Score',
              key: 'score',
              render: (_, record) => (
                <span>
                  {record.score}/{record.maxScore}
                </span>
              ),
            },
            {
              title: 'Result',
              key: 'result',
              render: (_, record) => (
                <Space>
                  <Progress
                    type="circle"
                    percent={record.percentage}
                    width={40}
                    format={(p) => `${p}%`}
                  />
                  <Tag color={record.passed ? 'success' : 'error'}>
                    {record.passed ? 'Passed' : 'Failed'}
                  </Tag>
                </Space>
              ),
            },
            {
              title: 'Completed',
              dataIndex: 'completedAt',
              key: 'completedAt',
              render: (date: string) =>
                new Date(date).toLocaleDateString('en-US', {
                  day: '2-digit',
                  month: '2-digit',
                  year: 'numeric',
                  hour: '2-digit',
                  minute: '2-digit',
                }),
            },
          ]}
        />
      </Card>

      {/* Employee Skills */}
      <Card
        title="Employee Skills List"
        style={{ marginTop: 16 }}
        extra={
          <Select
            placeholder="Filter by team"
            allowClear
            style={{ width: 200 }}
            value={selectedTeamId}
            onChange={setSelectedTeamId}
            options={teamOptions}
          />
        }
      >
        <Table
          dataSource={employeeSkills}
          columns={employeeColumns}
          rowKey="employeeId"
          loading={loadingEmployeeSkills}
          pagination={{ pageSize: 10 }}
        />
      </Card>

      {/* Skill Matrix */}
      <Card title={`Skill Matrix - ${skillMatrix?.teamName || 'All'}`} style={{ marginTop: 16 }}>
        {loadingSkillMatrix ? (
          <Spin />
        ) : skillMatrix?.skills?.length ? (
          <Table
            dataSource={skillMatrix.employees}
            columns={matrixColumns}
            rowKey="employeeId"
            scroll={{ x: 'max-content' }}
            pagination={false}
            size="small"
            bordered
          />
        ) : (
          <Text type="secondary">No skill matrix data available</Text>
        )}
      </Card>

      {/* Legend */}
      <Card title="Proficiency Levels Legend (SFIA)" style={{ marginTop: 16 }}>
        <Space wrap>
          {Object.entries(levelNames).map(([level, name]) => (
            <Tag key={level} color={levelColors[parseInt(level)]}>
              Level {level}: {name}
            </Tag>
          ))}
        </Space>
      </Card>
    </div>
  );
}
