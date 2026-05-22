using Grpc.Core;
using TouristApp.Protos.Tours;
using ToursProto = TouristApp.Protos.Tours.Tours;

namespace GatewayApi.Grpc;

public class ToursGatewayService : ToursProto.ToursBase
{
    private readonly ToursProto.ToursClient _tourClient;

    public ToursGatewayService(ToursProto.ToursClient tourClient)
    {
        _tourClient = tourClient;
    }

    public override async Task<GetByAuthorResponse> GetByAuthor(GetByAuthorRequest request, ServerCallContext context)
    {
        var response = await _tourClient.GetByAuthorAsync(new GetByAuthorRequest
        {
            AuthorId = request.AuthorId,
            Page = request.Page <= 0 ? 1 : request.Page,
            PageSize = request.PageSize <= 0 ? 10 : request.PageSize
        }, cancellationToken: context.CancellationToken);

        return response;
    }

    public override async Task<GetActiveResponse> GetActive(GetActiveRequest request, ServerCallContext context)
    {
        var response = await _tourClient.GetActiveAsync(new GetActiveRequest
        {
            Page = request.Page <= 0 ? 1 : request.Page,
            PageSize = request.PageSize <= 0 ? 10 : request.PageSize
        }, cancellationToken: context.CancellationToken);

        return response;
    }
}