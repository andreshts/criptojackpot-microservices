using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Application.Interfaces;

public interface IReferralService
{
    Task<User?> ValidateReferralCodeAsync(string? referralCode);
    Task CreateReferralAsync(User referrer, User referred, string referralCode);
}

