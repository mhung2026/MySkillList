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
Get proficiency levels for a specific skill. Returns **only** levels listed in the skill's `ApplicableLevels` column (e.g., ITSP returns L4-L7 only, not L1-L7).

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

**Level targeting behavior:**

Levels are filtered by the skill's `ApplicableLevels` column in DB (e.g., ITSP only has L4-L7). Questions are generated only within the skill's applicable range.

| Scenario | Behavior |
|----------|----------|
| `target_proficiency_level` provided, enough questions | Gen from skill's min level up to target. E.g., ITSP target=6 → L4,L5,L6 |
| `target_proficiency_level` provided, fewer questions than levels | Trim to N levels closest to target. E.g., ITSP target=6, 2 questions → L5,L6 |
| No `target_proficiency_level`, enough questions | Gen across all available levels. E.g., ITSP → L4,L5,L6,L7 |
| No `target_proficiency_level`, fewer questions than levels | Trim to N highest levels. E.g., ITSP, 2 questions → L6,L7 |

**Response:**
```json
{
  "questions": [
    {
      "skill_id": "30000000-0000-0000-0000-000000000001",
      "type": "MultipleChoice",
      "content": "You are responsible for...",
      "code_snippet": null,
      "target_level": 4,
      "difficulty": "Medium",
      "points": 15,
      "time_limit_seconds": 600,
      "tags": ["strategic_planning", "level_4"],
      "options": [
        {
          "content": "Draft an agenda and share it with the team",
          "is_correct": true,
          "display_order": 1,
          "explanation": "Level 4 involves working independently...",
          "effectiveness_level": null
        }
      ],
      "grading_rubric": null,
      "explanation": "The correct option reflects Level 4...",
      "hints": ["Consider the level of autonomy expected."]
    }
  ],
  "metadata": {
    "total_questions": 6,
    "requested_questions": 6,
    "generation_attempts": 1,
    "generation_timestamp": "2026-01-27T21:30:00.000000",
    "ai_model": "gpt-4o",
    "skill_id": "30000000-0000-0000-0000-000000000001",
    "skill_name": "Strategic planning",
    "language": "en",
    "target_proficiency_level": [6],
    "available_levels": [4, 5, 6, 7],
    "min_defined_level": 4
  }
}
```

---

## 3. Answer Grading Endpoint

### POST /grade-answer
Grade a submitted answer using AI.

**Request:**
```json
{
  "question_id": "q_001",
  "question_content": "Explain the difference between a list and a tuple in Python.",
  "submitted_answer": "A list is mutable, meaning you can change its contents. A tuple is immutable. Lists use [], tuples use ().",
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
| submitted_answer | string | Yes | Candidate's submitted answer |
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
  "detailed_analysis": "The candidate demonstrated understanding of the fundamental difference..."
}
```

---

## 4. Skill Gap Analysis Endpoints

### POST /analyze-gap
Analyze a single skill gap using AI.

**Request:**
```json
{
  "employee_id": "f3b4f0fb-710f-4674-adaf-93b756b8faaf",
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
| employee_id | string | Yes | Employee UUID (looked up from DB to resolve name and job role) |
| job_role | string | No | Job role name (auto-resolved from employee's DB record if omitted) |
| skill_id | string | No | Skill UUID |
| skill_name | string | Yes | Skill name |
| skill_code | string | Yes | Skill code |
| current_level | int | Yes | 0-7 (SFIA level) |
| required_level | int | Yes | 1-7 (SFIA level) |
| skill_description | string | No | Description of the skill |
| current_level_description | string | No | Description of current level |
| required_level_description | string | No | Description of required level |
| language | string | No | "en" or "vi" (default: "en") |

**Error:** Returns 404 if `employee_id` not found in DB.

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
  "employee_id": "f3b4f0fb-710f-4674-adaf-93b756b8faaf",
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

**Note:** `employee_id` replaces `employee_name`. Name and job role are resolved from DB. Returns 404 if employee not found.
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

**Coursera integration:** When `skill_id` is provided, the endpoint fetches real Coursera courses directly from the `CourseraCourse` table (matched by `SkillId`, up to 15 courses, sorted by rating). Courses are **filtered by difficulty level** based on the learning range:

| Course `Level` column | Included when |
|---|---|
| `Beginner level` | `current_level <= 2` |
| `Intermediate level` | `current_level <= 4 AND target_level >= 3` |
| Other (not N/A) | `target_level > 4` |
| `N/A` or empty | Always included |

The `learning_items` array contains **only** real Coursera courses from DB. AI generates path metadata (title, description, milestones, rationale) but does not generate learning items.

**Request:**
```json
{
  "employee_id": "f3b4f0fb-710f-4674-adaf-93b756b8faaf",
  "skill_id": "30000000-0000-0000-0000-000000000078",
  "skill_name": "Accessibility and inclusion",
  "skill_code": "ACIN",
  "current_level": 1,
  "target_level": 3,
  "skill_description": "Designing accessible and inclusive digital products",
  "available_resources": null,
  "time_constraint_months": 6,
  "language": "en"
}
```

**Request Fields:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| employee_id | string | Yes | Employee UUID (looked up from DB to resolve name). Returns 404 if not found. |
| skill_id | string | No | Skill UUID (used to fetch Coursera courses from DB) |
| skill_name | string | Yes | Skill name |
| skill_code | string | Yes | SFIA skill code |
| current_level | int | Yes | 0-7 |
| target_level | int | Yes | 1-7 |
| skill_description | string | No | Description of skill |
| available_resources | LearningResourceInfo[] | No | Additional resources |
| time_constraint_months | int | No | 1-24 months |
| language | string | No | "en" or "vi" |

**Response:**
```json
{
  "success": true,
  "path_title": "Accessibility and Inclusion Skill Development Path",
  "path_description": "A structured learning journey to advance accessibility and inclusion skills.",
  "estimated_total_hours": 70,
  "estimated_duration_weeks": 24,
  "learning_items": [
    {
      "order": 1,
      "title": "An Introduction to Accessibility and Inclusive Design",
      "description": "This course introduces fundamental principles of accessibility...",
      "item_type": "Course",
      "source": "Coursera",
      "estimated_hours": 10,
      "target_level_after": 0,
      "success_criteria": "",
      "resource_id": "1",
      "url": "https://www.coursera.org/learn/accessibility",
      "organization": "N/A",
      "difficulty": "Beginner level",
      "rating": null,
      "reviews_count": null,
      "certificate_available": null
    },
    {
      "order": 2,
      "title": "Defining Diversity, Equity and Inclusion in Organizations",
      "description": "Introduces core definitions of diversity, equity, and inclusion...",
      "item_type": "Course",
      "source": "Coursera",
      "estimated_hours": 6,
      "target_level_after": 0,
      "success_criteria": "",
      "resource_id": "2",
      "url": "https://www.coursera.org/learn/defining-diversity-equity-and-inclusion-in-organizations",
      "organization": "N/A",
      "difficulty": "Intermediate level",
      "rating": null,
      "reviews_count": null,
      "certificate_available": null
    },
    {
      "order": 3,
      "title": "Making Accessible Designs",
      "description": "Learn how to create inclusive, user-friendly designs...",
      "item_type": "Course",
      "source": "Coursera",
      "estimated_hours": 2,
      "target_level_after": 0,
      "success_criteria": "",
      "resource_id": "3",
      "url": "https://www.coursera.org/learn/making-accessible-designs",
      "organization": "N/A",
      "difficulty": "Beginner level",
      "rating": null,
      "reviews_count": null,
      "certificate_available": null
    }
  ],
  "milestones": [
    {
      "after_item": 1,
      "description": "Foundational understanding of accessibility principles.",
      "expected_level": 2
    },
    {
      "after_item": 3,
      "description": "Able to apply accessibility principles independently.",
      "expected_level": 3
    }
  ],
  "ai_rationale": "This path builds from foundational to applied accessibility skills.",
  "key_success_factors": [
    "Complete all courses in order",
    "Apply concepts in real projects"
  ],
  "potential_challenges": [
    "Limited hands-on practice opportunities",
    "Balancing learning with work responsibilities"
  ]
}
```

**Notes:**
- `learning_items` contains **only** real Coursera courses fetched from DB by `skill_id`
- `resource_id` is the DB primary key of the `CourseraCourse` record
- AI generates `path_title`, `path_description`, `milestones`, `ai_rationale`, `key_success_factors`, `potential_challenges`
- If no Coursera courses match the skill/level filter, `learning_items` will be empty

### POST /generate-learning-paths
Generate AI-powered learning paths for **multiple skills** in a **single AI call**.

Same field structure as single endpoint, but wraps results in `learning_paths[]` array with cross-skill analysis.

**Request:**
```json
{
  "employee_id": "f3b4f0fb-710f-4674-adaf-93b756b8faaf",
  "skills": [
    {
      "skill_id": "30000000-0000-0000-0000-000000000078",
      "skill_name": "Accessibility and inclusion",
      "skill_code": "ACIN",
      "current_level": 1,
      "target_level": 3,
      "skill_description": "Designing accessible and inclusive digital products"
    },
    {
      "skill_id": "30000000-0000-0000-0000-000000000001",
      "skill_name": "Strategic planning",
      "skill_code": "ITSP",
      "current_level": 4,
      "target_level": 6
    }
  ],
  "time_constraint_months": 6,
  "language": "en"
}
```

**Request Fields:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| employee_id | string | Yes | Employee UUID (resolved from DB). Returns 404 if not found. |
| skills | LearningPathSkillInfo[] | Yes | Skills to generate paths for (min 1) |
| skills[].skill_id | string | No | Skill UUID (used to fetch Coursera courses) |
| skills[].skill_name | string | Yes | Skill name |
| skills[].skill_code | string | Yes | SFIA skill code |
| skills[].current_level | int | Yes | 0-7 |
| skills[].target_level | int | Yes | 1-7 |
| skills[].skill_description | string | No | Description of skill |
| time_constraint_months | int | No | 1-24 months |
| language | string | No | "en" or "vi" |

**Response:**
```json
{
  "success": true,
  "learning_paths": [
    {
      "skill_id": "30000000-0000-0000-0000-000000000078",
      "skill_name": "Accessibility and inclusion",
      "skill_code": "ACIN",
      "path_title": "Accessibility and Inclusion Development Path",
      "path_description": "A structured journey to advance accessibility skills.",
      "estimated_total_hours": 70,
      "estimated_duration_weeks": 24,
      "learning_items": [
        {
          "order": 1,
          "title": "An Introduction to Accessibility",
          "item_type": "Course",
          "source": "Coursera",
          "url": "https://www.coursera.org/learn/accessibility",
          "..."
        }
      ],
      "milestones": [
        { "after_item": 1, "description": "Foundational understanding", "expected_level": 2 }
      ],
      "ai_rationale": "This path builds from foundational to applied skills.",
      "key_success_factors": ["Complete courses in order", "Apply in real projects"],
      "potential_challenges": ["Limited practice opportunities"]
    },
    {
      "skill_id": "30000000-0000-0000-0000-000000000001",
      "skill_name": "Strategic planning",
      "skill_code": "ITSP",
      "path_title": "Strategic Planning Advanced Path",
      "path_description": "...",
      "..."
    }
  ],
  "overall_summary": "Focus on Accessibility first as it has a larger gap. Strategic planning can be developed in parallel through leadership activities.",
  "recommended_learning_order": ["ACIN", "ITSP"]
}
```

**Notes:**
- **1 AI call** for all skills (not N separate calls)
- Each entry in `learning_paths[]` has **identical fields** to the single `generate-learning-path` response
- `learning_items` contains **only** real Coursera courses from DB per skill
- `overall_summary`: cross-skill analysis of priorities and synergies
- `recommended_learning_order`: skill codes sorted by learning priority

---

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

## 7. Assessment Evaluation Endpoints

### POST /evaluate-assessment
Evaluate an assessment and determine CurrentLevel using SFIA bottom-up consecutive logic.

**Logic:**
- Looks up the skill's defined levels from DB (e.g., Strategic Planning only has levels [4,5,6,7])
- Groups responses by target_level
- Calculates % correct per level (70% threshold)
- CurrentLevel = highest CONSECUTIVE level where ALL levels from **min_defined_level** through L pass
- **NOT** always from L1 -- starts from the skill's lowest defined level

**Example (skill with levels [4,5,6]):**
- L4=80%, L5=75%, L6=60% → CurrentLevel = 5 (L6 failed, chain breaks)
- L4=60%, L5=90%, L6=90% → CurrentLevel = 0 (L4 failed, chain never starts)

**Example (skill with levels [1,2,3,4]):**
- L1=80%, L2=75%, L3=60%, L4=85% → CurrentLevel = 2 (same as before)

**Request:**
```json
{
  "skill_id": "30000000-0000-0000-0000-000000000001",
  "skill_name": "Strategic Planning",
  "responses": [
    {
      "question_id": "q1",
      "question_type": "MultipleChoice",
      "target_level": 4,
      "is_correct": true
    },
    {
      "question_id": "q2",
      "question_type": "MultipleChoice",
      "target_level": 4,
      "is_correct": true
    },
    {
      "question_id": "q3",
      "question_type": "MultipleChoice",
      "target_level": 5,
      "is_correct": true
    },
    {
      "question_id": "q4",
      "question_type": "MultipleChoice",
      "target_level": 5,
      "is_correct": false
    },
    {
      "question_id": "q5",
      "question_type": "SituationalJudgment",
      "target_level": 6,
      "is_correct": true
    },
    {
      "question_id": "q6",
      "question_type": "ShortAnswer",
      "target_level": 6,
      "score": 80,
      "max_score": 100
    }
  ]
}
```

**Request Schema:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| skill_id | string | Yes | Skill ID being assessed (used to look up defined levels from DB) |
| skill_name | string | No | Skill name (optional) |
| responses | array | Yes | List of assessment responses |

**Response Item Schema:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| question_id | string | Yes | Question ID |
| question_type | string | Yes | MultipleChoice, SituationalJudgment, ShortAnswer, etc. |
| target_level | int | Yes | Target SFIA level (1-7) |
| is_correct | bool | No* | For objective types (MultipleChoice, TrueFalse, etc.) |
| score | float | No* | For graded types (ShortAnswer, LongAnswer, etc.) |
| max_score | float | No | Max possible score (default: 100) |

*Either `is_correct` or `score` should be provided depending on question type.

**Response:**
```json
{
  "skill_id": "30000000-0000-0000-0000-000000000001",
  "skill_name": "Strategic Planning",
  "current_level": 4,
  "min_defined_level": 4,
  "level_results": {
    "4": {
      "total": 2,
      "correct": 2,
      "percentage": 100.0,
      "passed": true
    },
    "5": {
      "total": 2,
      "correct": 1,
      "percentage": 50.0,
      "passed": false
    },
    "6": {
      "total": 2,
      "correct": 2,
      "percentage": 100.0,
      "passed": true
    }
  },
  "consecutive_levels_passed": 1,
  "highest_level_with_responses": 6,
  "total_questions": 6,
  "overall_score_percentage": 83.3,
  "evaluation_details": {
    "method": "bottom_up_consecutive",
    "threshold": 70,
    "start_level": 4,
    "breakdown": [
      {
        "level": 4,
        "status": "passed",
        "percentage": 100.0,
        "message": "Level 4 passed (100.0% >= 70%)"
      },
      {
        "level": 5,
        "status": "failed",
        "percentage": 50.0,
        "message": "Level 5 failed (50.0% < 70%)"
      }
    ]
  }
}
```

**New Response Fields:**

| Field | Type | Description |
|-------|------|-------------|
| min_defined_level | int | Skill's lowest defined SFIA level (consecutive check starts here) |
| evaluation_details.start_level | int | Same as min_defined_level (included in details for clarity) |

**Notes:**
- The endpoint queries `SkillLevelDefinitions` by `skill_id` to determine the skill's available levels
- If no DB data found, falls back to deriving start level from the responses
- 92.5% of SFIA skills start above Level 1 (e.g., Strategic Planning starts at L4)

### POST /evaluate-assessments
Evaluate multiple skill assessments at once.

**Request:**
```json
{
  "assessments": [
    {
      "skill_id": "skill-itsp",
      "skill_name": "Strategic Planning",
      "responses": [
        {"question_id": "q1", "question_type": "MultipleChoice", "target_level": 4, "is_correct": true},
        {"question_id": "q2", "question_type": "MultipleChoice", "target_level": 5, "is_correct": true}
      ]
    },
    {
      "skill_id": "skill-prog",
      "skill_name": "Programming",
      "responses": [
        {"question_id": "q3", "question_type": "MultipleChoice", "target_level": 2, "is_correct": true},
        {"question_id": "q4", "question_type": "CodingChallenge", "target_level": 3, "score": 85, "max_score": 100}
      ]
    }
  ]
}
```

**Response:**
```json
{
  "results": [
    {
      "skill_id": "skill-itsp",
      "skill_name": "Strategic Planning",
      "current_level": 5,
      "min_defined_level": 4,
      "level_results": {...},
      "consecutive_levels_passed": 2,
      "highest_level_with_responses": 5,
      "total_questions": 2,
      "overall_score_percentage": 100.0,
      "evaluation_details": {...}
    },
    {
      "skill_id": "skill-prog",
      "skill_name": "Programming",
      "current_level": 3,
      "min_defined_level": 2,
      "level_results": {...},
      "consecutive_levels_passed": 2,
      "highest_level_with_responses": 3,
      "total_questions": 2,
      "overall_score_percentage": 100.0,
      "evaluation_details": {...}
    }
  ],
  "summary": {
    "total_skills": 2,
    "skills_evaluated": 2,
    "average_level": 4.0
  }
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
