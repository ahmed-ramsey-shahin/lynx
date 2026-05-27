using System.Collections.Concurrent;
using RabbitMQ.Client;

namespace Lynx.IdentityService.Infrastructure.Services.RabbitMq
{
    public interface IRabbitMqChannelPool : IAsyncDisposable
    {
        ValueTask<IChannel> GetChannelAsync(CancellationToken cancellationToken=default);
        void ReturnChannel(IChannel channel);
    }

    public class RabbitMqChannelPool(IRabbitMqConnectionManager connManager, int maxPoolSize=30) : IRabbitMqChannelPool
    {
        private readonly ConcurrentQueue<IChannel> _pool = new();
        private readonly SemaphoreSlim _semaphore = new(maxPoolSize, maxPoolSize);

        public async ValueTask<IChannel> GetChannelAsync(CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                while (_pool.TryDequeue(out var channel))
                {
                    if (channel.IsOpen)
                    {
                        return channel;
                    }

                    await channel.DisposeAsync();
                }

                var connection = await connManager.GetConnectionAsync();
                return await connection.CreateChannelAsync(cancellationToken: cancellationToken);
            }
            catch
            {
                _semaphore.Release();
                throw;
            }
        }

        public void ReturnChannel(IChannel channel)
        {
            try
            {
                if (channel.IsOpen)
                {
                    _pool.Enqueue(channel);
                }
                else
                {
                    channel.DisposeAsync();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            while (_pool.TryDequeue(out var channel))
            {
                await channel.DisposeAsync();
            }

            _semaphore.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
