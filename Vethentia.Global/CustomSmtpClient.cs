
 namespace Vethentia.Global
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Configuration;
    using System.Net.Mail;
    using System.Text;
    using System.Threading.Tasks;
    using System.Configuration;

    public class CustomSmtpClient : IDisposable
    {
        private readonly SmtpClient _smtpClient;

        public CustomSmtpClient(string sectionName)
        {
            SmtpSection section = (SmtpSection)ConfigurationManager.GetSection("mailSettings/" + sectionName);

            _smtpClient = new SmtpClient();

            if (section != null)
            {
                _smtpClient.DeliveryMethod = section.DeliveryMethod;

                if (section.DeliveryMethod == SmtpDeliveryMethod.Network )
                {
                    _smtpClient.Host = section.Network.Host;
                    _smtpClient.Port = section.Network.Port;
                    _smtpClient.UseDefaultCredentials = section.Network.DefaultCredentials;

                    _smtpClient.Credentials = new NetworkCredential(section.Network.UserName, section.Network.Password, section.Network.ClientDomain);
                    _smtpClient.EnableSsl = section.Network.EnableSsl;

                    if (section.Network.TargetName != null)
                        _smtpClient.TargetName = section.Network.TargetName;
                }

                if (section.SpecifiedPickupDirectory != null && section.SpecifiedPickupDirectory.PickupDirectoryLocation != null)
                    _smtpClient.PickupDirectoryLocation = section.SpecifiedPickupDirectory.PickupDirectoryLocation;
            }
        }

        public void Send(MailMessage message)
        {
            _smtpClient.Send(message);
        }

        public Task SendMailAsync(MailMessage message)
        {
            return _smtpClient.SendMailAsync(message);
        }


        public void Dispose()
        {
            _smtpClient.Dispose();
        }
    }
}
