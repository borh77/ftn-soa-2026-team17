using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TouristApp.Tours.API.Dtos;
using TouristApp.Tours.API.Public;

namespace TouristApp.API.Controllers;

[Authorize(Policy = "touristPolicy")]
[ApiController]
[Route("api/position-simulator")]
public class PositionSimulatorController : ControllerBase
{
    private readonly ITouristPositionService _positionService;

    public PositionSimulatorController(ITouristPositionService positionService)
    {
        _positionService = positionService;
    }

    [HttpGet("me")]
    public ActionResult<TouristPositionDto?> GetMyPosition()
    {
        var touristId = User.PersonId();
        var position = _positionService.GetForTourist(touristId);

        return Ok(position);
    }

    [HttpPut("me")]
    public ActionResult<TouristPositionDto> SaveMyPosition([FromBody] UpdateTouristPositionDto dto)
    {
        var touristId = User.PersonId();
        var position = _positionService.SaveForTourist(touristId, dto);

        return Ok(position);
    }
}