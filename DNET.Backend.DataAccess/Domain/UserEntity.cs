namespace DNET.Backend.DataAccess.Domain;

public class UserEntity
{
    public int Id { get; set; }
    public string Username { get; set; } = String.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = String.Empty;
    public string? PasswordHash { get; set; } = String.Empty;
    public string? PasswordSalt { get; set; } = String.Empty;
    public DateTime CreatedAt { get; set; }
    public string? LoginProvider { get; set; }
    public string Role { get; set; }
    
    // Navigation property
    public List<ResetCodeEntity> ResetCodes { get; set; } = new();

}