#!/usr/bin/env python3
"""
Test script for database connection and custom functions
"""

import sys
import os
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from src.custom import getConn, LOGGER, getKeywordsTable

def test_database_connection():
    """Test basic database connection."""
    try:
        with getConn() as conn:
            with conn.cursor() as cur:
                cur.execute("SELECT version();")
                version = cur.fetchone()
                LOGGER.info(f"Database connection successful. PostgreSQL version: {version[0]}")
                return True
    except Exception as e:
        LOGGER.error(f"Database connection failed: {e}")
        return False

def test_get_keywords_table():
    """Test the getKeywordsTable function."""
    try:
        # Test with a sample docTemplate code
        result = getKeywordsTable("SAMPLE_TEMPLATE")
        LOGGER.info(f"getKeywordsTable result: {result}")
        return result
    except Exception as e:
        LOGGER.error(f"getKeywordsTable test failed: {e}")
        return None

if __name__ == "__main__":
    print("Testing database connection...")
    connection_ok = test_database_connection()

    if connection_ok:
        print("Testing getKeywordsTable function...")
        test_get_keywords_table()
    else:
        print("Database connection failed. Please check your configuration.")