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
        // Request to Command mappings (Controller layer)
        CreateMap<CreateUserRequest, CreateUserCommand>();
        CreateMap<UpdateUserRequest, UpdateUserCommand>();
        CreateMap<GenerateUploadUrlRequest, GenerateUploadUrlCommand>();
        CreateMap<UpdateUserImageRequest, UpdateUserImageCommand>();
        
        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<CreateUserCommand, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Set manually after hashing
            .ForMember(dest => dest.EmailVerified, opt => opt.Ignore())
            .ForMember(dest => dest.RoleId, opt => opt.Ignore()) // Set manually with default role
            .ForMember(dest => dest.UserGuid, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore())
            .ForMember(dest => dest.RecoveryCodes, opt => opt.Ignore())
            .ForMember(dest => dest.Referrals, opt => opt.Ignore())
            .ForMember(dest => dest.ReferredBy, opt => opt.Ignore());
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