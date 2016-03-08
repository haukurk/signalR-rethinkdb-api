using Microsoft.AspNet.SignalR;
using Realtime_WebApi_API.DataContracts;
using Realtime_WebApi_API.Hubs;
using RethinkDb;
using RethinkDb.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace Realtime_WebApi_API.Workers
{
    public static class BackgroundEventSubscriber
    {
        public static RethinkDB r = RethinkDB.R;

        public static void UpdateHandler()
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<CoreEventsHub>();

            var conn = r.Connection()
             .Hostname("localhost")
             .Port(RethinkDBConstants.DefaultPort)
             .Timeout(60)
             .Connect();

            var dbList = r.DbList();

            // Check if we need to create structure.
            if (!r.DbList().Contains("test"))
                r.DbCreate("test");
            if (!r.Db("test").TableList().Contains("events"))
                r.Db("test").TableCreate("events");

            var feed = r.Db("test").Table("events")
                              .Changes().RunChanges<InfrastructureEvent>(conn);

            foreach (var ev in feed)
                hub.Clients.All.onEvent(ev.NewValue.message.ToString());

        }
    }
}