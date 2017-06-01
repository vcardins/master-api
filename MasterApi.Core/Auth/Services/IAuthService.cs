using System.Threading.Tasks;
using MasterApi.Core.Auth.Models;
using MasterApi.Core.Auth.ViewModel;
using MasterApi.Core.Common;
using MasterApi.Core.Services;

namespace MasterApi.Core.Auth.Services
{
    public interface IAuthService : IService<Audience>
    {
        Audience FindClient(string clientId);
        Task<Audience> FindClientAsync(string clientId);
        Task<bool> AddRefreshToken(RefreshToken token);
        Task<bool> RemoveRefreshToken(string refreshTokenId);
        Task<bool> RemoveRefreshToken(RefreshToken refreshToken);
        Task<RefreshToken> FindRefreshToken(string refreshTokenId);
        Task<PackedList<RefreshTokenOutput>> GetAllRefreshTokens();
    }
}
