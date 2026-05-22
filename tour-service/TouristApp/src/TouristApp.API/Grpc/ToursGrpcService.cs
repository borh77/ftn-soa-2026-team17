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

    public ToursGrpcService(ITourService tourService)
    {
        _tourService = tourService;
    }

    public override Task<GetByAuthorResponse> GetByAuthor(GetByAuthorRequest request, ServerCallContext context)
    {
        var result = _tourService.GetByAuthor(request.AuthorId, request.Page, request.PageSize);
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
}