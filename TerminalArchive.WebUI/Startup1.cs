using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(TerminalArchive.WebUI.Startup1))]

namespace TerminalArchive.WebUI
{
    public class Startup1
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
        }

        //public void ConfigureServices(IServiceCollection services)
        //{
        //    services.AddMvc();

        //    services.AddAuthorization(options =>
        //    {
        //        options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrator"));
        //    });
        //}
    }
}
