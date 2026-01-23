# Use Python 3.11 base image
FROM python:3.11.9-slim

# Set working directory
WORKDIR /app

# Copy AI service files
COPY ai-gen/requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy application code
COPY ai-gen/ .

# Expose port
EXPOSE 8002

# Start command
CMD uvicorn main:app --host 0.0.0.0 --port ${PORT:-8002}
