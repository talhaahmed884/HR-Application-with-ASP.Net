using System.Security.Claims;
using HR_Application.Models.DTOs;
using HR_Application.Models.Wrappers;
using HR_Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR_Application.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IUserService userService, ILogger<EmployeesController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployee(int id)
    {
        var (userId, userRole) = GetUserClaims();
        var (success, employee, error) = await _userService.GetEmployeeByIdAsync(id, userId, userRole);

        if (!success || error != null)
            return StatusCode(error!.StatusCode, error);

        return Ok(ApiResponse<EmployeeDto>.SuccessResponse(employee!, "Employee retrieved successfully"));
    }

    [HttpGet]
    [Authorize(Roles = "HR")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmployeeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllEmployees()
    {
        var (_, userRole) = GetUserClaims();
        var (success, employees, error) = await _userService.GetAllEmployeesAsync(userRole);

        if (!success || error != null)
            return StatusCode(error!.StatusCode, error);

        return Ok(ApiResponse<IEnumerable<EmployeeDto>>.SuccessResponse(
            employees!,
            $"{employees!.Count()} employees retrieved successfully"
        ));
    }

    [HttpPost]
    [Authorize(Roles = "HR")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto createEmployeeDto)
    {
        if (!ModelState.IsValid)
        {
            var validationErrors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(ErrorResponse.ValidationError(validationErrors));
        }

        var (_, userRole) = GetUserClaims();
        var (success, employee, error) = await _userService.CreateEmployeeAsync(createEmployeeDto, userRole);

        if (!success || error != null)
            return StatusCode(error!.StatusCode, error);

        return CreatedAtAction(
            nameof(GetEmployee),
            new { id = employee!.Id },
            ApiResponse<EmployeeDto>.SuccessResponse(employee, "Employee created successfully", System.Net.HttpStatusCode.Created)
        );
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto updateEmployeeDto)
    {
        if (!ModelState.IsValid)
        {
            var validationErrors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(ErrorResponse.ValidationError(validationErrors));
        }

        var (userId, userRole) = GetUserClaims();
        var (success, employee, error) = await _userService.UpdateEmployeeAsync(id, updateEmployeeDto, userId, userRole);

        if (!success || error != null)
            return StatusCode(error!.StatusCode, error);

        return Ok(ApiResponse<EmployeeDto>.SuccessResponse(employee!, "Employee updated successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "HR")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var (_, userRole) = GetUserClaims();
        var (success, error) = await _userService.DeleteEmployeeAsync(id, userRole);

        if (!success || error != null)
            return StatusCode(error!.StatusCode, error);

        return Ok(ApiResponse<string>.SuccessResponse($"Employee with ID {id} deleted successfully", null));
    }

    private (int userId, string userRole) GetUserClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? "0";
        var userId = int.Parse(userIdClaim);
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Employee";

        return (userId, userRole);
    }
}
