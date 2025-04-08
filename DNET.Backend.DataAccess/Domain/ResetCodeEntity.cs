namespace DNET.Backend.DataAccess.Domain;

public class ResetCodeEntity
{
    public int Id { get; set; }
    public int UserId  { get; set; }
    public string Code { get; set; }
    public DateTime ExpiresAt { get; set; }
    
    // Navigation property
    public UserEntity User { get; set; } = null!;
}