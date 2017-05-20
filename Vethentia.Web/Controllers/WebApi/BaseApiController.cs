namespace Vethentia.Controllers.WebApi
{
    using Microsoft.AspNet.Identity;
    using System.Web;
    using System.Web.Http;
    using System.Net.Http;
    using Microsoft.AspNet.Identity.Owin;
    using Web;
    using ViewModels.WebApi.Account;
    using ViewModels.WebApi.Payment;
    public class BaseApiController : ApiController
    {

        private ModelFactory _modelFactory;
        private PaymentModelFactory _paymentmodelFactory;
        private ApplicationUserManager _AppUserManager = null;
        private ApplicationRoleManager _AppRoleManager = null;
        private ApplicationSignInManager _AppSignInManager = null;

        protected ApplicationUserManager AppUserManager
        {
            get
            {
                return _AppUserManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        protected ApplicationRoleManager AppRoleManager
        {
            get
            {
                return _AppRoleManager ?? Request.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            }
        }

        protected ApplicationSignInManager AppSignInManager
        {
            get
            {
                return _AppSignInManager ?? Request.GetOwinContext().GetUserManager<ApplicationSignInManager>();
            }
        }


        public BaseApiController()
        {
        }


        protected ModelFactory TheModelFactory
        {
            get
            {
                if (_modelFactory == null)
                {
                    _modelFactory = new ModelFactory(this.Request, this.AppUserManager);
                }
                return _modelFactory;
            }
        }

        protected PaymentModelFactory ThePaymentModelFactory
        {
            get
            {
                if (_paymentmodelFactory == null)
                {
                    _paymentmodelFactory = new PaymentModelFactory(this.Request, this.AppUserManager);
                }
                return _paymentmodelFactory;
            }
        }

        protected IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}
