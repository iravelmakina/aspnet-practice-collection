using System.ComponentModel.DataAnnotations;

namespace DNET.Backend.Api.Models;

public class ResetPasswordRequest
{
    [EmailAddress]
    public string Email { get; set; }
    public string ResetCode { get; set; }
    public string NewPassword { get; set; }
}