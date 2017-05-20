namespace Vethentia.Data.Models
{
    using System;

    using Base;
    using System.ComponentModel.DataAnnotations;
    using Global;
    public class UserMerchant : BaseModel
    {
        [Required]
        [MaxLength(ValidationConstants.ApiKey)]
        public string UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        [MaxLength(ValidationConstants.ApiKey)]
        public string MerchantId { get; set; }
        public virtual Merchant Merchant { get; set; }
    }
}
