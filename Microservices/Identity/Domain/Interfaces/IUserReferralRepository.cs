using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Domain.Interfaces
{
    public interface IUserReferralRepository
    {
        Task<UserReferral?> CheckIfUserIsReferred(long userId);
        Task<UserReferral> CreateUserReferralAsync(UserReferral userReferral);
        Task<IEnumerable<UserReferral>> GetAllReferralsByUserId(long userId);
        Task<IEnumerable<UserReferralWithStats>> GetReferralStatsAsync(long userId);
    }
}
