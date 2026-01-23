#!/usr/bin/env python3
"""
Test script for the new skill API endpoints
"""

import sys
import os
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from src.custom import getSkillData, getAllSkillsList, getKeywordsTable
import json

def test_api_functions():
    """Test the API functions that would be used by endpoints."""

    print("=== Testing Skills API Functions ===\n")

    # Test 1: Get all skills list
    print("1. Testing getAllSkillsList():")
    try:
        skills = getAllSkillsList()
        print(f"   ✓ Found {len(skills)} skills")
        print(f"   First 3 skills: {skills[:3]}")
    except Exception as e:
        print(f"   ✗ Error: {e}")

    # Test 2: Get skill data (default/first skill)
    print("\n2. Testing getSkillData() - default skill:")
    try:
        skill_data = getSkillData()
        if skill_data:
            print(f"   ✓ Retrieved skill: {skill_data['skill_name']}")
            print(f"   Skill ID: {skill_data['skill_id']}")
            print(f"   Number of levels: {len(skill_data['levels'])}")
            print(f"   Levels: {[level['level'] for level in skill_data['levels']]}")

            # Validate the data structure
            required_fields = ['skill_name', 'skill_id', 'levels']
            if all(field in skill_data for field in required_fields):
                print("   ✓ Data structure is valid")
            else:
                print("   ✗ Missing required fields")

            # Check levels structure
            if skill_data['levels']:
                level = skill_data['levels'][0]
                level_fields = ['level', 'description', 'autonomy', 'influence', 'complexity', 'business_skills', 'knowledge']
                if all(field in level for field in level_fields):
                    print("   ✓ Level structure is valid")
                else:
                    print("   ✗ Level missing required fields")
        else:
            print("   ✗ No skill data returned")
    except Exception as e:
        print(f"   ✗ Error: {e}")

    # Test 3: Get specific skill data
    print("\n3. Testing getSkillData() - specific skill:")
    try:
        # Use the first skill ID from the list
        skills = getAllSkillsList()
        if skills:
            specific_skill_id = skills[0]['id']
            print(f"   Testing with skill ID: {specific_skill_id}")

            skill_data = getSkillData(specific_skill_id)
            if skill_data:
                print(f"   ✓ Retrieved skill: {skill_data['skill_name']}")
                print(f"   ✓ Skill matches requested ID: {skill_data['skill_id'] == specific_skill_id}")
            else:
                print("   ✗ No skill data returned for specific ID")
        else:
            print("   ✗ No skills available to test specific ID")
    except Exception as e:
        print(f"   ✗ Error: {e}")

    # Test 4: Get keywords table
    print("\n4. Testing getKeywordsTable():")
    try:
        result = getKeywordsTable("SAMPLE_TEMPLATE")
        if result:
            print(f"   ✓ Retrieved keywords: {result[0][:100]}...")
        else:
            print("   ✗ No keywords table found (this is expected if table is empty)")
    except Exception as e:
        print(f"   ✗ Error: {e}")

    print("\n=== API Functions Test Complete ===")

if __name__ == "__main__":
    test_api_functions()