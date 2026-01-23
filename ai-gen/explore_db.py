#!/usr/bin/env python3
"""
Database exploration script to check available tables and schemas
"""

import sys
import os
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from src.custom import getConn, LOGGER

def explore_database():
    """Explore the database structure."""
    try:
        with getConn() as conn:
            with conn.cursor() as cur:
                # Check available schemas
                cur.execute("""
                    SELECT schema_name
                    FROM information_schema.schemata
                    ORDER BY schema_name;
                """)
                schemas = cur.fetchall()
                print("Available schemas:")
                for schema in schemas:
                    print(f"  - {schema[0]}")

                # Check tables in public schema
                cur.execute("""
                    SELECT table_schema, table_name
                    FROM information_schema.tables
                    WHERE table_type = 'BASE TABLE'
                    AND table_schema NOT IN ('pg_catalog', 'information_schema')
                    ORDER BY table_schema, table_name;
                """)
                tables = cur.fetchall()
                print("\nAvailable tables:")
                for schema, table in tables:
                    print(f"  - {schema}.{table}")

                # Look for tables that might be related to config or templates
                cur.execute("""
                    SELECT table_schema, table_name
                    FROM information_schema.tables
                    WHERE table_type = 'BASE TABLE'
                    AND table_schema NOT IN ('pg_catalog', 'information_schema')
                    AND (table_name ILIKE '%config%' OR table_name ILIKE '%template%' OR table_name ILIKE '%doc%')
                    ORDER BY table_schema, table_name;
                """)
                config_tables = cur.fetchall()
                print("\nTables related to config/template:")
                for schema, table in config_tables:
                    print(f"  - {schema}.{table}")

    except Exception as e:
        LOGGER.error(f"Database exploration failed: {e}")

if __name__ == "__main__":
    explore_database()