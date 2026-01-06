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

    public async Task<(bool Success, EmployeeDto? Employee, ErrorResponse? Error)> GetEmployeeByIdAsync(int employeeId)
    {
        try
        {
            var employee = await _userRepository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                _logger.LogWarning("Employee not found - EmployeeId: {EmployeeId}", employeeId);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.EmployeeErrors.UserNotFound));
            }

            var employeeDto = MapToDto(employee);
            return (true, employeeDto, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee - EmployeeId: {EmployeeId}", employeeId);
            return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.CommonErrors.InternalServerError, ex.Message));
        }
    }

    public async Task<(bool Success, IEnumerable<EmployeeDto>? Employees, ErrorResponse? Error)> GetAllEmployeesAsync()
    {
        try
        {
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
        CreateEmployeeDto createEmployeeDto)
    {
        try
        {
            if (await _userRepository.EmailExistsAsync(createEmployeeDto.Email))
            {
                _logger.LogWarning("Attempt to create employee with existing email - Email: {Email}", createEmployeeDto.Email);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.EmployeeErrors.UserAlreadyExists));
            }

            var role = await _roleRepository.GetByIdAsync(createEmployeeDto.RoleId);
            if (role == null)
            {
                _logger.LogWarning("Attempt to create employee with invalid role - RoleId: {RoleId}", createEmployeeDto.RoleId);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.EmployeeErrors.InvalidEmployeeData,
                    "Invalid role specified"));
            }

            var employee = new Employee
            {
                Email = createEmployeeDto.Email,
                Name = createEmployeeDto.Name,
                Address = createEmployeeDto.Address,
                CellNumber = createEmployeeDto.CellNumber,
                RoleId = createEmployeeDto.RoleId,
                IsActive = true
            };

            var createdEmployee = await _userRepository.CreateAsync(employee);

            var passwordHash = PasswordHasher.HashPassword(createEmployeeDto.Password);
            var userPassword = new UserPassword
            {
                UserId = createdEmployee.Id,
                PasswordHash = passwordHash,
            };

            await _authRepository.CreateUserPasswordAsync(userPassword);

            _logger.LogInformation("Employee created successfully - EmployeeId: {EmployeeId}, Email: {Email}",
                createdEmployee.Id, createdEmployee.Email);

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
        int employeeId, UpdateEmployeeDto updateEmployeeDto, bool isHrUser)
    {
        try
        {
            var employee = await _userRepository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                _logger.LogWarning("Update failed: Employee not found - EmployeeId: {EmployeeId}", employeeId);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.EmployeeErrors.UserNotFound));
            }

            if (!string.IsNullOrWhiteSpace(updateEmployeeDto.Name))
                employee.Name = updateEmployeeDto.Name;

            if (updateEmployeeDto.Address != null)
                employee.Address = updateEmployeeDto.Address;

            if (updateEmployeeDto.CellNumber != null)
                employee.CellNumber = updateEmployeeDto.CellNumber;

            if (isHrUser)
            {
                if (updateEmployeeDto.RoleId.HasValue)
                {
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

            var updateSuccess = await _userRepository.UpdateAsync(employee);
            if (!updateSuccess)
            {
                _logger.LogError("Failed to update employee in database - EmployeeId: {EmployeeId}", employeeId);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.CommonErrors.DatabaseError));
            }

            _logger.LogInformation("Employee updated successfully - EmployeeId: {EmployeeId}", employeeId);

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

    public async Task<(bool Success, ErrorResponse? Error)> DeleteEmployeeAsync(int employeeId)
    {
        try
        {
            var employee = await _userRepository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                _logger.LogWarning("Delete failed: Employee not found - EmployeeId: {EmployeeId}", employeeId);
                return (false, ErrorResponse.FromErrorCode(ErrorCodes.EmployeeErrors.UserNotFound));
            }

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

    public async Task<(bool Success, IEnumerable<RoleCountDto>? RoleCounts, ErrorResponse? Error)> GetRoleCountsAsync()
    {
        try
        {
            var roles = await _roleRepository.GetAllAsync();
            var employeeCounts = await _userRepository.GetEmployeeCountByRoleAsync();

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

    public async Task<(bool Success, IEnumerable<EmployeesByRoleDto>? EmployeesByRole, ErrorResponse? Error)> GetEmployeesByRoleAsync()
    {
        try
        {
            var roles = await _roleRepository.GetAllAsync();
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
