using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vethentia.Global;

namespace Vethentia.ViewModels.WebApi.Payment
{
    public class PayRequestBindingModel
    {
        public int msgId { get; set; }

        public long tid { get; set; }

        public string ttime { get; set; }

        public string vid { get; set; }

        [MaxLength(500)]
        public string shippingInfo { get; set; }

        public string amount { get; set; }

        [MaxLength(3)]
        public string countryCode { get; set; }

        [MaxLength(5)]
        public string currencyCode { get; set; }

        [MaxLength(ValidationConstants.ApiKey)]
        public string merchantIdentifier { get; set; }

        [MaxLength(255)]
        public string merchantName { get; set; }

        [MaxLength(500)]
        public string lineItems { get; set; }

        [MaxLength(128)]
        public string publicKey { get; set; }

        public string messageauthenticationcode { get; set; }

    }
}
