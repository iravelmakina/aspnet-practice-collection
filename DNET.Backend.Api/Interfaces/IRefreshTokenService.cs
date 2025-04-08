using DNET.Backend.Api.Models;

namespace DNET.Backend.Api.Services;

public interface IRefreshTokenService
{
    string GenerateAndStoreRefreshToken(int userId, string ip, string userAgent);
    User? ValidateRefreshToken(string token, string ip, string userAgent);
    void RevokeRefreshToken(string token);
}
