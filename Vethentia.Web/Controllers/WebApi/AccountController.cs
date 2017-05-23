namespace Vethentia.Web.Controllers.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin.Security;
    using Vethentia.Controllers.WebApi;
    using Vethentia.ViewModels.WebApi.Account;
    using System.Linq;
    using Vethentia.ViewModels;
    using Vethentia.Data.Models;
    using Vethentia.Services.Interfaces;
    using Microsoft.Azure.NotificationHubs.Messaging;
    using System.Net;
    using Microsoft.Azure.NotificationHubs;
    using Newtonsoft.Json;
    using UserSessionUtils;

    [RoutePrefix("api/accounts")]
    public class AccountsController : BaseApiController
    {
        private readonly IUserService userService;
        private readonly IMerchantService merchantService;
        private readonly INotificationService notificationService;
        private readonly IUserSessionService userSessionService;

        private readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AccountsController(IUserService userService, IMerchantService merchantService, INotificationService notificationSvc, IUserSessionService usrSession)
        {
            this.userService = userService;
            this.merchantService = merchantService;
            this.notificationService = notificationSvc;
            this.userSessionService = usrSession;
        }

        /// <summary>
        /// Need to send in as a Http Post and Content-Type: application/json header and the sample json
        /*
        http://vethentia.azurewebsites.net/api/accounts/login
{
"Email":"htbanh@hotmail.com",
"Password":"Mypassword123",
"RememberMe": true
}
        */
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<IHttpActionResult> Login(ViewModels.LoginViewModel model)
        {
            //if (!this.ModelState.IsValid)
            //{
            //    return this.View(model);
            //}

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await this.AppSignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {

                case SignInStatus.Success:
                    var user = await this.AppUserManager.FindByNameAsync(model.Email);

                    var request = HttpContext.Current.Request;
                    var tokenServiceUrl = request.Url.GetLeftPart(UriPartial.Authority) + request.ApplicationPath + "/Token";
                    using (var client = new HttpClient())
                    {
                        var requestParams = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("grant_type", "password"),
                            new KeyValuePair<string, string>("username", model.Email),
                            new KeyValuePair<string, string>("password", model.Password)
                        };
                        var requestParamsFormUrlEncoded = new FormUrlEncodedContent(requestParams);
                        var tokenServiceResponse = await client.PostAsync(tokenServiceUrl, requestParamsFormUrlEncoded);
                        var responseString = await tokenServiceResponse.Content.ReadAsStringAsync();
                        var responseCode = tokenServiceResponse.StatusCode;

                        var jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        var responseData =
                            jsSerializer.Deserialize<Dictionary<string, string>>(responseString);
                        var authToken = responseData["access_token"];
                        var userName = responseData["userName"];
                        //var userSessionManager = new UserSessionManager();
                        //userSessionManager.CreateUserSession(userName, authToken);

                        // Cleanup: delete expired sessions from the database
                        //userSessionManager.DeleteExpiredSessions();
                        userSessionService.CreateUserSession(userName, authToken);
                        // Cleanup: delete expired sessions from the database
                        userSessionService.DeleteExpiredSessions();

                        return Json(new { status = true, token = authToken });
                    }


                case SignInStatus.LockedOut:
                    return Json(new { status = false, error = "Lockout" });
                case SignInStatus.RequiresVerification:
                    return Json(new { status = false, error = "" });
                case SignInStatus.Failure:
                default:
                    this.ModelState.AddModelError("", "Invalid login attempt.");
                    return Json(new { status = false, error = "Invalid login attempt" });
            }
        }

        // POST http://www.keys.me/api/account/logout
        [HttpPost]
        [SessionAuthorize]
        [Route("logout")]
        public IHttpActionResult Logout()
        {
            // This does not actually perform logout! The OWIN OAuth implementation
            // does not support "revoke OAuth token" (logout) by design.
            this.Authentication.SignOut(DefaultAuthenticationTypes.ExternalBearer);

            // Delete the user's session from the database (revoke its bearer token)
            var userSessionManager = new UserSessionManager();
            userSessionManager.InvalidateUserSession();

            return this.Ok(new { message = "Logout successful." });
        }



        //[Authorize(Roles="Admin")]
        [Route("users")]
        public IHttpActionResult GetUsers()
        {
            //Only SuperAdmin or Admin can delete users (Later when implement roles)
            var identity = User.Identity as System.Security.Claims.ClaimsIdentity;
            return Ok(this.AppUserManager.Users.ToList().Select(u => this.TheModelFactory.Create(u)));
        }

        //[Authorize(Roles = "Admin")]
        [AllowAnonymous]
        [Route("user/{id:guid}", Name = "GetUserById")]
        public async Task<IHttpActionResult> GetUser(string Id)
        {
            //Only SuperAdmin or Admin can delete users (Later when implement roles)
            var user = await this.AppUserManager.FindByIdAsync(Id);

            if (user != null)
            {
                return Ok(this.TheModelFactory.Create(user));
            }

            return NotFound();

        }

        [Authorize(Roles = "Admin")]
        [Route("user/{username}")]
        public async Task<IHttpActionResult> GetUserByName(string username)
        {
            //Only SuperAdmin or Admin can delete users (Later when implement roles)
            var user = await this.AppUserManager.FindByNameAsync(username);

            if (user != null)
            {
                return Ok(this.TheModelFactory.Create(user));
            }

            return NotFound();

        }

        /// <summary>
        /// Need to send in as a Http Post and Content-Type: application/json header and the sample json
        /*
        http://vethentia.azurewebsites.net/api/accounts/register

        {
        "msgId": 15,
        "emailAddress":"htbanh@hotmail.com",
        "firstName":"John",
        "lastName":"Doe",
        "phoneNumber":"+17173683207",
        "billingZipCode":"21075",
        "billingStreetNumber":"1234",
        "password":"Mypassword01!",
        }
        */
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("register")]
        public async Task<IHttpActionResult> Register(CreateUserBindingModel model)
        {
            string slog = string.Format("Register model: {0}", JsonConvert.SerializeObject(model));
            logger.Debug(slog);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string phoneCode = string.Empty;
            Random rnd = new Random(DateTime.Now.Second);
            phoneCode = rnd.Next(1000, 9999).ToString();

            var user = new User()
            {
                UserName = model.emailAddress,
                Email = model.emailAddress,
                FirstName = model.firstName,
                LastName = model.lastName,
                PhoneNumber = model.phoneNumber,
                PostalCode = model.billingZipCode,
                StreetAddress = model.billingStreetNumber,
                RegisteredAt = DateTime.Now,
                PhoneNumberCode = phoneCode
            };

            bool val = userService.IsPhoneNumberRegistered(model.phoneNumber);
            if (val)
            {
                ModelState.AddModelError("", string.Format("{0} is already registered.", model.phoneNumber));
                return BadRequest(ModelState);
            }

            try
            {

                IdentityResult addUserResult = await this.AppUserManager.CreateAsync(user, model.password);

                if (!addUserResult.Succeeded)
                {
                    return GetErrorResult(addUserResult);
                }

                await AppSignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                string name = user.FirstName + " " + user.LastName;

                string code = await this.AppUserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                code = HttpUtility.UrlEncode(code);

                string uri = HttpContext.Current.Request.Url.AbsoluteUri;
                string host = uri.Substring(0, uri.IndexOf(HttpContext.Current.Request.Url.AbsolutePath));
                var callbackUrl = string.Format("{0}/Account/ConfirmEmail?userId={1}&code={2}", host, user.Id, code);

                //var callbackUrl = Url.Action("ConfirmEmail", "Account",   new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                string body = "Hello " + name + ",";
                body += "<br /><br />Please click the following link to activate your account";
                body += "<br /><a href = '" + callbackUrl + "'>Click here to activate your account.</a>";
                body += "<br /><br />Thanks";

                // Send Email out
                await AppUserManager.SendEmailAsync(user.Id, "Confirm your account", body);

                // Send SMS to phone
                await this.AppUserManager.SendSmsAsync(user.Id, "Vethentia Phone code is " + phoneCode);

            }
            catch (Exception ex)
            {
                logger.Error("Register", ex);
                throw;
            }
            Uri locationHeader = new Uri(Url.Link("GetUserById", new { id = user.Id }));

            // return Created(locationHeader, TheModelFactory.Create(user));
            RegisterResponseBindingModel resModel = new RegisterResponseBindingModel()
            {
                msgId = 16,
                phoneNumber = model.phoneNumber,
                status = 0,
                userId = user.Id
            };
            return Ok(resModel);
        }


        /// <summary>
        /// Need to send in as a Http Post and Content-Type: application/json header and the sample json
        /*
        http://vethentia.azurewebsites.net/api/accounts/updateuser

        {
        "Id":"guidnumber",
        "firstName":"John",
        "lastName":"Doe",
        "phoneNumber":"+17173683207",
        "billingZipCode":"21075",
        "billingStreetNumber":"1234",
        }
        */
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("updateuser")]
        public async Task<IHttpActionResult> UpdateUser(UpdateUserBindingModel model)
        {
            string slog = string.Format("UpdateUser model: {0}", JsonConvert.SerializeObject(model));
            logger.Debug(slog);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            User user = userService.GetUser(model.Id);
            if (user != null)
            {
                try
                {
                    bool isPhonenumberChanged = false;
                    string phoneCode = string.Empty;
                    Random rnd = new Random(DateTime.Now.Second);
                    phoneCode = rnd.Next(1000, 9999).ToString();

                    if (user.PhoneNumber != model.phoneNumber)
                    {
                        isPhonenumberChanged = true;
                        user.PhoneNumberCode = phoneCode;
                        user.PhoneNumber = model.phoneNumber;
                    }

                    user.FirstName = model.firstName;
                    user.LastName = model.lastName;
                    user.PostalCode = model.billingZipCode;
                    user.StreetAddress = model.billingStreetNumber;
                    userService.Update(model.Id, user);

                    if (isPhonenumberChanged)
                    { 
                        // Send SMS to phone
                        await this.AppUserManager.SendSmsAsync(user.Id, "Vethentia Phone code is " + phoneCode);
                    }

                    await AppSignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                catch (Exception ex)
                {
                    logger.Error("UpdateUser", ex);
                    throw;
                }
            }

            // return Created(locationHeader, TheModelFactory.Create(user));
            RegisterResponseBindingModel resModel = new RegisterResponseBindingModel()
            {
                msgId = 16,
                phoneNumber = model.phoneNumber,
                status = 0,
                userId = user.Id
            };

            return Ok(resModel);
        }



        /// <summary>
        /// Taken a http Put
        /// test sample code as http://vethentia.azurewebsites.net/api/accounts/confirmphone/7a3e2b89-b99a-47d6-8a2b-dc2bf84f27e4/token123
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPut]
        [Route("confirmphone/{id:guid}/{code}")]
        public IHttpActionResult ConfirmPhone(string Id = "", string code = "")
        {
            if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(code))
            {
                ModelState.AddModelError("", "User Id and Code are required");
                return BadRequest(ModelState);
            }

            bool val = false;

            //var user = await this.AppUserManager.FindByIdAsync(userId);

            val = userService.ConfirmPhoneCode(Id, code);
            if (val)
            {
                return Ok();
            }
            else
            {
                ModelState.AddModelError("", "Invalid userId or code");
                return BadRequest(ModelState);
            }
        }


        [AllowAnonymous]
        [HttpPut]
        [Route("ConfirmEmail/{id:guid}/{code}", Name = "ConfirmEmailRoute")]
        public async Task<IHttpActionResult> ConfirmEmail(string Id = "", string code = "")
        {
            if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(code))
            {
                ModelState.AddModelError("", "User Id and Code are required");
                return BadRequest(ModelState);
            }

            IdentityResult result = await this.AppUserManager.ConfirmEmailAsync(Id, code);

            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return GetErrorResult(result);
            }
        }



        [Authorize]
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await this.AppUserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        //[Authorize(Roles = "Admin")]
        [Route("deleteuser/{id:guid}")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteUser(string id)
        {

            //Only SuperAdmin or Admin can delete users (Later when implement roles)

            var appUser = await this.AppUserManager.FindByIdAsync(id);

            if (appUser != null)
            {
                IdentityResult result = await this.AppUserManager.DeleteAsync(appUser);

                if (!result.Succeeded)
                {
                    return GetErrorResult(result);
                }

                return Ok();

            }

            return NotFound();

        }

        [Authorize(Roles = "Admin")]
        [Route("user/{id:guid}/roles")]
        [HttpPut]
        public async Task<IHttpActionResult> AssignRolesToUser([FromUri] string id, [FromBody] string[] rolesToAssign)
        {

            var appUser = await this.AppUserManager.FindByIdAsync(id);

            if (appUser == null)
            {
                return NotFound();
            }

            var currentRoles = await this.AppUserManager.GetRolesAsync(appUser.Id);

            var rolesNotExists = rolesToAssign.Except(this.AppRoleManager.Roles.Select(x => x.Name)).ToArray();

            if (rolesNotExists.Count() > 0)
            {

                ModelState.AddModelError("", string.Format("Roles '{0}' does not exixts in the system", string.Join(",", rolesNotExists)));
                return BadRequest(ModelState);
            }

            IdentityResult removeResult = await this.AppUserManager.RemoveFromRolesAsync(appUser.Id, currentRoles.ToArray());

            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove user roles");
                return BadRequest(ModelState);
            }

            IdentityResult addResult = await this.AppUserManager.AddToRolesAsync(appUser.Id, rolesToAssign);

            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add user roles");
                return BadRequest(ModelState);
            }

            return Ok();

        }

        [Authorize(Roles = "Admin")]
        [Route("user/{id:guid}/assignclaims")]
        [HttpPut]
        public async Task<IHttpActionResult> AssignClaimsToUser([FromUri] string id, [FromBody] List<ClaimBindingModel> claimsToAssign)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var appUser = await this.AppUserManager.FindByIdAsync(id);

            if (appUser == null)
            {
                return NotFound();
            }

            foreach (ClaimBindingModel claimModel in claimsToAssign)
            {
                if (appUser.Claims.Any(c => c.ClaimType == claimModel.Type))
                {

//                    await this.AppUserManager.RemoveClaimAsync(id, ExtendedClaimsProvider.CreateClaim(claimModel.Type, claimModel.Value));
                }

//                await this.AppUserManager.AddClaimAsync(id, ExtendedClaimsProvider.CreateClaim(claimModel.Type, claimModel.Value));
            }

            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [Route("user/{id:guid}/removeclaims")]
        [HttpPut]
        public async Task<IHttpActionResult> RemoveClaimsFromUser([FromUri] string id, [FromBody] List<ClaimBindingModel> claimsToRemove)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var appUser = await this.AppUserManager.FindByIdAsync(id);

            if (appUser == null)
            {
                return NotFound();
            }

            foreach (ClaimBindingModel claimModel in claimsToRemove)
            {
                if (appUser.Claims.Any(c => c.ClaimType == claimModel.Type))
                {
//                    await this.AppUserManager.RemoveClaimAsync(id, ExtendedClaimsProvider.CreateClaim(claimModel.Type, claimModel.Value));
                }
            }

            return Ok();
        }

        /// <summary>
        /// Taken a http Put
        /// test sample code as http://vethentia.azurewebsites.net/api/accounts/registerphonetoken/7a3e2b89-b99a-47d6-8a2b-dc2bf84f27e4/token123
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPut]
        [Route("registerphonetoken/{id:guid}/{token}")]
        public async Task<IHttpActionResult> RegisterPhoneToken(string Id, string token)
        {
            var user = await this.AppUserManager.FindByIdAsync(Id);
            if (user != null)
            {
                try
                {
                    user.PhoneDeviceToken = token;
                    string regId = await notificationService.RegisterDeviceToken(Id, token);
                    user.APNSRegistrationId = regId;

                    userService.Update(Id, user);
                }
                catch (MessagingException e)
                {
                    ReturnGoneIfHubResponseIsGone(e);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return BadRequest("Bad UserId"); 
            }
            return Ok();
        }

        /// <summary>
        /// Delete deviceToken from Azure Notification Hub
        /// http://vethentia.azurewebsites.net/api/accounts/deletephonetoken/7a3e2b89-b99a-47d6-8a2b-dc2bf84f27e4/{device}/
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpDelete]
        [Route("deletephonetoken/{id:guid}/{token}")]
        public async Task<IHttpActionResult> DeletePhoneToken([FromUri] string Id, [FromUri] string token)
        {
            var user = await this.AppUserManager.FindByIdAsync(Id);
            if (user != null && user.PhoneDeviceToken == token)
            {
                try
                {
                    user.PhoneDeviceToken = string.Empty;
                    await notificationService.DeleteDeviceToken(token);
                    
                    userService.Update(Id, user);
                }
                catch (MessagingException e)
                {
                    ReturnGoneIfHubResponseIsGone(e);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return BadRequest("Bad UserId");
            }
            return Ok();
        }

        /// <summary>
        /// Need to send in as a Http Post and Content-Type: application/json and the sample json
        /*
        // Test sending a simple alert message
        // AspNetUser.Id fromUser
        // AspNetUser.Id  toUser 
        http://vethentia.azurewebsites.net/api/accounts/sendnotification
        {
            "Id":"39892482-58c3-4027-87b9-906ba8799917",         
            "ToUser":"39892482-58c3-4027-87b9-906ba8799917",     
            "Message":"Hello Wrold fm Hai"
        }
        */
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("sendnotification")]
        public async Task<IHttpActionResult> SendNotification(MessageBindingModel model)
        {
            HttpStatusCode ret = HttpStatusCode.InternalServerError;

            string pns = "apns";
            var user = await this.AppUserManager.FindByIdAsync(model.Id);
            if (user != null)
            {
                try
                {
                    NotificationOutcome outcome = await notificationService.SendMessage(pns, model.Message, user.UserName, model.ToUser);
                    if (outcome != null)
                    {
                        if (!((outcome.State == NotificationOutcomeState.Abandoned) ||
                            (outcome.State == NotificationOutcomeState.Unknown)))
                        {
                            ret = HttpStatusCode.OK;
                        }
                    }
                }
                catch (MessagingException e)
                {
                    ReturnGoneIfHubResponseIsGone(e);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return BadRequest("Bad UserId");
            }
            return Ok(ret);
        }

        private IAuthenticationManager Authentication
        {
            get
            {
                return this.Request.GetOwinContext().Authentication;
            }
        }


        private static void ReturnGoneIfHubResponseIsGone(MessagingException e)
        {
            var webex = e.InnerException as WebException;
            if (webex.Status == WebExceptionStatus.ProtocolError)
            {
                var response = (HttpWebResponse)webex.Response;
                if (response.StatusCode == HttpStatusCode.Gone)
                    throw new HttpRequestException(HttpStatusCode.Gone.ToString());
            }
        }
    }


}