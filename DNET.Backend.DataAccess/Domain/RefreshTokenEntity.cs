namespace DNET.Backend.DataAccess.Domain;

public class RefreshTokenEntity
{
    public int Id { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string IpAddress { get; set; } = null!;
    public string UserAgent { get; set; } = null!;
    public int UserId { get; set; }
    
    // Navigation property
    public UserEntity User { get; set; } = null!;
}
