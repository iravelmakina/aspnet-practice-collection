using System.Security.Claims;
using Consul;
using DNET.Backend.Api.Models;
using DNET.Backend.DataAccess;
using DNET.Backend.DataAccess.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace DNET.Backend.Api.Services;

public class UserService : IUserService
{
    private readonly TableReservationsDbContext _dbContext;
    private readonly IJwtValidator _jwtValidator;
    private IConfiguration _configuration;
    
    public UserService(TableReservationsDbContext dbContext, IJwtValidator jwtValidator, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _jwtValidator = jwtValidator;
        _configuration = configuration;
    }

    public User RegisterUser(RegisterUserRequest request)
    {
        var trimmedEmail = request.Email.Trim().ToLower();
        
        var existingUser = _dbContext.Users
            .FirstOrDefault(u => u.Email == trimmedEmail);

        if (existingUser != null)
            throw new ServerException("User with this email already exists", 400);

        var salt = Guid.NewGuid().ToString();

        var user = new UserEntity
        {
            Username = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = trimmedEmail,
            PasswordHash = HashPassword(request.Password, salt),
            PasswordSalt = salt,
            LoginProvider = "Local",
            Role = trimmedEmail.EndsWith("@kse.org.ua") ? "Admin" : "User"
        };

        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();
        
        return new User(user);
    }

    public User? GetUser(int id)
    {
        var user = _dbContext.Users
            .FirstOrDefault(u => u.Id == id);

        if (user == null)
            return null;

        return new User(user);
    }
    
    public User? LoginUser(LoginUserRequest request)
    {
        var trimmedEmail = request.Email.Trim().ToLower();

        var user = _dbContext.Users
            .FirstOrDefault(u => u.Email == trimmedEmail);

        if (user == null)
            return null;
        
        if (user.LoginProvider == null)
            throw new ServerException("Invalid login provider", 401);

        var hashedPassword = HashPassword(request.Password, user.PasswordSalt);
        if (user.PasswordHash != hashedPassword)
            return null;

        return new User(user);
    }

    
    public (User user, AuthResult authResult) LoginWithGoogle(List<Claim> claims)
    {
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            throw new ServerException("Invalid email", 400);
        
        var firstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
        var lastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;

        var trimmedEmail = email.Trim().ToLower();

        var user = _dbContext.Users
            .FirstOrDefault(u => u.Email == trimmedEmail);

        if (user == null)
        {
            user = new UserEntity
            {
                Id = _dbContext.Users.Count() + 1,
                Username = firstName + "_" + lastName,
                Email = trimmedEmail,
                FirstName = firstName,
                LastName = lastName,
                LoginProvider = "Google",
                Role = trimmedEmail.EndsWith("@kse.org.ua") ? "Admin" : "User"
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            
            claims = new List<Claim>
            {
                new("id", user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            };
        }
        
        claims.Add(new(ClaimTypes.Role, user.Role));

        var authResult = _jwtValidator.CreateJwtToken(claims);

        return (new User(user), authResult);
    }

    
    
    public ResetCodeEntity GenerateResetCode(string email)
    {
        var trimmedEmail = email.Trim().ToLower();
        
        var user = _dbContext.Users
            .FirstOrDefault(u => u.Email == trimmedEmail);
        
        if (user == null)
            throw new ServerException("User not found", 404);
        
        
        var resetCode = GenerateRandomCode();
        var expiration = DateTime.UtcNow.AddMinutes(15);

        var resetRequest = new ResetCodeEntity
        {
            UserId = user.Id,
            Code = resetCode,
            ExpiresAt = expiration
        };

        _dbContext.ResetCodes.Add(resetRequest);
        _dbContext.SaveChanges();

        return resetRequest;
    }

    public User ResetPassword(string email, string resetCode, string newPassword)
    {
        var trimmedEmail = email.Trim().ToLower();
        
        var user = _dbContext.Users
            .FirstOrDefault(u => u.Email == trimmedEmail);
        
        if (user == null)
            throw new ServerException("User not found", 404);
        
        var resetRequest = _dbContext.ResetCodes
            .FirstOrDefault(r => r.UserId == user.Id && r.Code == resetCode);

        if (resetRequest == null)
            throw new ServerException("Invalid reset code", 400);
        
        if (resetRequest.ExpiresAt < DateTime.UtcNow)
            throw new ServerException("Reset code expired", 400);

        var salt = Guid.NewGuid().ToString();
        user.PasswordHash = HashPassword(newPassword, salt);
        user.PasswordSalt = salt;
        
        _dbContext.ResetCodes.Remove(resetRequest);
        _dbContext.SaveChanges();

        return new User(user);
    }
    
    
    
    public IJwtValidator GetJWTvalidator()
    {
        return _jwtValidator;
    }
    
    private static string GenerateRandomCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString(); // 6-digit code
    }
    
    private static string HashPassword(string password, string salt)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + salt));
        return Convert.ToBase64String(hash);
    }
}
