using HR_Application.Model.DTOs;
using HR_Application.Model.Wrappers;
using HR_Application.Service;
using Microsoft.AspNetCore.Mvc;

namespace HR_Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
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

        var (success, response, error) = await _authService.LoginAsync(loginRequest);

        if (!success || error != null)
        {
            _logger.LogWarning("Login failed for email: {Email}", loginRequest.Email);
            return StatusCode(error!.StatusCode, error);
        }

        var apiResponse = ApiResponse<LoginResponseDto>.SuccessResponse(response!, "Login successful");
        return Ok(apiResponse);
    }
}
