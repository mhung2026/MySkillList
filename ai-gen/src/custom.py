import os
import json
import logging
import psycopg2
from psycopg2 import pool
from contextlib import contextmanager
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# Database configuration - read from environment variable
def get_db_config():
    """Parse DB_CONNECT_STRING from environment or use defaults."""
    db_connect_string = os.getenv("DB_CONNECT_STRING")
    if db_connect_string:
        try:
            config = json.loads(db_connect_string)
            return {
                "host": config.get("ServerName", "localhost"),
                "database": config.get("CatalogName", "MySkillList_NGE_DEV"),
                "user": config.get("Username", "postgres"),
                "password": config.get("Password", ""),
                "port": config.get("Port", 5432)
            }
        except json.JSONDecodeError:
            logging.error("Failed to parse DB_CONNECT_STRING")

    # Fallback to defaults
    return {
        "host": os.getenv("DATABASE_HOST", "192.168.0.21"),
        "database": os.getenv("DATABASE_NAME", "MySkillList_NGE_DEV"),
        "user": os.getenv("DATABASE_USER", "postgres"),
        "password": os.getenv("DATABASE_PASSWORD", "@ll1@nceP@ss2o21"),
        "port": int(os.getenv("DATABASE_PORT", "5432"))
    }

DB_CONFIG = get_db_config()

# Connection pool
connection_pool = None

# Logger setup
LOGGER = logging.getLogger(__name__)
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)

def init_connection_pool():
    """Initialize the database connection pool."""
    global connection_pool
    if connection_pool is None:
        try:
            connection_pool = psycopg2.pool.SimpleConnectionPool(
                minconn=1,
                maxconn=1000,
                **DB_CONFIG
            )
            LOGGER.info("Database connection pool initialized successfully")
        except Exception as e:
            LOGGER.error(f"Failed to initialize connection pool: {e}")
            raise

@contextmanager
def getConn():
    """Context manager for database connections."""
    global connection_pool
    if connection_pool is None:
        init_connection_pool()

    conn = None
    try:
        conn = connection_pool.getconn()
        yield conn
    except Exception as e:
        LOGGER.error(f"Database connection error: {e}")
        raise
    finally:
        if conn:
            connection_pool.putconn(conn)

def getKeywordsTable(docTemplate: str):
    """Get keywords table for a document template from database."""
    with getConn() as conn:
        with conn.cursor() as cur:
            cur.execute("""
                SELECT "KeyWordsTable" FROM public."ConfigDocTemplate"
                WHERE NOT "IsDisable"
                    AND NOT "IsDeleted"
                    AND "Code"  = %s
            """, (docTemplate,))
            keywordsTable = cur.fetchone()
            LOGGER.debug(f"Success get Keywords for Table of Document Template - {docTemplate} in DB.")
    return keywordsTable

def getSkillData(skill_id: str = None):
    """Get skill data from database and format for input_skill_schema.json."""
    with getConn() as conn:
        with conn.cursor() as cur:
            if skill_id:
                # Get specific skill
                cur.execute("""
                    SELECT s."Id", s."Name", s."Code", sld."Level", sld."Description",
                           sld."Autonomy", sld."Influence", sld."Complexity",
                           sld."BusinessSkills", sld."Knowledge",
                           sld."BehavioralIndicators", sld."EvidenceExamples"
                    FROM public."Skills" s
                    JOIN public."SkillLevelDefinitions" sld ON s."Id" = sld."SkillId"
                    WHERE s."Id" = %s AND NOT s."IsDeleted" AND NOT sld."IsDeleted"
                    ORDER BY sld."Level";
                """, (skill_id,))
            else:
                # Get first skill as example
                cur.execute("""
                    SELECT s."Id", s."Name", s."Code", sld."Level", sld."Description",
                           sld."Autonomy", sld."Influence", sld."Complexity",
                           sld."BusinessSkills", sld."Knowledge",
                           sld."BehavioralIndicators", sld."EvidenceExamples"
                    FROM public."Skills" s
                    JOIN public."SkillLevelDefinitions" sld ON s."Id" = sld."SkillId"
                    WHERE NOT s."IsDeleted" AND NOT sld."IsDeleted"
                    ORDER BY s."Id", sld."Level"
                    LIMIT 10;
                """)

            rows = cur.fetchall()
            LOGGER.debug(f"Retrieved {len(rows)} skill level definitions from database")

            if not rows:
                return None

            # Group by skill
            skills_data = {}
            for row in rows:
                skill_id_val = str(row[0])
                if skill_id_val not in skills_data:
                    skills_data[skill_id_val] = {
                        "skill_id": skill_id_val,
                        "skill_name": row[1],
                        "skill_code": row[2],
                        "levels": []
                    }

                # Parse JSON arrays
                behavioral_indicators = row[10] if row[10] else []
                evidence_examples = row[11] if row[11] else []

                level_data = {
                    "level": row[3],
                    "description": row[4],
                    "autonomy": row[5],
                    "influence": row[6],
                    "complexity": row[7],
                    "business_skills": row[8],
                    "knowledge": row[9],
                    "behavioral_indicators": behavioral_indicators,
                    "evidence_examples": evidence_examples
                }
                skills_data[skill_id_val]["levels"].append(level_data)

            # Return first skill data in the expected format
            first_skill = list(skills_data.values())[0]
            result = {
                "skill_name": first_skill["skill_name"],
                "skill_id": first_skill["skill_id"],
                "levels": sorted(first_skill["levels"], key=lambda x: x["level"])
            }

            return result

def getAllSkillsList():
    """Get list of all available skills."""
    with getConn() as conn:
        with conn.cursor() as cur:
            cur.execute("""
                SELECT s."Id", s."Name", s."Code"
                FROM public."Skills" s
                WHERE NOT s."IsDeleted"
                ORDER BY s."Name";
            """)
            skills = cur.fetchall()
            return [{"id": str(row[0]), "name": row[1], "code": row[2]} for row in skills]