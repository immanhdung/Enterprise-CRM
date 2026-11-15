# Database Management & Schema Migrations

This directory contains the database migration scripts, initial seed fixtures, and ERD documentation for the Enterprise CRM.

## Structure
- `migrations/`: Contains raw SQL snapshots of EF Core migrations and schema modifications.
- `seeds/`: Initial system seeding scripts (system roles, default permissions, administrator profile).
- `documentation/`: Schema diagrams and data dictionaries.

## Supabase PostgreSQL Configuration
- **Host**: `db.ehmjgxwrmzlccnivqqxb.supabase.co`
- **Port**: `5432`
- **Database**: `postgres`
- **Username**: `postgres`
- **SSL Mode**: `Require`
- **Password**: Provided via environment variable `SUPABASE_DB_PASSWORD`.
