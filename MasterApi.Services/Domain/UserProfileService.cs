using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MasterApi.Core.Data.UnitOfWork;
using MasterApi.Core.Models;
using MasterApi.Core.Extensions;
using MasterApi.Core.Services;
using MasterApi.Core.ViewModels.UserProfile;
using Omu.ValueInjecter;
using System.ComponentModel.DataAnnotations;
using MasterApi.Core.Events;

namespace MasterApi.Services.Domain
{
    public class UserProfileService : Service<UserProfile>, IUserProfileService
    {
        public UserProfileService(IUnitOfWorkAsync unitOfWork)
            : base(unitOfWork)
        {
            Repository.OnEntityUpdated += HandleSaveChanges;
            Repository.OnEntityCreated += HandleSaveChanges;
            Repository.OnEntityDeleted += HandleSaveChanges;
        }

        public void HandleSaveChanges(object sender, EventArgs e)
        {
            // TODO: Execute any task upon user profile update
        }

        public async Task<UserProfileOutput> GetAsync(int userId) {
            return await GetAsync(p => p.UserId == userId);
        }

        public async Task<UserProfileOutput> GetAsync(Expression<Func<UserProfile, bool>> filter)
        {
            var query = await Repository.
                        Query(filter).
                        Include(p => p.UserAccount).
                        SelectAsync<UserProfileOutput>(x => new
                        {
                            x.UserId,
                            x.UserAccount.Username,
                            x.FirstName,
                            x.LastName,
                            x.DisplayName,
                            x.City,
                            x.ProvinceState,
                            x.UserAccount.Email,
                            x.UserAccount.Created,
                            LastLogin = x.UserAccount.LastLogin.GetValueOrDefault(),
                            UserGuid = x.UserAccount.Guid,
                            x.Iso2,
                            x.Avatar,
                        });
            var entity = query.SingleOrDefault();

            return entity == null 
                ? null
                : new UserProfileOutput().InjectFrom<NullableInjection>(entity) as UserProfileOutput;
        }

        public async Task<UserProfileOutput> UpdateProfileAsync(int userId, UserProfileInput model)
        {
            var entity = await Repository.FirstOrDefaultAsync(n => n.UserId == userId);
            if (entity == null)
            {
                return null;
            }

            entity.InjectFrom<NullableInjection>(model);

            await Repository.UpdateAsync(entity, true);

            var result = await GetAsync(userId);

            await Task.Run(() =>
            {
                RaiseEvent(new UserProfileUpdatedEvent
                {
                    User = entity,
                    Object = result,
                });
            });

            return result;
        }

        public async Task<string> UpdatePhoto(int userId, string username, string avatar)
        {
            var entity = await Repository.FirstOrDefaultAsync(n => n.UserId == userId);
            if (entity == null)
            {
                return null;
            }

            entity.Avatar = avatar;
            await Repository.UpdateAsync(entity, true);

            RaiseEvent(new UserProfileAvatarUpdatedEvent { User = entity });

            return entity.Avatar.GetPhotoPath(username);
        }
        
        public Task<string> UpdatePhotoAsync(int userId, string username, string photoId)
        {
            return Task.Run(() => UpdatePhoto(userId, username, photoId));
        }
    }

}
