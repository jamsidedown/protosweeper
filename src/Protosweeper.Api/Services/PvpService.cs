using Grpc.Core;

namespace Protosweeper.Api.Services;

public class PvpService(ILogger<PvpService> logger): PvP.PvPBase
{
    public override Task Play(IAsyncStreamReader<PvpRequest> requestStream, IServerStreamWriter<PvpResponse> responseStream, ServerCallContext context)
    {
        return base.Play(requestStream, responseStream, context);
    }
}
