@echo off
echo ============================================
echo   SkillMatrix - Build for Production
echo ============================================
echo.

:: Set variables
set PUBLISH_DIR=publish
set API_PUBLISH=%PUBLISH_DIR%\api
set WEB_PUBLISH=%PUBLISH_DIR%\web

:: Clean previous build
echo [1/4] Cleaning previous build...
if exist %PUBLISH_DIR% rmdir /s /q %PUBLISH_DIR%
mkdir %PUBLISH_DIR%
mkdir %API_PUBLISH%
mkdir %WEB_PUBLISH%

:: Build Backend API
echo.
echo [2/4] Building Backend API...
cd src\SkillMatrix.Api
dotnet publish -c Release -o ..\..\%API_PUBLISH%
if errorlevel 1 (
    echo ERROR: Backend build failed!
    pause
    exit /b 1
)
cd ..\..

:: Build Frontend
echo.
echo [3/4] Building Frontend...
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
echo [4/4] Copying files...
xcopy /s /e /y dist\* ..\%WEB_PUBLISH%\
copy web.config ..\%WEB_PUBLISH%\
cd ..

echo.
echo ============================================
echo   Build completed successfully!
echo ============================================
echo.
echo Output directories:
echo   - API:      %API_PUBLISH%
echo   - Frontend: %WEB_PUBLISH%
echo.
echo Next steps:
echo   1. Copy %API_PUBLISH% to IIS API site folder
echo   2. Copy %WEB_PUBLISH% to IIS Web site folder
echo   3. Configure IIS sites (see DEPLOY_GUIDE.md)
echo.
pause