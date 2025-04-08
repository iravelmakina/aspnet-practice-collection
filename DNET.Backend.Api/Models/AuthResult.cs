namespace DNET.Backend.Api.Models;

public class AuthResult
{
    public string Token { get; set; }
    public long Expiration { get; set; }
}
