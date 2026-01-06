namespace HR_Application.Model.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public EmployeeDto User { get; set; } = new();
}
