using System;
using MasterApi.Core.Account.Events;
using MasterApi.Core.EventHandling;
using MasterApi.Services.Messaging.Email;

namespace MasterApi.Services.Account.Messaging
{
    public class EmailUserAccountEventsHandler : EmailEventHandler<UserAccountEvent>,
        IEventHandler<AccountCreatedEvent>,
        IEventHandler<MobileVerifiedEvent>,
        IEventHandler<AccountClosedEvent>,
        IEventHandler<AccountReopenedEvent>,
        IEventHandler<EmailChangeRequestedEvent>,
        IEventHandler<EmailChangedEvent>,
        IEventHandler<EmailVerifiedEvent>,
        IEventHandler<MobilePhoneChangedEvent>,
        IEventHandler<MobilePhoneChangeRequestedEvent>,
        IEventHandler<MobilePhoneRemovedEvent>,
        IEventHandler<PasswordResetRequestedEvent>,
        IEventHandler<PasswordChangedEvent>,
        IEventHandler<PasswordResetSecretAddedEvent>,
        IEventHandler<PasswordResetSecretRemovedEvent>,
        IEventHandler<UsernameReminderRequestedEvent>,        
        IEventHandler<UsernameChangedEvent>
    {
        protected override string TemplateFolder => Module; 
        
        public EmailUserAccountEventsHandler(IServiceProvider serviceProvider) 
            : base(serviceProvider) {
        }

        private void Process(UserAccountEvent evt)
        {
            evt.AppInfo = Settings.Information;
            evt.Urls = Settings.Urls;
            Send(evt, evt.Account.Email);
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
