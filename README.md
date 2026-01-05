# HR Application - REST API

A robust, production-ready REST API for managing employees and roles with JWT authentication and role-based authorization.

## Table of Contents
- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Database Design](#database-design)
- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Authentication & Authorization](#authentication--authorization)
- [Project Structure](#project-structure)
- [Key Features](#key-features)

## Overview

This HR Application API provides a complete employee management system with secure authentication, role-based access control, and comprehensive reporting capabilities. Built following clean architecture principles with strict separation of concerns.

## Tech Stack

- **Framework**: ASP.NET Core (.NET 10.0)
- **Database**: SQL Server
- **ORM**: Dapper (lightweight, high-performance micro-ORM)
- **Authentication**: JWT (JSON Web Tokens)
- **Authorization**: Role-based with custom business rules
- **API Documentation**: Swagger/OpenAPI

## Architecture

### Layered Architecture

```
┌─────────────────────────────────────────┐
│  Controllers (API Layer)                │  ← Thin controllers, HTTP only
├─────────────────────────────────────────┤
│  Services (Business Logic Layer)        │  ← Validations, authorization
├─────────────────────────────────────────┤
│  Repositories (Data Access Layer)       │  ← Dapper queries
├─────────────────────────────────────────┤
│  Database (SQL Server)                  │  ← 3NF normalized schema
└─────────────────────────────────────────┘
```

### Key Architectural Decisions

1. **Repository Pattern**: Abstracts data access logic
2. **Service Layer**: Encapsulates business rules and authorization
3. **DTOs**: Separate data transfer objects from entities
4. **Custom Error Codes**: Standardized error handling with specific error codes
5. **API Response Wrappers**: Consistent response format across all endpoints

## Database Design

### Schema (3NF Normalized)

**Roles Table**
```sql
- Id (PK)
- RoleName (UNIQUE)
- Description
- CreatedAt, UpdatedAt
```

**Employees Table**
```sql
- Id (PK)
- Email (UNIQUE)
- Name
- Address
- CellNumber
- RoleId (FK → Roles)
- IsActive
- CreatedAt, UpdatedAt
```

**UserPasswords Table** (1:1 with Employees)
```sql
- UserId (PK, FK → Employees)
- PasswordHash
- Salt
- LastPasswordChange
- FailedLoginAttempts
- LockedUntil
- CreatedAt, UpdatedAt
```

### Security Design
- **Passwords stored separately** from employee data
- **1:1 relationship** between Employees and UserPasswords
- **Cascade delete** removes password when employee is deleted
- **SHA256 hashing** for password security

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- SQL Server (LocalDB, Express, or Full)
- SQL Server Management Studio (SSMS) or Azure Data Studio (optional)

### Installation Steps

1. **Clone the repository**
   ```bash
   cd "HR Application"
   ```

2. **Set up the database**
   ```bash
   # Run schema creation script
   sqlcmd -S localhost -i Database/Schema.sql

   # Run seed data script
   sqlcmd -S localhost -i Database/SeedData.sql
   ```

3. **Update connection string** (if needed)

   Edit `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=HRApplicationDB;..."
   }
   ```

4. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

5. **Build the project**
   ```bash
   dotnet build
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

7. **Access Swagger UI**

   Open your browser to: `http://localhost:5222` or `https://localhost:7132`

### Test Credentials

After running the seed data script:

**HR User:**
- Email: `hr@company.com`
- Password: `HRPass123!`

**Standard Employee:**
- Email: `john.doe@company.com`
- Password: `EmpPass123!`

## API Endpoints

### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/login` | Login and get JWT token | No |
| GET | `/api/auth/health` | Health check | No |

### Employees

| Method | Endpoint | Description | Auth Required | Role |
|--------|----------|-------------|---------------|------|
| GET | `/api/employees/{id}` | Get employee by ID | Yes | Employee (own) / HR (all) |
| GET | `/api/employees` | Get all employees | Yes | HR only |
| POST | `/api/employees` | Create new employee | Yes | HR only |
| PUT | `/api/employees/{id}` | Update employee | Yes | Employee (own) / HR (all) |
| DELETE | `/api/employees/{id}` | Delete employee | Yes | HR only |

### Reports

| Method | Endpoint | Description | Auth Required | Role |
|--------|----------|-------------|---------------|------|
| GET | `/api/reports/role-counts` | Get employee count per role | Yes | HR only |
| GET | `/api/reports/employees-by-role` | Get employees grouped by role | Yes | HR only |
| GET | `/api/reports/summary` | Get summary statistics | Yes | HR only |

## Authentication & Authorization

### JWT Token Authentication

1. **Login** to receive a JWT token:
   ```bash
   POST /api/auth/login
   {
     "email": "hr@company.com",
     "password": "HRPass123!"
   }
   ```

2. **Response** includes token:
   ```json
   {
     "Success": true,
     "StatusCode": 200,
     "Message": "Login successful",
     "Data": {
       "Token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
       "TokenType": "Bearer",
       "ExpiresIn": 3600,
       "User": { ... }
     }
   }
   ```

3. **Use token** in subsequent requests:
   ```
   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```

### Authorization Rules

#### Standard Employee Role
- ✅ Can GET their **own** employee data
- ✅ Can PUT (update) their **own** data
  - Allowed fields: Name, Address, CellNumber
  - Blocked fields: RoleId, IsActive
- ❌ Cannot access other employees' data (returns 403 Forbidden)
- ❌ Cannot access HR reports

#### HR Role
- ✅ Full CRUD access to **all** employee data
- ✅ Can modify all fields (including RoleId, IsActive)
- ✅ Can create new employees
- ✅ Can delete employees
- ✅ Can access all reporting endpoints

### Account Lockout Policy
- After **5 failed login attempts**, account is locked for **15 minutes**
- Failed attempts counter resets on successful login

## Project Structure

```
HR Application/
├── Controllers/
│   ├── AuthController.cs              # Login endpoints
│   ├── EmployeesController.cs         # Employee CRUD
│   └── ReportsController.cs           # HR reports
│
├── Services/
│   ├── IAuthService.cs                # Auth interface
│   ├── AuthService.cs                 # Auth implementation
│   ├── IUserService.cs                # User service interface
│   ├── UserService.cs                 # User service implementation
│   └── Utils/
│       ├── PasswordHasher.cs          # SHA256 password hashing
│       └── JwtTokenGenerator.cs       # JWT token generation
│
├── DataAccess/
│   ├── IDbConnectionFactory.cs        # DB connection interface
│   ├── SqlConnectionFactory.cs        # SQL Server connection
│   └── Repositories/
│       ├── IUserRepository.cs         # User data interface
│       ├── UserRepository.cs          # User data implementation
│       ├── IAuthRepository.cs         # Auth data interface
│       ├── AuthRepository.cs          # Auth data implementation
│       ├── IRoleRepository.cs         # Role data interface
│       └── RoleRepository.cs          # Role data implementation
│
├── Models/
│   ├── Entities/
│   │   ├── Employee.cs                # Employee entity
│   │   ├── UserPassword.cs            # Password entity
│   │   └── Role.cs                    # Role entity
│   ├── DTOs/
│   │   ├── LoginRequestDto.cs         # Login request
│   │   ├── LoginResponseDto.cs        # Login response
│   │   ├── EmployeeDto.cs             # Employee data
│   │   ├── CreateEmployeeDto.cs       # Create employee
│   │   ├── UpdateEmployeeDto.cs       # Update employee
│   │   ├── RoleCountDto.cs            # Role count report
│   │   └── EmployeesByRoleDto.cs      # Employees by role
│   ├── Enums/
│   │   └── ErrorCodes.cs              # Custom error codes
│   └── Wrappers/
│       ├── ApiResponse.cs             # Success response wrapper
│       └── ErrorResponse.cs           # Error response wrapper
│
├── Database/
│   ├── Schema.sql                     # Database schema
│   └── SeedData.sql                   # Initial data
│
├── Program.cs                         # App entry, DI, middleware
├── appsettings.json                   # Configuration
└── HR Application.csproj              # Project file
```

## Key Features

### 1. Secure Password Management
- SHA256 password hashing
- Passwords stored in separate table
- Password validation on login
- Account lockout after failed attempts

### 2. JWT Token Management
- Configurable token expiration
- Signed with secret key
- Contains user ID, email, and role claims
- Validated on every protected endpoint

### 3. Dapper for Data Access
- Lightweight and fast
- Parameterized queries (SQL injection prevention)
- Proper connection disposal with `using` statements
- Async operations throughout

### 4. Comprehensive Error Handling
- Custom error codes (1000s, 2000s, 3000s)
- HTTP status code mapping
- User-friendly error messages
- Validation error details

### 5. Clean Code Practices
- Dependency Injection throughout
- Interface-based design
- Async/await for all I/O operations
- Structured logging with `ILogger`
- XML documentation comments

### 6. API Documentation
- Swagger/OpenAPI integration
- JWT authentication in Swagger UI
- Request/response examples
- Detailed endpoint descriptions

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HRApplicationDB;..."
  },
  "JwtSettings": {
    "SecretKey": "YOUR_SECRET_KEY_HERE",
    "Issuer": "HRApplication",
    "Audience": "HRApplicationUsers",
    "ExpirationMinutes": 60
  }
}
```

### Important Security Notes

⚠️ **For Production:**
- Store JWT SecretKey in environment variables or Azure Key Vault
- Use strong connection string passwords
- Restrict CORS policy (don't use "AllowAll")
- Enable HTTPS only
- Implement rate limiting
- Add comprehensive logging and monitoring

## Example API Usage

### 1. Login as HR
```bash
curl -X POST "http://localhost:5222/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "hr@company.com",
    "password": "HRPass123!"
  }'
```

### 2. Get All Employees (HR only)
```bash
curl -X GET "http://localhost:5222/api/employees" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### 3. Create New Employee (HR only)
```bash
curl -X POST "http://localhost:5222/api/employees" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "new.employee@company.com",
    "name": "New Employee",
    "address": "123 Street",
    "cellNumber": "+1-555-0199",
    "roleId": 2,
    "password": "NewPass123!"
  }'
```

### 4. Update Own Profile (Employee)
```bash
curl -X PUT "http://localhost:5222/api/employees/2" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Updated Doe",
    "cellNumber": "+1-555-9999"
  }'
```

### 5. Get Role Counts Report (HR only)
```bash
curl -X GET "http://localhost:5222/api/reports/role-counts" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Learning Resources

This project demonstrates:
- ✅ Clean Architecture principles
- ✅ Repository and Service patterns
- ✅ Dependency Injection
- ✅ JWT Authentication
- ✅ Role-based Authorization
- ✅ Dapper ORM usage
- ✅ ADO.NET connection management
- ✅ RESTful API design
- ✅ Error handling best practices
- ✅ SQL Server database design (3NF)

## License

This is a learning project for ASP.NET Core 8.0 development.

## Author

Built as a comprehensive learning example for ASP.NET Core Web API development.
