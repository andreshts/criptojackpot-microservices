using AutoMapper;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Requests;
using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Application.Configuration;

public class IdentityMappingProfile : Profile
{
    public IdentityMappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>();
        
        // CreateUserRequest -> CreateUserCommand mapping
        CreateMap<CreateUserRequest, CreateUserCommand>();
        
        // CreateUserCommand -> User mapping
        CreateMap<CreateUserCommand, User>()
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .ForMember(dest => dest.SecurityCode, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.Referrals, opt => opt.Ignore())
            .ForMember(dest => dest.ReferredBy, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordResetCodeExpiration, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        
        // Role mappings
        CreateMap<Role, RoleDto>();
        
        // Country mappings
        CreateMap<Country, CountryDto>();
        
        // UserReferral mappings
        CreateMap<UserReferralWithStats, UserReferralDto>();
    }
}