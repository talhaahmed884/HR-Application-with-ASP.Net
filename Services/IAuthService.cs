using HR_Application.Models.DTOs;
using HR_Application.Models.Wrappers;

namespace HR_Application.Services;

public interface IAuthService
{
    Task<(bool Success, LoginResponseDto? Response, ErrorResponse? Error)> LoginAsync(LoginRequestDto loginRequest);

}
