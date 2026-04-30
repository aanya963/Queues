
### What Are Hosted Services?
In ASP.NET Core, a hosted service is:
A background process that runs alongside your web app (independent of HTTP requests).

Think of it like:
    A worker running in the background
    Starts when your app starts
    Stops when your app shuts down

# In ASP.NET Core, services have lifetimes:

Lifetime	    Meaning
Singleton	    One instance for whole app
Scoped	        One per request
Transient	    New every time

# Core Interface 
Every hosted service implements: IHostedService

public interface IHostedService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}

🔹 What happens:
    StartAsync → runs when app starts
    StopAsync → runs during shutdown


🧱 Easier Base Class: BackgroundService
Instead of implementing everything manually, you usually inherit:
public abstract class BackgroundService : IHostedService
{
    protected abstract Task ExecuteAsync(CancellationToken stoppingToken);
}

👉 You only focus on:
protected override async Task ExecuteAsync(CancellationToken stoppingToken)

# 🔁 1. Timer-Based Background Task : Run something repeatedly (like cron job)
    public class TimedService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("Running task...");

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }


# 🧩 2. Using Scoped Services (VERY Important)

A long-living object (hosted service) is trying to use a short-living object (scoped service)
❗ Problem:
    Hosted services are singleton, but:
    DB contexts (DbContext) are scoped
    👉 You cannot inject scoped services directly.
✅ Solution: Create a Scope
    Use : IServiceScopeFactory

Example:
    public class ScopedWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ScopedWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var service = scope.ServiceProvider.GetRequiredService<MyScopedService>();

            await service.DoWorkAsync();
        }
    }


# 📬 3. Queued Background Tasks (Producer–Consumer)
Instead of running immediately: Add tasks to a queue -> Process them one by one

🧱 Step 1: Create Queue

public class BackgroundTaskQueue
{
    private readonly ConcurrentQueue<Func<Task>> _queue = new();

    public void Enqueue(Func<Task> task)
    {
        _queue.Enqueue(task);
    }

    public async Task<Func<Task>> DequeueAsync()
    {
        while (!_queue.TryDequeue(out var task))
        {
            await Task.Delay(100);
        }
        return task;
    }
}

🧱 Step 2: Worker Service

public class QueuedWorker : BackgroundService
{
    private readonly BackgroundTaskQueue _queue;

    public QueuedWorker(BackgroundTaskQueue queue)
    {
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _queue.DequeueAsync();

            await workItem();
        }
    }
}

🧱 Step 3: Add Tasks

queue.Enqueue(async () =>
{
    Console.WriteLine("Processing task...");
    await Task.Delay(1000);
});

# 🧩 How to Register Hosted Services
In Program.cs:
    builder.Services.AddHostedService<TimedService>();
    builder.Services.AddHostedService<QueuedWorker>();

