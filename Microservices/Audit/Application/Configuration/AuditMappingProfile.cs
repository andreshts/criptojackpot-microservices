using AutoMapper;
using CryptoJackpot.Audit.Application.Commands;
using CryptoJackpot.Audit.Application.DTOs;
using CryptoJackpot.Audit.Domain.Helpers;
using CryptoJackpot.Audit.Domain.Models;

namespace CryptoJackpot.Audit.Application.Configuration;

/// <summary>
/// AutoMapper profile for Audit mappings.
/// </summary>
public class AuditMappingProfile : Profile
{
    public AuditMappingProfile()
    {
        // Command to Entity
        CreateMap<CreateAuditLogCommand, AuditLog>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Timestamp, opt => opt.Ignore())
            .ForMember(dest => dest.Request, opt => opt.MapFrom(src => new AuditRequestInfo
            {
                Endpoint = src.Endpoint,
                Method = src.HttpMethod,
                IpAddress = src.IpAddress,
                UserAgent = src.UserAgent
            }))
            .ForMember(dest => dest.Response, opt => opt.MapFrom(src => new AuditResponseInfo
            {
                StatusCode = src.StatusCode,
                DurationMs = src.DurationMs
            }))
            .ForMember(dest => dest.OldValue, opt => opt.MapFrom(src => BsonHelper.ToBsonDocument(src.OldValue)))
            .ForMember(dest => dest.NewValue, opt => opt.MapFrom(src => BsonHelper.ToBsonDocument(src.NewValue)))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => BsonHelper.ToBsonDocument(src.Metadata)));

        // Entity to DTO
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType.ToString()))
            .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.OldValue, opt => opt.MapFrom(src => src.OldValue != null ? src.OldValue.ToString() : null))
            .ForMember(dest => dest.NewValue, opt => opt.MapFrom(src => src.NewValue != null ? src.NewValue.ToString() : null))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata != null ? src.Metadata.ToString() : null));

        CreateMap<AuditRequestInfo, AuditRequestDto>();
        CreateMap<AuditResponseInfo, AuditResponseDto>();
    }
}
