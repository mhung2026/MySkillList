# SkillMatrix Deployment Guide for IIS

## Domain Configuration
- **Frontend (UI):** https://myskilllist-ngeteam-ad.allianceitsc.com
- **Backend (API):** https://myskilllist-ngeteam-api.allianceitsc.com

## System Requirements

### Server Requirements
- Windows Server 2016/2019/2022 or Windows 10/11
- IIS 10.0+
- .NET 8.0 Runtime (ASP.NET Core Hosting Bundle)
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

---

## Step 1: Build the Application

### Option 1: Using automated script
```batch
cd "e:\Ngệ Team\SkillMatrix"
publish.bat
```

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

## Step 6: Test

1. Open browser, access Frontend URL
2. Login with: `admin@skillmatrix.com` / `admin123`
3. Verify Dashboard displays correctly

### Test API directly
```
https://myskilllist-ngeteam-api.allianceitsc.com/api/dashboard/overview
https://myskilllist-ngeteam-api.allianceitsc.com/swagger
```

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

### View logs
```batch
:: API logs
type C:\inetpub\skillmatrix-api\logs\stdout*.log

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
└── skillmatrix-web\
    ├── index.html
    ├── web.config
    └── assets\
        ├── index-*.js
        └── index-*.css
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
iisreset /stop
xcopy /s /e /y "publish\api\*" "C:\inetpub\skillmatrix-api\"
xcopy /s /e /y "publish\web\*" "C:\inetpub\skillmatrix-web\"
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
