using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoJackpot.Identity.Domain.Models
{
    public class UserReferralWithStats
    {
        public DateTime RegisterDate { get; set; }
        public string UsedSecurityCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
