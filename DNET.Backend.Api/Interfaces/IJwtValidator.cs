using System.Security.Claims;
using DNET.Backend.Api.Models;

namespace DNET.Backend.Api.Services;

public interface IJwtValidator
{
    public AuthResult CreateJwtToken(List<Claim> claims);
}
