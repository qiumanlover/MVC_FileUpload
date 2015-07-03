using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebAppMVC.Startup))]
namespace WebAppMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
