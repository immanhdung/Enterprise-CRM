# Enterprise CRM – Setup Guide & Environment Checklist

## 1. Prerequisites Checklist

Ensure the following tools are installed on the development host:

| Tool | Minimum Version | Verified Version on Machine |
| :--- | :--- | :--- |
| **.NET SDK** | .NET 10.0 | 10.0.404 (`C:\Program Files\dotnet\sdk`) |
| **Node.js** | 20.x or 22.x LTS | v22.17.1 |
| **Package Manager** | npm 10.x or pnpm 10.x | pnpm 10.33.2 / npm 10.8.2 |
| **Git** | 2.40+ | 2.46.0 |
| **PostgreSQL Host** | Supabase PostgreSQL 15+ | `db.ehmjgxwrmzlccnivqqxb.supabase.co:5432` |

---

## 2. Supabase PostgreSQL Configuration

The CRM uses Supabase PostgreSQL as its persistent data store.

### Connection Parameters
- **Host**: `db.ehmjgxwrmzlccnivqqxb.supabase.co`
- **Port**: `5432`
- **Database**: `postgres`
- **Username**: `postgres`
- **SSL Mode**: `Require`
- **Password**: Provided via environment variable or local config (`SUPABASE_DB_PASSWORD`)

### Connection String Template
```
Host=db.ehmjgxwrmzlccnivqqxb.supabase.co;Port=5432;Database=postgres;Username=postgres;Password={SUPABASE_DB_PASSWORD};SSL Mode=Require;Trust Server Certificate=true;
```

### Security Rules
- **Never commit passwords or secrets to Git**: `appsettings.Development.json` with live passwords is included in `.gitignore`.
- Provide the database password securely using one of:
  1. Environment variable: `SUPABASE_DB_PASSWORD=<your_password>`
  2. Full connection string: `ConnectionStrings__DefaultConnection="Host=...;Password=...;"`
  3. Local development file: `backend/src/EnterpriseCRM.API/appsettings.Development.json` (gitignored).

---

## 3. Backend Setup & Run Instructions

```bash
# Navigate to backend directory
cd backend

# Restore NuGet dependencies
dotnet restore EnterpriseCRM.sln

# Build the solution
dotnet build EnterpriseCRM.sln

# Run EF Core database migrations (once credentials provided)
dotnet ef database update --project src/EnterpriseCRM.Infrastructure --startup-project src/EnterpriseCRM.API

# Run the API project
cd src/EnterpriseCRM.API
dotnet run
```

The API will listen at:
- `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`
- Health check: `http://localhost:5000/api/v1/health`
- Database check: `http://localhost:5000/api/v1/health/database`

---

## 4. Frontend Setup & Run Instructions

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies (use pnpm or npm)
pnpm install

# Start Vite development server
pnpm run dev
```

The Frontend will be available at:
- `http://localhost:5173`
- Dev server proxies `/api` calls to `http://localhost:5000`.

---

## 5. Verification Checklist for Phase 1
- [x] Solution architecture and database schema documented in `docs/`.
- [x] Backend Clean Architecture projects initialized (.NET 10).
- [x] Frontend React 19 + TypeScript + Vite + Tailwind CSS initialized.
- [x] Serilog structured logging and Swagger/OpenAPI configured.
- [x] Health check endpoints implemented (`/api/v1/health`, `/api/v1/health/database`).
- [x] Frontend health-check dashboard and Enterprise CRM layout created.
- [x] Backend and Frontend builds succeed with 0 errors.
