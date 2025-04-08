using System.ComponentModel.DataAnnotations;

namespace DNET.Backend.Api.Models;

public class LoginUserRequest
{
    [EmailAddress]
    public string Email { get; set; } = String.Empty;
    public string Password { get; set; } = String.Empty;
}
