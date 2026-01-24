import os
import json
import logging
from typing import List, Dict, Any

import psycopg2
from psycopg2 import pool
from psycopg2.extras import RealDictCursor
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# Setup logging
LOGGER = logging.getLogger(__name__)

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
                "port": config.get("Port", 5432),
                "max_pool_size": config.get("MaxPoolSize", 1000)
            }
        except json.JSONDecodeError:
            LOGGER.error("Failed to parse DB_CONNECT_STRING")

    # Fallback to defaults
    return {
        "host": os.getenv("DATABASE_HOST", "192.168.0.21"),
        "database": os.getenv("DATABASE_NAME", "MySkillList_NGE_DEV"),
        "user": os.getenv("DATABASE_USER", "postgres"),
        "password": os.getenv("DATABASE_PASSWORD", "@ll1@nceP@ss2o21"),
        "port": int(os.getenv("DATABASE_PORT", "5432")),
        "max_pool_size": 1000
    }

DB_CONFIG = get_db_config()

# Global pool (khởi tạo 1 lần)
db_pool: pool.ThreadedConnectionPool = None

def init_db_pool():
    global db_pool
    if db_pool is None:
        try:
            db_pool = psycopg2.pool.ThreadedConnectionPool(
                minconn=5,                     # min connections
                maxconn=DB_CONFIG["max_pool_size"],
                host=DB_CONFIG["host"],
                database=DB_CONFIG["database"],
                user=DB_CONFIG["user"],
                password=DB_CONFIG["password"],
                port=DB_CONFIG.get("port", 5432),
                cursor_factory=RealDictCursor   # trả về dict thay vì tuple
            )
            LOGGER.info("PostgreSQL connection pool initialized successfully.")
        except Exception as e:
            LOGGER.error(f"Failed to initialize DB pool: {e}")
            raise

def get_db_connection():
    """Lấy connection từ pool (tương tự getConn() của bạn)"""
    if db_pool is None:
        init_db_pool()
    try:
        conn = db_pool.getconn()
        conn.autocommit = True  # nếu không dùng transaction dài
        return conn
    except Exception as e:
        LOGGER.error(f"Error getting connection: {e}")
        raise

def release_db_connection(conn):
    """Trả connection về pool"""
    if db_pool and conn:
        db_pool.putconn(conn)

# Context manager tiện lợi (giống with getConn() as conn)
class DBConnection:
    def __enter__(self):
        self.conn = get_db_connection()
        return self.conn

    def __exit__(self, exc_type, exc_val, exc_tb):
        release_db_connection(self.conn)
        if exc_type is not None:
            LOGGER.error(f"DB error: {exc_val}")

def get_skill_level_definitions() -> List[Dict[str, Any]]:
    """
    Đọc toàn bộ bảng public.SkillLevelDefinitions
    Trả về list of dict (mỗi row là 1 dict)
    """
    query = """
    SELECT 
        "Id", "SkillId", "Level", "Description", 
        "Autonomy", "Influence", "Complexity", "BusinessSkills", "Knowledge",
        "BehavioralIndicators", "EvidenceExamples",
        "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy",
        "IsDeleted", "DeletedAt", "DeletedBy", "CustomLevelName"
    FROM public."SkillLevelDefinitions"
    WHERE "IsDeleted" = FALSE
    ORDER BY "SkillId", "Level" ASC;
    """

    results = []
    with DBConnection() as conn:
        with conn.cursor() as cur:
            try:
                cur.execute(query)
                results = cur.fetchall()  # list of RealDictRow → dict
                LOGGER.debug(f"Retrieved {len(results)} rows from SkillLevelDefinitions.")
            except Exception as e:
                LOGGER.error(f"Query failed: {e}")
                raise

    return [dict(row) for row in results]  # đảm bảo là dict thường


# Ví dụ hàm filter theo SkillId (nếu cần group để generate per skill)
def get_levels_by_skill(skill_id: str) -> List[Dict[str, Any]]:
    query = """
    SELECT * FROM public."SkillLevelDefinitions"
    WHERE "SkillId" = %s AND "IsDeleted" = FALSE
    ORDER BY "Level" ASC;
    """
    with DBConnection() as conn:
        with conn.cursor() as cur:
            cur.execute(query, (skill_id,))
            rows = cur.fetchall()
            return [dict(row) for row in rows]


# Ví dụ sử dụng
if __name__ == "__main__":
    logging.basicConfig(level=logging.DEBUG)
    data = get_skill_level_definitions()
    print(f"Total rows: {len(data)}")
    if data:
        print("Sample row keys:", list(data[0].keys()))
        print("Sample SkillId:", data[0]["SkillId"])