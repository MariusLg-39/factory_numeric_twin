using Microsoft.EntityFrameworkCore;
using ProjetFullstack.Data;
using ProjetFullstack.Models;
using ProjetFullstack.Repositories.Interfaces;

namespace ProjetFullstack.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
private readonly ApplicationDbContext _context;

public RefreshTokenRepository(ApplicationDbContext context)
{
    _context = context;
}

public async Task<RefreshToken?> GetByHashAsync(
    string tokenHash)
{
    return await _context.RefreshTokens
        .Include(x => x.User)
        .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
}

public async Task AddAsync(RefreshToken refreshToken)
{
    await _context.RefreshTokens.AddAsync(refreshToken);

    await _context.SaveChangesAsync();
}

public async Task UpdateAsync(RefreshToken refreshToken)
{
    _context.RefreshTokens.Update(refreshToken);

    await _context.SaveChangesAsync();
}

}
