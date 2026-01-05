using System.ComponentModel.DataAnnotations;

namespace HR_Application.Models.DTOs;

public class CreateEmployeeDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
    public string? Address { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? CellNumber { get; set; }

    [Required(ErrorMessage = "RoleId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Valid RoleId is required")]
    public int RoleId { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;
}
