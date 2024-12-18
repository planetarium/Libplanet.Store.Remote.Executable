using Libplanet.RocksDBStore;
using Libplanet.Store.Trie;
using Microsoft.Extensions.Hosting;

namespace Libplanet.Store.Remote.Executable;

public class RocksDbSynchronizer(IKeyValueStore keyValueStore) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (keyValueStore is RocksDBKeyValueStore rocksDbKeyValueStore)
            {
                rocksDbKeyValueStore.TryCatchUpWithPrimary();
            }

            await Task.Delay(TimeSpan.FromSeconds(0.5), stoppingToken);
        }
    }
}
