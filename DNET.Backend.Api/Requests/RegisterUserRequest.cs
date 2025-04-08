using System.ComponentModel.DataAnnotations;

namespace DNET.Backend.Api.Models;

public class RegisterUserRequest
{
    public string Username { get; set; } = String.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    [EmailAddress]
    public string Email { get; set; } = String.Empty;
    public string Password { get; set; } = String.Empty;
}