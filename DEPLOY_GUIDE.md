# Hướng dẫn Deploy SkillMatrix lên IIS

## Domain Configuration
- **Frontend (UI):** https://myskilllist-ngeteam-ad.allianceitsc.com
- **Backend (API):** https://myskilllist-ngeteam-api.allianceitsc.com

## Yêu cầu hệ thống

### Server Requirements
- Windows Server 2016/2019/2022 hoặc Windows 10/11
- IIS 10.0+
- .NET 8.0 Runtime (ASP.NET Core Hosting Bundle)
- PostgreSQL Server (có thể ở máy khác)

### Cài đặt .NET 8.0 Hosting Bundle
1. Tải từ: https://dotnet.microsoft.com/download/dotnet/8.0
2. Chọn **ASP.NET Core Runtime** → **Hosting Bundle**
3. Cài đặt và restart IIS

### Bật IIS Features
Vào **Control Panel** → **Programs** → **Turn Windows features on or off**:
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

### Cài đặt URL Rewrite Module (cho Frontend)
1. Tải từ: https://www.iis.net/downloads/microsoft/url-rewrite
2. Cài đặt URL Rewrite 2.1

---

## Bước 1: Build ứng dụng

### Cách 1: Sử dụng script tự động
```batch
cd "e:\Ngệ Team\SkillMatrix"
publish.bat
```

### Cách 2: Build thủ công

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
Copy thư mục `dist\*` và `web.config` vào `C:\publish\skillmatrix-web`

---

## Bước 2: Cấu hình trước khi deploy

### 2.1 Cập nhật API URL cho Frontend

File `web\.env.production` đã được cấu hình sẵn:
```env
VITE_API_URL=https://myskilllist-ngeteam-api.allianceitsc.com/api
```

### 2.2 Cập nhật Connection String cho Backend

Sửa file `src\SkillMatrix.Api\appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_DB_SERVER;Database=SkillMatrix;Username=YOUR_USER;Password=YOUR_PASS"
  }
}
```

---

## Bước 3: Tạo IIS Sites

### 3.1 Tạo Application Pool

**Cho Backend API:**
1. Mở **IIS Manager**
2. Click phải **Application Pools** → **Add Application Pool**
3. Name: `SkillMatrixAPI`
4. .NET CLR Version: **No Managed Code**
5. Managed Pipeline Mode: **Integrated**
6. Click **OK**
7. Click phải pool vừa tạo → **Advanced Settings**
   - Start Mode: `AlwaysRunning`
   - Identity: `ApplicationPoolIdentity` (hoặc user có quyền)

**Cho Frontend:**
1. Tạo pool mới: `SkillMatrixWeb`
2. .NET CLR Version: **No Managed Code**
3. Managed Pipeline Mode: **Integrated**

### 3.2 Tạo Website/Application

**Cách A: Dùng 2 Sites riêng biệt (khuyến nghị)**

**Site 1 - Backend API:**
1. Click phải **Sites** → **Add Website**
2. Site name: `SkillMatrixAPI`
3. Application pool: `SkillMatrixAPI`
4. Physical path: `C:\inetpub\skillmatrix-api`
5. Binding:
   - Type: https
   - IP: All Unassigned
   - Port: 443
   - Host name: `myskilllist-ngeteam-api.allianceitsc.com`
   - SSL Certificate: Chọn certificate phù hợp

**Site 2 - Frontend:**
1. Click phải **Sites** → **Add Website**
2. Site name: `SkillMatrixWeb`
3. Application pool: `SkillMatrixWeb`
4. Physical path: `C:\inetpub\skillmatrix-web`
5. Binding:
   - Type: https
   - Port: 443
   - Host name: `myskilllist-ngeteam-ad.allianceitsc.com`
   - SSL Certificate: Chọn certificate phù hợp

---

**Cách B: Dùng 1 Site với Virtual Directory**

1. Tạo site chính cho Frontend ở port 80
2. Click phải site → **Add Application**
   - Alias: `api`
   - Application pool: `SkillMatrixAPI`
   - Physical path: `C:\inetpub\skillmatrix-api`

URL sẽ là:
- Frontend: `http://your-server/`
- API: `http://your-server/api/`

---

## Bước 4: Copy files

```batch
:: Copy API
xcopy /s /e /y "publish\api\*" "C:\inetpub\skillmatrix-api\"

:: Copy Frontend
xcopy /s /e /y "publish\web\*" "C:\inetpub\skillmatrix-web\"
```

### Tạo thư mục logs cho API
```batch
mkdir C:\inetpub\skillmatrix-api\logs
```

### Cấp quyền cho IIS
```batch
icacls "C:\inetpub\skillmatrix-api" /grant "IIS AppPool\SkillMatrixAPI":(OI)(CI)M
icacls "C:\inetpub\skillmatrix-web" /grant "IIS AppPool\SkillMatrixWeb":(OI)(CI)R
```

---

## Bước 5: Cấu hình CORS (Backend)

Nếu Frontend và API ở khác domain/port, cần cấu hình CORS trong `Program.cs`:

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

// Thêm trước app.MapControllers()
app.UseCors();
```

---

## Bước 6: Test

1. Mở browser, truy cập Frontend URL
2. Đăng nhập với: `admin@skillmatrix.com` / `admin123`
3. Kiểm tra Dashboard hiển thị đúng

### Kiểm tra API trực tiếp
```
https://myskilllist-ngeteam-api.allianceitsc.com/api/dashboard/overview
https://myskilllist-ngeteam-api.allianceitsc.com/swagger
```

---

## Troubleshooting

### Lỗi 500.19 - Configuration Error
- Kiểm tra URL Rewrite Module đã cài chưa
- Kiểm tra file web.config đúng format

### Lỗi 502.5 - ANCM Out-Of-Process Startup Failure
- Kiểm tra .NET 8.0 Hosting Bundle đã cài
- Restart IIS: `iisreset`
- Xem log trong `C:\inetpub\skillmatrix-api\logs\`

### Lỗi 404 khi refresh trang React
- Kiểm tra URL Rewrite đã cấu hình trong web.config
- Kiểm tra URL Rewrite Module đã cài

### Không kết nối được Database
- Kiểm tra firewall PostgreSQL port 5432
- Kiểm tra connection string đúng
- Test kết nối từ server đến DB

### Xem logs
```batch
:: API logs
type C:\inetpub\skillmatrix-api\logs\stdout*.log

:: Windows Event Viewer
eventvwr.msc
:: → Windows Logs → Application
```

---

## Cấu trúc thư mục sau khi deploy

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

## Cập nhật ứng dụng

1. Build mới: `publish.bat`
2. Stop site trong IIS
3. Copy files mới
4. Start site

Hoặc dùng script:
```batch
@echo off
iisreset /stop
xcopy /s /e /y "publish\api\*" "C:\inetpub\skillmatrix-api\"
xcopy /s /e /y "publish\web\*" "C:\inetpub\skillmatrix-web\"
iisreset /start
echo Done!
```

---

## HTTPS (Khuyến nghị cho Production)

1. Mua SSL certificate hoặc dùng Let's Encrypt
2. Trong IIS, thêm HTTPS binding với certificate
3. Cập nhật `.env.production`:
   ```env
   VITE_API_URL=https://your-domain.com/api
   ```
4. Redirect HTTP to HTTPS trong web.config