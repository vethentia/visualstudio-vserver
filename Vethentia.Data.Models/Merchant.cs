namespace Vethentia.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using Base;

    using Global;

    public class Merchant : IAuditInfo, IDeletableEntity
    {
        public Merchant()
        {
            string guid = Guid.NewGuid().ToString("N");
            Id = guid;
            Alias = guid;
        }

        [Key]
        [MaxLength(ValidationConstants.ApiKey)]
        public string Id { get; set; }

        [MaxLength(ValidationConstants.ApiKey)]
        public string Alias { get; set; }

        [Required]
        [MaxLength(ValidationConstants.ApiKey)]
        public string MerchantName { get; set; }

        [Required]
        [MaxLength(ValidationConstants.UserFirstNameMaxLength)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(ValidationConstants.UserLastNameMaxLength)]
        public string LastName { get; set; }

        [MaxLength(ValidationConstants.StreetAddress)]
        public string StreetAddress { get; set; }

        [MaxLength(ValidationConstants.City)]
        public string City { get; set; }

        [MaxLength(ValidationConstants.State)]
        public string State { get; set; }

        [MaxLength(ValidationConstants.PostalCode)]
        public string PostalCode { get; set; }

        [MaxLength(ValidationConstants.Email)]
        public string Email { get; set; }

        [MaxLength(ValidationConstants.Phone)]
        public string Phone { get; set; }

        public int SupportNetwork { get; set; }

        public int MerchantCapabilities { get; set; }

        public string PaymnetMethodTokenizationType { get; set; }

        public DateTime? LastSeen { get; set; }

        public DateTime RegisteredAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
