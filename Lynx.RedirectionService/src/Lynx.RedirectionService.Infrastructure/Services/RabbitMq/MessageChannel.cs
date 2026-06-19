using System.Threading.Channels;

namespace Lynx.RedirectionService.Infrastructure.Services.RabbitMq
{
    public record QueuedMessage(string QueueName, string SerializedBody);

    public interface IMessageChannel
    {
        ValueTask QueueMessageAsync(QueuedMessage message, CancellationToken cancellationToken=default);
        IAsyncEnumerable<QueuedMessage> ReadAllAsync(CancellationToken cancellationToken=default);
    }

    public sealed class MessageChannel : IMessageChannel
    {
        private readonly Channel<QueuedMessage> _channel = Channel.CreateBounded<QueuedMessage>(new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

        public ValueTask QueueMessageAsync(QueuedMessage message, CancellationToken cancellationToken = default)
        {
            return _channel.Writer.WriteAsync(message, cancellationToken);
        }

        public IAsyncEnumerable<QueuedMessage> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }
    }
}
