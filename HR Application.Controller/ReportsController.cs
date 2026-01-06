using System.Security.Claims;
using HR_Application.Model.DTOs;
using HR_Application.Model.Wrappers;
using HR_Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_Application.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "HrOnly")]
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
        var (success, roleCounts, error) = await _userService.GetRoleCountsAsync();

        if (!success || error != null)
        {
            _logger.LogWarning("Failed to retrieve role counts");
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
        var (success, employeesByRole, error) = await _userService.GetEmployeesByRoleAsync();

        if (!success || error != null)
        {
            _logger.LogWarning("Failed to retrieve employees by role");
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
        var (success, roleCounts, error) = await _userService.GetRoleCountsAsync();

        if (!success || error != null)
        {
            _logger.LogWarning("Failed to retrieve summary");
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
