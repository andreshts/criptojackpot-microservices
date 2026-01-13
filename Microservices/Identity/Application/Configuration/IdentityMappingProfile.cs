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
        
        // Role mappings
        CreateMap<Role, RoleDto>();
        
        // Country mappings
        CreateMap<Country, CountryDto>();
        
        // UserReferral mappings
        CreateMap<UserReferralWithStats, UserReferralDto>();
    }
}