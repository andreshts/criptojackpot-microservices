using CryptoJackpot.Identity.Domain.Models;
namespace CryptoJackpot.Identity.Domain.Interfaces;

public interface IRoleRepository
{
    Task<List<Role>> GetAllRoles();
    Task<Role?> GetByNameAsync(string name);
    Task<Role?> GetDefaultRoleAsync();
}