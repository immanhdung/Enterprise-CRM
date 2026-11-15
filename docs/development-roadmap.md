# Enterprise CRM – Development Roadmap

## Phase-by-Phase Plan

```mermaid
gantt
    title CRM Development Phases
    dateFormat  YYYY-MM-DD
    section Foundation
    Phase 1: Project Foundation & Architecture       :done, p1, 2026-11-15, 1d
    section Core Infrastructure
    Phase 2: Database, Identity & RBAC              :active, p2, 2026-11-16, 2d
    section Core CRM
    Phase 3: Leads, Customers, Deals & Pipeline     :p3, 2026-11-18, 3d
    section Business Operations
    Phase 4: Quotations, Contracts, Projects, Billing :p4, 2026-11-21, 3d
    section Performance & Finance
    Phase 5: Commissions, KPIs & Payroll Processing  :p5, 2026-11-24, 2d
    section Intelligence & Automation
    Phase 6: Hangfire Automation & AI Co-pilot       :p6, 2026-11-26, 2d
    section Governance & System Admin
    Phase 7: Administration, Audit Logs & Integrations :p7, 2026-11-28, 2d
    section Hardening & Deployment
    Phase 8: Security, Performance, Docker & Launch  :p8, 2026-11-30, 2d
```

---

### Phase 1: Project Foundation (Current)
- Complete architectural analysis and technical specifications in `docs/`.
- Initialize monorepo directory layout (`frontend/`, `backend/`, `database/`, `docs/`).
- Initialize ASP.NET Core 10 Web API Clean Architecture (`Domain`, `Application`, `Infrastructure`, `API`, `Tests`).
- Initialize React 19 + TypeScript + Vite + Tailwind CSS design system with Professional Blue theme.
- Implement Health Check API (`/api/v1/health` and `/api/v1/health/database`) with Supabase PostgreSQL connection management.
- Verify compilation, Swagger interface, and frontend connectivity.

### Phase 2: Database & Authentication
- Execute EF Core migrations for Identity, Organization (`departments`, `teams`, `employees`), and RBAC (`roles`, `permissions`).
- Implement JWT token generation, refresh token rotation, and password hashing (PBKDF2/SHA-512).
- Build login, registration, and user session management in Frontend.
- Enforce role-based access control and data-scoping middleware.

### Phase 3: Core CRM
- **Lead Management**: CRUD, sources (Ads/Web/Manual), round-robin assignment, duplicate detection, conversion into Account/Contact/Deal.
- **Account & Contact Management**: 360-degree customer profile, interaction timeline.
- **Deal Management & Pipeline**: Dynamic Kanban board with `@dnd-kit`, stage drag-and-drop, deal probability, expected revenue.
- **Activities & Tasks**: Task scheduler, calendar appointments, overdue notifications.

### Phase 4: Business Operations (CPQ & Finance)
- **Service Catalog**: Service items, categories, pricing matrix.
- **Quotations**: Line-item calculation with VAT, discounts, approval workflow, PDF export.
- **Contracts**: Template generation, contract appendices, lifecycle management, electronic signature integration adapter.
- **Projects & Finance**: Milestone delivery tracking, payment schedule installments, cash collection, debt aging reports.
- **Invoicing**: E-invoice request workflow, provider adapters (MISA, VNPT, Viettel).

### Phase 5: Sales Performance & Payroll
- **KPI Management**: Revenue, deal volume, and lead generation targets vs. actuals.
- **Commission Engine**: Multi-tiered commission calculations based on revenue, collected cash, and service type.
- **Payroll**: Monthly payroll generation, adjustments (bonuses, penalties), approval workflow (Manager -> CEO).

### Phase 6: Automation & AI
- **Hangfire Background Engine**: Automated follow-up reminders, stagnant lead alerts, overdue debt notifications, scheduled reports.
- **AI Co-pilot**: Natural language queries over sales data, revenue forecasting, anomaly detection, executive summaries.

### Phase 7: System Administration & Integrations
- User & department administration, fine-grained permission matrix.
- Immutable audit logging and edit history.
- External webhook receivers and adapter management.
- Excel/CSV import and export pipelines.

### Phase 8: Finalization & Production Readiness
- Integration and unit test suites verification.
- Performance optimization (indexing, caching, bundle optimization).
- Containerization with `docker-compose.yml`.
- Deployment and operations guide.
