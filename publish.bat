@echo off
echo ============================================
echo   SkillMatrix - Build for Production
echo ============================================
echo.

:: Set variables
set PUBLISH_DIR=publish
set API_PUBLISH=%PUBLISH_DIR%\api
set WEB_PUBLISH=%PUBLISH_DIR%\web
set AI_PUBLISH=%PUBLISH_DIR%\ai-gen

:: Clean previous build
echo [1/5] Cleaning previous build...
if exist %PUBLISH_DIR% rmdir /s /q %PUBLISH_DIR%
mkdir %PUBLISH_DIR%
mkdir %API_PUBLISH%
mkdir %WEB_PUBLISH%
mkdir %AI_PUBLISH%

:: Build Backend API
echo.
echo [2/5] Building Backend API...
cd src\SkillMatrix.Api
dotnet publish -c Release -o ..\..\%API_PUBLISH%
if errorlevel 1 (
    echo ERROR: Backend build failed!
    pause
    exit /b 1
)
cd ..\..

:: Prepare AI Service
echo.
echo [3/5] Preparing AI Service...
cd ai-gen
echo Copying AI service files...
xcopy /s /e /y /i src ..\%AI_PUBLISH%\src\
xcopy /s /e /y /i schemas ..\%AI_PUBLISH%\schemas\
copy main.py ..\%AI_PUBLISH%\
copy requirements.txt ..\%AI_PUBLISH%\
copy .env.example ..\%AI_PUBLISH%\.env.example
if exist db_skill_reader.py copy db_skill_reader.py ..\%AI_PUBLISH%\
if exist web.config copy web.config ..\%AI_PUBLISH%\
echo AI Service files copied successfully
cd ..

:: Build Frontend
echo.
echo [4/5] Building Frontend...
cd web
call npm install
call npm run build
if errorlevel 1 (
    echo ERROR: Frontend build failed!
    pause
    exit /b 1
)

:: Copy frontend build to publish
echo.
echo [5/5] Copying frontend files...
xcopy /s /e /y dist\* ..\%WEB_PUBLISH%\
copy web.config ..\%WEB_PUBLISH%\
cd ..

echo.
echo ============================================
echo   Build completed successfully!
echo ============================================
echo.
echo Output directories:
echo   - Backend API:  %API_PUBLISH%
echo   - Frontend:     %WEB_PUBLISH%
echo   - AI Service:   %AI_PUBLISH%
echo.
echo Next steps:
echo   1. Copy %API_PUBLISH% to IIS Backend site folder
echo      Domain: https://myskilllist-ngeteam-ad.allianceitsc.com
echo.
echo   2. Copy %WEB_PUBLISH% to IIS Frontend site folder
echo      Domain: https://myskilllist-ngeteam-ad.allianceitsc.com
echo.
echo   3. Copy %AI_PUBLISH% to IIS AI Service site folder
echo      Domain: https://myskilllist-ngeteam-ai.allianceitsc.com
echo      - Install Python 3.11+ on server
echo      - Run: pip install -r requirements.txt
echo      - Configure .env file with API keys
echo      - Configure IIS with HttpPlatformHandler
echo.
echo   4. Configure IIS sites (see DEPLOY_GUIDE.md)
echo.
pause