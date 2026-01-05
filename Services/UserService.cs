using HR_Application.DataAccess.Repositories;
using HR_Application.Models.DTOs;
using HR_Application.Models.Entities;
using HR_Application.Models.Enums;
using HR_Application.Models.Wrappers;
using HR_Application.Services.Utils;

namespace HR_Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<UserService> _logger;

    // Role name constants
    private const string HR_ROLE = "HR";
    private const string EMPLOYEE_ROLE = "Employee";

    public UserService(
        IUserRepository userRepository,
        IAuthRepository authRepository,
        IRoleRepository roleRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _authRepository = authRepository;
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task<(bool Success, EmployeeDto? Employee, ErrorResponse? Error)> GetEmployeeByIdAsync(
        int employeeId, int requestingUserId, string requestingUserRole)
    {
        try
        {
            // Authorization check: Employee can only access their own data
            if (requestingUserRole != HR_ROLE && requestingUserId != employeeId)
            {
                _logger.LogWarning("Unauthorized access attempt - UserId: {UserId} tried to access EmployeeId: {EmployeeId}",
                    requestingUserId, employeeId);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.AuthErrors.InsufficientPermissions));
            }

            // Retrieve the employee
            var employee = await _userRepository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                _logger.LogWarning("Employee not found - EmployeeId: {EmployeeId}", employeeId);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.EmployeeErrors.UserNotFound));
            }

            // Map to DTO
            var employeeDto = MapToDto(employee);
            return (true, employeeDto, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee - EmployeeId: {EmployeeId}", employeeId);
            return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.CommonErrors.InternalServerError, ex.Message));
        }
    }

    public async Task<(bool Success, IEnumerable<EmployeeDto>? Employees, ErrorResponse? Error)> GetAllEmployeesAsync(
        string requestingUserRole)
    {
        try
        {
            // Authorization check: Only HR can view all employees
            if (requestingUserRole != HR_ROLE)
            {
                _logger.LogWarning("Unauthorized access attempt to get all employees - Role: {Role}", requestingUserRole);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.AuthErrors.InsufficientPermissions));
            }

            // Retrieve all employees
            var employees = await _userRepository.GetAllAsync();
            var employeeDtos = employees.Select(MapToDto);

            return (true, employeeDtos, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all employees");
            return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.CommonErrors.InternalServerError, ex.Message));
        }
    }

    public async Task<(bool Success, EmployeeDto? Employee, ErrorResponse? Error)> CreateEmployeeAsync(
        CreateEmployeeDto createEmployeeDto, string requestingUserRole)
    {
        try
        {
            // Authorization check: Only HR can create employees
            if (requestingUserRole != HR_ROLE)
            {
                _logger.LogWarning("Unauthorized attempt to create employee - Role: {Role}", requestingUserRole);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.AuthErrors.InsufficientPermissions));
            }

            // Validation: Check if email already exists
            if (await _userRepository.EmailExistsAsync(createEmployeeDto.Email))
            {
                _logger.LogWarning("Attempt to create employee with existing email - Email: {Email}", createEmployeeDto.Email);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.EmployeeErrors.UserAlreadyExists));
            }

            // Validation: Check if role exists
            var role = await _roleRepository.GetByIdAsync(createEmployeeDto.RoleId);
            if (role == null)
            {
                _logger.LogWarning("Attempt to create employee with invalid role - RoleId: {RoleId}", createEmployeeDto.RoleId);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.EmployeeErrors.InvalidEmployeeData,
                    "Invalid role specified"));
            }

            // Create employee entity
            var employee = new Employee
            {
                Email = createEmployeeDto.Email,
                Name = createEmployeeDto.Name,
                Address = createEmployeeDto.Address,
                CellNumber = createEmployeeDto.CellNumber,
                RoleId = createEmployeeDto.RoleId,
                IsActive = true
            };

            // Save employee to database
            var createdEmployee = await _userRepository.CreateAsync(employee);

            // Hash the password and create password record
            var passwordHash = PasswordHasher.HashPassword(createEmployeeDto.Password);
            var userPassword = new UserPassword
            {
                UserId = createdEmployee.Id,
                PasswordHash = passwordHash,
            };

            await _authRepository.CreateUserPasswordAsync(userPassword);

            _logger.LogInformation("Employee created successfully - EmployeeId: {EmployeeId}, Email: {Email}",
                createdEmployee.Id, createdEmployee.Email);

            // Return the created employee
            var employeeDto = MapToDto(createdEmployee);
            return (true, employeeDto, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee - Email: {Email}", createEmployeeDto.Email);
            return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.CommonErrors.InternalServerError, ex.Message));
        }
    }

    public async Task<(bool Success, EmployeeDto? Employee, ErrorResponse? Error)> UpdateEmployeeAsync(
        int employeeId, UpdateEmployeeDto updateEmployeeDto, int requestingUserId, string requestingUserRole)
    {
        try
        {
            // Authorization check: Employee can only update their own data
            if (requestingUserRole != HR_ROLE && requestingUserId != employeeId)
            {
                _logger.LogWarning("Unauthorized update attempt - UserId: {UserId} tried to update EmployeeId: {EmployeeId}",
                    requestingUserId, employeeId);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.AuthErrors.InsufficientPermissions));
            }

            // Retrieve existing employee
            var employee = await _userRepository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                _logger.LogWarning("Update failed: Employee not found - EmployeeId: {EmployeeId}", employeeId);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.EmployeeErrors.UserNotFound));
            }

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(updateEmployeeDto.Name))
                employee.Name = updateEmployeeDto.Name;

            if (updateEmployeeDto.Address != null)
                employee.Address = updateEmployeeDto.Address;

            if (updateEmployeeDto.CellNumber != null)
                employee.CellNumber = updateEmployeeDto.CellNumber;

            // Only HR can update RoleId and IsActive
            if (requestingUserRole == HR_ROLE)
            {
                if (updateEmployeeDto.RoleId.HasValue)
                {
                    // Validate role exists
                    var role = await _roleRepository.GetByIdAsync(updateEmployeeDto.RoleId.Value);
                    if (role == null)
                    {
                        return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.EmployeeErrors.InvalidEmployeeData,
                            "Invalid role specified"));
                    }
                    employee.RoleId = updateEmployeeDto.RoleId.Value;
                }

                if (updateEmployeeDto.IsActive.HasValue)
                    employee.IsActive = updateEmployeeDto.IsActive.Value;
            }

            // Save updates
            var updateSuccess = await _userRepository.UpdateAsync(employee);
            if (!updateSuccess)
            {
                _logger.LogError("Failed to update employee in database - EmployeeId: {EmployeeId}", employeeId);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.CommonErrors.DatabaseError));
            }

            _logger.LogInformation("Employee updated successfully - EmployeeId: {EmployeeId}", employeeId);

            // Retrieve updated employee with role information
            var updatedEmployee = await _userRepository.GetByIdAsync(employeeId);
            var employeeDto = MapToDto(updatedEmployee!);

            return (true, employeeDto, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee - EmployeeId: {EmployeeId}", employeeId);
            return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.CommonErrors.InternalServerError, ex.Message));
        }
    }

    public async Task<(bool Success, ErrorResponse? Error)> DeleteEmployeeAsync(int employeeId, string requestingUserRole)
    {
        try
        {
            // Authorization check: Only HR can delete employees
            if (requestingUserRole != HR_ROLE)
            {
                _logger.LogWarning("Unauthorized delete attempt - Role: {Role}", requestingUserRole);
                return (false, ErrorResponse.FromErrorCode(ErrorCodes.AuthErrors.InsufficientPermissions));
            }

            // Check if employee exists
            var employee = await _userRepository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                _logger.LogWarning("Delete failed: Employee not found - EmployeeId: {EmployeeId}", employeeId);
                return (false, ErrorResponse.FromErrorCode(ErrorCodes.EmployeeErrors.UserNotFound));
            }

            // Delete employee (cascade delete will remove password record)
            var deleteSuccess = await _userRepository.DeleteAsync(employeeId);
            if (!deleteSuccess)
            {
                _logger.LogError("Failed to delete employee from database - EmployeeId: {EmployeeId}", employeeId);
                return (false, ErrorResponse.FromErrorCode(ErrorCodes.CommonErrors.DatabaseError));
            }

            _logger.LogInformation("Employee deleted successfully - EmployeeId: {EmployeeId}", employeeId);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee - EmployeeId: {EmployeeId}", employeeId);
            return (false, ErrorResponse.FromErrorCode(ErrorCodes.CommonErrors.InternalServerError, ex.Message));
        }
    }

    public async Task<(bool Success, IEnumerable<RoleCountDto>? RoleCounts, ErrorResponse? Error)> GetRoleCountsAsync(
        string requestingUserRole)
    {
        try
        {
            // Authorization check: Only HR can access reports
            if (requestingUserRole != HR_ROLE)
            {
                _logger.LogWarning("Unauthorized access to role counts report - Role: {Role}", requestingUserRole);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.AuthErrors.InsufficientPermissions));
            }

            // Get all roles
            var roles = await _roleRepository.GetAllAsync();

            // Get employee counts by role
            var employeeCounts = await _userRepository.GetEmployeeCountByRoleAsync();

            // Build report
            var roleCounts = roles.Select(role => new RoleCountDto
            {
                RoleId = role.Id,
                RoleName = role.RoleName,
                EmployeeCount = employeeCounts.ContainsKey(role.Id) ? employeeCounts[role.Id] : 0
            });

            return (true, roleCounts, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating role counts report");
            return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.CommonErrors.InternalServerError, ex.Message));
        }
    }

    public async Task<(bool Success, IEnumerable<EmployeesByRoleDto>? EmployeesByRole, ErrorResponse? Error)> GetEmployeesByRoleAsync(
        string requestingUserRole)
    {
        try
        {
            // Authorization check: Only HR can access reports
            if (requestingUserRole != HR_ROLE)
            {
                _logger.LogWarning("Unauthorized access to employees by role report - Role: {Role}", requestingUserRole);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.AuthErrors.InsufficientPermissions));
            }

            // Get all roles
            var roles = await _roleRepository.GetAllAsync();

            // Build report
            var employeesByRole = new List<EmployeesByRoleDto>();

            foreach (var role in roles)
            {
                var employees = await _userRepository.GetByRoleIdAsync(role.Id);
                employeesByRole.Add(new EmployeesByRoleDto
                {
                    RoleId = role.Id,
                    RoleName = role.RoleName,
                    Employees = employees.Select(MapToDto).ToList()
                });
            }

            return (true, employeesByRole, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating employees by role report");
            return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.CommonErrors.InternalServerError, ex.Message));
        }
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            Email = employee.Email,
            Name = employee.Name,
            Address = employee.Address,
            CellNumber = employee.CellNumber,
            RoleId = employee.RoleId,
            RoleName = employee.RoleName ?? "Unknown",
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt
        };
    }
}
