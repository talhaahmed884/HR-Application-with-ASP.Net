-- =============================================
-- HR Application Seed Data
-- Purpose: Insert initial roles and users
-- =============================================

USE HRApplicationDB;
GO

-- =============================================
-- Seed Roles
-- =============================================
SET IDENTITY_INSERT dbo.Roles ON;

INSERT INTO dbo.Roles (Id, RoleName, Description, CreatedAt, UpdatedAt)
VALUES
    (1, 'HR', 'Human Resources - Full access to all employee data', GETUTCDATE(), GETUTCDATE()),
    (2, 'Employee', 'Standard Employee - Can only view and edit own data', GETUTCDATE(), GETUTCDATE()),
    (3, 'Manager', 'Manager - Can view team data', GETUTCDATE(), GETUTCDATE());

SET IDENTITY_INSERT dbo.Roles OFF;
GO

-- =============================================
-- Seed Employees
-- =============================================
SET IDENTITY_INSERT dbo.Employees ON;

INSERT INTO dbo.Employees (Id, Email, Name, Address, CellNumber, RoleId, IsActive, CreatedAt, UpdatedAt)
VALUES
    -- HR User
    (1, 'hr@company.com', 'Sarah Johnson', '123 Main Street, New York, NY 10001', '+1-555-0101', 1, 1, GETUTCDATE(), GETUTCDATE()),

    -- Standard Employee User
    (2, 'john.doe@company.com', 'John Doe', '456 Elm Street, Los Angeles, CA 90001', '+1-555-0102', 2, 1, GETUTCDATE(), GETUTCDATE()),

    -- Additional Sample Employees
    (3, 'jane.smith@company.com', 'Jane Smith', '789 Oak Avenue, Chicago, IL 60601', '+1-555-0103', 2, 1, GETUTCDATE(), GETUTCDATE()),
    (4, 'mike.manager@company.com', 'Mike Manager', '321 Pine Road, Houston, TX 77001', '+1-555-0104', 3, 1, GETUTCDATE(), GETUTCDATE());

SET IDENTITY_INSERT dbo.Employees OFF;
GO

-- =============================================
-- Seed User Passwords
-- Note: These are pre-hashed passwords using SHA256
-- For demonstration purposes:
--   - HR User password: "HRPass123!"
--     SHA256 Hash: 78CD7DED97E8EFEA770A342A9BD0A3A432D321AE661646A5E59B879C90463D41
--   - Employee User password: "EmpPass123!"
--     SHA256 Hash: 0731627B7CD886F44354F105158B9C92976D968C14BE26A1ECBBE0D3BC41EAE0
--   - Jane Smith password: "Jane123!"
--     SHA256 Hash: 7641CC8E3B75AEB58A172E1A3E4A484A334AEC712B3061EBFFDCC4DC76675741
--   - Mike Manager password: "Mike123!"
--     SHA256 Hash: CCC6D76F56AADC146F7FC851F5650518C9B7080EEBD8CFAFDFA8C979E006B767
-- =============================================

INSERT INTO dbo.UserPasswords (UserId, PasswordHash, LastPasswordChange, FailedLoginAttempts, CreatedAt, UpdatedAt)
VALUES
    -- HR User (password: HRPass123!)
    (1, '78CD7DED97E8EFEA770A342A9BD0A3A432D321AE661646A5E59B879C90463D41', GETUTCDATE(), 0, GETUTCDATE(), GETUTCDATE()),

    -- Standard Employee (password: EmpPass123!)
    (2, '0731627B7CD886F44354F105158B9C92976D968C14BE26A1ECBBE0D3BC41EAE0', GETUTCDATE(), 0, GETUTCDATE(), GETUTCDATE()),

    -- Jane Smith (password: Jane123!)
    (3, '7641CC8E3B75AEB58A172E1A3E4A484A334AEC712B3061EBFFDCC4DC76675741', GETUTCDATE(), 0, GETUTCDATE(), GETUTCDATE()),

    -- Mike Manager (password: Mike123!)
    (4, 'CCC6D76F56AADC146F7FC851F5650518C9B7080EEBD8CFAFDFA8C979E006B767', GETUTCDATE(), 0, GETUTCDATE(), GETUTCDATE());
GO

-- =============================================
-- Verification Queries
-- =============================================
PRINT 'Seed data inserted successfully!';
PRINT '';
PRINT 'Roles Count: ' + CAST((SELECT COUNT(*) FROM dbo.Roles) AS NVARCHAR(10));
PRINT 'Employees Count: ' + CAST((SELECT COUNT(*) FROM dbo.Employees) AS NVARCHAR(10));
PRINT 'UserPasswords Count: ' + CAST((SELECT COUNT(*) FROM dbo.UserPasswords) AS NVARCHAR(10));
PRINT '';
PRINT '=== Sample Login Credentials ===';
PRINT 'HR User:';
PRINT '  Email: hr@company.com';
PRINT '  Password: HRPass123!';
PRINT '';
PRINT 'Employee User:';
PRINT '  Email: john.doe@company.com';
PRINT '  Password: EmpPass123!';
GO

-- =============================================
-- Display Seeded Data
-- =============================================
SELECT 'Roles' AS TableName;
SELECT * FROM dbo.Roles;

SELECT 'Employees with Roles' AS TableName;
SELECT * FROM vw_EmployeesWithRoles;

SELECT 'UserPasswords (Masked)' AS TableName;
SELECT
    UserId,
    LEFT(PasswordHash, 10) + '...' AS PasswordHashPreview,
    LastPasswordChange,
    FailedLoginAttempts
FROM dbo.UserPasswords;
GO
