import { useEffect } from 'react';
import {
  Modal,
  Form,
  Input,
  InputNumber,
  Select,
  Button,
  Space,
  Card,
  Switch,
  Divider,
} from 'antd';
import {
  PlusOutlined,
  DeleteOutlined,
  CheckCircleOutlined,
} from '@ant-design/icons';
import type {
  QuestionDto,
  CreateQuestionDto,
  SkillListDto,
} from '../types';
import { ProficiencyLevel, DifficultyLevel } from '../types';

const { TextArea } = Input;

interface QuestionFormModalProps {
  open: boolean;
  onCancel: () => void;
  onSubmit: (values: CreateQuestionDto) => void;
  loading?: boolean;
  editingQuestion?: QuestionDto | null;
  sectionId: string;
  skills?: SkillListDto[];
  questionTypeOptions?: { value: number; label: string }[];
  difficultyOptions?: { value: number; label: string }[];
}

const proficiencyOptions = [
  { value: ProficiencyLevel.Follow, label: 'Level 1 - Follow' },
  { value: ProficiencyLevel.Assist, label: 'Level 2 - Assist' },
  { value: ProficiencyLevel.Apply, label: 'Level 3 - Apply' },
  { value: ProficiencyLevel.Enable, label: 'Level 4 - Enable' },
  { value: ProficiencyLevel.EnsureAdvise, label: 'Level 5 - Ensure/Advise' },
  { value: ProficiencyLevel.Initiate, label: 'Level 6 - Initiate' },
  { value: ProficiencyLevel.SetStrategy, label: 'Level 7 - Set Strategy' },
];

// Question types that need options (using numeric values)
// MultipleChoice=1, MultipleAnswer=2, TrueFalse=3
const MULTIPLE_CHOICE = 1;
const MULTIPLE_ANSWER = 2;
const TRUE_FALSE = 3;
const typesWithOptions = [MULTIPLE_CHOICE, MULTIPLE_ANSWER, TRUE_FALSE];

export default function QuestionFormModal({
  open,
  onCancel,
  onSubmit,
  loading,
  editingQuestion,
  sectionId,
  skills = [],
  questionTypeOptions = [],
  difficultyOptions = [],
}: QuestionFormModalProps) {
  const [form] = Form.useForm();
  const questionType = Form.useWatch('type', form);

  useEffect(() => {
    if (open) {
      // Always reset first
      form.resetFields();

      if (editingQuestion) {
        // Set timeout to ensure form is ready after reset
        setTimeout(() => {
          form.setFieldsValue({
            content: editingQuestion.content,
            type: editingQuestion.type,
            difficulty: editingQuestion.difficulty,
            targetLevel: editingQuestion.targetLevel,
            skillId: editingQuestion.skillId,
            points: editingQuestion.points,
            timeLimitSeconds: editingQuestion.timeLimitSeconds,
            codeSnippet: editingQuestion.codeSnippet,
            gradingRubric: editingQuestion.gradingRubric,
            options: editingQuestion.options?.map((opt, index) => ({
              content: opt.content,
              isCorrect: opt.isCorrect,
              displayOrder: opt.displayOrder || index + 1,
              explanation: opt.explanation,
            })) || [],
          });
        }, 0);
      } else {
        form.setFieldsValue({
          sectionId,
          points: 1,
          targetLevel: ProficiencyLevel.Apply,
          difficulty: DifficultyLevel.Medium,
          type: MULTIPLE_CHOICE,
          options: [
            { content: '', isCorrect: true, displayOrder: 1 },
            { content: '', isCorrect: false, displayOrder: 2 },
            { content: '', isCorrect: false, displayOrder: 3 },
            { content: '', isCorrect: false, displayOrder: 4 },
          ],
        });
      }
    }
  }, [open, editingQuestion, sectionId, form]);

  // Handle question type change - set appropriate options
  const handleTypeChange = (newType: number) => {
    const currentOptions = form.getFieldValue('options') || [];

    if (newType === TRUE_FALSE) {
      // True/False: always set fixed 2 options
      form.setFieldsValue({
        options: [
          { content: 'True', isCorrect: true, displayOrder: 1 },
          { content: 'False', isCorrect: false, displayOrder: 2 },
        ],
      });
    } else if (newType === MULTIPLE_CHOICE || newType === MULTIPLE_ANSWER) {
      // Multiple Choice/Answer: check if need to add default options
      const isTrueFalseOptions = currentOptions.length === 2 &&
        currentOptions[0]?.content === 'True' &&
        currentOptions[1]?.content === 'False';
      const hasNoOptions = currentOptions.length === 0;

      if (isTrueFalseOptions || hasNoOptions) {
        form.setFieldsValue({
          options: [
            { content: '', isCorrect: true, displayOrder: 1 },
            { content: '', isCorrect: false, displayOrder: 2 },
            { content: '', isCorrect: false, displayOrder: 3 },
            { content: '', isCorrect: false, displayOrder: 4 },
          ],
        });
      } else if (newType === MULTIPLE_CHOICE) {
        // When switching to Multiple Choice, ensure only one correct answer
        const correctCount = currentOptions.filter((opt: { isCorrect: boolean }) => opt.isCorrect).length;
        if (correctCount > 1) {
          // Keep only the first correct answer
          let foundFirst = false;
          const updatedOptions = currentOptions.map((opt: { content: string; isCorrect: boolean; displayOrder: number; explanation?: string }) => {
            if (opt.isCorrect && !foundFirst) {
              foundFirst = true;
              return opt;
            }
            return { ...opt, isCorrect: false };
          });
          form.setFieldsValue({ options: updatedOptions });
        }
      }
    }
  };

  const handleSubmit = (values: CreateQuestionDto) => {
    // Clean up options for non-option question types
    const valueType = values.type as number;
    if (!typesWithOptions.includes(valueType)) {
      values.options = [];
    }
    onSubmit({
      ...values,
      sectionId,
    });
  };

  // Use editingQuestion.type as fallback when questionType is not yet set
  const currentType = questionType ?? editingQuestion?.type ?? MULTIPLE_CHOICE;
  const needsOptions = typesWithOptions.includes(currentType);
  const isTrueFalse = currentType === TRUE_FALSE;

  return (
    <Modal
      title={editingQuestion ? 'Edit Question' : 'Add Question'}
      open={open}
      onCancel={onCancel}
      footer={null}
      width={800}
    >
      <Form
        form={form}
        layout="vertical"
        onFinish={handleSubmit}
      >
        <Form.Item
          name="content"
          label="Question Content"
          rules={[{ required: true, message: 'Please enter question content' }]}
        >
          <TextArea rows={3} placeholder="Enter your question here..." />
        </Form.Item>

        <Space style={{ width: '100%' }} size="middle">
          <Form.Item
            name="type"
            label="Question Type"
            rules={[{ required: true }]}
            style={{ flex: 1 }}
          >
            <Select
              options={questionTypeOptions}
              placeholder="Select type"
              onChange={handleTypeChange}
            />
          </Form.Item>

          <Form.Item
            name="difficulty"
            label="Difficulty"
            rules={[{ required: true }]}
            style={{ flex: 1 }}
          >
            <Select
              options={difficultyOptions}
              placeholder="Select difficulty"
            />
          </Form.Item>

          <Form.Item
            name="targetLevel"
            label="Target Level"
            rules={[{ required: true }]}
            style={{ flex: 1 }}
          >
            <Select
              options={proficiencyOptions}
              placeholder="Select level"
            />
          </Form.Item>
        </Space>

        <Space style={{ width: '100%' }} size="middle">
          <Form.Item
            name="skillId"
            label="Skill (Optional)"
            style={{ flex: 2 }}
          >
            <Select
              allowClear
              showSearch
              optionFilterProp="label"
              placeholder="Select a skill"
              options={skills.map((s) => ({
                value: s.id,
                label: `${s.code} - ${s.name}`,
              }))}
            />
          </Form.Item>

          <Form.Item
            name="points"
            label="Points"
            rules={[{ required: true }]}
            style={{ flex: 1 }}
          >
            <InputNumber min={1} max={100} style={{ width: '100%' }} />
          </Form.Item>

          <Form.Item
            name="timeLimitSeconds"
            label="Time Limit (sec)"
            style={{ flex: 1 }}
          >
            <InputNumber min={10} max={3600} style={{ width: '100%' }} placeholder="Optional" />
          </Form.Item>
        </Space>

        <Form.Item name="codeSnippet" label="Code Snippet (Optional)">
          <TextArea
            rows={4}
            placeholder="// Add code snippet if applicable"
            style={{ fontFamily: 'monospace' }}
          />
        </Form.Item>

        {/* Options section for multiple choice questions */}
        {needsOptions && (
          <>
            <Divider titlePlacement="left">Answer Options</Divider>
            <Form.List name="options">
              {(fields, { add, remove }) => (
                <>
                  {fields.map(({ key, name, ...restField }, index) => (
                    <Card
                      key={key}
                      size="small"
                      style={{ marginBottom: 8 }}
                      bodyStyle={{ padding: '12px' }}
                    >
                      <Space align="start" style={{ width: '100%' }}>
                        <Form.Item
                          {...restField}
                          name={[name, 'isCorrect']}
                          valuePropName="checked"
                          style={{ marginBottom: 0 }}
                        >
                          <Switch
                            checkedChildren={<CheckCircleOutlined />}
                            unCheckedChildren=""
                            onChange={(checked) => {
                              // For True/False and Multiple Choice, only one can be correct
                              const isSingleAnswer = currentType === TRUE_FALSE || currentType === MULTIPLE_CHOICE;
                              if (isSingleAnswer && checked) {
                                const options = form.getFieldValue('options');
                                const updatedOptions = options.map((opt: { content: string; isCorrect: boolean; displayOrder: number; explanation?: string }, idx: number) => ({
                                  ...opt,
                                  isCorrect: idx === name,
                                }));
                                form.setFieldsValue({ options: updatedOptions });
                              }
                            }}
                          />
                        </Form.Item>

                        <Form.Item
                          {...restField}
                          name={[name, 'content']}
                          rules={[{ required: true, message: 'Enter option content' }]}
                          style={{ marginBottom: 0, flex: 1, minWidth: 300 }}
                        >
                          <Input
                            placeholder={`Option ${index + 1}`}
                            disabled={isTrueFalse}
                          />
                        </Form.Item>

                        <Form.Item
                          {...restField}
                          name={[name, 'explanation']}
                          style={{ marginBottom: 0, flex: 1, minWidth: 200 }}
                        >
                          <Input placeholder="Explanation (optional)" />
                        </Form.Item>

                        <Form.Item
                          {...restField}
                          name={[name, 'displayOrder']}
                          initialValue={index + 1}
                          hidden
                        >
                          <InputNumber />
                        </Form.Item>

                        {!isTrueFalse && fields.length > 2 && (
                          <Button
                            type="text"
                            danger
                            icon={<DeleteOutlined />}
                            onClick={() => remove(name)}
                          />
                        )}
                      </Space>
                    </Card>
                  ))}

                  {!isTrueFalse && fields.length < 6 && (
                    <Button
                      type="dashed"
                      onClick={() =>
                        add({
                          content: '',
                          isCorrect: false,
                          displayOrder: fields.length + 1,
                        })
                      }
                      icon={<PlusOutlined />}
                      style={{ width: '100%' }}
                    >
                      Add Option
                    </Button>
                  )}
                </>
              )}
            </Form.List>
          </>
        )}

        <Form.Item name="gradingRubric" label="Grading Rubric (Optional)" style={{ marginTop: 16 }}>
          <TextArea rows={2} placeholder="Grading criteria for manual review..." />
        </Form.Item>

        <Form.Item style={{ marginBottom: 0, textAlign: 'right', marginTop: 24 }}>
          <Space>
            <Button onClick={onCancel}>Cancel</Button>
            <Button type="primary" htmlType="submit" loading={loading}>
              {editingQuestion ? 'Update Question' : 'Add Question'}
            </Button>
          </Space>
        </Form.Item>
      </Form>
    </Modal>
  );
}
