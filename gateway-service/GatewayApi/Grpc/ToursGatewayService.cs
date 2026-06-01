using Grpc.Core;
using TouristApp.Protos.Tours;
using ToursProto = TouristApp.Protos.Tours.Tours;
using Microsoft.AspNetCore.Http;
using System.Linq;

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
        var metadata = ForwardAuthorization(context);

        var response = await _tourClient.GetByAuthorAsync(new GetByAuthorRequest
        {
            AuthorId = request.AuthorId,
            Page = request.Page <= 0 ? 1 : request.Page,
            PageSize = request.PageSize <= 0 ? 10 : request.PageSize
        }, headers: metadata, cancellationToken: context.CancellationToken);

        return response;
    }

    public override async Task<GetActiveResponse> GetActive(GetActiveRequest request, ServerCallContext context)
    {
        var metadata = ForwardAuthorization(context);

        var response = await _tourClient.GetActiveAsync(new GetActiveRequest
        {
            Page = request.Page <= 0 ? 1 : request.Page,
            PageSize = request.PageSize <= 0 ? 10 : request.PageSize
        }, headers: metadata, cancellationToken: context.CancellationToken);

        return response;
    }

    public override async Task<TourExecution> StartTour(StartTourRequest request, ServerCallContext context)
    {
        var response = await _tourClient.StartTourAsync(new StartTourRequest
        {
            TourId = request.TourId,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        }, headers: ForwardAuthorization(context), cancellationToken: context.CancellationToken);

        return response;
    }

    public override async Task<KeyPointProximityResult> CheckKeyPointProximity(CheckKeyPointProximityRequest request, ServerCallContext context)
    {
        var response = await _tourClient.CheckKeyPointProximityAsync(new CheckKeyPointProximityRequest
        {
            ExecutionId = request.ExecutionId,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        }, headers: ForwardAuthorization(context), cancellationToken: context.CancellationToken);

        return response;
    }

    private static Metadata ForwardAuthorization(ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        var auth = httpContext?.Request.Headers["Authorization"].FirstOrDefault();
        var metadata = new Metadata();
        if (!string.IsNullOrEmpty(auth))
            metadata.Add("Authorization", auth);

        return metadata;
    }
}
