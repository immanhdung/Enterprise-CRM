# Enterprise CRM – Service Business Management Platform

An enterprise-grade, full-stack Customer Relationship Management (CRM) platform designed specifically for service-based businesses (digital agencies, consulting, professional services, B2B technology providers).

## Architecture & Technology Stack

- **Frontend**: React 19, TypeScript, Vite, Tailwind CSS, Lucide Icons, React Router v7, TanStack Query, Zustand, React Hook Form, Zod, Recharts, @dnd-kit.
- **Backend**: ASP.NET Core Web API (.NET 10), C#, Entity Framework Core 10, Npgsql PostgreSQL Provider, Serilog, Swagger/OpenAPI, SignalR, Hangfire.
- **Database**: Supabase PostgreSQL with SSL Mode=Require.

## Documentation
- [System Architecture](file:///docs/architecture.md)
- [Database Schema & ERD](file:///docs/database-schema.md)
- [API Documentation](file:///docs/api-documentation.md)
- [Setup & Environment Guide](file:///docs/setup-guide.md)
- [Development Roadmap](file:///docs/development-roadmap.md)

## Quick Start

### Backend
```bash
cd backend
dotnet restore EnterpriseCRM.sln
dotnet build EnterpriseCRM.sln
cd src/EnterpriseCRM.API
dotnet run
```

### Frontend
```bash
cd frontend
pnpm install
pnpm run dev
```
