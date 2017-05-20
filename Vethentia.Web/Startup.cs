using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Vethentia.Web.Startup))]
[assembly: log4net.Config.XmlConfigurator(ConfigFile ="Web.config", Watch = true)]  // For logging
namespace Vethentia.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
