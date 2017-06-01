using System.Threading.Tasks;

namespace MasterApi.Core.Messaging.Email
{
    public interface IEmailSender
    {
        void Send(EmailMessage message, EmailAttachment attachment = null);

        Task SendAsync(EmailMessage message, EmailAttachment attachment = null);
    }
}
