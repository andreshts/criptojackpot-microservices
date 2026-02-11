using AutoMapper;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Application.Models;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Handlers.Queries;

/// <summary>
/// Handles SyncCurrentUserQuery: finds user by KeycloakId or auto-provisions
/// from Keycloak profile for self-registered users.
/// </summary>
public class SyncCurrentUserQueryHandler : IRequestHandler<SyncCurrentUserQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IKeycloakUserService _keycloakUserService;
    private readonly IStorageService _storageService;
    private readonly IMapper _mapper;
    private readonly ILogger<SyncCurrentUserQueryHandler> _logger;

    /// <summary>
    /// Default role ID for self-registered users (client/user role).
    /// </summary>
    private const long DefaultUserRoleId = 2;

    public SyncCurrentUserQueryHandler(
        IUserRepository userRepository,
        IKeycloakUserService keycloakUserService,
        IStorageService storageService,
        IMapper mapper,
        ILogger<SyncCurrentUserQueryHandler> logger)
    {
        _userRepository = userRepository;
        _keycloakUserService = keycloakUserService;
        _storageService = storageService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(SyncCurrentUserQuery request, CancellationToken cancellationToken)
    {
        // 1. Try to find by KeycloakId
        var user = await _userRepository.GetByKeycloakIdAsync(request.KeycloakId);

        if (user is not null)
        {
            var dto = _mapper.Map<UserDto>(user);
            if (!string.IsNullOrEmpty(dto.ImagePath))
                dto.ImagePath = _storageService.GetPresignedUrl(dto.ImagePath);
            return Result.Ok(dto);
        }

        // 2. Try to find by email (user might exist from admin creation without KeycloakId)
        user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is not null)
        {
            // Link existing user to Keycloak
            user.KeycloakId = request.KeycloakId;
            await _userRepository.UpdateAsync(user);
            await SetUserIdAttributeInKeycloak(request.KeycloakId, user.Id, cancellationToken);

            _logger.LogInformation(
                "Linked existing user {UserId} to Keycloak ID {KeycloakId}",
                user.Id, request.KeycloakId);

            var dto = _mapper.Map<UserDto>(user);
            if (!string.IsNullOrEmpty(dto.ImagePath))
                dto.ImagePath = _storageService.GetPresignedUrl(dto.ImagePath);
            return Result.Ok(dto);
        }

        // 3. Auto-provision: fetch full profile from Keycloak and create user in DB
        _logger.LogInformation(
            "Auto-provisioning user from Keycloak. KeycloakId={KeycloakId}, Email={Email}",
            request.KeycloakId, request.Email);

        var keycloakUser = await _keycloakUserService.GetUserByIdAsync(request.KeycloakId, cancellationToken);

        var countryId = GetCountryIdFromKeycloak(keycloakUser);
        var statePlace = GetAttributeValue(keycloakUser, "statePlace") ?? "";
        var city = GetAttributeValue(keycloakUser, "city") ?? "";

        var newUser = new User
        {
            KeycloakId = request.KeycloakId,
            Name = keycloakUser?.FirstName ?? request.FirstName ?? "",
            LastName = keycloakUser?.LastName ?? request.LastName ?? "",
            Email = request.Email,
            Phone = GetAttributeValue(keycloakUser, "phone"),
            CountryId = countryId,
            StatePlace = statePlace,
            City = city,
            Address = GetAttributeValue(keycloakUser, "address"),
            Status = request.EmailVerified,
            RoleId = DefaultUserRoleId,
        };

        var createdUser = await _userRepository.CreateAsync(newUser);

        // 4. Set user_id attribute in Keycloak so future tokens include it
        await SetUserIdAttributeInKeycloak(request.KeycloakId, createdUser.Id, cancellationToken);

        _logger.LogInformation(
            "Auto-provisioned user {UserId} from Keycloak ID {KeycloakId}",
            createdUser.Id, request.KeycloakId);

        // Reload with navigation properties
        var reloadedUser = await _userRepository.GetByIdAsync(createdUser.Id);
        var resultDto = _mapper.Map<UserDto>(reloadedUser ?? createdUser);
        return Result.Ok(resultDto);
    }

    private async Task SetUserIdAttributeInKeycloak(string keycloakId, long userId, CancellationToken cancellationToken)
    {
        try
        {
            var attributes = new Dictionary<string, List<string>>
            {
                ["user_id"] = [userId.ToString()]
            };
            await _keycloakUserService.UpdateUserAsync(
                keycloakId, attributes: attributes, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            // Non-fatal: user was created, attribute sync can be retried
            _logger.LogWarning(ex,
                "Failed to set user_id attribute in Keycloak for user {UserId}, KeycloakId {KeycloakId}",
                userId, keycloakId);
        }
    }

    private static long GetCountryIdFromKeycloak(KeycloakUserDto? keycloakUser)
    {
        var countryValue = GetAttributeValue(keycloakUser, "country");
        if (!string.IsNullOrEmpty(countryValue) && long.TryParse(countryValue, out var countryId))
            return countryId;

        // Default country ID (fallback)
        return 1;
    }

    private static string? GetAttributeValue(KeycloakUserDto? keycloakUser, string attributeName)
    {
        if (keycloakUser?.Attributes is null)
            return null;

        return keycloakUser.Attributes.TryGetValue(attributeName, out var values)
            ? values.FirstOrDefault()
            : null;
    }
}
