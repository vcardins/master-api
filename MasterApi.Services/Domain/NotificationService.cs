using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MasterApi.Core.Config;
using MasterApi.Core.Data.UnitOfWork;
using MasterApi.Core.Messaging.Email;
using MasterApi.Core.Models;
using MasterApi.Core.Services;
using MasterApi.Services.Account;

namespace MasterApi.Services.Domain
{
    public class NotificationService : Service<Notification>, INotificationService
    {
        public AuthSettings Settings { get; set; }
        private readonly ILogger<UserAccountService> _logger;

        public NotificationService(IUnitOfWorkAsync unitOfWork, ILogger<UserAccountService> logger,
            IEmailSender emailSender, IOptions<AppSettings> settings) : base(unitOfWork)
        {
            _logger = logger;
            Settings = settings.Value.Auth;
        }

    }
}
