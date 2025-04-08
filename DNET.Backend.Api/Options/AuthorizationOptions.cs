namespace DNET.Backend.Api.Options;

public class AuthorizationOptions
{
    public Dictionary<string, UserAuthInfo> ApiKeys { get; set; } = new();
}

public class UserAuthInfo
{
    public int UserId { get; set; }
    public DateTime Expiration { get; set; }
}
