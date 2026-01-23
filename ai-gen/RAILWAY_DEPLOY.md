# Railway Deployment Guide - AI Service

## 🚀 Quick Deploy to Railway

### Prerequisites
- Railway account: https://railway.app
- GitHub repository connected to Railway

---

## Option 1: Deploy via Railway Dashboard (Recommended)

### Step 1: Create New Project
1. Go to https://railway.app/dashboard
2. Click **"New Project"**
3. Select **"Deploy from GitHub repo"**
4. Choose your repository
5. Select the `ai-gen` folder (or configure root path)

### Step 2: Configure Environment Variables
In Railway dashboard, add these environment variables:

```env
# Required - Azure OpenAI
OPENAI_API_KEY=your_azure_openai_api_key
OPENAI_BASE_URL=https://your-resource.openai.azure.com/openai/v1/
LLM_MODEL=gpt-4o

# Required - Database
DB_CONNECT_STRING={"ServerName":"your-db-host","CatalogName":"your_db_name","Username":"postgres","Password":"your_password","MaxPoolSize":1000}

# Optional
DEBUG=False
```

### Step 3: Deploy
1. Railway will automatically detect:
   - `Procfile` → Run command
   - `requirements.txt` → Install dependencies
   - `runtime.txt` → Python version
2. Click **"Deploy"**
3. Wait for build to complete

### Step 4: Get Public URL
1. Go to **Settings** → **Networking**
2. Click **"Generate Domain"**
3. Your AI service will be available at: `your-service.up.railway.app`

---

## Option 2: Deploy via Railway CLI

### Install Railway CLI
```bash
# Install via npm
npm install -g @railway/cli

# Or via cargo
cargo install railway-cli

# Or download binary from https://docs.railway.app/develop/cli
```

### Deploy Steps
```bash
# 1. Login to Railway
railway login

# 2. Initialize project (in ai-gen folder)
cd ai-gen
railway init

# 3. Link to existing project (optional)
railway link

# 4. Set environment variables
railway variables set OPENAI_API_KEY="your_key"
railway variables set OPENAI_BASE_URL="https://your-resource.openai.azure.com/openai/v1/"
railway variables set LLM_MODEL="gpt-4o"
railway variables set DB_CONNECT_STRING='{"ServerName":"host","CatalogName":"db","Username":"user","Password":"pass","MaxPoolSize":1000}'

# 5. Deploy
railway up

# 6. Open in browser
railway open
```

---

## Configuration Files Explained

### `Procfile`
```
web: uvicorn main:app --host 0.0.0.0 --port $PORT
```
- Tells Railway how to start your app
- `$PORT` is automatically provided by Railway

### `runtime.txt`
```
python-3.11.9
```
- Specifies Python version
- Railway will use this version to build

### `requirements.txt`
```
fastapi==0.128.0
uvicorn[standard]==0.40.0
...
```
- Lists all Python dependencies
- Railway installs these automatically

### `railway.json` (Optional)
```json
{
  "build": {
    "builder": "NIXPACKS"
  },
  "deploy": {
    "numReplicas": 1,
    "restartPolicyType": "ON_FAILURE"
  }
}
```
- Advanced Railway configuration
- Controls build and deployment behavior

---

## Database Configuration on Railway

### Option 1: Use External Database
Set `DB_CONNECT_STRING` environment variable with your existing PostgreSQL connection.

### Option 2: Use Railway PostgreSQL
1. In Railway dashboard, click **"New"** → **"Database"** → **"PostgreSQL"**
2. Railway will auto-create and set these variables:
   - `DATABASE_URL`
   - `PGHOST`, `PGPORT`, `PGDATABASE`, `PGUSER`, `PGPASSWORD`
3. Update your code to use these variables

Example code update:
```python
import os
import json

# Try Railway's DATABASE_URL first
database_url = os.getenv("DATABASE_URL")
if database_url:
    # Parse DATABASE_URL
    # postgres://user:pass@host:port/dbname
    pass
else:
    # Use DB_CONNECT_STRING
    db_config = json.loads(os.getenv("DB_CONNECT_STRING"))
```

---

## Environment Variables Reference

| Variable | Required | Description | Example |
|----------|----------|-------------|---------|
| `OPENAI_API_KEY` | ✅ | Azure OpenAI API key | `1QgbEPcnu5n...` |
| `OPENAI_BASE_URL` | ✅ | Azure OpenAI endpoint | `https://xxx.openai.azure.com/openai/v1/` |
| `LLM_MODEL` | ✅ | Model name | `gpt-4o` |
| `DB_CONNECT_STRING` | ✅ | Database connection JSON | `{"ServerName":"host",...}` |
| `DEBUG` | ❌ | Debug mode | `False` |
| `PORT` | ❌ | Port (auto-set by Railway) | `8000` |

---

## Testing Your Deployment

### 1. Health Check
```bash
curl https://your-service.up.railway.app/health
```

Expected response:
```json
{
  "status": "healthy",
  "version": "0.1.0",
  "api_ready": true
}
```

### 2. V2 API Health Check
```bash
curl https://your-service.up.railway.app/api/v2/health
```

Expected response:
```json
{
  "status": "healthy",
  "api_version": "v2",
  "database": "connected",
  "total_definitions": 589
}
```

### 3. Generate Questions
```bash
curl -X POST https://your-service.up.railway.app/api/v2/generate-questions \
  -H "Content-Type: application/json" \
  -d '{
    "question_type": ["Multiple Choice"],
    "language": "English",
    "number_of_questions": 2,
    "skills": [{
      "skill_id": "30000000-0000-0000-0000-000000000078",
      "skill_name": "Accessibility and inclusion",
      "skill_code": "ACIN"
    }],
    "target_proficiency_level": [2],
    "difficulty": "Medium"
  }'
```

---

## Updating Backend to Use Railway URL

After deployment, update your Backend's `appsettings.Production.json`:

```json
{
  "AiService": {
    "UseMock": false,
    "BaseUrl": "https://your-service.up.railway.app",
    "ModelName": "gpt-4o",
    "TimeoutSeconds": 120
  }
}
```

---

## Monitoring & Logs

### View Logs
```bash
# Via CLI
railway logs

# Or in Dashboard
# Go to your service → Deployments → Click on deployment → View logs
```

### Metrics
Railway dashboard provides:
- CPU usage
- Memory usage
- Network traffic
- Request count

---

## Troubleshooting

### Build Fails

**Error: Python version not found**
- Check `runtime.txt` has valid Python version
- Use: `python-3.11.9` or `python-3.12.0`

**Error: Module not found**
- Verify all dependencies in `requirements.txt`
- Check for typos in package names

### Runtime Errors

**Error: Cannot connect to database**
- Verify `DB_CONNECT_STRING` is correct
- Check database allows connections from Railway IPs
- Test connection manually

**Error: OpenAI API key invalid**
- Verify `OPENAI_API_KEY` in environment variables
- Check Azure OpenAI endpoint URL is correct
- Ensure key has not expired

### Port Issues

**Error: Port already in use**
- Railway sets `PORT` environment variable automatically
- Ensure your code uses `os.getenv("PORT", 8002)`
- Check `Procfile` uses `$PORT`

---

## Cost Optimization

### Free Tier Limits
- Railway offers $5 free credit per month
- After that, pay-as-you-go pricing

### Tips to Reduce Costs
1. **Use sleep mode**: Service sleeps after inactivity
2. **Optimize memory**: Reduce memory usage in code
3. **Cache responses**: Implement caching for repeated requests
4. **Use external database**: If you have existing database

---

## Security Best Practices

1. **Never commit `.env` file**
   - Add to `.gitignore`
   - Use Railway environment variables

2. **Rotate API keys regularly**
   - Change Azure OpenAI keys periodically
   - Update in Railway dashboard

3. **Use HTTPS only**
   - Railway provides HTTPS by default
   - Never use HTTP in production

4. **Implement rate limiting**
   - Protect your API from abuse
   - Use FastAPI middleware

---

## Custom Domain (Optional)

### Add Custom Domain
1. Go to **Settings** → **Networking**
2. Click **"Custom Domain"**
3. Add your domain: `ai.yourdomain.com`
4. Update DNS records as instructed
5. Railway handles SSL automatically

---

## CI/CD with Railway

Railway automatically deploys when you push to GitHub:

1. **Automatic deploys**: Push to `main` branch
2. **Preview deploys**: Create PR for preview URL
3. **Rollback**: Revert to previous deployment in dashboard

---

## Support & Resources

- **Railway Docs**: https://docs.railway.app
- **Railway Discord**: https://discord.gg/railway
- **Railway Status**: https://status.railway.app
- **API Documentation**: Your service's `/api/docs` endpoint

---

## Migration from IIS to Railway

If moving from IIS:

1. ✅ Remove `web.config` (not needed for Railway)
2. ✅ Keep `Procfile`, `requirements.txt`, `runtime.txt`
3. ✅ Update environment variables from `.env` to Railway
4. ✅ Test thoroughly before switching production traffic
5. ✅ Update Backend `BaseUrl` to Railway URL

---

**Status**: ✅ Ready for Railway Deployment

**Next Steps**:
1. Push code to GitHub
2. Connect to Railway
3. Set environment variables
4. Deploy and test
