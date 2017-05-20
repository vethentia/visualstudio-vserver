
namespace Vethentia.ViewModels.WebApi.Account
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class MessageBindingModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [Display(Name = "To User")]
        public string ToUser { get; set; }

        [Required]
        [Display(Name = "Message")]
        public string Message { get; set; }

    }
}