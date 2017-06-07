using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Automation_Foreman.Startup))]
namespace Automation_Foreman
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
