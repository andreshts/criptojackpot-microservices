using AutoMapper;
using CryptoJackpot.Order.Application.Commands;
using CryptoJackpot.Order.Application.DTOs;
using CryptoJackpot.Order.Application.Requests;
using CryptoJackpot.Order.Domain.Models;

namespace CryptoJackpot.Order.Application.Configuration;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        // Entity to DTO
        CreateMap<Domain.Models.Order, OrderDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderDetails))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems));
        
        CreateMap<OrderDetail, OrderDetailDto>();
        CreateMap<Ticket, TicketDto>();

        // Request to Command
        CreateMap<CreateOrderRequest, CreateOrderCommand>();
        CreateMap<CreateOrderItemRequest, CreateOrderItemCommand>();
        CreateMap<CompleteOrderRequest, CompleteOrderCommand>();
    }
}

