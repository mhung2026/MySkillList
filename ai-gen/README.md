# AI Question Generator

A FastAPI service for generating assessment questions based on skill definitions using Azure OpenAI.

## Setup

1. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```

2. Configure environment variables in `.env`:
   - `OPENAI_API_KEY`: Your Azure OpenAI API key
   - `OPENAI_BASE_URL`: Azure OpenAI endpoint URL
   - `LLM_MODEL`: Model name (default: gpt-4o)
   - `DB_CONNECT_STRING`: PostgreSQL connection string
   - `DEBUG`: Set to True for development

3. Run the server:
   ```bash
   python main.py
   ```

## API Endpoints (V2)

### Question Generation
- `POST /api/v2/generate-questions`: Generate questions for a skill.

  Request body:
  ```json
  {
    "question_type": ["MultipleChoice", "SituationalJudgment"],
    "language": "en",
    "number_of_questions": 5,
    "skills": [
      {
        "skill_id": "30000000-0000-0000-0000-000000000001",
        "skill_name": "Strategic planning",
        "skill_code": "ITSP"
      }
    ],
    "difficulty": "Medium"
  }
  ```

### Answer Grading
- `POST /api/v2/grade-answer`: Grade a student's answer.

  Request body:
  ```json
  {
    "question_content": "Explain the benefits of...",
    "student_answer": "The main benefits are...",
    "max_points": 10,
    "grading_rubric": "Clear explanation of concepts..."
  }
  ```

### Health Check
- `GET /health`: Check API health status
- `GET /test`: Simple test endpoint

## Database Schema

The API reads from PostgreSQL database with tables:
- `public.Skills`: Skill definitions
- `public.SkillLevelDefinitions`: Detailed level definitions for each skill

Skill data includes: description, autonomy, influence, complexity, business_skills, knowledge, behavioral_indicators, evidence_examples.

## Testing

Run tests:
```bash
pytest
```

## Structure

- `src/`: Source code
  - `schemas/`: JSON schemas for validation
  - `validators/`: Input/output validation
  - `generators/`: Question generation logic (question_generator_v2.py, answer_grader.py)
  - `api/`: FastAPI routes (routes_v2.py)
- `tests/`: Unit tests
- `config/`: Configuration settings
