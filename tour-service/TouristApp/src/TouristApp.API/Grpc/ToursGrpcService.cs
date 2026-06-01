using System.Globalization;
using Grpc.Core;
using TouristApp.BuildingBlocks.Core.UseCases;
using TouristApp.Protos.Tours;
using TouristApp.Tours.API.Dtos;
using TouristApp.Tours.API.Public;
using ToursProto = TouristApp.Protos.Tours.Tours;

namespace TouristApp.API.Grpc;

public class ToursGrpcService : ToursProto.ToursBase
{
    private readonly ITourService _tourService;
    private readonly ITourExecutionService _tourExecutionService;

    public ToursGrpcService(ITourService tourService, ITourExecutionService tourExecutionService)
    {
        _tourService = tourService;
        _tourExecutionService = tourExecutionService;
    }

    public override Task<GetByAuthorResponse> GetByAuthor(GetByAuthorRequest request, ServerCallContext context)
    {
        // Authorization: only allow admin or the author himself to request by authorId
        var httpContext = context.GetHttpContext();
        var user = httpContext?.User;
        if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Authentication required"));
        }

        // Admins may request any author
        var isAdmin = user.IsInRole("ADMIN");
        var callerId = user.PersonId();

        long effectiveAuthorId = request.AuthorId;
        if (!isAdmin)
        {
            // If no authorId provided, default to caller
            if (effectiveAuthorId == 0)
                effectiveAuthorId = callerId;

            if (effectiveAuthorId != callerId)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Only the author or an admin can request this resource."));
            }
        }

        var result = _tourService.GetByAuthor(effectiveAuthorId, request.Page, request.PageSize);
        return Task.FromResult(new GetByAuthorResponse
        {
            Tours = { result.Results.Select(ToGrpcTour) },
            Total = result.TotalCount
        });
    }

    public override Task<GetActiveResponse> GetActive(GetActiveRequest request, ServerCallContext context)
    {
        var result = _tourService.GetActive(request.Page, request.PageSize);
        return Task.FromResult(new GetActiveResponse
        {
            Tours = { result.Results.Select(ToGrpcTour) },
            Total = result.TotalCount
        });
    }

    public override Task<TourExecution> StartTour(StartTourRequest request, ServerCallContext context)
    {
        var touristId = GetAuthenticatedTouristId(context);
        var execution = _tourExecutionService.Start(
            touristId,
            request.TourId,
            new StartTourExecutionDto(request.Latitude, request.Longitude));

        return Task.FromResult(ToGrpcExecution(execution));
    }

    public override Task<KeyPointProximityResult> CheckKeyPointProximity(CheckKeyPointProximityRequest request, ServerCallContext context)
    {
        var touristId = GetAuthenticatedTouristId(context);
        var result = _tourExecutionService.CheckKeyPointProximity(
            touristId,
            request.ExecutionId,
            new CheckKeyPointProximityDto(request.Latitude, request.Longitude));

        return Task.FromResult(new KeyPointProximityResult
        {
            Reached = result.Reached,
            KeyPointOrdinalNo = result.KeyPointOrdinalNo ?? 0,
            LastActivity = result.LastActivity.ToString("O", CultureInfo.InvariantCulture),
            Execution = ToGrpcExecution(result.Execution)
        });
    }

    private static Tour ToGrpcTour(TourResponseDto dto)
    {
        return new Tour
        {
            Id = dto.Id,
            AuthorId = dto.AuthorId,
            Name = dto.Name,
            Description = dto.Description,
            Difficulty = dto.Difficulty,
            Status = dto.Status,
            Price = (double)dto.Price,
            RouteLengthKm = (double)dto.RouteLengthKm,
            PublishedAt = dto.PublishedAt?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            ArchivedAt = dto.ArchivedAt?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            Tags = { dto.Tags },
            TravelTimes = { dto.TravelTimes.Select(time => new TourTravelTime
            {
                TransportType = time.TransportType.ToString(),
                Minutes = time.Minutes
            }) },
            KeyPoints = { dto.KeyPoints.Select(keyPoint => new KeyPoint
            {
                OrdinalNo = keyPoint.OrdinalNo ?? 0,
                Name = keyPoint.Name,
                Description = keyPoint.Description,
                SecretText = keyPoint.SecretText,
                ImageUrl = keyPoint.ImageUrl,
                Latitude = keyPoint.Latitude,
                Longitude = keyPoint.Longitude
            }) }
        };
    }

    private static TourExecution ToGrpcExecution(TourExecutionDto dto)
    {
        return new TourExecution
        {
            Id = dto.Id,
            TourId = dto.TourId,
            TouristId = dto.TouristId,
            Status = dto.Status,
            StartedAt = dto.StartedAt.ToString("O", CultureInfo.InvariantCulture),
            CompletedAt = dto.CompletedAt?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            AbandonedAt = dto.AbandonedAt?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            LastActivity = dto.LastActivity.ToString("O", CultureInfo.InvariantCulture),
            StartedLatitude = dto.StartedLatitude,
            StartedLongitude = dto.StartedLongitude,
            CompletedKeyPoints =
            {
                dto.CompletedKeyPoints.Select(point => new CompletedKeyPoint
                {
                    KeyPointOrdinalNo = point.KeyPointOrdinalNo,
                    CompletedAt = point.CompletedAt.ToString("O", CultureInfo.InvariantCulture)
                })
            }
        };
    }

    private static long GetAuthenticatedTouristId(ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        var user = httpContext?.User;
        if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Authentication required"));
        }

        if (!user.IsInRole("TOURIST"))
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Only tourists can execute tours."));
        }

        return user.PersonId();
    }
}
