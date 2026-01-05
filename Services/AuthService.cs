using HR_Application.DataAccess.Repositories;
using HR_Application.Models.DTOs;
using HR_Application.Models.Enums;
using HR_Application.Models.Wrappers;
using HR_Application.Services.Utils;

namespace HR_Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthRepository _authRepository;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IAuthRepository authRepository,
        JwtTokenGenerator jwtTokenGenerator,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _authRepository = authRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<(bool Success, LoginResponseDto? Response, ErrorResponse? Error)> LoginAsync(LoginRequestDto loginRequest)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(loginRequest.Email);
            if (user == null)
            {
                _logger.LogWarning("Login attempt failed: User not found - {Email}", loginRequest.Email);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.AuthErrors.InvalidCredentials));
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt failed: Account inactive - UserId: {UserId}", user.Id);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.EmployeeErrors.UserInactive));
            }

            var userPassword = await _authRepository.GetUserPasswordAsync(user.Id);
            if (userPassword == null)
            {
                _logger.LogError("Password record not found for UserId: {UserId}", user.Id);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.CommonErrors.InternalServerError));
            }

            var passwordHash = PasswordHasher.HashPassword(loginRequest.Password);
            if (!string.Equals(passwordHash, userPassword.PasswordHash, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Login attempt failed: Invalid credentials - UserId: {UserId}", user.Id);
                return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.AuthErrors.InvalidCredentials));
            }

            var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, user.RoleName ?? "Employee");
            var expiresIn = _jwtTokenGenerator.GetExpirationSeconds();

            var loginResponse = new LoginResponseDto
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresIn = expiresIn,
                User = new EmployeeDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Address = user.Address,
                    CellNumber = user.CellNumber,
                    RoleId = user.RoleId,
                    RoleName = user.RoleName ?? "Employee",
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                }
            };

            _logger.LogInformation("User logged in successfully - UserId: {UserId}, Email: {Email}", user.Id, user.Email);
            return (true, loginResponse, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", loginRequest.Email);
            return (false, null, ErrorResponse.FromErrorCode(ErrorCodes.CommonErrors.InternalServerError, ex.Message));
        }
    }
}
