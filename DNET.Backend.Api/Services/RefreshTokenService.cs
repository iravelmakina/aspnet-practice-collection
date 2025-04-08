using DNET.Backend.Api.Models;
using DNET.Backend.DataAccess;
using DNET.Backend.DataAccess.Domain;

namespace DNET.Backend.Api.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly TableReservationsDbContext _dbContext;

    public RefreshTokenService(TableReservationsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string GenerateAndStoreRefreshToken(int userId, string ip, string userAgent)
    {
        var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        var entity = new RefreshTokenEntity
        {
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            IpAddress = ip,
            UserAgent = userAgent,
            UserId = userId
        };

        _dbContext.RefreshTokens.Add(entity);
        _dbContext.SaveChanges();

        return refreshToken;
    }


    public User? ValidateRefreshToken(string token, string ip, string userAgent)
    {
        var storedToken = _dbContext.RefreshTokens.FirstOrDefault(t => t.Token == token && !t.IsRevoked);

        if (storedToken == null)
            return null;

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            throw new ServerException("Refresh token has expired", 401);

        if (storedToken.IpAddress != ip || storedToken.UserAgent != userAgent)
            throw new ServerException("Token from unauthorized device or IP address", 401);

        var user = _dbContext.Users.FirstOrDefault(u => u.Id == storedToken.UserId);
        return user == null ? null : new User(user);
    }

    
    public void RevokeRefreshToken(string token)
    {
        var storedToken = _dbContext.RefreshTokens.FirstOrDefault(t => t.Token == token);
        if (storedToken != null)
        {
            storedToken.IsRevoked = true;
            _dbContext.SaveChanges();
        }
    }
}
