namespace Vethentia.ViewModels.WebApi.Payment
{
    using Data.Models;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Web.Http.Routing;
    using Web;
    public class PaymentModelFactory
    {

        private UrlHelper _UrlHelper;
        private ApplicationUserManager _AppUserManager;

        public PaymentModelFactory(HttpRequestMessage request, ApplicationUserManager appUserManager)
        {
            _UrlHelper = new UrlHelper(request);
            _AppUserManager = appUserManager;
        }

        public PayRequestReturnModel Create(PaymentLog pay)
        {
            return new PayRequestReturnModel
            {
                Url = _UrlHelper.Link("GetPaymentById", new { id = pay.Id }),
                Id = pay.Id
            };

        }

        /*
        public RoleReturnModel Create(IdentityRole appRole)
        {

            return new RoleReturnModel
            {
                Url = _UrlHelper.Link("GetRoleById", new { id = appRole.Id }),
                Id = appRole.Id,
                Name = appRole.Name
            };

        }
        */
    }

    public class PayRequestReturnModel
    {

        public string Url { get; set; }
        public string Id { get; set; }
    }


}
