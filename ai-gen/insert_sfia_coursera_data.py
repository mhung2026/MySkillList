"""
Script to insert SFIA skills and Coursera courses data from JSON into PostgreSQL database.
"""

import json
import psycopg2
from psycopg2.extras import execute_values
from datetime import datetime
from config.settings import DB_CONNECT_STRING

def parse_db_connection_string(db_string):
    """Parse DB_CONNECT_STRING from JSON format to psycopg2 connection string"""
    if not db_string:
        raise ValueError(
            "DB_CONNECT_STRING is not set. Please create a .env file in the ai-gen directory "
            "with DB_CONNECT_STRING variable. See .env.example for reference."
        )

    try:
        # Try to parse as JSON first
        db_config = json.loads(db_string)
        conn_string = f"host={db_config['ServerName']} dbname={db_config['CatalogName']} user={db_config['Username']} password={db_config['Password']}"
        return conn_string
    except (json.JSONDecodeError, KeyError):
        # If not JSON, assume it's already a connection string
        return db_string

def create_tables(conn):
    """Create tables if they don't exist"""
    with open('create_sfia_coursera_tables.sql', 'r', encoding='utf-8') as f:
        sql_script = f.read()

    cursor = conn.cursor()
    cursor.execute(sql_script)
    conn.commit()
    cursor.close()
    print("✓ Tables created successfully")

def load_json_data(file_path):
    """Load JSON data from file"""
    with open(file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    return data

def insert_skills(conn, skills_data):
    """Insert SFIA skills into database"""
    cursor = conn.cursor()

    # Prepare data for bulk insert
    skills_to_insert = []
    for skill in skills_data:
        skills_to_insert.append((
            skill['skill_id'],
            skill['skill_name'],
            skill['skill_code'],
            skill['level_count'],
            skill['courses_found']
        ))

    # Bulk insert with conflict handling
    insert_query = """
        INSERT INTO public."SFIASkillCoursera"
        ("SkillId", "SkillName", "SkillCode", "LevelCount", "CoursesFound")
        VALUES %s
        ON CONFLICT ("SkillId") DO UPDATE SET
            "SkillName" = EXCLUDED."SkillName",
            "SkillCode" = EXCLUDED."SkillCode",
            "LevelCount" = EXCLUDED."LevelCount",
            "CoursesFound" = EXCLUDED."CoursesFound",
            "ModifiedDate" = CURRENT_TIMESTAMP
    """

    execute_values(cursor, insert_query, skills_to_insert)
    conn.commit()
    cursor.close()

    print(f"✓ Inserted {len(skills_to_insert)} skills")
    return len(skills_to_insert)

def insert_courses(conn, skills_data):
    """Insert Coursera courses into database"""
    cursor = conn.cursor()

    # Prepare data for bulk insert
    courses_to_insert = []
    for skill in skills_data:
        skill_id = skill['skill_id']
        for course in skill.get('courses', []):
            # Parse scraped_at timestamp
            scraped_at = None
            if 'scraped_at' in course and course['scraped_at']:
                try:
                    scraped_at = datetime.fromisoformat(course['scraped_at'])
                except:
                    scraped_at = None

            # Handle certificate_available
            cert_available = course.get('certificate_available')
            if cert_available == 'N/A' or cert_available is None:
                cert_available = None

            courses_to_insert.append((
                skill_id,
                course.get('url'),
                course.get('title'),
                course.get('instructor', []),
                course.get('organization'),
                course.get('description'),
                course.get('rating'),
                course.get('reviews_count'),
                course.get('enrollment_count'),
                course.get('duration'),
                course.get('level'),
                course.get('language'),
                course.get('subtitles', []),
                course.get('skills', []),
                course.get('syllabus', []),
                course.get('prerequisites', []),
                course.get('price'),
                cert_available,
                scraped_at
            ))

    # Bulk insert courses
    insert_query = """
        INSERT INTO public."CourseraCourse"
        ("SkillId", "Url", "Title", "Instructor", "Organization", "Description",
         "Rating", "ReviewsCount", "EnrollmentCount", "Duration", "Level", "Language",
         "Subtitles", "Skills", "Syllabus", "Prerequisites", "Price",
         "CertificateAvailable", "ScrapedAt")
        VALUES %s
    """

    execute_values(cursor, insert_query, courses_to_insert)
    conn.commit()
    cursor.close()

    print(f"✓ Inserted {len(courses_to_insert)} courses")
    return len(courses_to_insert)

def main():
    """Main execution function"""
    print("Starting SFIA Coursera data insertion...")
    print("=" * 60)

    # Load JSON data
    json_file_path = '../crawldata/sfia_skills_coursera_courses.json'
    print(f"Loading data from: {json_file_path}")
    data = load_json_data(json_file_path)

    # Print metadata
    metadata = data.get('scraping_metadata', {})
    print(f"\nMetadata:")
    print(f"  Scraped at: {metadata.get('scraped_at')}")
    print(f"  Total skills: {metadata.get('total_skills')}")
    print(f"  Max courses per skill: {metadata.get('max_courses_per_skill')}")
    print()

    # Connect to database
    print("Connecting to database...")
    conn_string = parse_db_connection_string(DB_CONNECT_STRING)
    conn = psycopg2.connect(conn_string)

    try:
        # Create tables
        print("\nCreating tables...")
        create_tables(conn)

        # Insert data
        print("\nInserting skills...")
        skills_data = data.get('skills_with_courses', [])
        skills_count = insert_skills(conn, skills_data)

        print("\nInserting courses...")
        courses_count = insert_courses(conn, skills_data)

        print("\n" + "=" * 60)
        print("Data insertion completed successfully!")
        print(f"Summary:")
        print(f"  Skills inserted/updated: {skills_count}")
        print(f"  Courses inserted: {courses_count}")

    except Exception as e:
        print(f"\n✗ Error occurred: {e}")
        conn.rollback()
        raise

    finally:
        conn.close()
        print("\nDatabase connection closed.")

if __name__ == "__main__":
    main()
