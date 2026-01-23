import json
from pathlib import Path
from jsonschema import validate, ValidationError, Draft202012Validator

INPUT_SCHEMA_PATH = Path(__file__).parent.parent / "schemas" / "input_skill_schema.json"

with open(INPUT_SCHEMA_PATH, "r", encoding="utf-8") as f:
    INPUT_SCHEMA = json.load(f)

validator = Draft202012Validator(INPUT_SCHEMA)

def validate_input_skill(data: dict) -> None:
    try:
        validator.validate(data)
    except ValidationError as e:
        raise ValueError(f"Input validation failed: {e.message}") from e