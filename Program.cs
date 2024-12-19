using Libplanet.RocksDBStore;
using Libplanet.Store.Remote.Executable;
using Libplanet.Store.Remote.Server;
using Libplanet.Store.Trie;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using Serilog;

public static class Program
{
    public static async Task Main(string[] args)
    {
        await ConsoleApp.RunAsync(args, Run);
    }

    /// <summary>
    /// Run server with given arguments and options.
    /// </summary>
    /// <param name="path">Path to key-value store. (e.g., /path/to/snapshot/states)</param>
    /// <param name="port">Port number to listen gRPC requests.</param>
    private static async Task Run([Argument] string path, CancellationToken cancellationToken, int port = 5000)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        var builder = WebApplication.CreateBuilder();

        // Add services to the container.
        builder.Services.AddGrpc();

        // If use `RocksDBStore`, try this:
        builder.Services.AddSingleton<IKeyValueStore>(_ => new RocksDBKeyValueStore(path, RocksDBInstanceType.Secondary));

        builder.Services.AddSingleton<Serilog.ILogger>(
            new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Warning()
                .CreateLogger());

        builder.Services.AddHealthChecks();
        builder.Services.AddHostedService<RocksDbSynchronizer>();
        builder.Services.AddOpenTelemetry()
            .WithMetrics(b => 
                b.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter());

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(port, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.MapGrpcService<RemoteKeyValueService>();
        app.MapHealthChecks("/health");
        app.UseOpenTelemetryPrometheusScrapingEndpoint();

        await app.RunAsync(cancellationToken);
    }
}
