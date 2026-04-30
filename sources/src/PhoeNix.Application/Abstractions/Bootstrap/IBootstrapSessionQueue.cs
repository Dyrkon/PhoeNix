using System.Threading.Channels;
using PhoeNix.Domain.Entities.SetupSessions;

namespace PhoeNix.Application.Abstractions.Bootstrap;

public interface IBootstrapSessionQueue
{
    void Enqueue(SetupSessionId sessionId);
    ChannelReader<SetupSessionId> Reader { get; }
}
