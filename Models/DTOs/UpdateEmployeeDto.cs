using System.ComponentModel.DataAnnotations;

namespace HR_Application.Models.DTOs;

public class UpdateEmployeeDto
{
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string? Name { get; set; }

    [MaxLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
    public string? Address { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? CellNumber { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Valid RoleId is required")]
    public int? RoleId { get; set; }

    public bool? IsActive { get; set; }
}
