# Enterprise CRM – System Security Audit & Compliance Report

This report documents the security architecture, cryptographic mechanisms, authorization controls, audit logging, and vulnerability mitigations implemented across the Enterprise CRM platform.

---

## 1. Security Architecture Summary

| Security Domain | Implementation Standard | Status |
| :--- | :--- | :--- |
| **Password Storage** | PBKDF2 with HMAC-SHA512 (100,000 iterations, 32-byte salt) | Compliant ✅ |
| **API Key Storage** | SHA-256 cryptographic hashing (Plaintext never stored) | Compliant ✅ |
| **Webhook Signatures**| HMAC-SHA256 (`X-Hub-Signature-256`) | Compliant ✅ |
| **Authentication** | JWT (HMAC-SHA256, 60-min expiry) + Refresh Token Rotation | Compliant ✅ |
| **Data Scope Control** | Multi-level data scoping (`Own`, `Assigned`, `Team`, `Dept`, `Company`) | Compliant ✅ |
| **Audit Log Scrubbing**| Automatic masking of sensitive keys (`password`, `token`, `secret`) | Compliant ✅ |
| **SQL Injection** | 100% Parameterized queries via EF Core LINQ | Zero Raw SQL ✅ |
| **Anti-Lockout** | Protected System Roles (`Admin`, `Sales`, `Manager`, etc.) | Protected ✅ |

---

## 2. Authentication & Credential Hardening

### 2.1 Password Hashing (PBKDF2/SHA-512)
Passwords are never stored in plaintext. Passwords are processed through `IPasswordHasher`:
- Algorithm: PBKDF2 (Password-Based Key Derivation Function 2).
- Pseudo-Random Function: HMAC-SHA512.
- Salt: 128-bit cryptographically secure random salt (`RNGCryptoServiceProvider`).
- Iteration Count: 100,000 rounds.

### 2.2 Account Lockout Protection
- After 5 consecutive failed login attempts (`FailedLoginAttempts >= 5`), the account is automatically locked out for 15 minutes (`LockoutEnd = DateTime.UtcNow.AddMinutes(15)`).
- Protects against brute-force and dictionary attacks.

---

## 3. Authorization & Granular RBAC

### 3.1 Role & Permission Model
- Every controller endpoint is guarded by `[Authorize]` and permission policies.
- Custom roles can be created, but protected system roles (`Admin`, `Sales`, `Manager`, `Accountant`, `CEO_BOD`) cannot be deleted or compromised.

### 3.2 Dynamic Data Scopes
Every permission binding enforces a data boundary:
- `Own`: Rep sees only their own assigned records.
- `Assigned`: User sees records where they are explicitly tagged.
- `Team`: Team leaders view records across their team.
- `Department`: Department heads view records across their department.
- `Company`: Executives and Admins view entire enterprise records.

---

## 4. Cryptographic API Keys & Webhooks

### 4.1 Zero-Plaintext API Key Storage
API keys generated for third-party systems follow format `crm_live_{hex32}`:
1. The plaintext key is shown to the user exactly once at creation.
2. The backend immediately computes the SHA-256 hash:
   $$\text{HashedSecret} = \text{SHA256}(\text{Key})$$
3. When authenticating an external request, incoming key is hashed and matched against `HashedSecret`. If the database is compromised, the actual API keys cannot be recovered.

### 4.2 Webhook Authenticity (HMAC-SHA256)
Outbound webhooks calculate a signature:
$$\text{Signature} = \text{HMAC-SHA256}(\text{PayloadJson}, \text{Secret})$$
Sent in header `X-Hub-Signature-256: sha256=<hex>`. Receiving servers verify this signature to prevent replay and man-in-the-middle attacks.

---

## 5. Audit Trail & Data Protection

### 5.1 Immutable Audit Logs
All modifications to Users, Roles, Deals, Contracts, Invoices, and Configurations write to `audit_logs`:
- User ID, Email, IP Address, User Agent, Timestamp, Action, Module, Success status.

### 5.2 Automated Secret Masking
The `AuditLogService` scrubs serialized JSON payloads:
```csharp
// Keys containing "password", "token", "secret", "apikey", "salt"
// are replaced with "********" prior to database insertion.
```
This guarantees credentials never leak into audit records.

---

## 6. Vulnerability Assessment & Compliance Checklist

- [x] **OWASP Top 10 A01: Broken Access Control**: Mitigated via RBAC & Data Scope filters.
- [x] **OWASP Top 10 A02: Cryptographic Failures**: Mitigated via PBKDF2-SHA512, SHA-256 API keys, HMAC-SHA256 webhooks, and TLS 1.3 requirement.
- [x] **OWASP Top 10 A03: Injection**: Mitigated via EF Core parameterized queries; natural language query engine translates to strongly-typed LINQ with zero raw SQL execution.
- [x] **OWASP Top 10 A04: Insecure Design**: Mitigated via anti-self-approval on KPI evaluations, commission payouts, and payroll finalization.
- [x] **OWASP Top 10 A05: Security Misconfiguration**: Default configuration values securely initialized; system role deletion prohibited.
- [x] **OWASP Top 10 A07: Identification and Authentication Failures**: Mitigated via lockout policies, JWT expiry, and remote session revocation.
- [x] **OWASP Top 10 A08: Software and Data Integrity Failures**: Backup archives verified via SHA-256 checksums before restore.
- [x] **OWASP Top 10 A09: Security Logging and Monitoring Failures**: Centralized immutable audit logs with correlation IDs and CSV export.
