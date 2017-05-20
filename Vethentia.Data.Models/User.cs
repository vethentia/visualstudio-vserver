namespace Vethentia.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Security.Claims;

    using Base;

    using Global;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System.Threading.Tasks;
    public class User : IdentityUser, IAuditInfo
    {
        public User()
        {
        }

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

        [MaxLength(ValidationConstants.PhoneDeviceToken)]
        public string PhoneDeviceToken { get; set; }

        [MaxLength(ValidationConstants.PhoneDeviceToken)]
        public string APNSRegistrationId { get; set; }

        [MaxLength(ValidationConstants.PostalCode)]
        public string PhoneNumberCode { get; set; }

        [MaxLength(ValidationConstants.Cookie)]
        public string Cookie { get; set; }

        public DateTime? LastSeen { get; set; }

        public DateTime RegisteredAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }


    }
}
