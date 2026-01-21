using AutoMapper;
using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Domain.Core.Requests;
using CryptoJackpot.Lottery.Application.Commands;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Requests;
using CryptoJackpot.Lottery.Domain.Models;

namespace CryptoJackpot.Lottery.Application.Configuration;

public class LotteryMappingProfile : Profile
{
    public LotteryMappingProfile()
    {
        // Prize mappings
        CreateMap<Prize, PrizeDto>();
        CreateMap<PrizeImage, PrizeImageDto>();
        
        // LotteryDraw mappings
        CreateMap<LotteryDraw, LotteryDrawDto>();
        
        // Request to Command mappings
        CreateMap<CreatePrizeRequest, CreatePrizeCommand>();
        CreateMap<UpdatePrizeRequest, UpdatePrizeCommand>();
        CreateMap<CreateLotteryDrawRequest, CreateLotteryDrawCommand>();
        CreateMap<UpdateLotteryDrawRequest, UpdateLotteryDrawCommand>();
        
        // Command to Entity mappings
        CreateMap<CreatePrizeCommand, Prize>()
            .ForMember(dest => dest.PrizeGuid, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.AdditionalImages, opt => opt.MapFrom(src => 
                src.AdditionalImageUrls.Select(img => new PrizeImage
                {
                    ImageUrl = img.ImageUrl,
                    Caption = img.Caption,
                    DisplayOrder = img.DisplayOrder
                }).ToList()));

        CreateMap<CreateLotteryDrawCommand, LotteryDraw>()
            .ForMember(dest => dest.LotteryGuid, opt => opt.Ignore())
            .ForMember(dest => dest.LotteryNo, opt => opt.Ignore())
            .ForMember(dest => dest.SoldTickets, opt => opt.Ignore())
            .ForMember(dest => dest.Prizes, opt => opt.Ignore())
            .ForMember(dest => dest.LotteryNumbers, opt => opt.Ignore());

        // LotteryNumber mappings
        CreateMap<LotteryNumber, LotteryNumberDto>();

        // Pagination mappings
        CreateMap<PaginationRequest, Pagination>();
        
        // PagedList mappings
        CreateMap<PagedList<Prize>, PagedList<PrizeDto>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.PageNumber, opt => opt.MapFrom(src => src.PageNumber))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize));

        CreateMap<PagedList<LotteryDraw>, PagedList<LotteryDrawDto>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.PageNumber, opt => opt.MapFrom(src => src.PageNumber))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize));
    }
}


