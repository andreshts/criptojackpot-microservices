using CryptoJackpot.Identity.Data.Context;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;
namespace CryptoJackpot.Identity.Data.Repositories;

public class RoleRepository : IRoleRepository
{
    private const string DefaultRoleName = "client";
    
    private readonly IdentityDbContext _context;
    
    public RoleRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public Task<List<Role>> GetAllRoles()
        => _context.Roles.AsNoTracking().ToListAsync();

    public Task<Role?> GetByNameAsync(string name)
        => _context.Roles.FirstOrDefaultAsync(r => r.Name == name);

    public Task<Role?> GetDefaultRoleAsync()
        => GetByNameAsync(DefaultRoleName);
}