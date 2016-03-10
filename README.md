Realtime Data API with SignalR and RethinkDB
========================

This project is an example of how you can utilize the powerful change feed feature that RethinkDB offers, with your WebApi APIs.
Main idea is to be able to push changes in RethinkDB to clients via SignalR.

# WebApi project

## Hubs
A simple WebApi project (Owin based) that has two SignalR Hubs:
* CoreEventsHub - For infrastructure events saved to a RethinkDB table.
* ClientPushHub - For system time changes.

## Workers 
Two workers are instantiated in ``Startup.cs``:
```
// Workers
...
bstt = new BackgroundServerTimeTimer();
// We need to run RethinkDB worker asynchrounusly otherwise it blocks the main thread.
Task.Factory.StartNew(
	BackgroundEventSubscriber.UpdateHandler,
	TaskCreationOptions.LongRunning);
...
```

The ``BackgroundServerTimeTimer`` class is simply inherited from ``IRegisteredObject`` thus registered to the hosting environment:
```
public class BackgroundServerTimeTimer : IRegisteredObject
{
	private Timer taskTimer;
	private IHubContext hub;

	public BackgroundServerTimeTimer()
	{
		HostingEnvironment.RegisterObject(this);

		hub = GlobalHost.ConnectionManager.GetHubContext<ClientPushHub>();

		taskTimer = new Timer(OnTimerElapsed, null,
			TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
	}

	private void OnTimerElapsed(object sender)
	{
		hub.Clients.All.serverTime(DateTime.UtcNow.ToString());
	}

	public void Stop(bool immediate)
	{
		taskTimer.Dispose();

		HostingEnvironment.UnregisterObject(this);
	}
}
```

The ``BackgroundEventSubscriber`` class is however static and is instantiated with ``TaskFactory``:

```
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
```

We use the community driven RethinkDB library https://github.com/bchavez/RethinkDb.Driver

# SignalR clients examples

An example of how you can subscribe to the SignalR Hubs is depicted in the following HTML files and uses the ``jquery.signalr`` library:
* monitor-rethinkdb-table.html
* monitor-server-time.html
