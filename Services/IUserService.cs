using HR_Application.Models.DTOs;
using HR_Application.Models.Wrappers;

namespace HR_Application.Services;

public interface IUserService
{
    Task<(bool Success, EmployeeDto? Employee, ErrorResponse? Error)> GetEmployeeByIdAsync(
        int employeeId, int requestingUserId, string requestingUserRole);

    Task<(bool Success, IEnumerable<EmployeeDto>? Employees, ErrorResponse? Error)> GetAllEmployeesAsync(
        string requestingUserRole);

    Task<(bool Success, EmployeeDto? Employee, ErrorResponse? Error)> CreateEmployeeAsync(
        CreateEmployeeDto createEmployeeDto, string requestingUserRole);

    Task<(bool Success, EmployeeDto? Employee, ErrorResponse? Error)> UpdateEmployeeAsync(
        int employeeId, UpdateEmployeeDto updateEmployeeDto, int requestingUserId, string requestingUserRole);

    Task<(bool Success, ErrorResponse? Error)> DeleteEmployeeAsync(int employeeId, string requestingUserRole);

    Task<(bool Success, IEnumerable<RoleCountDto>? RoleCounts, ErrorResponse? Error)> GetRoleCountsAsync(
        string requestingUserRole);

    Task<(bool Success, IEnumerable<EmployeesByRoleDto>? EmployeesByRole, ErrorResponse? Error)> GetEmployeesByRoleAsync(
        string requestingUserRole);
}
