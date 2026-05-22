using TouristApp.Tours.API.Dtos;
using TouristApp.Tours.API.Public;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TouristApp.BuildingBlocks.Core.UseCases;

namespace TouristApp.API.Controllers;

/// <summary>
/// Kontroler za upravljanje turama.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ToursController : ControllerBase
{
    private readonly IHealthService _healthService;
    private readonly ITourService _tourService;
    private readonly ITourReviewService _tourReviewService;

    public ToursController(
        IHealthService healthService,
        ITourService tourService,
        ITourReviewService tourReviewService)
    {
        _healthService = healthService;
        _tourService = tourService;
        _tourReviewService = tourReviewService;
    }

    /// <summary>GET /api/tours/ping – provera da li je modul aktivan.</summary>
    [AllowAnonymous]
    [HttpGet("ping")]
    public ActionResult<string> Ping() => Ok(_healthService.Ping());

    /// <summary>
    /// POST /api/tours – Kreiranje nove ture (samo guide).
    /// Status se automatski postavlja na Draft, cena na 0.
    /// </summary>
    [Authorize(Policy = "guidePolicy")]
    [HttpPost]
    [ProducesResponseType(typeof(TourResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<TourResponseDto> Create([FromBody] CreateTourDto dto)
    {
        var authorId = User.PersonId();
        var tour = _tourService.Create(authorId, dto);
        return CreatedAtAction(nameof(GetByAuthor), new { authorId = tour.AuthorId }, tour);
    }

    /// <summary>
    /// GET /api/tours?authorId={authorId} – Lista tura određenog autora (samo autoru).
    /// Prikazuje samo ture tog autora.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TourResponseDto>), StatusCodes.Status200OK)]
    public ActionResult<PagedResult<TourResponseDto>> GetByAuthor(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var authorId = User.PersonId();
        var tours = _tourService.GetByAuthor(authorId, page, pageSize);
        return Ok(tours);
    }

    /// <summary>
    /// GET /api/tours/active – Lista svih aktivnih (publikovanih) tura.
    /// Prikazuje sve ture u stanju Published.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(PagedResult<TourResponseDto>), StatusCodes.Status200OK)]
    public ActionResult<PagedResult<TourResponseDto>> GetActive(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var tours = _tourService.GetActive(page, pageSize);
        return Ok(tours);
    }

    /// <summary>
    /// POST /api/tours/{tourId}/keypoints – Dodavanje ključne tačke u turu (samo guide autoru).
    /// </summary>
    /// <summary>
    /// GET /api/tours/{tourId}/reviews - Lista recenzija za turu.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{tourId}/reviews")]
    [ProducesResponseType(typeof(PagedResult<TourReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<PagedResult<TourReviewDto>> GetReviews(
        [FromRoute] long tourId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var reviews = _tourReviewService.GetByTour(tourId, page, pageSize);
        return Ok(reviews);
    }

    /// <summary>
    /// POST /api/tours/{tourId}/reviews - Turista ostavlja recenziju za turu.
    /// </summary>
    [Authorize(Policy = "touristPolicy")]
    [HttpPost("{tourId}/reviews")]
    [ProducesResponseType(typeof(TourReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<TourReviewDto> CreateReview(
        [FromRoute] long tourId,
        [FromBody] CreateTourReviewDto dto)
    {
        var touristId = User.PersonId();
        var review = _tourReviewService.Create(tourId, touristId, User.Username(), dto);
        return CreatedAtAction(nameof(GetReviews), new { tourId = review.TourId }, review);
    }

    [Authorize(Policy = "guidePolicy")]
    [HttpPost("{tourId}/keypoints")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult AddKeyPoint(
        [FromRoute] long tourId,
        [FromBody] KeyPointDto dto)
    {
        var authorId = User.PersonId();
        _tourService.AddKeyPoint(tourId, authorId, dto);
        return Ok();
    }

    /// <summary>
    /// PUT /api/tours/{tourId}/keypoints/{ordinalNo} – Ažuriranje ključne tačke (samo guide autoru).
    /// </summary>
    [Authorize(Policy = "guidePolicy")]
    [HttpPut("{tourId}/keypoints/{ordinalNo}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult UpdateKeyPoint(
        [FromRoute] long tourId,
        [FromRoute] int ordinalNo,
        [FromBody] KeyPointDto dto)
    {
        var authorId = User.PersonId();
        _tourService.UpdateKeyPoint(tourId, ordinalNo, authorId, dto);
        return Ok();
    }

    /// <summary>
    /// PUT /api/tours/{tourId} – Ažuriranje osnovnih polja ture od strane autora dok je Draft.
    /// </summary>
    [Authorize(Policy = "guidePolicy")]
    [HttpPut("{tourId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult Update([FromRoute] long tourId, [FromBody] UpdateTourDto dto)
    {
        var authorId = User.PersonId();
        _tourService.Update(tourId, authorId, dto);
        return Ok();
    }

    /// <summary>
    /// POST /api/tours/{tourId}/publish – Objavljuje turu ako ispunjava uslove (samo guide autoru).
    /// </summary>
    [Authorize(Policy = "guidePolicy")]
    [HttpPost("{tourId}/publish")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult Publish([FromRoute] long tourId)
    {
        var authorId = User.PersonId();
        _tourService.Publish(tourId, authorId);
        return Ok();
    }

    /// <summary>
    /// POST /api/tours/{tourId}/archive – Arhivira turu (samo guide autoru).
    /// </summary>
    [Authorize(Policy = "guidePolicy")]
    [HttpPost("{tourId}/archive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult Archive([FromRoute] long tourId)
    {
        var authorId = User.PersonId();
        _tourService.Archive(tourId, authorId);
        return Ok();
    }

    /// <summary>
    /// POST /api/tours/{tourId}/reactivate – Ponovo aktivira arhiviranu turu (samo guide autoru).
    /// </summary>
    [Authorize(Policy = "guidePolicy")]
    [HttpPost("{tourId}/reactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult Reactivate([FromRoute] long tourId)
    {
        var authorId = User.PersonId();
        _tourService.Reactivate(tourId, authorId);
        return Ok();
    }

    /// <summary>
    /// DELETE /api/tours/{tourId} – Briše turu samo autor može dok je Draft.
    /// </summary>
    [Authorize(Policy = "guidePolicy")]
    [HttpDelete("{tourId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult Delete([FromRoute] long tourId)
    {
        var authorId = User.PersonId();
        _tourService.Delete(tourId, authorId);
        return Ok();
    }

    /// <summary>
    /// DELETE /api/tours/{tourId}/keypoints/{ordinalNo} – Brisanje ključne tačke (samo guide autoru).
    /// </summary>
    [Authorize(Policy = "guidePolicy")]
    [HttpDelete("{tourId}/keypoints/{ordinalNo}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult RemoveKeyPoint(
        [FromRoute] long tourId,
        [FromRoute] int ordinalNo)
    {
        var authorId = User.PersonId();
        _tourService.RemoveKeyPoint(tourId, ordinalNo, authorId);
        return Ok();
    }
}
