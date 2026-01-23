import pytest
from src.validators.input_validator import validate_input_skill
from src.validators.output_validator import validate_output_questions

def test_validate_skill_input_valid():
    skill = {
        "skill_name": "Python",
        "levels": [
            {"level": 1, "description": "Basic knowledge"}
        ]
    }
    try:
        validate_input_skill(skill)
        assert True
    except ValueError:
        assert False

def test_validate_skill_input_invalid():
    skill = {"skill_name": "Python"}  # Missing levels
    try:
        validate_input_skill(skill)
        assert False
    except ValueError:
        assert True

def test_validate_questions_output_valid():
    questions = [
        {
            "id": "q1",
            "type": "mcq",
            "stem": "What is Python?",
            "language": "en",
            "difficulty": "easy",
            "topic": "Python",
            "choices": [
                {"id": "a", "text": "Language", "is_correct": True},
                {"id": "b", "text": "Snake", "is_correct": False}
            ]
        }
    ]
    try:
        validate_output_questions(questions)
        assert True
    except ValueError:
        assert False

def test_validate_questions_output_invalid():
    questions = [{"type": "invalid"}]  # Missing required
    try:
        validate_output_questions(questions)
        assert False
    except ValueError:
        assert True