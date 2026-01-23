#!/usr/bin/env python3
"""
Script to explore SkillLevelDefinitions table structure and data
"""

import sys
import os
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from src.custom import getConn, LOGGER

def explore_skill_level_definitions():
    """Explore the SkillLevelDefinitions table."""
    try:
        with getConn() as conn:
            with conn.cursor() as cur:
                # Get table structure
                cur.execute("""
                    SELECT column_name, data_type, is_nullable, column_default
                    FROM information_schema.columns
                    WHERE table_schema = 'public'
                    AND table_name = 'SkillLevelDefinitions'
                    ORDER BY ordinal_position;
                """)
                columns = cur.fetchall()
                print('SkillLevelDefinitions table structure:')
                for col in columns:
                    print(f'  {col[0]}: {col[1]} ({col[2]})')

                # Get sample data
                cur.execute('SELECT * FROM public."SkillLevelDefinitions" LIMIT 5;')
                sample_data = cur.fetchall()
                print(f'\nSample data ({len(sample_data)} rows):')
                for row in sample_data:
                    print(f'  {row}')

                # Get all data to understand the structure better
                cur.execute('SELECT COUNT(*) FROM public."SkillLevelDefinitions";')
                total_count = cur.fetchone()[0]
                print(f'\nTotal rows in table: {total_count}')

                # Get distinct skills
                cur.execute('SELECT DISTINCT "SkillId", "SkillName" FROM public."SkillLevelDefinitions" LIMIT 10;')
                skills = cur.fetchall()
                print(f'\nDistinct skills (first 10):')
                for skill in skills:
                    print(f'  ID: {skill[0]}, Name: {skill[1]}')

    except Exception as e:
        LOGGER.error(f'Error exploring SkillLevelDefinitions: {e}')

if __name__ == "__main__":
    explore_skill_level_definitions()