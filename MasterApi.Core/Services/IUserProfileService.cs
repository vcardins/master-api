using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MasterApi.Core.Models;
using MasterApi.Core.ViewModels.UserProfile;

namespace MasterApi.Core.Services
{
    public interface IUserProfileService : IService<UserProfile>
    {
        Task<UserProfileOutput> GetAsync(int id);
        Task<UserProfileOutput> GetAsync(Expression<Func<UserProfile, bool>> query);
        Task<UserProfileOutput> UpdateProfileAsync(int userId, UserProfileInput input);
        Task<string> UpdatePhotoAsync(int userId, string username, string photoId);
    }
}
