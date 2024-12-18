using Libplanet.RocksDBStore;
using Libplanet.Store.Remote.Executable;
using Libplanet.Store.Remote.Server;
using Libplanet.Store.Trie;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

if (args.Length < 1 || args[0] is not { } path)
{
    Console.Error.WriteLine("Usage: dotnet Libplanet.Store.Remote.Executable.dll -- <path>");
    return -1;
}

// Add services to the container.
builder.Services.AddGrpc();

// If use `RocksDBStore`, try this:
builder.Services.AddSingleton<IKeyValueStore>(_ => new RocksDBKeyValueStore(path, RocksDBInstanceType.Secondary));

builder.Services.AddHealthChecks();
builder.Services.AddHostedService<RocksDbSynchronizer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<RemoteKeyValueService>();
app.MapHealthChecks("/health");

app.Run();

return 0;
