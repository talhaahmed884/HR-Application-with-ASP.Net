-- =============================================
-- HR Application Database Schema
-- SQL Server Database Design (3NF)
-- =============================================

USE master;
GO

-- Create Database if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'HRApplicationDB')
BEGIN
    CREATE DATABASE HRApplicationDB;
END
GO

USE HRApplicationDB;
GO

-- =============================================
-- Drop Tables if they exist (for clean setup)
-- =============================================
IF OBJECT_ID('dbo.UserPasswords', 'U') IS NOT NULL DROP TABLE dbo.UserPasswords;
IF OBJECT_ID('dbo.Employees', 'U') IS NOT NULL DROP TABLE dbo.Employees;
IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL DROP TABLE dbo.Roles;
GO

-- =============================================
-- Table: Roles
-- Purpose: Store different roles in the system
-- =============================================
CREATE TABLE dbo.Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200) NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL
);
GO

-- =============================================
-- Table: Employees
-- Purpose: Store employee information
-- Note: Passwords are NOT stored here for security
-- =============================================
CREATE TABLE dbo.Employees (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL,
    Address NVARCHAR(255) NULL,
    CellNumber NVARCHAR(20) NULL,
    RoleId INT NOT NULL,
    IsActive BIT DEFAULT 1 NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL,

    -- Foreign Key Constraint
    CONSTRAINT FK_Employees_Roles FOREIGN KEY (RoleId)
        REFERENCES dbo.Roles(Id) ON DELETE NO ACTION,

    -- Check Constraints
    CONSTRAINT CHK_Email CHECK (Email LIKE '%@%.%')
);
GO

-- =============================================
-- Table: UserPasswords
-- Purpose: Store hashed passwords separately (1:1 relationship with Employees)
-- Security: Separation of authentication credentials from user data
-- =============================================
CREATE TABLE dbo.UserPasswords (
    UserId INT PRIMARY KEY, -- This is both PK and FK
    PasswordHash NVARCHAR(500) NOT NULL,
    Salt NVARCHAR(100) NULL, -- Optional: for additional security
    LastPasswordChange DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    FailedLoginAttempts INT DEFAULT 0 NOT NULL,
    LockedUntil DATETIME2 NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE() NOT NULL,

    -- Foreign Key Constraint (1:1 relationship)
    CONSTRAINT FK_UserPasswords_Employees FOREIGN KEY (UserId)
        REFERENCES dbo.Employees(Id) ON DELETE CASCADE
);
GO

-- =============================================
-- Indexes for Performance
-- =============================================
CREATE NONCLUSTERED INDEX IX_Employees_Email ON dbo.Employees(Email);
CREATE NONCLUSTERED INDEX IX_Employees_RoleId ON dbo.Employees(RoleId);
CREATE NONCLUSTERED INDEX IX_Employees_IsActive ON dbo.Employees(IsActive);
GO

-- =============================================
-- Views for Common Queries
-- =============================================
CREATE VIEW vw_EmployeesWithRoles
AS
    SELECT
        e.Id,
        e.Email,
        e.Name,
        e.Address,
        e.CellNumber,
        e.RoleId,
        r.RoleName,
        e.IsActive,
        e.CreatedAt,
        e.UpdatedAt
    FROM dbo.Employees e
    INNER JOIN dbo.Roles r ON e.RoleId = r.Id;
GO

PRINT 'Database schema created successfully!';
GO
