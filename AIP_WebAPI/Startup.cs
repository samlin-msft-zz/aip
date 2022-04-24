using Microsoft.Owin;
using Owin;
using System.Linq;
using System.Web.Http;

[assembly: OwinStartup(typeof(AIP_WebAPI.Startup))]

namespace AIP_WebAPI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
