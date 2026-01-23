import pytest
from src.generators.question_generator import generate_questions

@pytest.mark.skip(reason="Requires valid Gemini API key and network access to Google API")
def test_generate_questions():
    skill = {
        "skill_name": "Python",
        "levels": [
            {"level": 1, "description": "Basic knowledge"}
        ]
    }
    questions = generate_questions(skill, num_questions=5, language="en", min_per_level=1)
    assert len(questions) > 0
    assert questions[0]["topic"] == "Python"