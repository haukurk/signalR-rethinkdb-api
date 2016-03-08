using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using Realtime_WebApi_API.App_Start;
using Realtime_WebApi_API.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

[assembly: OwinStartup(typeof(Realtime_WebApi_API.Startup))]

namespace Realtime_WebApi_API
{
    public class Startup
    {

        private BackgroundServerTimeTimer bstt;
        //private BackgroundEventSubscriber bes;

        public void Configuration(IAppBuilder app)
        {

            // CORS
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
                
            // Workers
            bstt = new BackgroundServerTimeTimer();
            // We need to run RethinkDB worker asynchrounusly otherwise it blocks the main thread.
            Task.Factory.StartNew(
                BackgroundEventSubscriber.UpdateHandler,
                TaskCreationOptions.LongRunning);

            // SignalR
            app.MapSignalR();

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

        }
    }
}