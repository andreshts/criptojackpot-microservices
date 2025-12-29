using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Lottery.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class LotteryController : ControllerBase
{
    public LotteryController()
    {
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get()
    {
        return Ok(new { message = "Lottery API is running" });
    }
}

