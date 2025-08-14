# CrewApp Backend

A modern, enterprise-grade ASP.NET Core Web API backend for managing organizations and their crew members. Built with clean architecture principles using Entity Framework Core and SQL Server.

## Key Features

### Authentication & Security
- Cookie-based authentication with secure session management
- Role-based authorization (Admin, Employee)
- Secure password handling using ASP.NET Core's `IPasswordHasher`
- HTTPS redirection and secure cookie policies
- CORS configuration for frontend integration

### User & Organization Management
- Complete user lifecycle management (CRUD operations)
- Organization management with multi-user support
- Role-based access control (RBAC)
- Soft delete functionality for data integrity
- Email uniqueness validation and conflict resolution

### Architecture & Design Patterns
- Clean Architecture with separation of concerns
- Repository pattern with Entity Framework Core
- SOLID principles implementation
- AutoMapper for object mapping
- Dependency Injection for loose coupling
- Custom exception handling middleware
- DTOs for request/response handling

### Data & Persistence
- Entity Framework Core with SQL Server
- Code-first migrations
- Database seeding with initial data
- Efficient query patterns
- Soft delete implementation for data integrity

### API Features
- RESTful API design
- Swagger/OpenAPI documentation
- Custom model validation
- Pagination support
- Error handling middleware
- Standardized API responses

### Dashboard & Analytics
- Organization growth tracking
- User role distribution analytics
- Quick action capabilities
- Real-time statistics

### Development & DevOps
- Automated setup scripts for Windows and Unix systems
- Environment-specific configurations
- Development and production environment separation
- Comprehensive logging system

## Technical Stack

- **Framework**: ASP.NET Core 8
- **ORM**: Entity Framework Core
- **Database**: SQL Server
- **Authentication**: Cookie-based with ASP.NET Core Identity
- **Documentation**: Swagger/OpenAPI
- **Testing**: (Prepared for unit tests and integration tests)
- **Validation**: Custom attributes and model validation
- **Mapping**: AutoMapper
- **Security**: HTTPS, secure cookies, password hashing

## Architecture

The project follows a clean architecture pattern with the following layers:

```
CrewBackend/
├── Controllers/         # API endpoints and route handling
├── Services/           # Business logic and service implementations
├── Models/             # Domain entities and models
├── DTOs/               # Data Transfer Objects for API contracts
├── Data/               # Database context and configurations
├── Exceptions/         # Custom exception handling
├── Middlewares/        # Custom middleware components
├── Helpers/            # Utility and helper classes
└── MappingProfiles/    # AutoMapper configuration
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (Developer or Express edition)
- IDE: Visual Studio 2022 or VS Code

### Quick Setup

#### Windows (PowerShell)

```powershell
Set-ExecutionPolicy Unrestricted -Scope Process
./setup.ps1
```

#### MacOS/Linux (Shell)

```bash
chmod +x setup.sh
./setup.sh
```

### Manual Setup

1. Configure your database connection in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=CrewApp;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

2. Run the following commands:
```bash
dotnet ef database drop --force --no-build
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

## Security Features

- Secure password hashing using ASP.NET Core's `IPasswordHasher`
- HTTPS enforcement
- Secure cookie policies
- CORS protection
- Input validation and sanitization
- Role-based access control

## API Endpoints

The API provides the following main endpoints:

- `/api/auth`: Authentication endpoints (login, logout)
- `/api/users`: User management
- `/api/organisations`: Organization management
- `/api/dashboard`: Analytics and statistics
- `/api/reports`: Report generation and management

## Development Workflow

1. Make changes to models if needed
2. Add/update migrations: `dotnet ef migrations add YourMigrationName`
3. Update database: `dotnet ef database update`
4. Run the application: `dotnet run`

## Note for Contributors

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.