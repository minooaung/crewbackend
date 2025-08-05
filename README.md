# CrewApp Backend

An ASP.NET Core Web API backend for managing users, roles, and organisations — built with Entity Framework Core and SQL Server.

## ✅ Features

- 🔐 Secure password handling using `IPasswordHasher`
- 👥 User & Role management (Admin, Employee)
- 🏢 Organisation support
- 📦 Seeded initial data (including default Admin)
- 🧼 Clean project structure and automation scripts for easy setup

---

## 🚀 Setup Instructions

### 1. Requirements

- .NET 8 SDK
- Ensure you have SQL Server installed.
- SQL Server (Developer or Express edition) for Windows
- Ensure you have a valid connection string for SQL Server in `appsettings.json`

---

## 🖥️ Windows Setup (PowerShell)

Open PowerShell as **Administrator**, navigate to the project directory, and run:
Run the `setup.ps1` script

```powershell
Set-ExecutionPolicy Unrestricted -Scope Process
./setup.ps1
```

This script (setup.ps1) will:

- Delete any existing Migrations folder.
- Drop the existing database.
- Add the initial migration.
- Update the database.
- Run the application.

---

## 🖥️ MacOS/Linux Setup (Shell)

```shell
chmod +x setup.sh
./setup.sh
bash setup.sh
```

This script (setup.sh) will:

- Removes old migrations.
- Drop the existing database.
- Add the initial migration.
- Update the database.
- Run the application.

---

## Manual Setup (Optional)

If you prefer to set up the project manually instead of using an automated script, follow these steps:

### Configure the Connection String

- Update the appsettings.json file with your database connection settings:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CrewApp;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

- 💡 Adjust the server, database name, or authentication method as needed.

### Run the following setup commands

```
dotnet ef database drop --force --no-build
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

These commands will:

- Drop any existing database
- Create a new migration
- Apply the migration to generate the database schema
- Launch the application

---
