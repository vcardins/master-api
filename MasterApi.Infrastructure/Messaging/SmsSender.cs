using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MasterApi.Core.Config;
using MasterApi.Core.Messaging.Sms;

namespace MasterApi.Infrastructure.Messaging
{
    public class SmsSender : ISmsSender
    {
        //private static TwilioSettings _smsSettings;

        //public SmsSender(IOptions<AppSettings> settings)
        //{
        //    _smsSettings = settings.Value.Twilio;
        //}
        public Task<string> SendSmsAsync(SmsMessage message)
        {
            throw new NotImplementedException();
        }

        //public async Task<string> SendSmsAsync(SmsMessage sms)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        var byteArray = Encoding.ASCII.GetBytes($"{_smsSettings.Sid}:{_smsSettings.Token}");
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        //        var content = new FormUrlEncodedContent(new[]
        //        {
        //            new KeyValuePair<string, string>("To", sms.PhoneNumber),
        //            new KeyValuePair<string, string>("From", _smsSettings.From),
        //            new KeyValuePair<string, string>("Body", sms.Message)
        //        });
        //        var url = new Uri(_smsSettings.BaseUri + _smsSettings.RequestUri);
        //        var message = await client.PostAsync(url, content);
        //        return message.IsSuccessStatusCode ? "Success" : "Error";
        //    }
        //}
    }
}
