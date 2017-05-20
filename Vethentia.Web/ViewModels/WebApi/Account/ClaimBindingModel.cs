
namespace Vethentia.ViewModels.WebApi.Account
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web;

    public class ClaimBindingModel
    {
        [Required]
        [Display(Name = "Claim Type")]
        public string Type { get; set; }

        [Required]
        [Display(Name = "Claim Value")]
        public string Value { get; set; }
    }
}