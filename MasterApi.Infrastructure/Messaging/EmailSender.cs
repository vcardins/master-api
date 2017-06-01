using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SendGrid;
using MasterApi.Core.Config;
using MasterApi.Core.Messaging.Email;
using System;
using SendGrid.Helpers.Mail;

namespace MasterApi.Infrastructure.Messaging
{
    public class EmailSender : IEmailSender
    {
        private static EmailSettings _emailSettings;
        private static SendGridClient _client;

        public EmailSender(IOptions<AppSettings> settings)
        {
            _emailSettings = settings.Value.Email;
            _client = new SendGridClient(_emailSettings.ApiKey);
        }

        public void Send(EmailMessage message, EmailAttachment attachment = null)
        {
            throw new NotImplementedException();
        }

        public async Task SendAsync(EmailMessage message, EmailAttachment attachment = null)
        {
            // Send a Single Email using the Mail Helper with convenience methods and initialized SendGridMessage object
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = message.Subject,
                HtmlContent = message.Body
            };
            msg.AddTo(new EmailAddress(message.To));
            var response = await _client.SendEmailAsync(msg);
        }

    }
}
//    public void Send(EmailMessage message, EmailAttachment attachment = null)
//    {
//        var transport = GetTransport(message, attachment);
//        Task.Run(async () =>
//        {
//            await transport.DeliverAsync(_message);
//        });
//    }

//    public async Task SendAsync(EmailMessage message, EmailAttachment attachment = null)
//    {
//        var transport = GetTransport(message, attachment);
//        await transport.DeliverAsync(_message);
//    }

//    private static Web GetTransport(EmailMessage message, EmailAttachment attachment = null)
//    {
//        if (message.Recipients == null)
//        {
//            if (string.IsNullOrEmpty(message.To))
//            {
//                throw new ValidationException("Recepient is required");
//            }
//            _message.AddTo(message.To);
//        }
//        else
//        {
//            if (message.Recipients.Count == 0)
//            {
//                throw new ValidationException("Recepient is required");
//            }
//            _message.AddTo(message.Recipients);
//        }

//        // Create the email object first, then add the properties.
//        _message.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
//        _message.Subject = message.Subject;

//        if (!message.AsHtml) { 
//            _message.Text = message.Body;
//        } else { 
//            _message.Html = message.Body;
//        }

//        if (attachment != null) { 
//            _message.AddAttachment(attachment.File, attachment.Name);
//        }

//        // Create credentials, specifying your user name and password.
//        var credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);

//        // Create a REST transport for sending email.
//        return new Web(credentials);
//    }
//}
    