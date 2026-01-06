namespace HR_Application.Model.DTOs;

public class EmployeesByRoleDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public List<EmployeeDto> Employees { get; set; } = new();
}
