using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MasterApi.Core.Messaging.Email;
using RazorLight;
using Microsoft.Extensions.Logging;

namespace MasterApi.Services.Messaging.Email
{
    public abstract class EmailEventHandler<TEvent> : MessagingEventHandler<TEvent>
    {
        private readonly IEmailSender _emailService;
        private readonly IRazorLightEngine _razorEngine;
        private readonly ILogger<EmailEventHandler<TEvent>> _logger;
        private readonly IDictionary<string, Dictionary<string, string>> _subjects;

        protected abstract string TemplateFolder { get; }

        protected EmailEventHandler(IServiceProvider serviceProvider)
         : base(serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            _emailService = (IEmailSender)serviceProvider.GetService(typeof(IEmailSender));
            _emailService = (IEmailSender)serviceProvider.GetService(typeof(IEmailSender));
            _razorEngine = (IRazorLightEngine)serviceProvider.GetService(typeof(IRazorLightEngine));
            _logger = (ILogger<EmailEventHandler<TEvent>>)serviceProvider.GetService(typeof(ILogger<EmailEventHandler<TEvent>>));

            _subjects = (IDictionary<string, Dictionary<string, string>>)serviceProvider.GetService(typeof(IEmailSubjects));
        }

        private EmailMessage GetMessage(TEvent evt, string to)
        {
            var message = GetBody(evt);
            if (message == null)
                return null;

            message.Subject = GetSubject(evt);
            message.AsHtml = true;
            message.To = to;
            return message;
        }

        protected void Send(TEvent evt, string to) 
        {
            var message = GetMessage(evt, to);
            if (message==null) { return; }
            _emailService.SendAsync(message);
        }

        protected async Task SendAsync(TEvent evt, string to, IDictionary<string, string> data)
        {
            var message = GetMessage(evt, to);
            if (message == null) { return; }
            await _emailService.SendAsync(message);
        }

        private EmailMessage GetBody(TEvent evt)
        {
            var template = !string.IsNullOrEmpty(TemplateFolder) ?
                string.Format("{0}/", TemplateFolder) :
                string.Empty;

            template = string.Format("{0}{1}.cshtml", template, GetEventName(evt));

            try
            {
                var body = _razorEngine.Parse(template, evt);
                return new EmailMessage {
                    From = $"{Settings.Information.ContactName} {Settings.Information.ContactEmail}",
                    Body = body
                };
            } catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
           
        }

        private string GetSubject(TEvent evt)
        {
            Dictionary<string, string> dict;
            var subject = string.Empty;
            var ev = GetEventName(evt);

            if (_subjects.TryGetValue(TemplateFolder, out dict))
            {
                subject = !dict.TryGetValue(ev, out subject) ? "Unknown" : subject.Replace("{{ApplicationName}}", Settings.Information.Name);
            }
            return subject;
        }

    }

}
