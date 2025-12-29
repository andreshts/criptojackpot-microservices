using CryptoJackpot.Identity.Data.Context;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Identity.Data.Repositories
{
    public class UserReferralRepository(IdentityDbContext context) : IUserReferralRepository
    {
        public async Task<UserReferral?> CheckIfUserIsReferred(long userId)
         => await context.UserReferrals.FirstOrDefaultAsync(x => x.ReferredId == userId);

        public async Task<UserReferral> CreateUserReferralAsync(UserReferral userReferral)
        {
            await context.UserReferrals.AddAsync(userReferral);
            await context.SaveChangesAsync();

            return await context.UserReferrals
                .Include(ur => ur.Referrer)
                .Include(ur => ur.Referred)
                .FirstAsync(ur => ur.Id == userReferral.Id);
        }

        public async Task<IEnumerable<UserReferral>> GetAllReferralsByUserId(long userId)
            => await context.UserReferrals.Where(x => x.ReferrerId == userId).ToListAsync();

        public async Task<IEnumerable<UserReferralWithStats>> GetReferralStatsAsync(long userId)
            => await context.UserReferrals
                .Where(ur => ur.ReferrerId == userId)
                .Select(ur => new UserReferralWithStats
                {
                    UsedSecurityCode = ur.UsedSecurityCode,
                    RegisterDate = ur.Referred.CreatedAt,
                    FullName = ur.Referred.Name + " " + ur.Referred.LastName,
                    Email = ur.Referred.Email,
                })
                .ToListAsync();
    }
}
