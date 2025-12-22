using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Application.Extensions;

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Name = user.Name,
        LastName = user.LastName,
        Phone = user.Phone,
        ImagePath = user.ImagePath,
        Status = user.Status,
        Role = user.Role.ToDto()
    };

    public static RoleDto ToDto(this Role role) => new()
    {
        Id = role.Id,
        Name = role.Name
    };
}

