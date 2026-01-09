using Asp.Versioning;
using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.Requests;
using CryptoJackpot.Lottery.Application.Commands;
using CryptoJackpot.Lottery.Application.Queries;
using CryptoJackpot.Lottery.Application.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Lottery.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PrizeController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public PrizeController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    // [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreatePrize([FromBody] CreatePrizeRequest request)
    {
        var command = _mapper.Map<CreatePrizeCommand>(request);
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllPrizes([FromQuery] PaginationRequest pagination)
    {
        var query = new GetAllPrizesQuery
        {
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };

        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpGet("{prizeId:guid}")]
    public async Task<IActionResult> GetPrizeById([FromRoute] Guid prizeId)
    {
        var query = new GetPrizeByIdQuery { PrizeId = prizeId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpPut("{prizeId:guid}")]
    public async Task<IActionResult> UpdatePrize([FromRoute] Guid prizeId, [FromBody] UpdatePrizeRequest request)
    {
        var command = _mapper.Map<UpdatePrizeCommand>(request);
        command.PrizeId = prizeId;

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpDelete("{prizeId:guid}")]
    public async Task<IActionResult> DeletePrize([FromRoute] Guid prizeId)
    {
        var command = new DeletePrizeCommand { PrizeId = prizeId };
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
