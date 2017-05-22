namespace Vethentia.Web.UserSessionUtils
{
    using Data;
    using Data.Models;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;


    public class SessionAuthorizeAttribute : AuthorizeAttribute
    {
        //       protected KeysmeData Data { get; private set; }

        private readonly IRepository<UserSession> Data;


        public SessionAuthorizeAttribute(IRepository<UserSession> session)
        {
            this.Data = session;
        }

        public SessionAuthorizeAttribute()
        {
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (SkipAuthorization(actionContext))
            {
                return;
            }

            var userSessionManager = new UserSessionManager();
            if (userSessionManager.ReValidateSession())
            {
                base.OnAuthorization(actionContext);
            }
            else
            {
                actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized, "Session token expried or not valid.");
            }
        }

        private static bool SkipAuthorization(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any()
                   || actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any();
        }
    }
}