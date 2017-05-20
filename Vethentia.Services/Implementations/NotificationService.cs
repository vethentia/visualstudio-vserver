namespace Vethentia.Services.Implementations
{
    using Microsoft.Azure.NotificationHubs;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Vethentia.Services.Interfaces;
    using Global.Models;
    using Global;
    using Newtonsoft.Json;
    using System.Diagnostics;
    public class NotificationService : INotificationService
    {
        private NotificationHubClient Hub { get; set; }

        private readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //public static NotificationService Instance = new NotificationService();

        public NotificationService()
        {
            Hub = NotificationHubClient.CreateClientFromConnectionString(ConfigurationManager.AppSettings["AzureHubNotificationEndpoint"], ConfigurationManager.AppSettings["AzureHubName"]);

            // Get a Push Notification channel from the PushNotificationChannelManager
            //var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

            // This is over simplification. In a real world app, you would probably be getting those through the app and loading them here
            //string[] tagsToSubscribeTo = { "vAP", "politics" };

            // Register with the Notification Hub, passing the push channel uri and the string array of tags
            //await hub.RegisterNativeAsync(channel.Uri, tagsToSubscribeTo);

        }

        public async Task<string> RegisterDeviceToken(string userId, string deviceToken)
        {
            string newRegistrationId = null;

            // make sure there are no existing registrations for this push handle (used for iOS and Android)
            if (deviceToken != null)
            {
                // Get all registration channels
                var registrations = await Hub.GetRegistrationsByChannelAsync(deviceToken, 100);

                foreach (RegistrationDescription registration in registrations)
                {
                    //if (newRegistrationId == null)
                    //{
                    //    newRegistrationId = registration.RegistrationId;
                    //}
                    //else
                    //{
                    //    await Hub.DeleteRegistrationAsync(registration);
                    //}

                    // Delete the old registration
                    await Hub.DeleteRegistrationAsync(registration);
                }

                // Register the deviceToken with the UserId.
                string uId = "userId_" + userId.Trim();
                RegistrationDescription reg = new AppleRegistrationDescription(deviceToken, new string[] { uId });
                reg = await Hub.CreateRegistrationAsync(reg);
                newRegistrationId = reg.RegistrationId;
            }

            //if (newRegistrationId == null)
            //    newRegistrationId = await Hub.CreateRegistrationIdAsync();

            return newRegistrationId;
        }

        public async Task DeleteDeviceToken(string deviceToken)
        {
            //            Hub.DeleteRegistrationAsync(deviceToken);

            if (deviceToken != null)
            {
                var registrations = await Hub.GetRegistrationsByChannelAsync(deviceToken, 100);

                foreach (RegistrationDescription registration in registrations)
                {
                    await Hub.DeleteRegistrationAsync(registration);
                }
            }
        }

        public async Task<NotificationOutcome> SendMessage(string pns, string message, string fromUser, string toUser)
        {
            string[] userTag = new string[1];
            userTag[0] = "userId_" + toUser.Trim();

            Microsoft.Azure.NotificationHubs.NotificationOutcome outcome = null;


            switch (pns.ToLower())
            {
                case "wns":
                    // Windows 8.1 / Windows Phone 8.1
                    var toast = @"<toast><visual><binding template=""ToastText01""><text id=""1"">" +
                                "From " + fromUser + ": " + message + "</text></binding></visual></toast>";
                    outcome = await Hub.SendWindowsNativeNotificationAsync(toast, userTag);
                    break;
                case "apns":
                    // iOS
                    var alert = "{\"aps\":{\"alert\":\"" + "From " + fromUser + ": " + message + "\", \"sound\":\"default\"}}"; 
                    if (!string.IsNullOrEmpty(toUser))
                        outcome = await Hub.SendAppleNativeNotificationAsync(alert, userTag);
                    else
                        outcome = await Hub.SendAppleNativeNotificationAsync(alert);
                    break;
                case "gcm":
                    // Android
                    var notif = "{ \"data\" : {\"message\":\"" + "From " + fromUser + ": " + message + "\"}}";
                    outcome = await Hub.SendGcmNativeNotificationAsync(notif, userTag);
                    break;
            }

            return outcome;

        }

        /// <summary>
        /// Note: Although the Apple devicetoken is stored in pass-in, but at the time the devicetoken was registered through our Account/RegisterPhoneToken,
        /// Azure Notification Hub is associated userId to the DeviceToken.  So the UserId is the real identifier (not the APNS devicetoken), userId_[12125...] is the real key.
        /// </summary>
        /// <param name="pns"></param>
        /// <param name="userId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<NotificationOutcome> TokenRequest(NotificationHubPNS pns, string userId, TokenRequestModel model)
        {
            string[] userTag = new string[1];
            userTag[0] = "userId_" + userId.Trim();

            Microsoft.Azure.NotificationHubs.NotificationOutcome outcome = null;
            switch (pns)
            {
                case NotificationHubPNS.APNS:
                    // iOS
                   
                    string serializeModel = TokenRequestModelAPNS(model);

                    string logStr = string.Format("APNS TokenRequest SerializeModel: {0}",serializeModel);
                    logger.Debug(logStr);

                    int n = serializeModel.IndexOf('{');
                    if (n >= 0)
                       serializeModel = serializeModel.Remove(n, 1);

                    n = serializeModel.LastIndexOf('}');
                    if (n > 0)
                       serializeModel = serializeModel.Remove(n, 1);

                    string alert = "{";
                    alert += "\"aps\":{\"alert\":\"" + "vid: " + model.vid + ", tid " + model.tid + ", merchant " + model.merchantIdentifier + "\", \"sound\":\"default\"},";
                    alert += serializeModel;
                    alert += "}";

                    logStr = string.Format("APNS TokenRequest Alert: {0}", alert);
                    logger.Debug(logStr);

                    //outcome = await Hub.SendAppleNativeNotificationAsync(alert);

                    if (!string.IsNullOrEmpty(userId))
                        outcome = await Hub.SendAppleNativeNotificationAsync(alert, userTag);
                    else
                        outcome = await Hub.SendAppleNativeNotificationAsync(alert);
                    break;
            }
            return outcome;
        }

        //public Task<NotificationOutcome> TokenRequest(string pns, TokenRequestModel model)
        //{
        //    throw new NotImplementedException();
        //}


        private string TokenRequestModelAPNS(TokenRequestModel model)
        {
            string ret = JsonConvert.SerializeObject(model);
            return ret;
        }
    }
}
