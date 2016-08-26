using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebAppTestPacker.Startup))]
namespace WebAppTestPacker
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
