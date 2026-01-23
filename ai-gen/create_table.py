#!/usr/bin/env python3
"""
Script to create the ConfigDocTemplate table
"""

import sys
import os
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from src.custom import getConn, LOGGER

def create_table():
    """Create the ConfigDocTemplate table."""
    try:
        with getConn() as conn:
            with conn.cursor() as cur:
                # Read and execute the SQL file
                sql_file_path = os.path.join(os.path.dirname(__file__), 'create_table.sql')
                with open(sql_file_path, 'r', encoding='utf-8') as f:
                    sql_script = f.read()

                cur.execute(sql_script)
                conn.commit()

                LOGGER.info("ConfigDocTemplate table created successfully")

                # Verify the table was created
                cur.execute("""
                    SELECT table_name
                    FROM information_schema.tables
                    WHERE table_schema = 'public'
                    AND table_name = 'ConfigDocTemplate';
                """)
                result = cur.fetchone()
                if result:
                    LOGGER.info("Table verification successful: ConfigDocTemplate exists")
                else:
                    LOGGER.error("Table verification failed: ConfigDocTemplate not found")

    except Exception as e:
        LOGGER.error(f"Failed to create table: {e}")
        conn.rollback() if 'conn' in locals() else None

if __name__ == "__main__":
    create_table()