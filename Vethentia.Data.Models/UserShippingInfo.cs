
namespace Vethentia.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Vethentia.Data.Models.Base;
    using Vethentia.Global;

    public class UserShippingInfo : BaseModel
    {
        [Required]
        [MaxLength(ValidationConstants.ApiKey)]
        public string UserId { get; set; }
        public virtual User User { get; set; }

        [MaxLength(500)]
        public string ShippingInfo { get; set; }

    }
}
