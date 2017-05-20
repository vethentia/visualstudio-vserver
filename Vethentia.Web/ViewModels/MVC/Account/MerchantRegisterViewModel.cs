using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vethentia.ViewModels.MVC.Account
{
    public class MerchantRegisterViewModel
    {
        [Required]
        [Display(Name = "Merchant Key")]
        public string Alias { get; set; }
        [Required]
        [Display(Name = "MerchantName")]
        public string MerchantName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string StreetAddress { get; set; }

        [Required]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required]
        [Display(Name = "State")]
        public string State { get; set; }

        [Required]
        [Display(Name = "Zip Code")]
        public string PostCode { get; set; }

    }

}
