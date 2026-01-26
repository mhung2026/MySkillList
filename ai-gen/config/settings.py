import os
from dotenv import load_dotenv

load_dotenv()

# Azure OpenAI API settings
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")
OPENAI_BASE_URL = os.getenv("OPENAI_BASE_URL")
LLM_MODEL = os.getenv("LLM_MODEL", "gpt-4o")

# Database settings
DB_CONNECT_STRING = os.getenv("DB_CONNECT_STRING")

# Other settings
DEBUG = os.getenv("DEBUG", "False").lower() == "true"