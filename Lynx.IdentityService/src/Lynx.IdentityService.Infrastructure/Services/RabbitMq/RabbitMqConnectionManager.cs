using RabbitMQ.Client;

namespace Lynx.IdentityService.Infrastructure.Services.RabbitMq
{
    public interface IRabbitMqConnectionManager : IAsyncDisposable
    {
        Task<IConnection> GetConnectionAsync();
    }

    public class RabbitMqConnectionManager : IRabbitMqConnectionManager
    {
        private readonly ConnectionFactory _factory;
        private readonly SemaphoreSlim _semaphore;
        private IConnection? _connection;

        public RabbitMqConnectionManager(string connectionString)
        {
            _factory = new()
            {
                Uri = new Uri(connectionString)
            };
            _semaphore = new(1, 1);
        }

        public async Task<IConnection> GetConnectionAsync()
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            await _semaphore.WaitAsync();

            try
            {
                if (_connection is { IsOpen: true })
                {
                    return _connection;
                }

                _connection = await _factory.CreateConnectionAsync();
                return _connection;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }

            GC.SuppressFinalize(this);
        }
    }
}
