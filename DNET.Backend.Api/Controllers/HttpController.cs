using System.ComponentModel.DataAnnotations;
using DNET.Backend.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DNET.Backend.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HttpController : ControllerBase
{
    private readonly IHttpService _httpService;
    private readonly ILogger<HttpController> _logger;

    
    public HttpController(IHttpService httpService, ILogger<HttpController> logger)
    {
        _httpService = httpService;
        _logger = logger;
    }

    
    [HttpGet("test")]
    public async Task<IActionResult> Test([FromQuery] [EmailAddress] string email)
    {
        _logger.LogInformation("Fetching confirmation code for {Email}", email);
        
        var result = await _httpService.GetConfirmationCode(email);
        
        _logger.LogInformation("Fetched confirmation code for {Email}", email);
        
        return Ok(result);
    }
}
