# Enterprise CRM – Development Roadmap

## Phase-by-Phase Plan

```mermaid
gantt
    title CRM Development Phases
    dateFormat  YYYY-MM-DD
    section Completed
    Phase 1: Project Foundation & Architecture        :done, p1, 2026-11-15, 1d
    Phase 2: Database, Identity & RBAC               :done, p2, 2026-11-16, 1d
    Phase 3: Core CRM - Leads, Accounts & Deals      :done, p3, 2026-11-16, 1d
    Phase 4: CPQ - Service Catalog, Quotes & Contracts:done, p4, 2026-11-16, 1d
    Phase 5: Project & Finance Management            :done, p5, 2026-11-17, 1d
    Phase 6: Sales Performance, KPIs & Payroll       :done, p6, 2026-11-26, 1d
    Phase 7: Automation & AI Intelligence            :done, p7, 2026-11-26, 1d
    section Next Phases
    Phase 8: System Governance & Administration      :p8, 2026-11-29, 2d
```

---

### Phase 1: Project Foundation (Completed ✅)
- Architectural analysis and technical specifications in `docs/`.
- Monorepo directory layout (`frontend/`, `backend/`, `database/`, `docs/`).
- ASP.NET Core 10 Web API Clean Architecture (`Domain`, `Application`, `Infrastructure`, `API`, `Tests`).
- React 19 + TypeScript + Vite + Tailwind CSS design system with Professional Blue theme.
- Health Check API (`/api/v1/health` and `/api/v1/health/database`) with Supabase PostgreSQL connection management.

### Phase 2: Database & Authentication (Completed ✅)
- EF Core migrations for Identity, Organization (`departments`, `teams`, `employees`), and RBAC (`roles`, `permissions`).
- JWT token generation, refresh token rotation, and password hashing (PBKDF2/SHA-512).
- Login, registration, and user session management in Frontend.
- Role-based access control and data-scoping middleware.

### Phase 3: Core CRM (Completed ✅)
- **Lead Management**: CRUD, sources (Ads/Web/Manual), round-robin & workload assignment, duplicate detection, conversion into Account/Contact/Deal.
- **Account & Contact Management**: 360-degree customer profile, interaction timeline.
- **Deal Management & Pipeline**: Dynamic Kanban board with `@dnd-kit`, stage drag-and-drop, deal probability, expected revenue.
- **Activities & Tasks**: Task scheduler, calendar appointments, overdue notifications.
- **Import/Export**: Excel/CSV bulk pipeline with schema validation.

### Phase 4: Quotation & Contract Management (CPQ) (Completed ✅)
- **Service Catalog**: Service categories, pricing matrix, code generator.
- **Quotations**: Line-item calculation with VAT, discounts, approval workflow, branded PDF export.
- **Contracts**: Template generation, contract appendices, lifecycle management.
- **Electronic Signature**: Provider abstraction (`IElectronicSignatureProvider`) and lifecycle tracking.

### Phase 5: Project & Finance Management (Completed ✅)
- **Contract Financial Management**: Multi-installment milestone schedules (e.g. 30/40/30) with configurable percentage validation (sum=100%).
- **Payment Management**: Transaction tracking, payment verification, reversal workflow with mandatory audit reasons. Strict accounting rule: only confirmed payments count toward collected cash.
- **Multi-Dimensional Revenue Analytics**: Incurred vs Invoiced vs Collected vs Receivables across time, customer, contract, sales employee, and service category.
- **Accounts Receivable & Debt Aging**: Aging brackets (Current, 1–7d, 8–30d, 31–60d, >60d overdue), customer debt details, debt collection activity logging.
- **Electronic Invoicing**: Provider abstraction (`IElectronicInvoiceProvider`, `MockElectronicInvoiceProvider`) for MISA/VNPT/Viettel formats, realistic VAT invoice generation (`INV-YYYYMMDD-XXXX`), series (`1C26TBB`), lookup code/URL, PDF preview, synchronization, and cancellation workflow.
- **Financial Reconciliation**: Cross-reconciliation engine comparing Contracts vs Invoiced vs Collected Cash, automated discrepancy detection, authorized resolution.
- **Frontend Pages**: Complete UI suite with 8 integrated pages under `/finance/*`.

### Phase 6: Sales Performance, KPIs, Commission & Payroll (Completed ✅)
- **Module A – KPI Management**: Configurable KPI definitions (`kpi_definitions`), target assignments (`kpis`), auto-calculation from CRM data, weighted blended score, evaluation history, and approval workflow with anti-self-approval protection.
- **Module B – Sales Performance Analytics**: Multi-dimensional metrics (Lead funnel, Deals Won/Lost, Win Rate, Contract Value vs Invoiced vs Collected Cash, Outstanding Receivables, Avg Deal Value), MoM comparison, employee rankings leaderboard, quarterly/monthly timeline trends.
- **Module C & D – Commission Policy & Engine**: Multi-method policy support (Collected Cash, Contract Value, Tiered Target Achievement rates based on KPI score e.g. <70%: 0%, 70-99%: 2%, 100-119%: 3%, >=120%: 5%, Customer Type new vs existing). Policy versioning, duplicate prevention, min threshold / max cap, manual adjustments with mandatory audit reason (`compensation_audit_logs`), anti-self-approval rule. Strict rule: for collected cash policies, only Confirmed payments count; pending/reversed payments are excluded.
- **Module E – Bonus Management**: Configurable bonus policies (`bonus_policies`: Individual, Team, New Customer Acquisition, Revenue Target, Special Achievement), bonus records (`bonus_records`), approval workflow.
- **Module F – Payroll Management**: Compensation periods (`payroll_periods`), monthly employee records (`payroll_records`: Base Salary, Approved Commission, Bonuses, Allowances, Deductions, Gross, Net), adjustments (`payroll_adjustments`), multi-level approval (Manager -> Director/CEO), finalization locking and payout mark (`Paid`).
- **Module G – Frontend Implementation**: Complete React 19 + TypeScript + Vite suite with 13 dedicated pages under `/performance/*`, `/commissions/*`, `/bonuses/*`, `/payroll/*`, full sidebar integration, and printable employee payslip statement.
- **Module H – Automated Testing**: Comprehensive unit tests covering weighted KPI blending, commission calculations, tiered rate selection, max cap enforcement, manual adjustments, gross/net aggregation, and anti-self-approval enforcement with 100% pass rate.

### Phase 7: Automation & AI Intelligence (Completed ✅)
- **Module A – Workflow Automation Engine**: Event-driven rules (`workflow_definitions`, `workflow_executions`, `workflow_execution_logs`), triggers (`LeadCreated`, `DealWon`, `PaymentOverdue`, `ContractSigned`), condition evaluation engine (AND/OR, Equals, GreaterThan, LessThan, Contains), automated actions (`AssignLead`, `CreateTask`, `SendNotification`, `SendEmail`).
- **Module B – Automatic Lead Distribution**: Multi-strategy router (`lead_assignment_rules`, `lead_assignment_logs`) supporting Round-Robin, Workload balancing (active leads capacity cap = 50 per rep), Territory, Source, and Service Specialization routing with real-time rep workload tracking.
- **Module C – Smart Alerts & Reminders**: Configurable alert triggers (`alert_rules`, `business_alerts`, `notification_preferences`) for stagnant leads (>3d), stagnant deals (>14d), expiring contracts (30d notice), and overdue debts with 24-hour deduplication and multichannel preferences (InApp, Email, SMS).
- **Module D – AI Assistant (Co-pilot)**: Conversational copilot (`ai_conversations`, `ai_conversation_messages`, `ai_provider_configurations`, `ai_usage_logs`) with global floating widget (`FloatingAiWidget.tsx`), dedicated chat workspace (`/ai-assistant`), token usage and cost tracking, configurable provider adapter (`IAiProvider`) supporting Google Gemini & OpenAI with robust domain-heuristic fallback.
- **Module E – Natural Language Business Query Engine**: Semantic query parser translating natural language queries in Vietnamese and English into strongly-typed EF Core LINQ aggregates (Revenue, Top Sales Leaderboard, Overdue Receivables, Pipeline Stage Breakdown, Lead Sources) with zero raw dynamic SQL execution.
- **Module F – AI Business Analytics & Forecasting**: Sales performance SWOT analysis, pipeline health score (0-100), linear regression revenue forecasting ($RunRate + \sum (DealAmount \times Probability)$) with 85% confidence interval bounds over 7/30/90/365-day horizons, statistical anomaly detection (revenue drops, lost deal spikes, stagnant clusters, payment delays), and actionable business recommendations.
- **Module G – Automated Management Reporting**: Daily, Weekly, and Monthly management reports (`scheduled_reports`, `generated_reports`) summarizing revenue, collected cash, pipeline metrics, and top performer rankings.
- **Module H – Role-Based Analytical Dashboards**: Dedicated UI views for Sales Rep (`/dashboard/sales`), Sales Manager (`/dashboard/manager`), and Executive C-Level (`/dashboard/executive`) with data-scoping.
- **Module I – Frontend Implementation**: Complete React 19 + TypeScript + Vite suite with 10 feature pages across `/automation/*`, `/ai-assistant/*`, `/ai-analytics/*`, `/reports`, `/dashboard/*`, and global floating AI widget.
- **Module J – Comprehensive Verification**: 10 unit tests for condition evaluation, round-robin, capacity limits, stagnant thresholds, regression bounds, anomaly deviations, and business query routing with 100% pass rate (31/31 passed across the solution).

### Phase 8: System Governance, Administration & Hardening (Upcoming)
- Role-based access control (RBAC) management UI, user & department hierarchy editor, comprehensive audit log search/export, containerization, and final security audit.
