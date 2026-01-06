using HR_Application.Models.DTOs;
using HR_Application.Models.Wrappers;

namespace HR_Application.Services;

public interface IUserService
{
    Task<(bool Success, EmployeeDto? Employee, ErrorResponse? Error)> GetEmployeeByIdAsync(int employeeId);

    Task<(bool Success, IEnumerable<EmployeeDto>? Employees, ErrorResponse? Error)> GetAllEmployeesAsync();

    Task<(bool Success, EmployeeDto? Employee, ErrorResponse? Error)> CreateEmployeeAsync(
        CreateEmployeeDto createEmployeeDto);

    Task<(bool Success, EmployeeDto? Employee, ErrorResponse? Error)> UpdateEmployeeAsync(
        int employeeId, UpdateEmployeeDto updateEmployeeDto, bool isHrUser);

    Task<(bool Success, ErrorResponse? Error)> DeleteEmployeeAsync(int employeeId);

    Task<(bool Success, IEnumerable<RoleCountDto>? RoleCounts, ErrorResponse? Error)> GetRoleCountsAsync();

    Task<(bool Success, IEnumerable<EmployeesByRoleDto>? EmployeesByRole, ErrorResponse? Error)> GetEmployeesByRoleAsync();
}
