# Enterprise CRM – System Administration Guide

This guide provides operational procedures for Enterprise Administrators and Governance Officers managing the Enterprise CRM platform.

---

## 1. User & Account Management (`/admin/users`)

### 1.1 Creating New Users
1. Navigate to **Administration & Governance** $\rightarrow$ **Users & Accounts**.
2. Click **+ Thêm Người Dùng Mới**.
3. Fill in required fields:
   - **Họ & Tên**: User's first and last name.
   - **Email công việc**: Primary corporate email (used as unique login username).
   - **Mật khẩu khởi tạo**: Initial password (minimum 8 characters, must meet complexity requirements).
   - **Phòng ban & Đội ngũ**: Assign to primary organizational unit.
   - **Quản lý trực tiếp (Manager)**: Select reporting manager.
   - **Vai trò hệ thống**: Assign one or more roles (`Admin`, `Sales`, `Manager`, `Accountant`, `CEO_BOD`, or custom roles).
4. Click **Tạo Tài Khoản**. An active account record is created with default status `Active`.

### 1.2 User Status Transitions
- **Active**: User can authenticate and perform authorized actions.
- **Suspended**: Temporarily locks the user out (e.g., during internal audits or leave).
- **Deactivated**: Permanently disables login while preserving historical audit, deal, and financial links.

### 1.3 Administrative Password Reset
When an employee forgets credentials:
1. Select user $\rightarrow$ click **Đặt lại mật khẩu**.
2. Enter a new temporary password.
3. Upon saving, existing active sessions are terminated, and the user must use the new credentials.

### 1.4 Active Session Monitoring & Revocation
Under user details $\rightarrow$ **Active Sessions**:
- Displays all currently active JWT sessions, client IP addresses, and user-agent devices.
- Administrators can click **Thu hồi phiên** to immediately invalidate a session token remotely.

---

## 2. Organization Structure & Employee Movement (`/admin/organization`)

### 2.1 Viewing Organizational Hierarchy
The visual tree displays:
- **Departments**: Head of department, total employee headcount, sub-teams.
- **Teams**: Team leader, active team members.
- **Employees**: Position, email, reporting manager.

### 2.2 Employee Department / Team Transfer Workflow
When an employee changes roles or departments:
1. Click **Điều chuyển nhân sự**.
2. Select target employee, destination department, target team, and new reporting manager.
3. Enter mandatory **Lý do điều chuyển** (Audit requirement).
4. Verify checkbox **Bảo lưu dữ liệu lịch sử** (Ensures historical KPI evaluations, commission payouts, and contract assignments remain linked to past periods without disruption).
5. Click **Xác nhận điều chuyển**. An immutable log is written to `organization_change_histories`.

---

## 3. RBAC & Data Scope Management (`/admin/roles` & `/admin/permission-matrix`)

### 3.1 System Roles vs Custom Roles
- **Protected System Roles**: `Admin`, `Sales`, `Manager`, `Accountant`, `CEO_BOD` cannot be deleted or renamed to preserve system invariants.
- **Custom Roles**: Additional specialized roles (e.g., `Auditor`, `ProcurementOfficer`, `MarketingSpecialist`) can be created and assigned arbitrary permissions.

### 3.2 Granular Data Scopes
Every role-permission binding supports 5 data scopes:
| Data Scope | Description |
| :--- | :--- |
| **`Own`** | Access restricted strictly to records created by or assigned to the user. |
| **`Assigned`** | Access to records where the user is an assigned collaborator or sales owner. |
| **`Team`** | Access to all records owned by members of the user's immediate team. |
| **`Department`**| Access to all records across all teams within the user's department. |
| **`Company`** | Full visibility across the entire enterprise organization. |

---

## 4. Audit Log & Compliance Center (`/admin/audit-logs`)

### 4.1 Log Immutability
All CRUD operations, authentication attempts, batch imports, and financial adjustments are recorded in `audit_logs`. The table has no deletion or update endpoints.

### 4.2 Automated Secret Masking
Before audit details are persisted, the system scrubs sensitive keys:
- Keywords matched: `password`, `token`, `secret`, `apikey`, `salt`.
- Values are replaced with `********` to prevent credential leakage in audit trails.

### 4.3 Exporting Audit Trails
Click **Xuất Báo Cáo Audit (CSV)** to stream full filtered records for external compliance and security reviews.

---

## 5. Excel Import / Export Center (`/admin/data-management`)

### 5.1 Supported Entity Types
- **Leads**: Full name, company, phone, email, source, budget, priority.
- **Accounts**: Company name, tax code, industry, phone, email, address.
- **Contacts**: Full name, account, email, phone, position.
- **Deals**: Deal name, account, value, pipeline stage, probability.
- **Employees**: Full name, email, department, position, base salary.
- **Services**: Service name, code, category, unit, base price.

### 5.2 Transaction Safety & Validation
- **Template Download**: Always download the pre-formatted CSV template before uploading.
- **Preview & Duplicate Check**: Preview records before commit. Existing emails or tax codes are detected.
- **Atomic Rollback**: If fatal validation errors occur, the transaction rolls back, and error rows are logged in `error_log_json`.

---

## 6. API & Webhook Management (`/admin/integrations`)

### 6.1 Cryptographic API Keys
1. Click **+ Tạo API Key Mới**.
2. Specify key name, permitted scopes (`crm.read`, `crm.write`, `crm.admin`), and rate limit (requests per minute).
3. The system generates a cryptographically random key formatted as:
   `crm_live_7a9f8b2c4d6e1a0b3f5c7e9a1b3d5f7a`
4. **Copy the key immediately**. The backend computes and stores only the SHA-256 hash. The plaintext key is never recoverable after closing the dialog.

### 6.2 Webhooks & HMAC Verification
1. Register outbound webhook URL and subscribe to business events:
   - `LeadCreated`, `DealWon`, `ContractSigned`, `PaymentConfirmed`, `InvoiceIssued`.
2. Every outgoing HTTP POST includes a cryptographic HMAC-SHA256 signature in header:
   `X-Hub-Signature-256: sha256=<hex_digest>`
3. Receiving applications verify the signature using the shared webhook secret to guarantee authenticity.

---

## 7. Disaster Recovery & Backup Management (`/admin/backups`)

- **Supabase Continuous WAL (PITR)**: Supabase continuously archives write-ahead logs, enabling point-in-time restore to any second within the retention window.
- **On-Demand Backups**: Click **Tạo Bản Sao Lưu Mới** to take an instant logical snapshot with calculated SHA-256 checksum.
- **Restore Drills**: Click **Thử Nghiệm Phục Hồi (Drill)** to simulate a non-destructive restoration against a sandbox target, verifying backup integrity without touching production.

---

## 8. System Configurations (`/admin/settings`)

Centralized parameters organized by category:
- **General**: Company name, base currency (`VND`), timezone (`Asia/Ho_Chi_Minh`), date format.
- **CRM**: Max active leads per rep (`50`), stagnant lead threshold (`3 days`), stagnant deal threshold (`14 days`).
- **Security**: Password expiry, minimum length (`8`), session timeout (`60 minutes`), max lockout attempts (`5`).
- **Notification**: Real-time dispatch, quiet hours, email summaries.
- **Integration**: Webhook retries (`3`), external provider timeouts (`10 seconds`).

Every change creates a versioned entry in `configuration_histories`.
