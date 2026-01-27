# AI Gen API Documentation

## Base URL
```
http://localhost:8002/api/v2
```

---

## 1. Health & Info Endpoints

### GET /health
Check API health status.

**Response:**
```json
{
  "status": "healthy",
  "api_version": "v2",
  "database": "connected",
  "total_definitions": 146
}
```

### GET /stats
Get database statistics.

**Response:**
```json
{
  "success": true,
  "stats": {
    "total_skills": 146,
    "total_level_definitions": 500,
    "sfia_levels": "1-7"
  }
}
```

### GET /skills
Get all available skills.

**Response:**
```json
{
  "success": true,
  "skills": [
    {
      "skill_id": "30000000-0000-0000-0000-000000000001",
      "skill_name": "Strategic planning",
      "skill_code": "ITSP",
      "level_count": 4
    }
  ],
  "total": 146
}
```

### GET /skills/{skill_id}/levels
Get proficiency levels for a specific skill.

**Response:**
```json
{
  "success": true,
  "skill_id": "30000000-0000-0000-0000-000000000001",
  "levels": [
    {
      "level": 4,
      "description": "Contributes to strategic planning...",
      "autonomy": "Works under general direction",
      "influence": "Influences organisation",
      "complexity": "Work includes complex technical activities",
      "business_skills": "Communicates effectively",
      "knowledge": "Has a thorough understanding..."
    }
  ]
}
```

---

## 2. Question Generation Endpoints

### POST /generate-questions
Generate assessment questions using AI.

**Request:**
```json
{
  "question_type": ["MultipleChoice", "ShortAnswer"],
  "language": "Vietnamese",
  "number_of_questions": 5,
  "skills": [
    {
      "skill_id": "30000000-0000-0000-0000-000000000001",
      "skill_name": "Strategic planning",
      "skill_code": "ITSP"
    }
  ],
  "target_proficiency_level": [3, 4],
  "difficulty": "Medium",
  "additional_context": "Focus on practical scenarios"
}
```

**Request Fields:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| question_type | string[] | Yes | Question types: MultipleChoice, MultipleAnswer, TrueFalse, ShortAnswer, LongAnswer, CodingChallenge |
| language | string | Yes | "English" or "Vietnamese" |
| number_of_questions | int | Yes | 1-100 |
| skills | SkillInfo[] | No | Skills to focus on |
| target_proficiency_level | int[] | No | SFIA levels 1-7 |
| difficulty | string | No | Easy, Medium, Hard |
| additional_context | string | No | Max 2000 chars |

**Response:**
```json
{
  "success": true,
  "questions": [
    {
      "question_id": "q_001",
      "type": "MultipleChoice",
      "content": "Trong quy trình lập kế hoạch chiến lược CNTT, bước nào sau đây nên được thực hiện đầu tiên?",
      "code_snippet": null,
      "options": [
        {
          "option_id": "a",
          "content": "Phân tích hiện trạng hệ thống",
          "is_correct": true,
          "explanation": "Phân tích hiện trạng giúp xác định điểm xuất phát..."
        },
        {
          "option_id": "b",
          "content": "Xác định ngân sách",
          "is_correct": false,
          "explanation": null
        }
      ],
      "correct_answer": "a",
      "explanation": "Phân tích hiện trạng là bước đầu tiên...",
      "difficulty": "Medium",
      "proficiency_level": 3,
      "skill_id": "30000000-0000-0000-0000-000000000001",
      "skill_name": "Strategic planning",
      "points": 10,
      "grading_rubric": null
    }
  ],
  "metadata": {
    "total_questions": 5,
    "generation_time": "2024-01-15T10:30:00Z",
    "model_used": "gpt-4o",
    "language": "Vietnamese"
  }
}
```

---

## 3. Answer Grading Endpoint

### POST /grade-answer
Grade a student's answer using AI.

**Request:**
```json
{
  "question_id": "q_001",
  "question_content": "Explain the difference between a list and a tuple in Python.",
  "student_answer": "A list is mutable, meaning you can change its contents. A tuple is immutable. Lists use [], tuples use ().",
  "max_points": 10,
  "grading_rubric": "{\"criteria\": [{\"description\": \"Explains mutability\", \"points\": 4}, {\"description\": \"Syntax difference\", \"points\": 3}, {\"description\": \"Performance mention\", \"points\": 3}]}",
  "expected_answer": "Lists are mutable, tuples are immutable. Lists use [], tuples use ().",
  "question_type": "ShortAnswer",
  "language": "en"
}
```

**Request Fields:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| question_id | string | No | Question identifier |
| question_content | string | Yes | The question text |
| student_answer | string | Yes | Student's submitted answer |
| max_points | int | Yes | 1-100 |
| grading_rubric | string | No | JSON string with grading criteria |
| expected_answer | string | No | Model answer |
| question_type | string | No | ShortAnswer, LongAnswer, CodingChallenge |
| language | string | No | "en" or "vi" (default: "en") |

**Response:**
```json
{
  "success": true,
  "points_awarded": 7,
  "max_points": 10,
  "percentage": 70.0,
  "feedback": "Good explanation of the core differences between lists and tuples.",
  "strength_points": [
    "Correctly identified mutability as the key difference",
    "Mentioned syntax differences"
  ],
  "improvement_areas": [
    "Could mention performance/memory differences",
    "Could provide use case examples"
  ],
  "detailed_analysis": "The student demonstrated understanding of the fundamental difference..."
}
```

---

## 4. Skill Gap Analysis Endpoints

### POST /analyze-gap
Analyze a single skill gap using AI.

**Request:**
```json
{
  "employee_name": "Nguyen Van A",
  "job_role": "Senior Backend Developer",
  "skill_id": "30000000-0000-0000-0000-000000000001",
  "skill_name": "System Design",
  "skill_code": "SYSDES",
  "current_level": 2,
  "required_level": 4,
  "skill_description": "Designing scalable system architectures",
  "current_level_description": "Assists with system design tasks",
  "required_level_description": "Enables others and ensures quality in system design",
  "language": "vi"
}
```

**Request Fields:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| employee_name | string | Yes | Employee name |
| job_role | string | Yes | Job role name |
| skill_id | string | No | Skill UUID |
| skill_name | string | Yes | Skill name |
| skill_code | string | Yes | Skill code |
| current_level | int | Yes | 0-7 (SFIA level) |
| required_level | int | Yes | 1-7 (SFIA level) |
| skill_description | string | No | Description of the skill |
| current_level_description | string | No | Description of current level |
| required_level_description | string | No | Description of required level |
| language | string | No | "en" or "vi" (default: "en") |

**Response:**
```json
{
  "success": true,
  "ai_analysis": "Khoảng cách 2 cấp độ trong System Design là đáng kể cho vai trò Senior Backend Developer. Ở cấp độ hiện tại (Assist), nhân viên có thể hỗ trợ các task thiết kế nhưng chưa đủ năng lực để dẫn dắt thiết kế độc lập. Để đạt cấp độ Enable, cần có khả năng thiết kế hệ thống end-to-end và hướng dẫn người khác.",
  "ai_recommendation": "Nên bắt đầu với khóa học chuyên sâu về Distributed Systems và Microservices Architecture. Kết hợp với việc tham gia các dự án thực tế dưới sự hướng dẫn của senior architect. Sau 3 tháng, nên chủ động lead thiết kế cho các module nhỏ.",
  "priority_rationale": "Ưu tiên cao vì System Design là kỹ năng cốt lõi cho vai trò Senior, ảnh hưởng trực tiếp đến chất lượng sản phẩm và khả năng scale của team.",
  "estimated_effort": "4-6 tháng với training tập trung và thực hành project",
  "key_actions": [
    "Hoàn thành khóa System Design Interview trên Educative",
    "Đọc sách 'Designing Data-Intensive Applications'",
    "Thực hành thiết kế 3-5 system design cases",
    "Shadow senior architect trong 2-3 dự án"
  ],
  "potential_blockers": [
    "Thiếu thời gian do workload hiện tại",
    "Cần mentor có kinh nghiệm system design",
    "Thiếu cơ hội thực hành với dự án thực tế"
  ]
}
```

### POST /analyze-gaps
Analyze multiple skill gaps at once.

**Request:**
```json
{
  "employee_name": "Nguyen Van A",
  "job_role": "Senior Backend Developer",
  "gaps": [
    {
      "skill_id": "skill-001",
      "skill_name": "System Design",
      "skill_code": "SYSDES",
      "current_level": 2,
      "required_level": 4,
      "skill_description": "Designing scalable systems"
    },
    {
      "skill_id": "skill-002",
      "skill_name": "Cloud Architecture",
      "skill_code": "CLOUD",
      "current_level": 3,
      "required_level": 4
    }
  ],
  "language": "en"
}
```

**Response:**
```json
{
  "success": true,
  "gap_analyses": [
    {
      "skill_id": "skill-001",
      "skill_name": "System Design",
      "gap_size": 2,
      "success": true,
      "ai_analysis": "A 2-level gap in System Design is significant...",
      "ai_recommendation": "Start with distributed systems fundamentals...",
      "priority_rationale": "High priority as core skill",
      "estimated_effort": "4-6 months",
      "key_actions": ["Complete course", "Practice designs"],
      "potential_blockers": ["Time constraints"]
    },
    {
      "skill_id": "skill-002",
      "skill_name": "Cloud Architecture",
      "gap_size": 1,
      "success": true,
      "ai_analysis": "A 1-level gap is manageable...",
      "ai_recommendation": "Focus on advanced cloud patterns...",
      "priority_rationale": "Medium priority",
      "estimated_effort": "2-3 months",
      "key_actions": ["AWS certification", "Hands-on projects"],
      "potential_blockers": ["Access to cloud resources"]
    }
  ],
  "overall_summary": "Analysis complete for 2 skill gaps. 1 critical gap (System Design) identified requiring immediate attention.",
  "priority_order": ["System Design", "Cloud Architecture"],
  "recommended_focus_areas": ["System Design"]
}
```

---

## 5. Learning Path Endpoints

### POST /generate-learning-path
Generate an AI-powered learning path.

**Request:**
```json
{
  "employee_name": "Nguyen Van A",
  "skill_id": "30000000-0000-0000-0000-000000000001",
  "skill_name": "System Design",
  "skill_code": "SYSDES",
  "current_level": 2,
  "target_level": 4,
  "skill_description": "Designing scalable system architectures",
  "available_resources": [
    {
      "id": "res-001",
      "title": "System Design Interview Course",
      "type": "Course",
      "description": "Comprehensive course on system design",
      "estimated_hours": 40,
      "difficulty": "Medium",
      "from_level": 2,
      "to_level": 4
    },
    {
      "id": "res-002",
      "title": "Designing Data-Intensive Applications",
      "type": "Book",
      "description": "Essential book for distributed systems",
      "estimated_hours": 30,
      "difficulty": "Hard",
      "from_level": 3,
      "to_level": 5
    }
  ],
  "time_constraint_months": 6,
  "language": "vi"
}
```

**Request Fields:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| employee_name | string | Yes | Employee name |
| skill_id | string | No | Skill UUID |
| skill_name | string | Yes | Skill name |
| skill_code | string | Yes | Skill code |
| current_level | int | Yes | 0-7 |
| target_level | int | Yes | 1-7 |
| skill_description | string | No | Description of skill |
| available_resources | LearningResourceInfo[] | No | Available resources to consider |
| time_constraint_months | int | No | 1-24 months |
| language | string | No | "en" or "vi" |

**LearningResourceInfo:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| id | string | Yes | Resource identifier |
| title | string | Yes | Resource title |
| type | string | Yes | Course, Book, Video, Project, etc. |
| description | string | No | Resource description |
| estimated_hours | int | No | Estimated hours to complete |
| difficulty | string | No | Easy, Medium, Hard |
| from_level | int | No | Appropriate starting level |
| to_level | int | No | Expected level after completion |

**Response:**
```json
{
  "success": true,
  "path_title": "Lộ trình phát triển System Design từ Level 2 đến Level 4",
  "path_description": "Lộ trình học tập toàn diện giúp bạn nâng cao kỹ năng System Design trong 6 tháng, từ mức Assist lên mức Enable với khả năng thiết kế hệ thống độc lập.",
  "estimated_total_hours": 120,
  "estimated_duration_weeks": 24,
  "learning_items": [
    {
      "order": 1,
      "title": "Fundamentals of Distributed Systems",
      "description": "Nắm vững các khái niệm cơ bản về hệ thống phân tán: CAP theorem, consistency models, partitioning strategies.",
      "item_type": "Course",
      "estimated_hours": 20,
      "target_level_after": 3,
      "success_criteria": "Có thể giải thích CAP theorem và áp dụng vào việc chọn database phù hợp",
      "resource_id": null
    },
    {
      "order": 2,
      "title": "System Design Interview Course",
      "description": "Học cách thiết kế các hệ thống phổ biến: URL shortener, chat system, social media feed.",
      "item_type": "Course",
      "estimated_hours": 40,
      "target_level_after": 3,
      "success_criteria": "Hoàn thành khóa học và thiết kế được 5 systems",
      "resource_id": "res-001"
    },
    {
      "order": 3,
      "title": "Đọc sách Designing Data-Intensive Applications",
      "description": "Đi sâu vào các pattern và best practices cho data systems.",
      "item_type": "Book",
      "estimated_hours": 30,
      "target_level_after": 4,
      "success_criteria": "Tóm tắt được key concepts từ mỗi chapter",
      "resource_id": "res-002"
    },
    {
      "order": 4,
      "title": "Hands-on System Design Project",
      "description": "Thiết kế và implement một microservices system cho e-commerce platform.",
      "item_type": "Project",
      "estimated_hours": 30,
      "target_level_after": 4,
      "success_criteria": "Hoàn thành design document và working prototype",
      "resource_id": null
    }
  ],
  "milestones": [
    {
      "after_item": 2,
      "description": "Checkpoint: Có thể độc lập thiết kế medium-complexity systems",
      "expected_level": 3
    },
    {
      "after_item": 4,
      "description": "Final: Đạt Level 4 - có thể lead system design và mentor người khác",
      "expected_level": 4
    }
  ],
  "ai_rationale": "Lộ trình này được thiết kế theo nguyên tắc progressive learning - bắt đầu từ fundamentals, sau đó áp dụng qua structured course, đi sâu với reading material, và cuối cùng consolidate kiến thức qua hands-on project. Thời gian 6 tháng là hợp lý cho gap 2 levels với workload ~5 giờ/tuần.",
  "key_success_factors": [
    "Dành ít nhất 5 giờ/tuần cho học tập",
    "Áp dụng ngay kiến thức vào công việc hàng ngày",
    "Tìm mentor để review designs",
    "Tham gia tech discussions trong team"
  ],
  "potential_challenges": [
    "Cân bằng giữa learning và delivery pressure",
    "Thiếu cơ hội thực hành với large-scale systems",
    "Cần access vào cloud resources cho hands-on practice"
  ]
}
```

### POST /rank-resources
Rank learning resources by relevance for a skill gap.

**Request:**
```json
{
  "skill_name": "System Design",
  "skill_code": "SYSDES",
  "current_level": 2,
  "target_level": 4,
  "resources": [
    {
      "id": "res-001",
      "title": "System Design Interview Course",
      "type": "Course",
      "description": "Comprehensive course covering major system design topics",
      "estimated_hours": 40,
      "difficulty": "Medium",
      "from_level": 2,
      "to_level": 4
    },
    {
      "id": "res-002",
      "title": "Introduction to Programming",
      "type": "Course",
      "description": "Basic programming concepts",
      "estimated_hours": 20,
      "difficulty": "Easy",
      "from_level": 0,
      "to_level": 1
    },
    {
      "id": "res-003",
      "title": "Designing Data-Intensive Applications",
      "type": "Book",
      "description": "Deep dive into data systems design",
      "estimated_hours": 30,
      "difficulty": "Hard",
      "from_level": 3,
      "to_level": 5
    }
  ],
  "language": "en"
}
```

**Response:**
```json
{
  "success": true,
  "ranked_resources": [
    {
      "resource_id": "res-001",
      "rank": 1,
      "relevance_score": 95,
      "reason": "Directly targets the skill gap with appropriate level range (2-4). Comprehensive coverage of system design topics."
    },
    {
      "resource_id": "res-003",
      "rank": 2,
      "relevance_score": 75,
      "reason": "Excellent for deepening knowledge but starts at level 3. Best used after initial course completion."
    },
    {
      "resource_id": "res-002",
      "rank": 3,
      "relevance_score": 10,
      "reason": "Not relevant - covers basic programming, not system design. Level range (0-1) is below current level."
    }
  ],
  "top_recommendations": ["res-001", "res-003"],
  "coverage_assessment": "The available resources provide good coverage for the 2-4 level transition. The System Design Interview Course is ideal as primary resource, with the DDIA book for advanced topics.",
  "gaps_in_resources": [
    "Missing hands-on project or lab component",
    "No mentorship or coaching resource available"
  ]
}
```

---

## Error Responses

All endpoints return consistent error format:

**400 Bad Request:**
```json
{
  "detail": "Invalid request: question_type must not be empty"
}
```

**404 Not Found:**
```json
{
  "detail": "No levels found for skill: invalid-skill-id"
}
```

**422 Unprocessable Entity:**
```json
{
  "detail": "Invalid request: number_of_questions must be between 1 and 100"
}
```

**500 Internal Server Error:**
```json
{
  "detail": "Failed to generate questions: Azure OpenAI service unavailable"
}
```

---

## SFIA Proficiency Levels Reference

| Level | Name | Description |
|-------|------|-------------|
| 0 | None | No proficiency |
| 1 | Follow | Follows instructions, learning |
| 2 | Assist | Assists others, developing skills |
| 3 | Apply | Applies skills independently |
| 4 | Enable | Enables others, ensures quality |
| 5 | Ensure/Advise | Ensures/advises at organizational level |
| 6 | Initiate | Initiates, influences strategic direction |
| 7 | Set Strategy | Sets strategy, leads industry |

---

## Question Types Reference

| Type | Description | Has Options |
|------|-------------|-------------|
| MultipleChoice | Single correct answer | Yes |
| MultipleAnswer | Multiple correct answers | Yes |
| TrueFalse | True or False | Yes (2 options) |
| ShortAnswer | Brief text answer | No |
| LongAnswer | Detailed text answer | No |
| CodingChallenge | Code implementation | No |

---

## Learning Resource Types Reference

| Type | Description |
|------|-------------|
| Course | Online or in-person course |
| Book | Reading material |
| Video | Video tutorials/lectures |
| Article | Blog posts, documentation |
| Workshop | Hands-on workshop |
| Certification | Professional certification |
| Project | Hands-on project work |
| Mentorship | 1-on-1 mentoring |
| Seminar | Internal knowledge sharing |
