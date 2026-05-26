using System.Collections.Concurrent;
using RabbitMQ.Client;

namespace Lynx.IdentityService.Infrastructure.Services.RabbitMq
{
    public interface IRabbitMqChannelPool : IAsyncDisposable
    {
        ValueTask<IChannel> GetChannelAsync(CancellationToken cancellationToken=default);
        void ReturnChannel(IChannel channel);
    }

    public class RabbitMqChannelPool(IRabbitMqConnectionManager connManager) : IRabbitMqChannelPool
    {
        private readonly ConcurrentQueue<IChannel> _pool = new();
        private readonly int _maxPoolSize =30;

        public async ValueTask<IChannel> GetChannelAsync(CancellationToken cancellationToken = default)
        {
            if (_pool.TryDequeue(out var channel))
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

        public void ReturnChannel(IChannel channel)
        {
            if (channel.IsOpen && _pool.Count < _maxPoolSize)
            {
                _pool.Enqueue(channel);
            }
            else
            {
                channel.DisposeAsync().AsTask();
            }
        }

        public async ValueTask DisposeAsync()
        {
            while (_pool.TryDequeue(out var channel))
            {
                await channel.DisposeAsync();
            }

            GC.SuppressFinalize(this);
        }
    }
}
