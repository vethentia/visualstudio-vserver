
namespace Vethentia.Services.Interfaces
{
    using Global;
    using Global.Models;
    using Microsoft.Azure.NotificationHubs;
    using System.Threading.Tasks;


    public interface INotificationService
    {
        Task<string> RegisterDeviceToken(string userId, string deviceToken);

        Task DeleteDeviceToken(string deviceToken);

        Task<NotificationOutcome> SendMessage(string pns, string message, string fromUser, string toTag);

        Task<NotificationOutcome> TokenRequest(NotificationHubPNS pns, string userId, TokenRequestModel model);
    }
}
