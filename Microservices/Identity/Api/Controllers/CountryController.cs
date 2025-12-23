using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Identity.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
namespace CryptoJackpot.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CountryController : ControllerBase
{
    private readonly IMediator _mediator;

    public CountryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllCountriesQuery();
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }
}

