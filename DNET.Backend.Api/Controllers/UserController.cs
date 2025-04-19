using System.Security.Claims;
using DNET.Backend.Api.Models;
using DNET.Backend.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DNET.Backend.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("auth")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, IRefreshTokenService refreshTokenService, ILogger<UserController> logger)
    {
        _userService = userService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    // POST /register
    [HttpPost]
    [Route("register")]
    public IActionResult RegisterUser(RegisterUserRequest request)
    {
        _logger.LogInformation("Registering user with email={Email}", request.Email);
        
        var user = _userService.RegisterUser(request);
        
        _logger.LogInformation("Registered user with email={Email}, ID={Id}", request.Email, user.Id);
        
        return Ok(new { Message = "User registered", UserId = user.Id });
    }


    // GET /user
    [HttpGet]
    [Authorize]
    [Route("/user")]
    public IActionResult GetUser()
    {
        _logger.LogInformation("Fetching current user");
        
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
        if (userIdClaim == null)
            return Unauthorized(new { Message = "User not found" });
        var userId = int.Parse(userIdClaim.Value);

        var user = _userService.GetUser(userId);
        if (user == null)
            return Unauthorized(new ErrorResponse { Message = "User not found" });
        
        _logger.LogInformation("Fetched current user with ID={Id}", userId);

        return Ok(user);
    }


    // POST /login
    [HttpPost]
    [Route("login")]
    public IActionResult LoginUser([FromBody] LoginUserRequest request)
    {
        _logger.LogInformation("Logging in user with email={Email}", request.Email);
            
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers["User-Agent"].ToString();

        var user = _userService.LoginUser(request);
        if (user == null)
            return Unauthorized(new ErrorResponse { Message = "Invalid email or password", Status = 401 });

        var refreshToken = _refreshTokenService.GenerateAndStoreRefreshToken(user.Id, ip, userAgent);

        var claims = new List<Claim>
        {
            new("id", user.Id.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Role, user.Role)
        };
        var authResult = _userService.GetJWTvalidator().CreateJwtToken(claims);
        
        _logger.LogInformation("Logged in user with email={Email}, ID={Id}", request.Email, user.Id);

        return Ok(new
        {
            Message = "Login successful",
            UserId = user.Id,
            AuthorizationToken = authResult.Token,
            TokenExpiration = authResult.Expiration,
            RefreshToken = refreshToken
        });
    }


    // POST /refresh
    [HttpPost]
    [Route("refresh")]
    public IActionResult RefreshToken([FromBody] string token)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers["User-Agent"].ToString();
        
        _logger.LogInformation("Refreshing token {Token} for {Ip}", token, ip);
        
        var user = _refreshTokenService.ValidateRefreshToken(token, ip, userAgent);
        if (user == null)
            return Unauthorized(new { Message = "User associated with this token was not found" });

        var claims = new List<Claim>
        {
            new("id", user.Id.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Role, user.Role)
        };
        var authResult = _userService.GetJWTvalidator().CreateJwtToken(claims);
        var newRefreshToken = _refreshTokenService.GenerateAndStoreRefreshToken(user.Id, ip, userAgent);
        
        _logger.LogInformation("Refreshed token {Token} for {Ip} with ID={Id}", token, ip, user.Id);

        return Ok(new { authResult.Token, authResult.Expiration, RefreshToken = newRefreshToken });
    }


    // POST /logout
    [HttpPost]
    [Route("logout")]
    public IActionResult LogoutUser([FromBody] string token)
    {
        _logger.LogInformation("Logging out with token={Token}", token);
        
        _refreshTokenService.RevokeRefreshToken(token);
        
        _logger.LogInformation("Logged out with token={Token}", token);
        
        return Ok(new { Message = "Logout successful" });
    }


    [HttpGet]
    [Route("forget-password")]
    public async Task<IActionResult> ForgetPassword([FromQuery] string email)
    {
        _logger.LogInformation("Sending reset code to {Email}", email);

        await _userService.SendResetCode(email);
        
        _logger.LogInformation("Sent reset code to {Email}", email);
        
        return Ok(new { Message = "Please check your email for a reset code" });
    }
    
    
    
    [HttpPost]
    [Route("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
    {
        _logger.LogInformation("Resetting password for {Email}", request.Email);
        
        var user = _userService.ResetPassword(request.Email, request.ResetCode, request.NewPassword);
        
        _logger.LogInformation("Reset password for {Email} with ID{Id}", request.Email, user.Id);
        
        return Ok(new
        {
            Message = "Password has been reset",  
            UserId = user.Id
        });
    }


    [HttpGet("login-with-google")]
    public IActionResult Login([FromQuery] string? returnUrl)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleLoginCallback", new { returnUrl }),
        };
        
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-login-callback")]
    public async Task<IActionResult> GoogleLoginCallback([FromQuery] string? returnUrl)
    {
        _logger.LogInformation("Logging in with Google by returnUrl={ReturnUrl}", returnUrl);
        
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!result.Succeeded)
            return BadRequest(new ErrorResponse { Message = "Google login failed" });

        var claims = result.Principal?.Identities.FirstOrDefault()?.Claims.ToList();
        if (claims == null)
            return BadRequest(new ErrorResponse { Message = "No claims found" });
        
        var (user, authResult) = _userService.LoginWithGoogle(claims);

        _logger.LogInformation("Logged in with Google user with ID={Id}", user.Id);
        
        return Ok(new
        {
            Message = "Login successful",
            UserId = user.Id,
            AuthorizationToken = authResult.Token,
            TokenExpiration = authResult.Expiration
        });
    }
}