using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Application.Services;

public class ReferralService : IReferralService
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityEventPublisher _eventPublisher;

    public ReferralService(IUserRepository userRepository, IIdentityEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<User?> ValidateReferralCodeAsync(string? referralCode)
    {
        if (string.IsNullOrEmpty(referralCode))
            return null;

        return await _userRepository.GetBySecurityCodeAsync(referralCode);
    }

    public async Task CreateReferralAsync(User referrer, User referred, string referralCode)
    {
        referred.ReferredBy = new UserReferral
        {
            ReferrerId = referrer.Id,
            ReferredId = referred.Id,
            UsedSecurityCode = referralCode
        };
        
        await _userRepository.UpdateAsync(referred);
        await _eventPublisher.PublishReferralCreatedAsync(referrer, referred, referralCode);
    }
}
