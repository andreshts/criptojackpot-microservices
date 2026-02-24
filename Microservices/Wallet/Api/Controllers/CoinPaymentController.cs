using Asp.Versioning;
using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Wallet.Application.Commands;
using CryptoJackpot.Wallet.Application.Queries;
using CryptoJackpot.Wallet.Application.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Wallet.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/coinpayments")]
public class CoinPaymentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public CoinPaymentController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }
    
    [HttpPost("transactions")]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateCoinPaymentTransactionRequest request)
    {
        var command = _mapper.Map<CreateCoinPaymentTransactionCommand>(request);
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
    
    [HttpGet("currencies")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllCurrencies(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCoinPaymentCurrenciesQuery(), cancellationToken);
        return result.ToActionResult();
    }
}
