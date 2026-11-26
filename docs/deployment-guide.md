# Enterprise CRM – Production Deployment Guide

This guide details the deployment of the Enterprise CRM platform for production environments, covering the ASP.NET Core 10 Web API backend, React 19 SPA frontend, Supabase PostgreSQL database, and environment configuration.

---

## 1. System Requirements

| Component | Minimum Specification | Recommended Production |
| :--- | :--- | :--- |
| **Backend Runtime** | .NET 10.0 SDK / ASP.NET Core Runtime | Ubuntu 22.04 LTS / Debian 12 Docker Container |
| **CPU / RAM** | 2 vCPU, 4 GB RAM | 4+ vCPU, 8+ GB RAM (per replica) |
| **Frontend Server** | Node.js 20+ (for build) / Nginx 1.24+ | Cloudflare CDN / Nginx Alpine Container |
| **Database** | PostgreSQL 15+ (Supabase Pro) | Dedicated Supabase Instance with PITR enabled |
| **Storage** | 20 GB SSD | 100+ GB NVMe SSD with automated scaling |

---

## 2. Environment Variables Configuration

Create an environment configuration file (`.env` or orchestrator secret store):

```ini
# ==========================================
# Database Connection (Supabase PostgreSQL)
# ==========================================
ConnectionStrings__DefaultConnection="Server=db.<ref>.supabase.co;Port=5432;Database=postgres;User Id=postgres;Password=<strong_password>;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=50;"

# ==========================================
# Security & JWT Tokens
# ==========================================
JwtSettings__Secret="<generate_cryptographically_secure_random_key_min_64_bytes>"
JwtSettings__Issuer="EnterpriseCRM.Security"
JwtSettings__Audience="EnterpriseCRM.Clients"
JwtSettings__TokenExpiryMinutes=60
JwtSettings__RefreshTokenExpiryDays=7

# ==========================================
# AI Provider Integration (Configurable)
# ==========================================
AiSettings__Provider="Gemini"
AiSettings__ApiKey="<your_google_ai_or_gemini_api_key>"
AiSettings__Model="gemini-1.5-flash"

# ==========================================
# Electronic Invoicing & E-Sign Adapters
# ==========================================
InvoiceSettings__Provider="Mock"
SignatureSettings__Provider="Mock"

# ==========================================
# Client & CORS
# ==========================================
Frontend__BaseUrl="https://crm.yourcompany.com"
```

---

## 3. Database Migration Execution

Before starting the web services, apply pending EF Core migrations:

```bash
cd backend/src/EnterpriseCRM.API
dotnet ef database update --connection "$ConnectionStrings__DefaultConnection"
```

Verify that all 64 tables and indexes are initialized.

---

## 4. Backend Container Deployment (Docker)

### Dockerfile (`backend/Dockerfile`)
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/EnterpriseCRM.API/EnterpriseCRM.API.csproj"
RUN dotnet publish "src/EnterpriseCRM.API/EnterpriseCRM.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "EnterpriseCRM.API.dll"]
```

### Build and Run
```bash
docker build -t enterprisecrm-api:v1.0.0 -f backend/Dockerfile backend/
docker run -d --name crm-backend -p 5000:5000 --env-file .env enterprisecrm-api:v1.0.0
```

---

## 5. Frontend Production Build & Nginx Hosting

### Build Frontend SPA Bundle
```bash
cd frontend
pnpm install --frozen-lockfile
pnpm run build
```

This compiles optimized assets into `frontend/dist/`.

### Nginx Configuration (`nginx.conf`)
```nginx
server {
    listen 80;
    server_name crm.yourcompany.com;
    root /usr/share/nginx/html;
    index index.html;

    # Gzip Compression
    gzip on;
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml;

    # Static assets caching
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # SPA routing fallback
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Reverse proxy to backend API
    location /api/ {
        proxy_pass http://crm-backend:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## 6. Health & Readiness Verification

Run automated health probes against the deployment:

1. **Liveness Probe**:
   ```bash
   curl -s http://localhost:5000/api/v1/health | jq .
   # Expected: {"status": "Healthy", "timestamp": "..."}
   ```

2. **Database Readiness Probe**:
   ```bash
   curl -s http://localhost:5000/api/v1/health/database | jq .
   # Expected: {"status": "Healthy", "database": "Connected", "provider": "Npgsql.EntityFrameworkCore.PostgreSQL"}
   ```

3. **Scalar API Documentation**:
   Navigate to `https://crm.yourcompany.com/scalar/v1` to verify all OpenAPI endpoints.
