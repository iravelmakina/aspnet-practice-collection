using System.Security.Claims;
using DNET.Backend.Api.Models;
using DNET.Backend.Api.Options;
using DNET.Backend.DataAccess;
using DNET.Backend.DataAccess.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace DNET.Backend.Api.Services;

public interface IUserService
{
    public User RegisterUser(RegisterUserRequest request);
    public User? GetUser(int id);
    public User? LoginUser(LoginUserRequest request);
    public (User user, AuthResult authResult) LoginWithGoogle(List<Claim> claims);
    public Task SendResetCode(string email);
    public User ResetPassword(string email, string resetCode, string newPassword);
    public IJwtValidator GetJWTvalidator();
}