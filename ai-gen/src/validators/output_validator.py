import json
from pathlib import Path
from jsonschema import validate, ValidationError, Draft202012Validator

OUTPUT_SCHEMA_PATH = Path(__file__).parent.parent / "schemas" / "output_question_schema.json"

with open(OUTPUT_SCHEMA_PATH, "r", encoding="utf-8") as f:
    OUTPUT_SCHEMA = json.load(f)

validator = Draft202012Validator(OUTPUT_SCHEMA)

def validate_output_questions(data: list) -> None:
    try:
        for item in data:
            validator.validate(item)
    except ValidationError as e:
        raise ValueError(f"Output validation failed: {e.message}") from e