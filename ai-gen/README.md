# AI Question Generator

A FastAPI service for generating assessment questions based on skill definitions using AI/LLM.

## Setup

1. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```

2. Configure environment variables in `.env`:
   - `GEMINI_API_KEY`: Your Google Gemini API key
   - `DEBUG`: Set to True for development

3. Configure database connection in `src/custom.py`:
   - Update `DB_CONFIG` with your PostgreSQL connection details

4. Run the server:
   ```bash
   python main.py
   ```

## API Endpoints

### Question Generation
- `POST /generate-questions`: Generate questions for a skill.

  Request body:
  ```json
  {
    "skill_data": {
      "skill_name": "Python Programming",
      "levels": [
        {
          "level": 1,
          "description": "Basic Python syntax and concepts",
          "autonomy": "Follows instructions",
          "knowledge": "Basic programming concepts"
        }
      ]
    },
    "num_questions": 10,
    "language": "en"
  }
  ```

### Database Integration

- `GET /skills`: Get list of all available skills from database
  ```json
  {
    "skills": [
      {"id": "uuid", "name": "Skill Name", "code": "CODE"}
    ],
    "total": 146
  }
  ```

- `POST /get-skill-data`: Get skill data formatted for question generation
  ```json
  // Request (optional - returns first skill if not specified)
  {
    "skill_id": "30000000-0000-0000-0000-000000000001"
  }

  // Response
  {
    "skill_name": "Strategic planning",
    "skill_id": "30000000-0000-0000-0000-000000000001",
    "levels": [
      {
        "level": 4,
        "description": "Level description...",
        "autonomy": "Autonomy description...",
        "influence": "Influence description...",
        "complexity": "Complexity description...",
        "business_skills": "Business skills description...",
        "knowledge": "Knowledge description...",
        "behavioral_indicators": ["indicator1", "indicator2"],
        "evidence_examples": ["example1", "example2"]
      }
    ]
  }
  ```

### Health Check
- `GET /health`: Check API health status
- `GET /test`: Simple test endpoint

## Database Schema

The API reads from PostgreSQL database with tables:
- `public.Skills`: Skill definitions
- `public.SkillLevelDefinitions`: Detailed level definitions for each skill

Skill data is automatically formatted to match `input_skill_schema.json` for question generation.
      "name": "Skill Name",
      "description": "Description",
      "category": "Technical",
      "skill_type": "Core"
    }
  }
  ```

  Response: Array of question objects.

## Testing

Run tests:
```bash
pytest
```

## Structure

- `src/`: Source code
  - `schemas/`: JSON schemas for validation
  - `validators/`: Input/output validation
  - `generators/`: Question generation logic
  - `api/`: FastAPI routes
- `tests/`: Unit tests
- `config/`: Configuration settings