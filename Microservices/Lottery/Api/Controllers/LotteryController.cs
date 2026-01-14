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
[Route("api/v{version:apiVersion}/lotteries")]
public class LotteryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public LotteryController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateLottery([FromBody] CreateLotteryDrawRequest request)
    {
        var command = _mapper.Map<CreateLotteryDrawCommand>(request);
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllLotteries([FromQuery] PaginationRequest pagination)
    {
        var query = new GetAllLotteryDrawsQuery
        {
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [HttpGet("{lotteryId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLotteryById([FromRoute] Guid lotteryId)
    {
        var query = new GetLotteryDrawByIdQuery { LotteryId = lotteryId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpPut("{lotteryId:guid}")]
    public async Task<IActionResult> UpdateLottery([FromRoute] Guid lotteryId, [FromBody] UpdateLotteryDrawRequest request)
    {
        var command = _mapper.Map<UpdateLotteryDrawCommand>(request);
        command.LotteryId = lotteryId;
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpDelete("{lotteryId:guid}")]
    public async Task<IActionResult> DeleteLottery([FromRoute] Guid lotteryId)
    {
        var command = new DeleteLotteryDrawCommand { LotteryId = lotteryId };
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}

