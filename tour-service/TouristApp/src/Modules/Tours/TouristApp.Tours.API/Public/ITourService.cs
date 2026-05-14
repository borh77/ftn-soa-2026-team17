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
    /// Dodaje ključnu tačku u turu sa verifikacijom autorstva.
    /// </summary>
    void AddKeyPoint(long tourId, long authorId, KeyPointDto dto);

    /// <summary>
    /// Ažurira ključnu tačku u turi.
    /// </summary>
    void UpdateKeyPoint(long tourId, int ordinalNo, KeyPointDto dto);

    /// <summary>
    /// Ažurira ključnu tačku u turi sa verifikacijom autorstva.
    /// </summary>
    void UpdateKeyPoint(long tourId, int ordinalNo, long authorId, KeyPointDto dto);

    /// <summary>
    /// Uklanja ključnu tačku iz ture.
    /// </summary>
    void RemoveKeyPoint(long tourId, int ordinalNo);

    /// <summary>
    /// Uklanja ključnu tačku iz ture sa verifikacijom autorstva.
    /// </summary>
    void RemoveKeyPoint(long tourId, int ordinalNo, long authorId);

    /// <summary>
    /// Objavljuje turu.
    /// </summary>
    void Publish(long tourId);

    /// <summary>
    /// Objavljuje turu sa verifikacijom autorstva.
    /// </summary>
    void Publish(long tourId, long authorId);

    /// <summary>
    /// Arhivira turu.
    /// </summary>
    void Archive(long tourId);

    /// <summary>
    /// Arhivira turu sa verifikacijom autorstva.
    /// </summary>
    void Archive(long tourId, long authorId);

        void Delete(long tourId, long authorId);

        void Update(long tourId, long authorId, UpdateTourDto dto);
}