using System.Security.Claims;
using DNET.Backend.Api.Models;
using DNET.Backend.Api.Services;
using DNET.Backend.DataAccess;
using DNET.Backend.DataAccess.Domain;
using Moq;

namespace DNET.Backend.Api.Tests;


[Collection("UserServiceTests")]
public sealed class UserServiceTests
{
    private TableReservationsDbContext _dbContext;
    private UserService _userService;

    public UserServiceTests()
    {
        var mockJwtValidator = new Mock<IJwtValidator>();
        var mockHttpService = new Mock<IHttpService>();
        
        _dbContext = Utils.CreateInMemoryDatabaseContext();
        _userService = new UserService(_dbContext, mockJwtValidator.Object, mockHttpService.Object);
    } 
    
    
    [Fact]
    public void RegisterUser_ShouldCreateUser()
    {
        var request = new RegisterUserRequest
        {
            Username = "johndoe",
            FirstName = "John",
            LastName = "Doe",
            Email = "  JOHNDOE@GMAIL.COM  ",
            Password = "password"
        };

        var result = _userService.RegisterUser(request);

        Assert.NotNull(result);
        Assert.Equal("johndoe", result.Username);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("User", result.Role);
        Assert.Equal(1, _dbContext.Users.Count());
    }


    [Fact]
    public void RegisterUserKse_ShouldCreateAdmin()
    {
        var request = new RegisterUserRequest
        {
            Username = "johndoe",
            FirstName = "John",
            LastName = "Doe",
            Email = "  JOHNDOE@KSE.ORG.UA  ",
            Password = "password"
        };

        var result = _userService.RegisterUser(request);

        Assert.NotNull(result);
        Assert.Equal("johndoe", result.Username);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("Admin", result.Role);
        Assert.Equal(1, _dbContext.Users.Count());
    }


    [Fact]
    public void RegisterUser_ShouldThrowException_WhenUserExists()
    {
        _dbContext.Users.Add(new UserEntity
        {
            Username = "johndoe", 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "johndoe@gmail.com", 
            PasswordHash = "hashedpassword", 
            PasswordSalt = "salt",
            CreatedAt = DateTime.Now, 
            LoginProvider = "Local", 
            Role = "User"
        });
        _dbContext.SaveChanges();

        var request = new RegisterUserRequest
        {
            Username = "johndoe",
            FirstName = "John",
            LastName = "Doe",
            Email = "  JOHNDOE@GMAIL.COM  ",
            Password = "password"
        };

        var ex = Assert.Throws<ServerException>(() => _userService.RegisterUser(request));
        
        Assert.Equal("User with this email already exists", ex.WrongMessage);
        Assert.Equal(400, ex.WrongCode);
    }


    [Fact]
    public void GetUser_ShouldReturnUser()
    {
        var salt = Guid.NewGuid().ToString();
        var password = "password";
        var hash = UserService.HashPassword(password, salt);
        
        _dbContext.Users.Add(new UserEntity
        {
            Username = "johndoe", 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "johndoe@gmail.com", 
            PasswordHash = hash, 
            PasswordSalt = salt,
            CreatedAt = DateTime.Now, 
            LoginProvider = "Local", 
            Role = "User"
        });
        _dbContext.SaveChanges();
        
        var result = _userService.GetUser(1);
        
        Assert.NotNull(result);
        Assert.Equal("johndoe", result.Username);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("User", result.Role);
    }
    
    
    [Fact]
    public void GetUser_ThrowException_WhenUserNotFound()
    {
        var ex = Assert.Throws<ServerException>(() => _userService.GetUser(1));
        
        Assert.Equal("User not found", ex.WrongMessage);
        Assert.Equal(404, ex.WrongCode);
    }


    [Fact]
    public void LoginUser_ShouldReturnUser()
    {
        var salt = Guid.NewGuid().ToString();
        var password = "password";
        var hash = UserService.HashPassword(password, salt);

        _dbContext.Users.Add(new UserEntity
        {
            Username = "johndoe", 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "johndoe@gmail.com", 
            PasswordHash = hash, 
            PasswordSalt = salt,
            CreatedAt = DateTime.Now, 
            LoginProvider = "Local", 
            Role = "User"
        });
        _dbContext.SaveChanges();

        var result = _userService.LoginUser(new LoginUserRequest
        {
            Email = "  JOHNDOE@GMAIL.COM  ", 
            Password = password
        });

        Assert.NotNull(result);
        Assert.Equal("johndoe", result.Username);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("User", result.Role);
    }


    [Fact]
    public void LoginUser_ShouldThrowException_WhenUserNotFound()
    {
        var request = new LoginUserRequest
        {
            Email = "  JOHNDOE@GMAIL.COM  ",
            Password = "password"
        };
        var ex = Assert.Throws<ServerException>(() => _userService.LoginUser(request));
        
        Assert.Equal("User not found", ex.WrongMessage);
        Assert.Equal(404, ex.WrongCode);
    } 
    
    
    [Fact]
    public void LoginUser_ShouldThrowException_WhenLoginProviderIsNull()
    {
        var salt = Guid.NewGuid().ToString();
        var password = "password";
        var hash = UserService.HashPassword(password, salt);

        _dbContext.Users.Add(new UserEntity
        {
            Username = "johndoe", 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "johndoe@gmail.com", 
            PasswordHash = hash, 
            PasswordSalt = salt,
            CreatedAt = DateTime.Now, 
            LoginProvider = null, 
            Role = "User"
        });
        _dbContext.SaveChanges();

        var request = new LoginUserRequest
        {
            Email = "  JOHNDOE@GMAIL.COM  ",
            Password = password
        };
        
        var ex = Assert.Throws<ServerException>(() => _userService.LoginUser(request));
        
        Assert.Equal("Invalid login provider", ex.WrongMessage);
        Assert.Equal(401, ex.WrongCode);
        
    }
    
    
    [Fact]
    public void LoginUser_ShouldThrowExceptionl_WhenInvalidPassword()
    {
        var salt = Guid.NewGuid().ToString();
        var password = "password";
        var hash = UserService.HashPassword(password, salt);

        _dbContext.Users.Add(new UserEntity
        {
            Username = "johndoe", 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "johndoe@gmail.com", 
            PasswordHash = hash, 
            PasswordSalt = salt,
            CreatedAt = DateTime.Now, 
            LoginProvider = "Local", 
            Role = "User"
        });
        _dbContext.SaveChanges();

        var request = new LoginUserRequest
        {
            Email = "  JOHNDOE@GMAIL.COM  ", 
            Password = "incorrect"
        };
        
        var ex = Assert.Throws<ServerException>(() => _userService.LoginUser(request));
        
        Assert.Equal("Invalid password", ex.WrongMessage);
        Assert.Equal(401, ex.WrongCode);
    }
    
    
    [Fact]
    public void LoginWithGoogle_ShouldReturnUserAndAuthResult_WhenUserExists()
    {
        var claims = new List<Claim>
        {
            new (ClaimTypes.Email, "johndoe@gmail.com"),
            new (ClaimTypes.GivenName, "John"),
            new (ClaimTypes.Surname, "Doe")
        };

        _dbContext.Users.Add(new UserEntity
        {
            Username = "johndoe",
            FirstName = "John",
            LastName = "Doe",
            Email = "johndoe@gmail.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTime.Now,
            LoginProvider = "Google",
            Role = "User"
        });
        _dbContext.SaveChanges();

        var jwtValidator = new Mock<IJwtValidator>();
        jwtValidator.Setup(j => j.CreateJwtToken(It.IsAny<List<Claim>>()))
            .Returns(new AuthResult { Token = "mockToken", Expiration = 10 });

        var userService = new UserService(_dbContext, jwtValidator.Object, new Mock<IHttpService>().Object);

        var result = userService.LoginWithGoogle(claims);

        Assert.NotNull(result.user);
        Assert.NotNull(result.authResult);
        Assert.Equal("johndoe", result.user.Username);
        Assert.Equal("John", result.user.FirstName);
        Assert.Equal("Doe", result.user.LastName);
        Assert.Equal("User", result.user.Role);
        Assert.Equal("mockToken", result.authResult.Token);
        Assert.Equal(10, result.authResult.Expiration);
    }


    [Fact]
    public void LoginWithGoogle_ShouldCreateUserAndReturnAuth_WhenUserDoesNotExist()
    {
        var claims = new List<Claim>
        {
            new (ClaimTypes.Email, "johndoe@gmail.com"),
            new (ClaimTypes.GivenName, "John"),
            new (ClaimTypes.Surname, "Doe")
        };

        var jwtValidatorMock = new Mock<IJwtValidator>();
        jwtValidatorMock
            .Setup(j => j.CreateJwtToken(It.IsAny<List<Claim>>()))
            .Returns(new AuthResult { Token = "mockToken", Expiration = 10});
        
        var userService = new UserService(_dbContext, jwtValidatorMock.Object, new Mock<IHttpService>().Object); ;

        var result = userService.LoginWithGoogle(claims);

        Assert.NotNull(result.user);
        Assert.NotNull(result.authResult);
        Assert.Equal("John_Doe", result.user.Username);
        Assert.Equal("John", result.user.FirstName);
        Assert.Equal("Doe", result.user.LastName);
        Assert.Equal("User", result.user.Role);
        Assert.Equal("mockToken", result.authResult.Token);
        Assert.Equal(10, result.authResult.Expiration);
    }


    [Fact]
    public void LoginKseWithGoogle_ShouldCreateAdminAndReturnAuth_WhenUserDoesNotExist()
    {
        var claims = new List<Claim>
        {
            new (ClaimTypes.Email, "johndoe@kse.org.ua"),
            new (ClaimTypes.GivenName, "John"),
            new (ClaimTypes.Surname, "Doe")
        };

        var jwtValidatorMock = new Mock<IJwtValidator>();
        jwtValidatorMock
            .Setup(j => j.CreateJwtToken(It.IsAny<List<Claim>>()))
            .Returns(new AuthResult { Token = "mockToken", Expiration = 10});
        
        var userService = new UserService(_dbContext, jwtValidatorMock.Object, new Mock<IHttpService>().Object); ;

        var result = userService.LoginWithGoogle(claims);

        Assert.NotNull(result.user);
        Assert.NotNull(result.authResult);
        Assert.Equal("John_Doe", result.user.Username);
        Assert.Equal("John", result.user.FirstName);
        Assert.Equal("Doe", result.user.LastName);
        Assert.Equal("Admin", result.user.Role);
        Assert.Equal("mockToken", result.authResult.Token);
        Assert.Equal(10, result.authResult.Expiration);
    }

    
    [Fact]
    public void LoginWithGoogle_ShouldThrowException_WhenInvalidEmail()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, ""),
            new (ClaimTypes.GivenName, "John"),
            new (ClaimTypes.Surname, "Doe")
        };
        
        var ex = Assert.Throws<ServerException>(() => _userService.LoginWithGoogle(claims));
        
        Assert.Equal("Invalid email", ex.WrongMessage);
        Assert.Equal(400, ex.WrongCode);
    }
    
    
    [Fact]
    public async Task SendResetCode_ShouldSendMail_AndSaveResetCode()
    {
        _dbContext.Users.Add(new UserEntity
        {
            Username = "johndoe",
            FirstName = "John",
            LastName = "Doe",
            Email = "johndoe@gmail.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTime.Now,
            LoginProvider = "Local",
            Role = "User"
        });
        _dbContext.SaveChanges();
        
        var httpServiceMock = new Mock<IHttpService>();
        var userService = new UserService(_dbContext, new Mock<JwtValidator>().Object, httpServiceMock.Object); ;

        await userService.SendResetCode("  JOHNDOE@GMAIL.COM  ");
        
        httpServiceMock.Verify(h =>
                h.SendMailAsync(
                    "johndoe@gmail.com",
                    "Table Reservations Password Reset",
                    It.Is<string>(body => body.Contains("Use this code") && body.Contains("<html>"))
                ),
            Times.Once);
        
        var savedCode = _dbContext.ResetCodes.FirstOrDefault(c => c.UserId == 1);
        Assert.NotNull(savedCode);
        Assert.Equal(1, savedCode.UserId);
        Assert.False(string.IsNullOrEmpty(savedCode.Code));
        Assert.True(savedCode.ExpiresAt > DateTime.UtcNow);
    }
    
    
    [Fact]
    public async Task SendResetCode_ShouldThrowException_WhenUserNotFound()
    {
        var ex = await Assert.ThrowsAsync<ServerException>(() =>
            _userService.SendResetCode("  JOHNDOE@GMAIL.COM  "));
        
        Assert.Equal("User not found", ex.WrongMessage);
        Assert.Equal(404, ex.WrongCode);
    }
    
    
    [Fact]
    public async Task SendResetCode_ShouldThrowException_WhenLoginProviderIsGoogle()
    {
        _dbContext.Users.Add(new UserEntity
        {
            Username = "johndoe",
            FirstName = "John",
            LastName = "Doe",
            Email = "johndoe@gmail.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTime.Now,
            LoginProvider = "Google",
            Role = "User"
        });
        _dbContext.SaveChanges();

        
         var ex = await Assert.ThrowsAsync<ServerException>(() =>
            _userService.SendResetCode("  JOHNDOE@GMAIL.COM  "));

        Assert.Equal("No password for Google login", ex.WrongMessage);
        Assert.Equal(400, ex.WrongCode);
    }


    [Fact]
    public void ResetPassword_ShouldUpdatePassword_WhenCodeIsValid()
    {
        var email = "  JOHNDOE@GMAIL.COM  ";
        var originalSalt = Guid.NewGuid().ToString();
        var originalHash = UserService.HashPassword("oldPassword", originalSalt);

        _dbContext.Users.Add(new UserEntity
        {
            Username = "johndoe", 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "johndoe@gmail.com", 
            PasswordHash = originalHash, 
            PasswordSalt = originalSalt,
            CreatedAt = DateTime.Now, 
            LoginProvider = "Local", 
            Role = "User"
        });

        _dbContext.ResetCodes.Add(new ResetCodeEntity
        {
            UserId = 1,
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        });
        _dbContext.SaveChanges();
        
        _userService.ResetPassword(email, "123456", "newSecurePassword");

        Assert.NotEqual(originalHash, _dbContext.Users.First().PasswordHash);
        Assert.NotEqual(originalSalt, _dbContext.Users.First().PasswordSalt);
        Assert.Empty(_dbContext.ResetCodes);
    }


    [Fact]
    public void ResetPassword_ShouldThrowException_WhenUserNotFound()
    {
        var ex = Assert.Throws<ServerException>(() =>
            _userService.ResetPassword("  JOHNDOE@GMAIL.COM  ", "123456", "newSecurePassword"));

        Assert.Equal("User not found", ex.WrongMessage);
        Assert.Equal(404, ex.WrongCode);
    }
    
    
    [Fact]
    public void ResetPassword_ShouldThrowException_WhenResetCodeInvalid()
    {
        _dbContext.Users.Add(new UserEntity
        {
            Username = "johndoe",
            FirstName = "John",
            LastName = "Doe",
            Email = "johndoe@gmail.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTime.Now,
            LoginProvider = "Local",
            Role = "User"
        });
        _dbContext.SaveChanges();
        
        var ex = Assert.Throws<ServerException>(() =>
            _userService.ResetPassword("  JOHNDOE@GMAIL.COM  ", "wrongcode", "newSecurePassword"));

        Assert.Equal("Invalid reset code", ex.WrongMessage);
        Assert.Equal(400, ex.WrongCode);
    }
    
    
    [Fact]
    public void ResetPassword_ShouldThrowException_WhenResetCodeExpired()
    {
        _dbContext.Users.Add(new UserEntity
        {
            Username = "johndoe",
            FirstName = "John",
            LastName = "Doe",
            Email = "johndoe@gmail.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTime.Now,
            LoginProvider = "Local",
            Role = "User"
        });

        _dbContext.ResetCodes.Add(new ResetCodeEntity
        {
            UserId = 1,
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1) // expired
        });
        _dbContext.SaveChanges();
        
        var ex = Assert.Throws<ServerException>(() =>
            _userService.ResetPassword("  JOHNDOE@GMAIL.COM  ", "123456", "newSecurePassword"));

        Assert.Equal("Reset code expired", ex.WrongMessage);
        Assert.Equal(400, ex.WrongCode);
    }
}
