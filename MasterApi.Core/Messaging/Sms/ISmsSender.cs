using System.Threading.Tasks;

namespace MasterApi.Core.Messaging.Sms
{
    public interface ISmsSender
    {
        Task<string> SendSmsAsync(SmsMessage message);
    }
}
