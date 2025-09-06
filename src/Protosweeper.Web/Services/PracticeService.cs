using Grpc.Core;
using Protosweeper.Web.Protos;

namespace Protosweeper.Web.Services;

public class PracticeService(ILogger<PracticeService> logger) : Practice.PracticeBase
{
    public override Task Play(IAsyncStreamReader<PracticeRequest> requestStream, IServerStreamWriter<PracticeResponse> responseStream, ServerCallContext context)
    {
        return base.Play(requestStream, responseStream, context);
    }
}
