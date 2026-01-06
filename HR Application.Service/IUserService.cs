using HR_Application.Model.DTOs;
using HR_Application.Model.Wrappers;

namespace HR_Application.Service;

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
