# SkillMatrix Deployment Guide for IIS

## Domain Configuration
- **Frontend (UI):** https://myskilllist-ngeteam-ad.allianceitsc.com
- **Backend (API):** https://myskilllist-ngeteam-api.allianceitsc.com
- **AI Service:** https://myskilllist-ngeteam-ai.allianceitsc.com

## System Requirements

### Server Requirements
- Windows Server 2016/2019/2022 or Windows 10/11
- IIS 10.0+
- .NET 9.0 Runtime (ASP.NET Core Hosting Bundle)
- Python 3.11+ (for AI Service)
- PostgreSQL Server (can be on a different machine)

### Installing .NET 8.0 Hosting Bundle
1. Download from: https://dotnet.microsoft.com/download/dotnet/8.0
2. Select **ASP.NET Core Runtime** → **Hosting Bundle**
3. Install and restart IIS

### Enable IIS Features
Go to **Control Panel** → **Programs** → **Turn Windows features on or off**:
- Internet Information Services
  - Web Management Tools
    - IIS Management Console
  - World Wide Web Services
    - Application Development Features
      - .NET Extensibility 4.8
      - ASP.NET 4.8
      - ISAPI Extensions
      - ISAPI Filters
    - Common HTTP Features
      - Default Document
      - Directory Browsing
      - HTTP Errors
      - Static Content
    - Security
      - Request Filtering
      - URL Authorization

### Installing URL Rewrite Module (for Frontend)
1. Download from: https://www.iis.net/downloads/microsoft/url-rewrite
2. Install URL Rewrite 2.1

### Installing HttpPlatformHandler (for AI Service)
1. Download from: https://www.iis.net/downloads/microsoft/httpplatformhandler
2. Install HttpPlatformHandler 1.2

### Installing Python (for AI Service)
1. Download Python 3.11 or later from: https://www.python.org/downloads/
2. Install to `C:\Python311`
3. Check "Add Python to PATH" during installation
4. Verify installation: `python --version`

---

## Step 1: Build the Application

### Option 1: Using automated script (Recommended)
```batch
cd "e:\Ngệ Team\SkillMatrix"
publish.bat
```

This will build:
- Backend API → `publish\api`
- Frontend → `publish\web`
- AI Service → `publish\ai-gen`

### Option 2: Manual build

**Build Backend API:**
```batch
cd src\SkillMatrix.Api
dotnet publish -c Release -o C:\publish\skillmatrix-api
```

**Build Frontend:**
```batch
cd web
npm install
npm run build
```

**Prepare AI Service:**
```batch
cd ai-gen
xcopy /s /e /y src C:\publish\skillmatrix-ai\src\
xcopy /s /e /y schemas C:\publish\skillmatrix-ai\schemas\
copy main.py C:\publish\skillmatrix-ai\
copy requirements.txt C:\publish\skillmatrix-ai\
copy web.config C:\publish\skillmatrix-ai\
copy .env.example C:\publish\skillmatrix-ai\
```
Copy the `dist\*` folder and `web.config` to `C:\publish\skillmatrix-web`

---

## Step 2: Pre-deployment Configuration

### 2.1 Update API URL for Frontend

The file `web\.env.production` is already configured:
```env
VITE_API_URL=https://myskilllist-ngeteam-api.allianceitsc.com/api
```

### 2.2 Update Connection String for Backend

Edit file `src\SkillMatrix.Api\appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_DB_SERVER;Database=SkillMatrix;Username=YOUR_USER;Password=YOUR_PASS"
  }
}
```

---

## Step 3: Create IIS Sites

### 3.1 Create Application Pool

**For Backend API:**
1. Open **IIS Manager**
2. Right-click **Application Pools** → **Add Application Pool**
3. Name: `SkillMatrixAPI`
4. .NET CLR Version: **No Managed Code**
5. Managed Pipeline Mode: **Integrated**
6. Click **OK**
7. Right-click the newly created pool → **Advanced Settings**
   - Start Mode: `AlwaysRunning`
   - Identity: `ApplicationPoolIdentity` (or a user with appropriate permissions)

**For Frontend:**
1. Create new pool: `SkillMatrixWeb`
2. .NET CLR Version: **No Managed Code**
3. Managed Pipeline Mode: **Integrated**

### 3.2 Create Website/Application

**Option A: Using 2 separate Sites (recommended)**

**Site 1 - Backend API:**
1. Right-click **Sites** → **Add Website**
2. Site name: `SkillMatrixAPI`
3. Application pool: `SkillMatrixAPI`
4. Physical path: `C:\inetpub\skillmatrix-api`
5. Binding:
   - Type: https
   - IP: All Unassigned
   - Port: 443
   - Host name: `myskilllist-ngeteam-api.allianceitsc.com`
   - SSL Certificate: Select appropriate certificate

**Site 2 - Frontend:**
1. Right-click **Sites** → **Add Website**
2. Site name: `SkillMatrixWeb`
3. Application pool: `SkillMatrixWeb`
4. Physical path: `C:\inetpub\skillmatrix-web`
5. Binding:
   - Type: https
   - Port: 443
   - Host name: `myskilllist-ngeteam-ad.allianceitsc.com`
   - SSL Certificate: Select appropriate certificate

---

**Option B: Using 1 Site with Virtual Directory**

1. Create main site for Frontend on port 80
2. Right-click site → **Add Application**
   - Alias: `api`
   - Application pool: `SkillMatrixAPI`
   - Physical path: `C:\inetpub\skillmatrix-api`

URLs will be:
- Frontend: `http://your-server/`
- API: `http://your-server/api/`

---

## Step 4: Copy Files

```batch
:: Copy API
xcopy /s /e /y "publish\api\*" "C:\inetpub\skillmatrix-api\"

:: Copy Frontend
xcopy /s /e /y "publish\web\*" "C:\inetpub\skillmatrix-web\"
```

### Create logs folder for API
```batch
mkdir C:\inetpub\skillmatrix-api\logs
```

### Grant permissions for IIS
```batch
icacls "C:\inetpub\skillmatrix-api" /grant "IIS AppPool\SkillMatrixAPI":(OI)(CI)M
icacls "C:\inetpub\skillmatrix-web" /grant "IIS AppPool\SkillMatrixWeb":(OI)(CI)R
```

---

## Step 5: Configure CORS (Backend)

If Frontend and API are on different domains/ports, configure CORS in `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "http://localhost",
            "http://your-domain.com",
            "http://skillmatrix.yourdomain.com"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Add before app.MapControllers()
app.UseCors();
```

---

## Step 6: Deploy AI Service to IIS

### 1. Copy AI Service Files
```batch
xcopy /s /e /y publish\ai-gen\* C:\inetpub\skillmatrix-ai\
```

### 2. Install Python Dependencies
```batch
cd C:\inetpub\skillmatrix-ai
pip install -r requirements.txt
```

### 3. Configure Environment Variables
```batch
cd C:\inetpub\skillmatrix-ai
copy .env.example .env
notepad .env
```

Edit `.env` file:
```env
# Azure OpenAI Configuration
OPENAI_API_KEY=your_azure_openai_api_key
OPENAI_BASE_URL=https://your-azure-openai-endpoint.openai.azure.com/openai/deployments/gpt-4o

# Database Configuration (same as Backend)
DATABASE_HOST=192.168.0.21
DATABASE_PORT=5432
DATABASE_NAME=MySkillList_NGE_DEV
DATABASE_USER=postgres
DATABASE_PASSWORD=your_password

# Optional
DEBUG=False
PORT=8002
```

### 4. Create IIS Site for AI Service

**In IIS Manager:**

1. **Add New Site**:
   - Site name: `SkillMatrix-AI`
   - Physical path: `C:\inetpub\skillmatrix-ai`
   - Binding:
     - Type: `https`
     - Host name: `myskilllist-ngeteam-ai.allianceitsc.com`
     - Port: `443`
     - SSL Certificate: Select your certificate

2. **Configure Application Pool**:
   - Right-click site → **Manage Website** → **Advanced Settings**
   - Application Pool: Create new pool `SkillMatrix-AI-Pool`
   - .NET CLR version: `No Managed Code`
   - Managed pipeline mode: `Integrated`

3. **Set Folder Permissions**:
```batch
icacls "C:\inetpub\skillmatrix-ai" /grant "IIS_IUSRS:(OI)(CI)F" /T
mkdir C:\inetpub\skillmatrix-ai\logs
icacls "C:\inetpub\skillmatrix-ai\logs" /grant "IIS_IUSRS:(OI)(CI)F" /T
```

4. **Verify web.config**:
   - Ensure Python path is correct: `C:\Python311\python.exe`
   - Verify `main.py` exists in root folder

5. **Start the site** in IIS Manager

### 5. Update Backend Configuration

Edit `C:\inetpub\skillmatrix-api\appsettings.Production.json`:
```json
{
  "AiService": {
    "UseMock": false,
    "BaseUrl": "https://myskilllist-ngeteam-ai.allianceitsc.com",
    "ModelName": "gpt-4o",
    "TimeoutSeconds": 120
  }
}
```

Restart Backend site after changes.

---

## Step 7: Test All Services

### Test Frontend
1. Open browser: `https://myskilllist-ngeteam-ad.allianceitsc.com`
2. Login with: `admin@skillmatrix.com` / `admin123`
3. Verify Dashboard displays correctly

### Test Backend API
```
https://myskilllist-ngeteam-api.allianceitsc.com/api/dashboard/overview
https://myskilllist-ngeteam-api.allianceitsc.com/swagger
```

### Test AI Service
```
https://myskilllist-ngeteam-ai.allianceitsc.com/health
https://myskilllist-ngeteam-ai.allianceitsc.com/api/v2/health
https://myskilllist-ngeteam-ai.allianceitsc.com/api/docs
```

Expected AI health response:
```json
{
  "status": "healthy",
  "api_version": "v2",
  "database": "connected",
  "total_definitions": 589
}
```

### Test Full Integration (FE → BE → AI)
1. Login to Frontend
2. Create a new Test Template
3. Add a Section to the template
4. Click "Generate Questions with AI"
5. Verify questions are generated and displayed

---

## Troubleshooting

### Error 500.19 - Configuration Error
- Check if URL Rewrite Module is installed
- Verify web.config format is correct

### Error 502.5 - ANCM Out-Of-Process Startup Failure
- Verify .NET 8.0 Hosting Bundle is installed
- Restart IIS: `iisreset`
- Check logs in `C:\inetpub\skillmatrix-api\logs\`

### Error 404 when refreshing React page
- Verify URL Rewrite is configured in web.config
- Verify URL Rewrite Module is installed

### Cannot connect to Database
- Check PostgreSQL firewall port 5432
- Verify connection string is correct
- Test connection from server to DB

### AI Service Errors

**Error 500.0 - Internal Server Error (Python)**
- Check Python path in web.config: `C:\Python311\python.exe`
- Verify Python is installed: `python --version`
- Check folder permissions
- View Python logs: `type C:\inetpub\skillmatrix-ai\logs\python-stdout.log`

**Module Not Found Error**
```batch
cd C:\inetpub\skillmatrix-ai
pip install -r requirements.txt
```

**Database Connection Failed (AI Service)**
- Verify DATABASE_URL in `.env`
- Check network connectivity: `ping 192.168.0.21`
- Verify PostgreSQL allows connections from IIS server

**OpenAI API Key Errors**
- Verify `OPENAI_API_KEY` in `.env` file
- Check Azure OpenAI endpoint URL is correct
- Test API key with curl or Postman

**AI Service Not Starting**
```batch
:: Check Python logs
type C:\inetpub\skillmatrix-ai\logs\python-stdout.log

:: Restart application pool
appcmd stop apppool /apppool.name:"SkillMatrix-AI-Pool"
appcmd start apppool /apppool.name:"SkillMatrix-AI-Pool"

:: Or restart IIS completely
iisreset
```

### View logs
```batch
:: Backend API logs
type C:\inetpub\skillmatrix-api\logs\stdout*.log

:: AI Service logs
type C:\inetpub\skillmatrix-ai\logs\python-stdout.log

:: IIS logs
type C:\inetpub\logs\LogFiles\W3SVC*\*.log

:: Windows Event Viewer
eventvwr.msc
:: → Windows Logs → Application
```

---

## Directory Structure After Deployment

```
C:\inetpub\
├── skillmatrix-api\
│   ├── SkillMatrix.Api.dll
│   ├── SkillMatrix.Api.exe
│   ├── appsettings.json
│   ├── appsettings.Production.json
│   ├── web.config
│   └── logs\
│       └── stdout_*.log
│
├── skillmatrix-web\
│   ├── index.html
│   ├── web.config
│   └── assets\
│       ├── index-*.js
│       └── index-*.css
│
└── skillmatrix-ai\
    ├── main.py
    ├── requirements.txt
    ├── web.config
    ├── .env
    ├── db_skill_reader.py
    ├── src\
    │   ├── api\
    │   ├── generators\
    │   ├── validators\
    │   └── custom\
    ├── schemas\
    │   ├── input_request_schema.json
    │   └── output_question_schema_v2.json
    └── logs\
        └── python-stdout.log
```

---

## Updating the Application

1. Build new version: `publish.bat`
2. Stop site in IIS
3. Copy new files
4. Start site

Or use script:
```batch
@echo off
echo Stopping IIS...
iisreset /stop

echo Updating Backend API...
xcopy /s /e /y "publish\api\*" "C:\inetpub\skillmatrix-api\"

echo Updating Frontend...
xcopy /s /e /y "publish\web\*" "C:\inetpub\skillmatrix-web\"

echo Updating AI Service...
xcopy /s /e /y "publish\ai-gen\*" "C:\inetpub\skillmatrix-ai\"
:: Don't overwrite .env file
if not exist "C:\inetpub\skillmatrix-ai\.env" (
    copy "C:\inetpub\skillmatrix-ai\.env.example" "C:\inetpub\skillmatrix-ai\.env"
)

echo Reinstalling AI dependencies...
cd C:\inetpub\skillmatrix-ai
pip install -r requirements.txt --upgrade

echo Starting IIS...
iisreset /start
echo Done!
```

---

## HTTPS (Recommended for Production)

1. Purchase SSL certificate or use Let's Encrypt
2. In IIS, add HTTPS binding with certificate
3. Update `.env.production`:
   ```env
   VITE_API_URL=https://your-domain.com/api
   ```
4. Redirect HTTP to HTTPS in web.config
