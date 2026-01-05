namespace HR_Application.Models.DTOs;

public class EmployeesByRoleDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public List<EmployeeDto> Employees { get; set; } = new();
}
