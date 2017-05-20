
namespace Vethentia.ViewModels.WebApi.Payment
{
    using Global;
    using System;
    using System.ComponentModel.DataAnnotations;

    public class TokenRequestBindingModel
    {
        public int msgId { get; set; }

        public string deviceID { get; set; }

        public long tid { get; set; }

        public int gatewayId { get; set; }

        public string amount { get; set; }

        [MaxLength(3)]
        public string countryCode { get; set; }

        [MaxLength(5)]
        public string currencyCode { get; set; }


        public string ttime { get; set; }

        public string vid { get; set; }

        [MaxLength(500)]
        public string shippingInfo { get; set; }

        public int supportedNetwork { get; set; }

        [MaxLength(ValidationConstants.ApiKey)]
        public string merchantIdentifier { get; set; }

        [MaxLength(50)]
        public string merchantCapabilities { get; set; }

        [MaxLength(255)]
        public string merchantName { get; set; }

        [MaxLength(500)]
        public string lineItems { get; set; }

        public string paymentMethodTokenizationType { get; set; }

        [MaxLength(128)]
        public string publicKey { get; set; }


    }
}
