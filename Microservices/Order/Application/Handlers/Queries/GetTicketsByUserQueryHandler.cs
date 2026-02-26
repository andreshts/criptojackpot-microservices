using AutoMapper;
using CryptoJackpot.Order.Application.DTOs;
using CryptoJackpot.Order.Application.Queries;
using CryptoJackpot.Order.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Order.Application.Handlers.Queries;

public class GetTicketsByUserQueryHandler : IRequestHandler<GetTicketsByUserQuery, Result<IEnumerable<TicketDto>>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IMapper _mapper;

    public GetTicketsByUserQueryHandler(
        ITicketRepository ticketRepository,
        IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<TicketDto>>> Handle(
        GetTicketsByUserQuery request,
        CancellationToken cancellationToken)
    {
        var tickets = await _ticketRepository.GetByUserIdAsync(request.UserId);
        var ticketDtos = _mapper.Map<IEnumerable<TicketDto>>(tickets);
        return Result.Ok(ticketDtos);
    }
}
