
namespace Vethentia.Global.Models
{
    public class TokenRequestModel
    {
        public int msgId { get; set; }

        public string deviceID { get; set; }

        public long tid { get; set; }

        public int gatewayId { get; set; }

        public string amount { get; set; }

        public string countryCode { get; set; }

        public string currencyCode { get; set; }

        public string ttime { get; set; }

        public string vid { get; set; }

        public string shippingInfo { get; set; }

        public int supportedNetwork { get; set; }

        public string merchantIdentifier { get; set; }

        public int merchantCapabilities { get; set; }

        public string merchantName { get; set; }

        public string lineItems { get; set; }

        public string paymentMethodTokenizationType { get; set; }

        public string publicKey { get; set; }


    }
}
