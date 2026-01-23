"""
Request validator for question generation API.
Validates incoming requests against input_request_schema.json
"""

import json
import os
from jsonschema import validate, ValidationError
from typing import Dict, Any, Optional, List


# Load schema
SCHEMA_PATH = os.path.join(
    os.path.dirname(os.path.dirname(__file__)),
    "schemas",
    "input_request_schema.json"
)

with open(SCHEMA_PATH, 'r', encoding='utf-8') as f:
    REQUEST_SCHEMA = json.load(f)


class RequestValidator:
    """Validator for question generation requests."""

    # Mapping between frontend values and database enum values
    QUESTION_TYPE_MAPPING = {
        "Multiple Choice": "MultipleChoice",
        "Multiple Answer": "MultipleAnswer",
        "True/False": "TrueFalse",
        "Short Answer": "ShortAnswer",
        "Long Answer": "LongAnswer",
        "Coding Challenge": "CodingChallenge",
        "Scenario": "Scenario",
        "Situational Judgment": "SituationalJudgment",
        "Rating": "Rating"
    }

    LANGUAGE_MAPPING = {
        "English": "en",
        "Vietnamese": "vi"
    }

    DIFFICULTY_MAPPING = {
        "Easy": "easy",
        "Medium": "medium",
        "Hard": "hard"
    }

    # SFIA Level names for reference
    SFIA_LEVELS = {
        1: "Follow",
        2: "Assist",
        3: "Apply",
        4: "Enable",
        5: "Ensure/Advise",
        6: "Initiate",
        7: "Set Strategy"
    }

    @staticmethod
    def validate_request(request_data: Dict[str, Any]) -> tuple[bool, Optional[str]]:
        """
        Validate request against schema.

        Args:
            request_data: Request data to validate

        Returns:
            tuple: (is_valid, error_message)
        """
        try:
            validate(instance=request_data, schema=REQUEST_SCHEMA)
            return True, None
        except ValidationError as e:
            return False, str(e.message)

    @staticmethod
    def normalize_request(request_data: Dict[str, Any]) -> Dict[str, Any]:
        """
        Normalize request data to internal format.
        Maps user-friendly values to database enum values.

        Args:
            request_data: Raw request data from frontend

        Returns:
            Normalized request data
        """
        normalized = request_data.copy()

        # Normalize question types
        if "question_type" in normalized and normalized["question_type"]:
            normalized["question_type"] = [
                RequestValidator.QUESTION_TYPE_MAPPING.get(qt, qt)
                for qt in normalized["question_type"]
            ]

        # Normalize language
        if "language" in normalized:
            normalized["language"] = RequestValidator.LANGUAGE_MAPPING.get(
                normalized["language"],
                normalized["language"]
            )

        # Normalize difficulty
        if "difficulty" in normalized and normalized["difficulty"]:
            normalized["difficulty"] = RequestValidator.DIFFICULTY_MAPPING.get(
                normalized["difficulty"],
                normalized["difficulty"]
            )

        return normalized

    @staticmethod
    def get_sfia_level_name(level: int) -> str:
        """Get SFIA level name from number."""
        return RequestValidator.SFIA_LEVELS.get(level, f"Level {level}")

    @staticmethod
    def create_sample_request(
        question_types: List[str] = None,
        language: str = "English",
        num_questions: int = 10,
        skill_id: str = None,
        skill_name: str = None,
        levels: List[int] = None,
        difficulty: str = None,
        context: str = None
    ) -> Dict[str, Any]:
        """
        Create a sample request for testing.

        Args:
            question_types: List of question types
            language: Language (English/Vietnamese)
            num_questions: Number of questions
            skill_id: Skill UUID
            skill_name: Skill name
            levels: Target proficiency levels
            difficulty: Difficulty level
            context: Additional context

        Returns:
            Sample request dict
        """
        if question_types is None:
            question_types = ["Multiple Choice"]

        request = {
            "question_type": question_types,
            "language": language,
            "number_of_questions": num_questions
        }

        # Add optional skill
        if skill_id and skill_name:
            request["skills"] = [{
                "skill_id": skill_id,
                "skill_name": skill_name
            }]
        else:
            request["skills"] = None

        # Add optional levels
        request["target_proficiency_level"] = levels

        # Add optional difficulty
        request["difficulty"] = difficulty

        # Add optional context
        request["additional_context"] = context

        return request


def validate_and_normalize(request_data: Dict[str, Any]) -> tuple[bool, Optional[str], Optional[Dict[str, Any]]]:
    """
    Validate and normalize request in one step.

    Args:
        request_data: Raw request data

    Returns:
        tuple: (is_valid, error_message, normalized_data)
    """
    validator = RequestValidator()

    # Validate
    is_valid, error = validator.validate_request(request_data)
    if not is_valid:
        return False, error, None

    # Normalize
    normalized = validator.normalize_request(request_data)

    return True, None, normalized


# Example usage and testing
if __name__ == "__main__":
    import sys
    import io

    # Set UTF-8 encoding
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

    print("=" * 80)
    print("Request Validator Testing")
    print("=" * 80)

    validator = RequestValidator()

    # Test 1: Valid request with all fields
    print("\n1. Testing valid request with all fields...")
    valid_request = {
        "question_type": ["Multiple Choice", "Short Answer"],
        "language": "English",
        "number_of_questions": 10,
        "skills": [{
            "skill_id": "30000000-0000-0000-0000-000000000078",
            "skill_name": "Accessibility and inclusion",
            "skill_code": "ACIN"
        }],
        "target_proficiency_level": [3, 4],
        "difficulty": "Medium",
        "additional_context": "Focus on WCAG standards"
    }

    is_valid, error = validator.validate_request(valid_request)
    print(f"   Valid: {is_valid}")
    if error:
        print(f"   Error: {error}")

    normalized = validator.normalize_request(valid_request)
    print(f"   Normalized question types: {normalized['question_type']}")
    print(f"   Normalized language: {normalized['language']}")
    print(f"   Normalized difficulty: {normalized['difficulty']}")

    # Test 2: Minimal valid request (random skills/levels/difficulty)
    print("\n2. Testing minimal valid request (with random values)...")
    minimal_request = {
        "question_type": ["Multiple Choice"],
        "language": "Vietnamese",
        "number_of_questions": 5,
        "skills": None,
        "target_proficiency_level": None,
        "difficulty": None,
        "additional_context": None
    }

    is_valid, error = validator.validate_request(minimal_request)
    print(f"   Valid: {is_valid}")
    if error:
        print(f"   Error: {error}")

    # Test 3: Invalid request (missing required field)
    print("\n3. Testing invalid request (missing number_of_questions)...")
    invalid_request = {
        "question_type": ["Multiple Choice"],
        "language": "English"
    }

    is_valid, error = validator.validate_request(invalid_request)
    print(f"   Valid: {is_valid}")
    if error:
        print(f"   Error: {error}")

    # Test 4: Invalid request (invalid question type)
    print("\n4. Testing invalid request (invalid question type)...")
    invalid_request2 = {
        "question_type": ["Invalid Type"],
        "language": "English",
        "number_of_questions": 10
    }

    is_valid, error = validator.validate_request(invalid_request2)
    print(f"   Valid: {is_valid}")
    if error:
        print(f"   Error: {error}")

    # Test 5: Create sample request
    print("\n5. Creating sample request using helper function...")
    sample = validator.create_sample_request(
        question_types=["Coding Challenge", "Short Answer"],
        language="Vietnamese",
        num_questions=15,
        skill_id="30000000-0000-0000-0000-000000000001",
        skill_name="Programming",
        levels=[2, 3, 4],
        difficulty="Hard",
        context="Focus on algorithms"
    )

    is_valid, error = validator.validate_request(sample)
    print(f"   Sample request valid: {is_valid}")
    print(f"   Sample: {json.dumps(sample, indent=2, ensure_ascii=False)}")

    # Test 6: SFIA level names
    print("\n6. SFIA Level Names:")
    for level in range(1, 8):
        print(f"   Level {level}: {validator.get_sfia_level_name(level)}")

    print("\n" + "=" * 80)
    print("Testing completed!")
    print("=" * 80)
