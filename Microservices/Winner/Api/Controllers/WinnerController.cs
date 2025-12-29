using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Winner.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class WinnerController : ControllerBase
{
    public WinnerController()
    {
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get()
    {
        return Ok(new { message = "Winner API is running" });
    }
}

