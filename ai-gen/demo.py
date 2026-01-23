#!/usr/bin/env python3
"""
Demo script showing how to use the API to generate questions from database skills
"""

import sys
import os
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from src.custom import getSkillData, getAllSkillsList
from src.generators.question_generator import generate_questions
import json

def demo_skill_question_generation():
    """Demo: Get skill from database and generate questions."""

    print("🎯 AI Question Generator - Database Integration Demo")
    print("=" * 60)

    # Step 1: Get available skills
    print("\n1. Getting available skills from database...")
    skills = getAllSkillsList()
    print(f"   Found {len(skills)} skills in database")

    # Step 2: Get skill data for question generation
    print("\n2. Getting skill data for question generation...")
    skill_data = getSkillData()  # Get first skill
    if skill_data:
        print(f"   Selected skill: {skill_data['skill_name']}")
        print(f"   Skill has {len(skill_data['levels'])} proficiency levels")
        print(f"   Levels: {[level['level'] for level in skill_data['levels']]}")
    else:
        print("   No skill data found!")
        return

    # Step 3: Generate questions using AI
    print("\n3. Generating questions using Google Gemini AI...")
    try:
        questions = generate_questions(
            skill_json=skill_data,
            num_questions=5,  # Generate 5 questions for demo
            language="en",
            min_per_level=1
        )

        print(f"   ✓ Generated {len(questions)} questions")

        # Step 4: Display results
        print("\n4. Generated Questions:")
        print("-" * 40)

        for i, question in enumerate(questions, 1):
            print(f"\nQuestion {i}:")
            print(f"   Text: {question.get('question_text', 'N/A')}")
            print(f"   Type: {question.get('question_type', 'N/A')}")
            print(f"   Level: {question.get('proficiency_level', 'N/A')}")
            print(f"   Category: {question.get('category', 'N/A')}")

            if i >= 3:  # Show only first 3 questions
                print(f"\n   ... and {len(questions) - 3} more questions")
                break

        print("\n" + "=" * 60)
        print("✅ Demo completed successfully!")
        print("💡 The skill data came directly from your PostgreSQL database")
        print("💡 Questions were generated using Google Gemini AI")
        print("🚀 API is ready for production use!")

    except Exception as e:
        print(f"   ✗ Error generating questions: {e}")

def demo_api_workflow():
    """Demo the complete API workflow."""
    print("\n🔄 API Workflow Demo:")
    print("-" * 30)
    print("1. GET /skills → Get available skills")
    print("2. POST /get-skill-data → Get formatted skill data")
    print("3. POST /generate-questions → Generate questions")
    print("4. POST /get-keywords-table → Get document keywords")
    print("\n📖 Check README.md for detailed API documentation")

if __name__ == "__main__":
    demo_skill_question_generation()
    demo_api_workflow()