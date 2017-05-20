namespace Vethentia.ViewModels.WebApi.Account
{
    using System.ComponentModel.DataAnnotations;

    public class UpdateUserBindingModel
    {
        public string Id { get; set; }

        [Required]
        public string phoneNumber { get; set; }

        [Required]
        public string firstName { get; set; }

        [Required]
        public string lastName { get; set; }

        [Required]
        public string billingZipCode { get; set; }

        [Required]
        public string billingStreetNumber { get; set; }
    }
}