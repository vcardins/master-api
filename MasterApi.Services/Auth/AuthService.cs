using System;
using System.Linq;
using System.Threading.Tasks;
using MasterApi.Core.Auth.Models;
using MasterApi.Core.Auth.Services;
using MasterApi.Core.Auth.ViewModel;
using MasterApi.Core.Common;
using MasterApi.Core.Data.Repositories;
using MasterApi.Core.Data.UnitOfWork;

namespace MasterApi.Services.Auth
{
    public class AuthService : Service<Audience>, IAuthService
    {
        private readonly IRepositoryAsync<RefreshToken> _repoRefreshToken;

        public AuthService(IUnitOfWorkAsync unitOfWork)
            : base(unitOfWork)
        {
            _repoRefreshToken = unitOfWork.RepositoryAsync<RefreshToken>();
        }        

        public Audience FindClient(string clientId)
        {
            return Repository.FirstOrDefault(x => x.ClientId == clientId);
        }

        public async Task<Audience> FindClientAsync(string clientId)
        {
            var client = await Repository.FirstOrDefaultAsync(x => x.ClientId == clientId);
            return client;
        }

        public async Task<bool> AddRefreshToken(RefreshToken token)
        {
            var existingToken = await _repoRefreshToken.FirstOrDefaultAsync(r => r.Subject == token.Subject && r.ClientId == token.ClientId);

            if (existingToken == null) return await _repoRefreshToken.InsertAsync(token, true) > 0;
            var result = await RemoveRefreshToken(existingToken);
            if (!result)
            {
                throw new Exception("Existing refresh token could not be deleted");
            }

            return await _repoRefreshToken.InsertAsync(token, true) > 0;
        }

        public async Task<bool> RemoveRefreshToken(string refreshTokenId)
        {
            var refreshToken = await FindRefreshToken(refreshTokenId);
            if (refreshToken != null)
            {
                return await RemoveRefreshToken(refreshToken);
            }
            return false;
        }

        public async Task<bool> RemoveRefreshToken(RefreshToken refreshToken)
        {
            return await _repoRefreshToken.DeleteAsync(refreshToken, true);
        }

        public async Task<RefreshToken> FindRefreshToken(string refreshTokenId)
        {
            return await _repoRefreshToken.FirstOrDefaultAsync(r => r.Id == refreshTokenId);
        }

        public async Task<PackedList<RefreshTokenOutput>> GetAllRefreshTokens()
        {
            var entries = _repoRefreshToken.Query().OrderBy(q => q.OrderBy(c => c.IssuedUtc))
                .SelectPagedAsync<RefreshTokenOutput>();
            return await entries;
        }
    }

}
