using System;
using System.ComponentModel.DataAnnotations;
using MasterApi.Core.Account.Events;
using MasterApi.Core.Account.Services;
using MasterApi.Core.EventHandling;

namespace MasterApi.Services.Account
{
    public class UserAccountValidator : IEventHandler<UserAccountEvent>
    {
        private readonly IUserAccountService _userAccountService;
        public UserAccountValidator(IUserAccountService userAccountService)
        {
            if (userAccountService == null) throw new ArgumentNullException(nameof(userAccountService));
            _userAccountService = userAccountService;
        }

        public void Handle(UserAccountEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            if (evt.Account == null) throw new ArgumentNullException("account");

            var account = evt.Account;
            var otherAccount = _userAccountService.GetById(evt.Account.UserId);
            if (otherAccount != null && otherAccount.Guid != account.Guid)
            {
                throw new ValidationException(_userAccountService.GetValidationMessage("CertificateAlreadyInUse"));
            }
        }

    }
}
