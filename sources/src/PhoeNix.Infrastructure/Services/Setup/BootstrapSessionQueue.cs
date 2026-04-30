using System.Threading.Channels;
using PhoeNix.Application.Abstractions.Bootstrap;
using PhoeNix.Domain.Entities.SetupSessions;

namespace PhoeNix.Infrastructure.Services.Setup;

internal sealed class BootstrapSessionQueue : IBootstrapSessionQueue
{
    private readonly Channel<SetupSessionId> _channel =
        Channel.CreateUnbounded<SetupSessionId>(new UnboundedChannelOptions { SingleReader = true });

    public void Enqueue(SetupSessionId sessionId) => _channel.Writer.TryWrite(sessionId);

    public ChannelReader<SetupSessionId> Reader => _channel.Reader;
}
