using TouristApp.Tours.API.Dtos;
using TouristApp.Tours.API.Public;
using Microsoft.AspNetCore.Mvc;
using TouristApp.BuildingBlocks.Core.UseCases;

namespace TouristApp.API.Controllers;

/// <summary>
/// Kontroler za upravljanje turama.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ToursController : ControllerBase
{
    private readonly IHealthService _healthService;
    private readonly ITourService _tourService;

    public ToursController(IHealthService healthService, ITourService tourService)
    {
        _healthService = healthService;
        _tourService = tourService;
    }

    /// <summary>GET /api/tours/ping – provera da li je modul aktivan.</summary>
    [HttpGet("ping")]
    public ActionResult<string> Ping() => Ok(_healthService.Ping());

    /// <summary>
    /// POST /api/tours – Kreiranje nove ture.
    /// Status se automatski postavlja na Draft, cena na 0.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TourResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<TourResponseDto> Create(
        [FromQuery] long authorId,
        [FromBody] CreateTourDto dto)
    {
        var tour = _tourService.Create(authorId, dto);
        return CreatedAtAction(nameof(GetByAuthor), new { authorId = tour.AuthorId }, tour);
    }

    /// <summary>
    /// GET /api/tours?authorId={authorId} – Lista tura određenog autora.
    /// Prikazuje samo ture tog autora.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TourResponseDto>), StatusCodes.Status200OK)]
    public ActionResult<PagedResult<TourResponseDto>> GetByAuthor(
        [FromQuery] long authorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var tours = _tourService.GetByAuthor(authorId, page, pageSize);
        return Ok(tours);
    }
}