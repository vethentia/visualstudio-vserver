
namespace Vethentia.ViewModels.WebApi.Payment
{
    using Global;
    using System;
    using System.ComponentModel.DataAnnotations;

    public class PayLoadBindingModel
    {
        public int msgId { get; set; }

        public long tid { get; set; }

        public string ttime { get; set; }

        public string vid { get; set; }

        public string shippingInfo { get; set; }

        public string amount { get; set; }

        [MaxLength(3)]
        public string countryCode { get; set; }

        [MaxLength(5)]
        public string currencyCode { get; set; }

        [MaxLength(ValidationConstants.ApiKey)]
        public string merchantIdentifier { get; set; }

        [MaxLength(255)]
        public string mechantName { get; set; }

        [MaxLength(500)]
        public string lineItems { get; set; }

        [MaxLength(128)]
        public string publicKey { get; set; }

        public string messageauthenticationcode { get; set; }

        public string token { get; set; }

        public int status { get; set; }

        public string rxCode { get; set; }

        public string phoneNumber { get; set; }

        public string deviceID { get; set; }

        public int gatewayId { get; set; }

        public int merchantCapabilities {get; set; }

        public string paymentMethodTokenizationType { get; set; }

        public string firstName { get; set; }

        public string lastName { get; set;  }

        [MaxLength(ValidationConstants.Email)]
        public string emailAddress { get; set; }

        public string billingZipCode { get; set; }

        public string billingStreetNumber { get; set; }

        public string password { get; set; }


    }
}