using System.ComponentModel.DataAnnotations;
using DNET.Backend.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DNET.Backend.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HttpController : ControllerBase
{
    private readonly IHttpService _httpService;

    
    public HttpController(IHttpService httpService)
    {
        _httpService = httpService;
    }

    
    [HttpGet("test")]
    public async Task<IActionResult> Test([FromQuery] [EmailAddress] string email)
    {
        var result = await _httpService.GetConfirmationCode(email);
        return Ok(result);
    }
}
