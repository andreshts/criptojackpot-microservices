using System.Security.Claims;
using Asp.Versioning;
using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Order.Application.Commands;
using CryptoJackpot.Order.Application.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Order.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/orders")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public OrderController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new order with a 5-minute countdown for payment.
    /// The lottery numbers are reserved until payment is completed or order expires.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var command = _mapper.Map<CreateOrderCommand>(request);
        command.UserId = userId.Value;

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Completes an order after successful payment.
    /// Creates a ticket (confirmed purchase) from the order.
    /// Must be called within 5 minutes of order creation.
    /// </summary>
    [HttpPost("{orderId:guid}/complete")]
    public async Task<IActionResult> CompleteOrder([FromRoute] Guid orderId, [FromBody] CompleteOrderRequest request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var command = _mapper.Map<CompleteOrderCommand>(request);
        command.OrderId = orderId;
        command.UserId = userId.Value;

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Cancels a pending order and releases reserved lottery numbers.
    /// </summary>
    [HttpPost("{orderId:guid}/cancel")]
    public async Task<IActionResult> CancelOrder([FromRoute] Guid orderId)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var command = new CancelOrderCommand
        {
            OrderId = orderId,
            UserId = userId.Value,
            Reason = "User cancelled"
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    private long? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
