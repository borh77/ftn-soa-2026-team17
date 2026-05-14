using TouristApp.Tours.API.Dtos;
using TouristApp.BuildingBlocks.Core.UseCases;

namespace TouristApp.Tours.API.Public;

/// <summary>
/// Javni interfejs Tours modula za upravljanje turama.
/// </summary>
public interface ITourService
{
    /// <summary>
    /// Kreira novu turu. Status se automatski postavlja na Draft, cena na 0.
    /// </summary>
    TourResponseDto Create(long authorId, CreateTourDto dto);

    /// <summary>
    /// Vraća listu tura koje je kreirao određeni autor.
    /// </summary>
    PagedResult<TourResponseDto> GetByAuthor(
        long authorId,
        int page,
        int pageSize);

    PagedResult<TourResponseDto> GetActive(
        int page,
        int pageSize);

    /// <summary>
    /// Dodaje ključnu tačku u turu.
    /// </summary>
    void AddKeyPoint(long tourId, KeyPointDto dto);

    /// <summary>
    /// Ažurira ključnu tačku u turi.
    /// </summary>
    void UpdateKeyPoint(long tourId, int ordinalNo, KeyPointDto dto);

    /// <summary>
    /// Uklanja ključnu tačku iz ture.
    /// </summary>
    void RemoveKeyPoint(long tourId, int ordinalNo);
}