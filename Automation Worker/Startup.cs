using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Automation_Worker.Startup))]
namespace Automation_Worker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
