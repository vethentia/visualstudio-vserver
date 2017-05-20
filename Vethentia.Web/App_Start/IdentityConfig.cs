
namespace Vethentia.Web
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin;
    using Microsoft.Owin.Security;
    using Vethentia.ViewModels;
    using Data.Models;
    using Data;
    using Services.Interfaces;
    using Global;
    using System.Configuration;
    using System.Diagnostics;
    using Twilio;
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            CustomSmtpClient smtp = new CustomSmtpClient("gmailConfig");
            string from = ConfigurationManager.AppSettings["SupportEmailAddress"];

            var mailMsg = new System.Net.Mail.MailMessage(from, message.Destination);
            mailMsg.Subject = message.Subject;
            mailMsg.Body = message.Body;
            mailMsg.IsBodyHtml = true;

            return smtp.SendMailAsync(mailMsg);
        }
    }


    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            string sid = ConfigurationManager.AppSettings["TwilioAccountSID"];
            string authToken = ConfigurationManager.AppSettings["TwilioAuthToken"];
            string fromPhone = ConfigurationManager.AppSettings["TwilioNumber"];

            var twilio = new TwilioRestClient(sid, authToken);

            var result = twilio.SendMessage(fromPhone, message.Destination, message.Body);

            Trace.TraceInformation(result.Status);

            // Twilio doesn't currently have an async API, so we return success.
            return Task.FromResult(0);

        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<User>
    {
        public ApplicationUserManager(IUserStore<User> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<User>(context.Get<VethentiaDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<User>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(10);
            manager.MaxFailedAccessAttemptsBeforeLockout = 10;

            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<User>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    public class ApplicationSignInManager : SignInManager<User, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(User user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager, DefaultAuthenticationTypes.ApplicationCookie);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }

    public class ApplicationRoleManager : RoleManager<IdentityRole>
    {
        public ApplicationRoleManager(IRoleStore<IdentityRole, string> roleStore)
            : base(roleStore)
        {
        }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            var appRoleManager = new ApplicationRoleManager(new RoleStore<IdentityRole>(context.Get<VethentiaDbContext>()));

            return appRoleManager;
        }
    }
}
