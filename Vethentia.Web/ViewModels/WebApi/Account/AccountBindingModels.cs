namespace Vethentia.ViewModels.WebApi.Account
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web;


    public class CreateUserBindingModel
    {
        public int msgId { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string phoneNumber { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string firstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string lastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string emailAddress { get; set; }

        [Required]
        [Display(Name = "Postal Code")]
        public string billingZipCode { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string billingStreetNumber { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }

    }

    public class ChangePasswordBindingModel
    {
       
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    
    }
}