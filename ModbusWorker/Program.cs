using ModbusWorker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddHostedService<TestWorker>();
    })
    .Build();

await host.RunAsync();
