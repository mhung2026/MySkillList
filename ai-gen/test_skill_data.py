#!/usr/bin/env python3
"""
Test script for skill data functions
"""

import sys
import os
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from src.custom import getSkillData, getAllSkillsList
import json

def test_skill_functions():
    """Test the skill data functions."""
    print('Available skills:')
    skills = getAllSkillsList()
    for skill in skills[:5]:  # Show first 5
        print(f'  {skill}')

    print('\nTesting getSkillData():')
    skill_data = getSkillData()
    if skill_data:
        print(f'Skill: {skill_data["skill_name"]}')
        print(f'ID: {skill_data["skill_id"]}')
        print(f'Levels: {len(skill_data["levels"])}')

        # Show first level details
        first_level = skill_data["levels"][0]
        print(f'First level ({first_level["level"]}): {first_level["description"][:100]}...')

        # Validate against schema
        from src.validators.input_validator import validate_input_skill
        try:
            validate_input_skill(skill_data)
            print('✓ Data validates against input_skill_schema.json')
        except Exception as e:
            print(f'✗ Validation failed: {e}')

        # Show sample JSON output
        print('\nSample JSON output:')
        print(json.dumps(skill_data, indent=2, ensure_ascii=False)[:500] + '...')
    else:
        print('No skill data found')

if __name__ == "__main__":
    test_skill_functions()