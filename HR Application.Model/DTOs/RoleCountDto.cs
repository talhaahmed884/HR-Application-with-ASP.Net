namespace HR_Application.Model.DTOs;

public class RoleCountDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
}
