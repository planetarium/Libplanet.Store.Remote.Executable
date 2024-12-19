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
using Serilog;

await ConsoleApp.RunAsync(args, async ([Argument] string path, CancellationToken cancellationToken, int port = 5000) =>
{
    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddGrpc();

    // If use `RocksDBStore`, try this:
    builder.Services.AddSingleton<IKeyValueStore>(_ => new RocksDBKeyValueStore(path, RocksDBInstanceType.Secondary));

    builder.Services.AddHealthChecks();
    builder.Services.AddHostedService<RocksDbSynchronizer>();
    builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

    builder.WebHost.ConfigureKestrel(options =>
    {
        // Get port from ASPNETCORE_URLS or somewhere, (don't use hard-coded port number)
        options.ListenAnyIP(port, listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http2;
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.MapGrpcService<RemoteKeyValueService>();
    app.MapHealthChecks("/health");

    await app.RunAsync(cancellationToken);
});
