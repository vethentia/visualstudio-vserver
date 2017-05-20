
namespace Vethentia.Data.Models
{
    using Base;
    using Global;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class PaymentLog : IAuditInfo, IDeletableEntity
    {
        public PaymentLog()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        [Key]
        [MaxLength(ValidationConstants.ApiKey)]
        public string Id { get; set; }

        [MaxLength(ValidationConstants.ApiKey)]
        public string TId { get; set; }

        [MaxLength(ValidationConstants.UserFirstNameMaxLength)]
        public string TTime { get; set; }

        [MaxLength(ValidationConstants.Email)]
        public string Email { get; set; }

        [MaxLength(ValidationConstants.ApiKey)]
        public string Mac { get; set; }

        [MaxLength(500)]
        public string ShippingInfo { get; set; }

        [MaxLength(20)]
        public string Amount { get; set; }

        [MaxLength(3)]
        public string CountryCode { get; set; }

        [MaxLength(5)]
        public string CurrencyCode { get; set; }

        public int SupportedNetwork { get; set; }

        [MaxLength(ValidationConstants.ApiKey)]
        public string MerchantId { get; set; }

        [MaxLength(255)]
        public string MechantName { get; set; }

        public int MerchantCapability { get; set; }

        [MaxLength(500)]
        public string LineItems { get; set; }

        [MaxLength(20)]
        public string PaymentMethodTokenizationType { get; set; }

        [MaxLength(128)]
        public string PublicKey { get; set; }

        public DateTime RegisteredAt { get; set; }

        [MaxLength(ValidationConstants.Cookie)]
        public string Cookie { get; set; }

        public int GatewayId { get; set; }

        [MaxLength(ValidationConstants.PhoneDeviceToken)]
        public string Token { get; set; }

        [MaxLength(12)]
        public string CodeCheck { get; set; }

        [Required]
        public int CodeCheckCount { get; set; }

        public bool IsCodeCheckValidated { get; set; }

        public int Status { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedOn { get; set; }

    }
}
