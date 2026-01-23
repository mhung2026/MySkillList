# Use Python 3.11 base image
FROM python:3.11.9-slim

# Set working directory
WORKDIR /app

# Copy requirements first for better layer caching
# Note: Build context is repo root, so we need ai-gen/ prefix
COPY ./ai-gen/requirements.txt ./requirements.txt

# Install Python dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Copy all application code from ai-gen folder
COPY ./ai-gen/ ./

# Expose port (Railway will override with $PORT)
EXPOSE 8002

# Start command - use shell form to allow environment variable expansion
CMD uvicorn main:app --host 0.0.0.0 --port ${PORT:-8002}
