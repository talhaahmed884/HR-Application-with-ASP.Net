namespace HR_Application.Model.Entities;

public class UserPassword
{
    public int UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string? Salt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
