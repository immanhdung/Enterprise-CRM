# Enterprise CRM – Disaster Recovery & Backup Guide

This document defines the disaster recovery policy, backup procedures, Point-in-Time Recovery (PITR) configuration, and non-destructive restore drill verification for Enterprise CRM.

---

## 1. Disaster Recovery Objectives

- **Recovery Point Objective (RPO)**: **< 5 minutes** (Maximum data loss window).
- **Recovery Time Objective (RTO)**: **< 30 minutes** (Maximum downtime window).
- **Integrity Requirement**: 100% cryptographic SHA-256 verification of backup archives before restoration.

---

## 2. Backup Architecture

### 2.1 Continuous Write-Ahead Log (WAL) Archiving – Supabase PITR
- The primary production database runs on Supabase PostgreSQL with Point-in-Time Recovery enabled.
- All database mutations write WAL records to encrypted cold storage continuously.
- Allows rolling back the entire database state to any specified second within the retention window (7 to 30 days depending on tier).

### 2.2 On-Demand & Scheduled Logical Snapshots
- Initiated via `/api/v1/admin/backups` or automated cron jobs.
- Output: Encrypted PostgreSQL dump (`.sql.gz` or custom format).
- Metadata: File size, row counts, creation timestamp, initiator email, and SHA-256 checksum.

---

## 3. Cryptographic Verification & Restore Drills

### 3.1 SHA-256 Checksum Calculation
When a backup is created, the system computes the SHA-256 digest:
$$\text{Checksum} = \text{SHA256}(\text{BackupPayload})$$
During restore, the archive is re-hashed. If the hash does not match, the restore is immediately aborted to prevent corrupted restoration.

### 3.2 Non-Destructive Restore Drill Workflow
To verify recoverability without disrupting live production:
1. Administrator navigates to **Disaster Recovery** $\rightarrow$ selects a backup record $\rightarrow$ clicks **Thử Nghiệm Phục Hồi (Drill)**.
2. The drill executes in an isolated sandbox database target (`SandboxVerification` or `StagingDrill`).
3. Steps performed:
   - Archive integrity check.
   - Schema and table creation.
   - Data restoration and foreign key constraint verification.
   - Checksum audit and execution duration logging.
4. An immutable report is saved in `restore_records`.

---

## 4. Emergency Disaster Recovery Playbook

If a critical database incident occurs (e.g. data corruption, accidental drop, ransomware):

### Step 1: Incident Assessment & Freeze
1. Declare critical incident.
2. Put the frontend into maintenance mode:
   ```nginx
   # Redirect all traffic to maintenance.html
   return 503;
   ```
3. Terminate active application backend pools to prevent further writes.

### Step 2: Point-in-Time Restore via Supabase Console / CLI
1. Open Supabase Management Console $\rightarrow$ **Database** $\rightarrow$ **Backups** $\rightarrow$ **Point in Time**.
2. Select target timestamp (1 minute prior to the corruption incident, e.g. `2026-11-26T21:45:00Z`).
3. Trigger Point-in-Time Restore to a clone instance or target branch.
4. Await completion notification.

### Step 3: Data Sanity Verification
Run the verification queries:
```sql
SELECT count(*) FROM users;
SELECT count(*) FROM leads;
SELECT count(*) FROM deals;
SELECT count(*) FROM contracts;
SELECT count(*) FROM payments;
```
Verify that counts match pre-incident audit benchmarks.

### Step 4: Route Traffic & Resume
1. Update `ConnectionStrings__DefaultConnection` to point to the restored database instance.
2. Start backend API services.
3. Verify `/api/v1/health` and `/api/v1/health/database`.
4. Remove maintenance mode from frontend proxy.
5. Log incident post-mortem in `audit_logs`.
