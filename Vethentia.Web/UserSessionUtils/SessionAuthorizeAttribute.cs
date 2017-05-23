namespace Vethentia.Web.UserSessionUtils
{
    using Services.Interfaces;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;


    public class SessionAuthorizeAttribute : AuthorizeAttribute
    {
        //       protected KeysmeData Data { get; private set; }

        //public IUserSessionService userSessionService { get; set; }


        //public SessionAuthorizeAttribute(IRepository<UserSession> session)
        //{
        //    this.Data = session;
        //}

        //public SessionAuthorizeAttribute()
        //{
        //}

        /*
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
        */


        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (SkipAuthorization(actionContext))
            {
                return;
            }

            // Get the request lifetime scope so you can resolve services.
            var requestScope = actionContext.Request.GetDependencyScope();

            // Resolve the service you want to use.
            var sessionService = requestScope.GetService(typeof(IUserSessionService)) as IUserSessionService;
            if (sessionService != null)
            {
                if (sessionService.ReValidateSession())
                {
                    base.OnAuthorization(actionContext);
                }
                else
                {
                    actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(
                        HttpStatusCode.Unauthorized, "Session token expired or not valid.");
                }
            }
        }


        private static bool SkipAuthorization(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any()
                   || actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any();
        }
    }
}