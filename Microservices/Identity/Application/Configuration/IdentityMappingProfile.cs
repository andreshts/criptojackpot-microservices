using AutoMapper;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Application.Configuration;

public class IdentityMappingProfile : Profile
{
    public IdentityMappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<User, AuthResponseDto>()
            .ForMember(dest => dest.UserGuid, opt => opt.MapFrom(src => src.UserGuid))
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.MapFrom(src => src.TwoFactorEnabled))
            .ForMember(dest => dest.ExpiresIn, opt => opt.Ignore()) // Set manually in handler
            .ForMember(dest => dest.RequiresTwoFactor, opt => opt.Ignore()); // Set manually in handler
        
        // Role mappings
        CreateMap<Role, RoleDto>();
        
        // Country mappings
        CreateMap<Country, CountryDto>();
        
        // UserReferral mappings
        CreateMap<UserReferralWithStats, UserReferralDto>();
    }
}