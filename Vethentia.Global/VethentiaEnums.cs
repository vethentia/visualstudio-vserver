using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vethentia.Global
{
    class VethentiaEnums
    {
    }

    public enum MessageType
    {
        PaymentRequest = 1,
        PaymentResponse,
        VIDRequest,
        VIDResponse,
        CodeCommand,
        CodeIndication,
        TokenRequest,
        TokenResponse,
        AuthorizationRequest,
        AuthorizationResponse,
        ErrorIndication,
        CodeViaSMSCommand,
        SignupRequest,
        SignupResponse,
        PhoneVerificationRequest,
        PhoneVerificationResponse,
        ConfigureNotificationRequest,
        ConfigureNotificationResponse
    }

    public enum NotificationHubPNS
    {
        WNS,        // Windos 8.1 or Windows Phone
        APNS,       // iOS Apple
        GCM         // Android
    }
}
