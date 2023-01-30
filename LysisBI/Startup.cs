using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LysisBI.Startup))]
namespace LysisBI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
