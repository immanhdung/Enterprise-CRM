# Enterprise CRM – API Documentation & Contract Specification

## 1. REST API Conventions & Global Standards

### 1.1 Base URL & Versioning
All endpoints are strictly versioned:
`https://api.crm.domain.com/api/v1`

### 1.2 Response Envelope
All API endpoints return a standardized JSON response:

```json
{
  "success": true,
  "data": { ... },
  "message": "Operation completed successfully.",
  "errors": null,
  "timestamp": "2026-11-15T11:45:00Z"
}
```

Paginated collections return:
```json
{
  "success": true,
  "data": {
    "items": [ ... ],
    "pageIndex": 1,
    "pageSize": 20,
    "totalCount": 142,
    "totalPages": 8,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "message": null,
  "errors": null,
  "timestamp": "2026-11-15T11:45:00Z"
}
```

Error response (RFC 7807):
```json
{
  "success": false,
  "data": null,
  "message": "Validation failed.",
  "errors": [
    "The 'phone' format is invalid.",
    "The 'email' is already registered with another account."
  ],
  "timestamp": "2026-11-15T11:45:00Z"
}
```

---

## 2. API Endpoints by Module

### 2.1 System Health & Diagnostics
- `GET /api/v1/health`
  - Health check endpoint returning API status, uptime, and database connectivity.
- `GET /api/v1/health/database`
  - Runs a test query (`SELECT 1`, PostgreSQL version) on Supabase PostgreSQL and returns latency and connection state.

### 2.2 Authentication & User Profile (`/api/v1/auth`)
- `POST /api/v1/auth/login` – Authenticates user credentials, returns JWT `accessToken` and `refreshToken`.
- `POST /api/v1/auth/refresh-token` – Issues fresh access token using valid refresh token.
- `POST /api/v1/auth/logout` – Revokes the active refresh token.
- `GET /api/v1/auth/me` – Returns currently authenticated user details, permissions, department, and role.
- `PUT /api/v1/auth/change-password` – Secure password update with current password verification.

### 2.3 Lead Management (`/api/v1/leads`)
- `GET /api/v1/leads` – Paginated list with filters (`status`, `sourceId`, `assignedEmployeeId`, `isKeyAccount`, `search`).
- `GET /api/v1/leads/{id}` – Lead details, full activity timeline, notes.
- `POST /api/v1/leads` – Create lead with duplicate check (phone/email match).
- `PUT /api/v1/leads/{id}` – Update lead info.
- `DELETE /api/v1/leads/{id}` – Soft delete lead.
- `POST /api/v1/leads/{id}/convert` – Atomically convert qualified Lead into Account, Contact, and Deal.
- `POST /api/v1/leads/{id}/assign` – Assign lead to employee (manual or auto round-robin rule).
- `POST /api/v1/leads/{id}/activities` – Add call, note, or follow-up task.
- `POST /api/v1/leads/webhook/{source}` – Ingest external leads from Facebook, Google Ads, TikTok, or landing forms.
- `POST /api/v1/leads/import` – Bulk upload Excel/CSV leads with schema validation.
- `GET /api/v1/leads/export` – Export filtered leads to XLSX.

### 2.4 Customers & Contacts (`/api/v1/accounts`, `/api/v1/contacts`)
- `GET /api/v1/accounts` – Paginated business accounts.
- `GET /api/v1/accounts/{id}` – 360-degree account profile (associated contacts, deals, contracts, tickets, revenue).
- `POST /api/v1/accounts` – Create account with tax code validation.
- `PUT /api/v1/accounts/{id}` – Update account.
- `GET /api/v1/contacts` – Filtered list of contacts.
- `POST /api/v1/contacts` – Create contact associated with an account.
- `PUT /api/v1/contacts/{id}` – Update contact.

### 2.5 Deals & Sales Pipeline (`/api/v1/deals`, `/api/v1/pipeline-stages`)
- `GET /api/v1/pipeline-stages` – Retrieve stages ordered by index with win probability and deal count.
- `GET /api/v1/deals/kanban` – Grouped deals by stage for drag-and-drop Kanban interface.
- `GET /api/v1/deals` – Paginated list of deals.
- `GET /api/v1/deals/{id}` – Deal detail, related quotes, tasks, files.
- `POST /api/v1/deals` – Create new deal opportunity.
- `PUT /api/v1/deals/{id}/stage` – Transition deal to new stage with drag-and-drop support.
- `PUT /api/v1/deals/{id}/close` – Mark deal as Won or Lost (with mandatory lost reason).

### 2.6 Activities, Tasks & Appointments (`/api/v1/tasks`, `/api/v1/appointments`)
- `GET /api/v1/tasks` – Paginated tasks by status, due date, employee, or deal.
- `POST /api/v1/tasks` – Create new task with due date reminder.
- `PUT /api/v1/tasks/{id}/status` – Mark task completed, in-progress, or cancel.
- `GET /api/v1/appointments` – Calendar view events for meetings.
- `POST /api/v1/appointments` – Schedule customer appointment.

### 2.7 Services, Quotations & Contracts (`/api/v1/services`, `/api/v1/quotations`, `/api/v1/contracts`)
- `GET /api/v1/services` – Active service catalog with tiered pricing and VAT rate.
- `POST /api/v1/services` – Create service item.
- `GET /api/v1/quotations` – List quotations.
- `POST /api/v1/quotations` – Generate quotation from Deal data with line items and discounts.
- `POST /api/v1/quotations/{id}/approve` – Manager approval workflow.
- `GET /api/v1/quotations/{id}/pdf` – Render and download branded quotation PDF.
- `GET /api/v1/contracts` – List contracts with status (`Draft`, `Signed`, `Active`, etc.).
- `POST /api/v1/contracts` – Generate contract from approved quotation.
- `POST /api/v1/contracts/{id}/esign/send` – Dispatch for electronic signature via adapter.
- `POST /api/v1/contracts/{id}/appendices` – Add contract appendix.

### 2.8 Projects, Invoices & Debt (`/api/v1/payment-schedules`, `/api/v1/payments`, `/api/v1/invoices`, `/api/v1/finance`)
- **Payment Schedules (`/api/v1/payment-schedules`)**:
  - `GET /api/v1/payment-schedules/contract/{contractId}` – Retrieve installment schedule for a contract.
  - `POST /api/v1/payment-schedules/generate` – Auto-calculate and create multi-installment milestone schedules (e.g. 30/40/30) enforcing 100% total allocation.
  - `POST /api/v1/payment-schedules` – Create individual payment schedule milestone.
  - `PUT /api/v1/payment-schedules/{id}` – Update milestone details, due date, or notes.
  - `DELETE /api/v1/payment-schedules/{id}` – Remove unpaid milestone schedule.
- **Payment Management (`/api/v1/payments`)**:
  - `GET /api/v1/payments` – Paginated payment transactions with search, date, contract, and status filters.
  - `GET /api/v1/payments/{id}` – Payment details, verification metadata, and associated invoice.
  - `POST /api/v1/payments` – Record incoming payment linked to contract/milestone.
  - `POST /api/v1/payments/{id}/confirm` – Verify and confirm payment (transitions to `Confirmed`, updates schedule `amountPaid` and contract cash collection).
  - `POST /api/v1/payments/{id}/reverse` – Reverse payment with mandatory audit reason (transitions to `Reversed`, decrements schedule `amountPaid`, re-opens milestones).
  - `GET /api/v1/payments/contract/{contractId}/summary` – Contract financial summary (Total Value, Paid, Outstanding, Invoiced, Schedules).
- **Electronic Invoicing (`/api/v1/invoices`)**:
  - `GET /api/v1/invoices` – List e-invoices with search, provider, and status filters.
  - `GET /api/v1/invoices/{id}` – E-invoice details with line items, tax rate, and lookup credentials.
  - `POST /api/v1/invoices` – Issue VAT electronic invoice with automated series (`1C26TBB`) and provider lookup code.
  - `GET /api/v1/invoices/{id}/pdf` – Download or stream rendered PDF invoice.
  - `POST /api/v1/invoices/{id}/sync` – Synchronize status with external provider (MISA, VNPT, Viettel).
  - `POST /api/v1/invoices/{id}/cancel` – Cancel published e-invoice with required cancellation reason.
  - `GET /api/v1/invoices/requests` – List sales invoice requests with status filter.
  - `POST /api/v1/invoices/requests` – Submit invoice request for accountant review.
  - `POST /api/v1/invoices/requests/{id}/approve` – Approve request and issue invoice.
  - `POST /api/v1/invoices/requests/{id}/reject` – Reject request with justification reason.
- **Finance Analytics, Receivables & Reconciliation (`/api/v1/finance`)**:
  - `GET /api/v1/finance/revenue` – Multi-dimensional revenue analytics (Timeline Daily/Monthly/Quarterly/Yearly, Customer, Contract, Sales Employee, Service Category).
  - `GET /api/v1/finance/receivables` – Receivables items with aging brackets (`Current`, `1-7 Days Overdue`, `8-30 Days Overdue`, `31-60 Days Overdue`, `>60 Days Overdue`).
  - `GET /api/v1/finance/receivables/summary` – Aggregate debt aging summary cards for financial KPI tracking.
  - `GET /api/v1/finance/collection-logs` – Log history of debt collection efforts (phone calls, email reminders, formal notices, direct meetings).
  - `POST /api/v1/finance/collection-logs` – Record customer collection activity with follow-up date and outcome status.
  - `GET /api/v1/finance/reconciliation` – List executed cross-reconciliation records and discrepancy reports.
  - `POST /api/v1/finance/reconciliation/run` – Execute cross-reconciliation engine (Contracts vs. Invoiced vs. Collected Cash).
  - `POST /api/v1/finance/reconciliation/items/{id}/resolve` – Authorized discrepancy resolution (`ConfirmAdjustment` or `Dismiss`) with mandatory audit notes.

### 2.9 Performance, Commission & Payroll (`/api/v1/commissions`, `/api/v1/payroll`, `/api/v1/kpis`)
- `GET /api/v1/kpis` – Sales target vs. achieved metrics per employee, team, department.
- `GET /api/v1/commissions` – Commission logs linked to signed deals and collected payments.
- `POST /api/v1/commissions/calculate` – Trigger commission calculation engine for a given period.
- `GET /api/v1/payroll/periods` – List payroll periods.
- `POST /api/v1/payroll/periods/{id}/generate` – Generate monthly payroll records.
- `POST /api/v1/payroll/periods/{id}/approve` – Multi-level approval (Manager -> CEO).

### 2.10 AI Assistant & Analytics (`/api/v1/ai`)
- `POST /api/v1/ai/chat` – Conversational co-pilot query against authorized business data.
- `GET /api/v1/ai/forecast` – Revenue and sales conversion forecast based on historical trends.
- `GET /api/v1/ai/anomalies` – Detect abnormal changes in sales velocity, stagnant deals, overdue debt.
- `GET /api/v1/ai/summary` – Generate executive summaries (Daily, Weekly, Monthly).

### 2.11 System Administration (`/api/v1/admin`)
- `GET /api/v1/admin/users` – User management and role assignment.
- `GET /api/v1/admin/departments` – Department hierarchy management.
- `GET /api/v1/admin/teams` – Team management.
- `GET /api/v1/admin/audit-logs` – Audit trail search and export.
- `GET /api/v1/admin/integrations` – Status of third-party API adapters (E-Invoice, E-Sign, Webhooks, AI).
