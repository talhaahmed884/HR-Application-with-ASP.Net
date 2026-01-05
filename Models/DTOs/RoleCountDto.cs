namespace HR_Application.Models.DTOs;

public class RoleCountDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
}
