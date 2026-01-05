using System.Security.Claims;
using HR_Application.Models.DTOs;
using HR_Application.Models.Wrappers;
using HR_Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_Application.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "HR")]
public class ReportsController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IUserService userService, ILogger<ReportsController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("role-counts")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleCountDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRoleCounts()
    {
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Employee";
        var (success, roleCounts, error) = await _userService.GetRoleCountsAsync(userRole);

        if (!success || error != null)
        {
            _logger.LogWarning("Failed to retrieve role counts - Role: {Role}", userRole);
            return StatusCode(error!.StatusCode, error);
        }

        return Ok(ApiResponse<IEnumerable<RoleCountDto>>.SuccessResponse(
            roleCounts!,
            "Role counts retrieved successfully"
        ));
    }

    [HttpGet("employees-by-role")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmployeesByRoleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmployeesByRole()
    {
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Employee";
        var (success, employeesByRole, error) = await _userService.GetEmployeesByRoleAsync(userRole);

        if (!success || error != null)
        {
            _logger.LogWarning("Failed to retrieve employees by role - Role: {Role}", userRole);
            return StatusCode(error!.StatusCode, error);
        }

        return Ok(ApiResponse<IEnumerable<EmployeesByRoleDto>>.SuccessResponse(
            employeesByRole!,
            "Employees by role retrieved successfully"
        ));
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSummary()
    {
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Employee";
        var (success, roleCounts, error) = await _userService.GetRoleCountsAsync(userRole);

        if (!success || error != null)
        {
            _logger.LogWarning("Failed to retrieve summary - Role: {Role}", userRole);
            return StatusCode(error!.StatusCode, error);
        }

        var summary = new
        {
            TotalEmployees = roleCounts!.Sum(rc => rc.EmployeeCount),
            RoleBreakdown = roleCounts,
            GeneratedAt = DateTime.UtcNow
        };

        return Ok(ApiResponse<object>.SuccessResponse(summary, "Summary statistics retrieved successfully"));
    }
}
