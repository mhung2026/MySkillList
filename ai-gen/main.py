import logging
import sys
from fastapi import FastAPI, HTTPException
from fastapi.responses import JSONResponse
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from config.settings import DEBUG, OPENAI_API_KEY

# Import V2 routes
from src.api import routes_v2

# Configure logging
logging.basicConfig(
    level=logging.DEBUG if DEBUG else logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    stream=sys.stdout
)
logger = logging.getLogger(__name__)

app = FastAPI(
    title="AI Question Generator API",
    description="Generate assessment questions automatically using AI from skill definitions",
    version="2.0.0",
    docs_url="/api/docs",
    redoc_url="/api/redoc",
    openapi_url="/api/openapi.json"
)

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Mount V2 router
app.include_router(routes_v2.router)

# Health response model
class HealthResponse(BaseModel):
    status: str
    version: str
    api_ready: bool

@app.on_event("startup")
async def startup_event():
    """Startup event handler."""
    logger.info("=" * 50)
    logger.info("AI Question Generator API Starting Up")
    logger.info("=" * 50)
    logger.info(f"DEBUG mode: {DEBUG}")
    logger.info(f"OpenAI API configured: {OPENAI_API_KEY is not None and len(OPENAI_API_KEY or '') > 0}")

@app.on_event("shutdown")
async def shutdown_event():
    """Shutdown event handler."""
    logger.info("=" * 50)
    logger.info("AI Question Generator API Shutting Down")
    logger.info("=" * 50)

@app.get("/", tags=["Info"])
async def root():
    """API information."""
    return {
        "title": "AI Question Generator API",
        "version": "2.0.0",
        "description": "Generate assessment questions using AI",
        "endpoints": {
            "health": "/health",
            "generate": "/api/v2/generate-questions",
            "grade": "/api/v2/grade-answer",
            "docs": "/api/docs",
            "redoc": "/api/redoc"
        }
    }

@app.get("/health", response_model=HealthResponse, tags=["Health"])
async def health_check():
    """Check API health status."""
    try:
        api_ready = (OPENAI_API_KEY is not None and len(OPENAI_API_KEY) > 0)
    except:
        api_ready = False

    return HealthResponse(
        status="healthy",
        version="2.0.0",
        api_ready=api_ready
    )

@app.get("/test", tags=["Test"])
async def test_endpoint():
    """Simple test endpoint."""
    return {"message": "Test endpoint working"}

@app.exception_handler(HTTPException)
async def http_exception_handler(request, exc):
    """Custom HTTP exception handler with logging."""
    logger.error(f"HTTP error {exc.status_code}: {exc.detail}")
    return JSONResponse(
        status_code=exc.status_code,
        content={"error": exc.detail, "status_code": exc.status_code}
    )

if __name__ == "__main__":
    import uvicorn
    import os

    # Get port from environment variable (Railway sets this automatically)
    port = int(os.getenv("PORT", 8000))
    host = os.getenv("HOST", "0.0.0.0")

    logger.info(f"Starting AI Question Generator API on {host}:{port}...")
    uvicorn.run(
        app,
        host=host,
        port=port,
        log_level="debug" if DEBUG else "info"
    )
