# AI Service - IIS Deployment Guide

## Prerequisites

### 1. Install Python on Server
```powershell
# Download Python 3.11 or later
# Install to: C:\Python311
# Make sure to check "Add Python to PATH"
```

### 2. Install HttpPlatformHandler for IIS
```powershell
# Download from: https://www.iis.net/downloads/microsoft/httpplatformhandler
# Or install via Web Platform Installer
```

### 3. Install Required Python Packages
```powershell
cd C:\inetpub\wwwroot\ai-service
pip install -r requirements.txt
```

## Deployment Steps

### 1. Copy Files to IIS Directory
```powershell
# Create directory
mkdir C:\inetpub\wwwroot\ai-service

# Copy all files from publish\ai-gen to:
xcopy /s /e /y publish\ai-gen\* C:\inetpub\wwwroot\ai-service\
```

### 2. Configure Environment Variables
```powershell
# Copy and edit .env file
copy .env.example .env

# Edit .env with your settings:
# - OPENAI_API_KEY=your_azure_openai_key
# - OPENAI_BASE_URL=https://your-azure-openai-endpoint
# - DATABASE_URL=postgresql://user:pass@host:5432/dbname
```

### 3. Create IIS Site

1. **Open IIS Manager**
2. **Add New Site**:
   - Site name: `SkillMatrix-AI`
   - Physical path: `C:\inetpub\wwwroot\ai-service`
   - Binding:
     - Type: `https`
     - Host name: `myskilllist-ngeteam-ai.allianceitsc.com`
     - SSL Certificate: Select your certificate

3. **Configure Application Pool**:
   - .NET CLR version: `No Managed Code`
   - Managed pipeline mode: `Integrated`
   - Identity: `ApplicationPoolIdentity` or custom account with permissions

### 4. Set Folder Permissions
```powershell
# Grant permissions to IIS_IUSRS
icacls "C:\inetpub\wwwroot\ai-service" /grant "IIS_IUSRS:(OI)(CI)F" /T

# Create logs directory
mkdir C:\inetpub\wwwroot\ai-service\logs
icacls "C:\inetpub\wwwroot\ai-service\logs" /grant "IIS_IUSRS:(OI)(CI)F" /T
```

### 5. Verify web.config
Ensure `web.config` points to correct Python path:
```xml
<httpPlatform processPath="C:\Python311\python.exe"
              arguments="-m uvicorn main:app --host 0.0.0.0 --port %HTTP_PLATFORM_PORT%"
              ...>
```

### 6. Test the Deployment

1. **Browse to**: `https://myskilllist-ngeteam-ai.allianceitsc.com/health`
2. **Expected response**:
   ```json
   {
     "status": "healthy",
     "version": "0.1.0",
     "api_ready": true
   }
   ```

3. **Check V2 API**: `https://myskilllist-ngeteam-ai.allianceitsc.com/api/v2/health`

### 7. Update Backend Configuration

Update Backend's `appsettings.Production.json`:
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

## Troubleshooting

### Check Logs
```powershell
# Python stdout logs
type C:\inetpub\wwwroot\ai-service\logs\python-stdout.log

# IIS logs
type C:\inetpub\logs\LogFiles\W3SVC1\*.log
```

### Common Issues

1. **500.0 Error**:
   - Check Python path in web.config
   - Verify Python is installed correctly
   - Check folder permissions

2. **Module Not Found**:
   - Install dependencies: `pip install -r requirements.txt`
   - Check Python environment

3. **Database Connection Failed**:
   - Verify DATABASE_URL in .env
   - Check network connectivity to PostgreSQL

4. **API Key Errors**:
   - Verify OPENAI_API_KEY in .env
   - Check Azure OpenAI endpoint URL

### Restart Site
```powershell
# Restart application pool
appcmd stop apppool /apppool.name:"SkillMatrix-AI"
appcmd start apppool /apppool.name:"SkillMatrix-AI"

# Or restart IIS
iisreset
```

## Security Notes

1. **SSL/TLS**: Always use HTTPS in production
2. **API Keys**: Never commit .env to source control
3. **Firewall**: Ensure port 443 is open
4. **CORS**: Configure appropriate CORS policies
5. **Rate Limiting**: Consider implementing rate limiting

## Performance Tuning

### Application Pool Settings
- **Idle Timeout**: Set to `0` (Never timeout)
- **Maximum Worker Processes**: `1` (for Python FastAPI)
- **CPU Limit**: Adjust based on server capacity

### Python Optimization
```powershell
# Use production ASGI server settings
# In web.config, add workers:
arguments="-m uvicorn main:app --host 0.0.0.0 --port %HTTP_PLATFORM_PORT% --workers 4"
```

## Monitoring

### Health Check Endpoints
- `/health` - Basic health check
- `/api/v2/health` - V2 API with database check
- `/api/docs` - API documentation (disable in production)

### Logs Location
- Python stdout: `C:\inetpub\wwwroot\ai-service\logs\python-stdout.log`
- IIS logs: `C:\inetpub\logs\LogFiles\`

## Maintenance

### Update Deployment
```powershell
# Stop site
appcmd stop apppool /apppool.name:"SkillMatrix-AI"

# Update files
xcopy /s /e /y publish\ai-gen\* C:\inetpub\wwwroot\ai-service\

# Reinstall dependencies if requirements.txt changed
pip install -r requirements.txt --upgrade

# Start site
appcmd start apppool /apppool.name:"SkillMatrix-AI"
```

### Backup
```powershell
# Backup before updates
xcopy /s /e /y C:\inetpub\wwwroot\ai-service\* C:\backups\ai-service-%date%\
```
