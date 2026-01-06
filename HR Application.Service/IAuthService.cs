using HR_Application.Model.DTOs;
using HR_Application.Model.Wrappers;

namespace HR_Application.Service;

public interface IAuthService
{
    Task<(bool Success, LoginResponseDto? Response, ErrorResponse? Error)> LoginAsync(LoginRequestDto loginRequest);

}
