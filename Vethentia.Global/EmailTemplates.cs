namespace Vethentia.Global
{
    public class EmailTemplates
    {
        public static readonly string Welcome = "Welcome.html";
        public static readonly string Registration = "Registration.html";
        public static readonly string ResendActivationCode = "ResendActivationCode.html";
        public static readonly string PasswordChange = "PasswordChange.html";
        public static readonly string EmailChange = "EmailChange.html";
        public static readonly string PasswordResetRequest = "PasswordResetRequest.html";
        public static readonly string NewMessage = "NewMessage.html";
        public static readonly string NewMessageAboutPost = "NewMessageAboutPost.html";
        public static readonly string NewMessageAboutEntity = "NewMessageAboutEntity.html";

        public static readonly string TemplateLocation = "~/App_Data/EmailTemplates";

        public static string GenerateEmailPath(string email)
        {
            return string.Format("{0}/{1}", TemplateLocation, email);
        }
    }
}
