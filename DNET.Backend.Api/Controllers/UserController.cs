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

    public UserController(IUserService userService, IRefreshTokenService refreshTokenService)
    {
        _userService = userService;
        _refreshTokenService = refreshTokenService;
    }

    // POST /register
    [HttpPost]
    [Route("register")]
    public IActionResult RegisterUser(RegisterUserRequest request)
    {
        try
        {
            var user = _userService.RegisterUser(request);
            return Ok(new { Message = "User registered", UserId = user.Id });
        }
        catch (ServerException e)
        {
            return StatusCode(e.WrongCode, new ErrorResponse
            {
                Message = e.WrongMessage,
                Status = e.WrongCode
            });
        }
    }


    // GET /user
    [HttpGet]
    [Authorize]
    [Route("/user")]
    public IActionResult GetUser()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
        if (userIdClaim == null)
            return Unauthorized(new { Message = "User not found" });
        var userId = int.Parse(userIdClaim.Value);

        var user = _userService.GetUser(userId);
        if (user == null)
            return Unauthorized(new ErrorResponse { Message = "User not found" });

        return Ok(user);
    }


    // POST /login
    [HttpPost]
    [Route("login")]
    public IActionResult LoginUser([FromBody] LoginUserRequest request)
    {
        try
        {
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

            return Ok(new
            {
                Message = "Login successful",
                UserId = user.Id,
                AuthorizationToken = authResult.Token,
                TokenExpiration = authResult.Expiration,
                RefreshToken = refreshToken
            });
        }
        catch (ServerException e)
        {
            return StatusCode(e.WrongCode, new ErrorResponse
            {
                Message = e.WrongMessage,
                Status = e.WrongCode
            });
        }
    }


    // POST /refresh
    [HttpPost]
    [Route("refresh")]
    public IActionResult RefreshToken([FromBody] string token)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers["User-Agent"].ToString();
        try
        {
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

            return Ok(new { authResult.Token, authResult.Expiration, RefreshToken = newRefreshToken });
        }
        catch (ServerException e)
        {
            return StatusCode(e.WrongCode, new ErrorResponse
            {
                Message = e.WrongMessage,
                Status = e.WrongCode
            });
        }
    }


    // POST /logout
    [HttpPost]
    [Route("logout")]
    public IActionResult LogoutUser([FromBody] string token)
    {
        _refreshTokenService.RevokeRefreshToken(token);
        return Ok(new { Message = "Logout successful" });
    }


    [HttpGet]
    [Route("forget-password")]
    public async Task<IActionResult> ForgetPassword([FromQuery] string email)
    {
        try
        {
            await _userService.SendResetCode(email);
            
            return Ok(new { Message = "Please check your email for a reset code" });
        }
        catch (ServerException e)
        {
            return StatusCode(e.WrongCode, new ErrorResponse
            {
                Message = e.WrongMessage,
                Status = e.WrongCode
            });
        }
    }
    
    
    
    [HttpPost]
    [Route("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var user = _userService.ResetPassword(request.Email, request.ResetCode, request.NewPassword);
            
            return Ok(new
            {
                Message = "Password has been reset",  
                UserId = user.Id
            });
        }
        catch (ServerException e)
        {
            return StatusCode(e.WrongCode, new ErrorResponse
            {
                Message = e.WrongMessage,
                Status = e.WrongCode
            });
        }
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
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!result.Succeeded)
            return BadRequest(new ErrorResponse { Message = "Google login failed" });

        var claims = result.Principal?.Identities.FirstOrDefault()?.Claims.ToList();
        if (claims == null)
            return BadRequest(new ErrorResponse { Message = "No claims found" });
        
        try
        {
            var (user, authResult) = _userService.LoginWithGoogle(claims);

            return Ok(new
            {
                Message = "Login successful",
                UserId = user.Id,
                AuthorizationToken = authResult.Token,
                TokenExpiration = authResult.Expiration
            });
        }
        catch (ServerException e)
        {
            return StatusCode(e.WrongCode, new ErrorResponse
            {
                Message = e.WrongMessage,
                Status = e.WrongCode
            });
        }
    }
}