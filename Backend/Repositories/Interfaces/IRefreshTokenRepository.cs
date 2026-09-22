using ProjetFullstack.Models;

namespace ProjetFullstack.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
Task<RefreshToken?> GetByHashAsync(string tokenHash);

Task AddAsync(RefreshToken refreshToken);

Task UpdateAsync(RefreshToken refreshToken);

}
