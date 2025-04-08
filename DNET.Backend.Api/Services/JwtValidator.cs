using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DNET.Backend.Api.Models;
using DNET.Backend.DataAccess.Domain;
using Microsoft.IdentityModel.Tokens;

namespace DNET.Backend.Api.Services;

public class JwtValidator : IJwtValidator
{
    private const string Token = "your_secret_key_should_be_long_enough_at_least_512_bits_long_to_secure_the_token";

    public AuthResult CreateJwtToken(List<Claim> claims)
    {
        var expiration = 15 * 60; // 15 minutes
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddSeconds(expiration),
            SigningCredentials = new SigningCredentials(CreateSecurityKey(), SecurityAlgorithms.HmacSha512Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new AuthResult
        {
            Token = tokenHandler.WriteToken(token),
            Expiration = expiration
        };
    }

    public static TokenValidationParameters CreateTokenValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = CreateSecurityKey(),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            LifetimeValidator = (notBefore, expires, token, parameters) =>
            {
                if (expires == null)
                    return false;

                return expires > DateTime.UtcNow;
            }
        };
    }

    private static SymmetricSecurityKey CreateSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Token));
    }
}
