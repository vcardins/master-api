using System;
using MasterApi.Core.Account.Events;
using MasterApi.Core.EventHandling;
using MasterApi.Services.Messaging.RealTime;
using Omu.ValueInjecter;
using MasterApi.Core.Enums;
using MasterApi.Core.ViewModels.UserProfile;

namespace MasterApi.Services.Account.Messaging
{
    /// <summary>
    /// Message Request class
    /// </summary>
    public class RealTimeUserAccountEventsHandler : RealTimeEventHandler<UserAccountEvent>,
        IEventHandler<AccountCreatedEvent>,
        IEventHandler<PasswordResetRequestedEvent>,
        IEventHandler<PasswordChangedEvent>,
        IEventHandler<PasswordResetSecretAddedEvent>,
        IEventHandler<PasswordResetSecretRemovedEvent>,
        IEventHandler<UsernameReminderRequestedEvent>,
        IEventHandler<AccountClosedEvent>,
        IEventHandler<AccountReopenedEvent>,
        IEventHandler<UsernameChangedEvent>,
        IEventHandler<EmailChangeRequestedEvent>,
        IEventHandler<EmailChangedEvent>,
        IEventHandler<EmailVerifiedEvent>,
        IEventHandler<MobilePhoneChangedEvent>,
        IEventHandler<MobilePhoneChangeRequestedEvent>,
        IEventHandler<MobilePhoneRemovedEvent>,
        IEventHandler<MobileVerifiedEvent>
    {
        public RealTimeUserAccountEventsHandler(IServiceProvider serviceProvider) 
            : base(serviceProvider) { }

        private void Process(UserAccountEvent evt, object extra = null)
        {
            evt.AppInfo = Settings.Information;
            evt.Urls = Settings.Urls;
            var contact = new UserContactInfo().InjectFrom(evt.Account) as UserContactInfo;
            Send(contact, NotificationTypes.AccountCreated);
        }

        public void Handle(AccountCreatedEvent evt) => Process(evt);

        public void Handle(MobileVerifiedEvent evt) => Process(evt);

        public void Handle(AccountClosedEvent evt) => Process(evt);

        public void Handle(AccountReopenedEvent evt) => Process(evt);

        public void Handle(PasswordResetRequestedEvent evt) => Process(evt);

        public void Handle(PasswordChangedEvent evt) => Process(evt);

        public void Handle(PasswordResetSecretAddedEvent evt) => Process(evt);

        public void Handle(PasswordResetSecretRemovedEvent evt) => Process(evt);

        public void Handle(UsernameReminderRequestedEvent evt) => Process(evt);

        public void Handle(UsernameChangedEvent evt) => Process(evt);

        public void Handle(EmailChangeRequestedEvent evt) => Process(evt);

        public void Handle(EmailChangedEvent evt) => Process(evt);

        public void Handle(EmailVerifiedEvent evt) => Process(evt);

        public void Handle(MobilePhoneChangedEvent evt) => Process(evt);

        public void Handle(MobilePhoneRemovedEvent evt) => Process(evt);

        public void Handle(MobilePhoneChangeRequestedEvent evt) => Process(evt);
    }

}
