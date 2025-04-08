namespace DNET.Backend.Api.Models;

public class LoginResult
{
    public User User { get; set; }
    public AuthResult AuthResult { get; set; }

    public LoginResult(User user, AuthResult authResult)
    {
        User = user;
        AuthResult = authResult;
    }
}
