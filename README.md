# Kisa School Management System

A WPF (.NET 8) desktop application for managing students, donors, exams, fees, and staff.

## Tech Stack
- **WPF** on **.NET 8 (Windows)**
- **MySQL/MariaDB**
- **MaterialDesignThemes**

## Getting Started
1. **Clone the repository**
2. **Restore NuGet packages**
3. **Configure database connection**
   - Update `App.config` with your MySQL credentials.
4. **Create the database**
   - Import your SQL dump into a database named `school_mgmt`.
5. **Run the app**
   - Open `KisaSchoolMangement.sln` in Visual Studio and run.

## Database Notes
Your SQL dump includes the core tables required for:
- Students, Donors, Teachers
- Exams, Subjects, Marks
- Attendance, Fees, Invoices
- Users/Roles

Ensure the database name is `school_mgmt` (as configured in `App.config`).

### Role Permissions Setup
To enable role-based permissions, import the schema file:

```
KisaSchoolMangement/Database/permissions_schema.sql
```

This will create `permissions` and `role_permissions` tables and seed common permissions.

## Recommended Improvements (Next Steps)
- Add automated tests (unit tests for services).
- Move secrets (DB credentials) to environment-specific configs.
- Add logging and error reporting.
- Add migration tooling (e.g., FluentMigrator or EF Core migrations).
