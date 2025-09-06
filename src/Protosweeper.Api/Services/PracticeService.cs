using Grpc.Core;
using Protosweeper.Api.Protos;

namespace Protosweeper.Api.Services;

public class PracticeService(ILogger<PracticeService> logger) : Practice.PracticeBase
{
    public override Task Play(IAsyncStreamReader<PracticeRequest> requestStream, IServerStreamWriter<PracticeResponse> responseStream, ServerCallContext context)
    {
        return base.Play(requestStream, responseStream, context);
    }
}
