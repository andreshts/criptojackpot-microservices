using CryptoJackpot.Order.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Order.Application.Queries;

public class GetTicketsByUserQuery : IRequest<Result<IEnumerable<TicketDto>>>
{
    public long UserId { get; set; }
}
