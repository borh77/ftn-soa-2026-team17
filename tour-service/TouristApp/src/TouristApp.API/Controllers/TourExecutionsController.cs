using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TouristApp.Tours.API.Dtos;
using TouristApp.Tours.API.Public;

namespace TouristApp.API.Controllers;

[Authorize(Policy = "touristPolicy")]
[ApiController]
[Route("api/tour-executions")]
public class TourExecutionsController : ControllerBase
{
    private readonly ITourExecutionService _tourExecutionService;

    public TourExecutionsController(ITourExecutionService tourExecutionService)
    {
        _tourExecutionService = tourExecutionService;
    }

    [HttpPost("tours/{tourId}")]
    [ProducesResponseType(typeof(TourExecutionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<TourExecutionDto> Start([FromRoute] long tourId, [FromBody] StartTourExecutionDto dto)
    {
        var touristId = User.PersonId();
        var execution = _tourExecutionService.Start(touristId, tourId, dto);

        return StatusCode(StatusCodes.Status201Created, execution);
    }

    [HttpPost("{executionId}/check-keypoints")]
    [ProducesResponseType(typeof(KeyPointProximityResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<KeyPointProximityResultDto> CheckKeyPointProximity(
        [FromRoute] long executionId,
        [FromBody] CheckKeyPointProximityDto dto)
    {
        var touristId = User.PersonId();
        var result = _tourExecutionService.CheckKeyPointProximity(touristId, executionId, dto);

        return Ok(result);
    }

    [HttpPost("{executionId}/complete")]
    [ProducesResponseType(typeof(TourExecutionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<TourExecutionDto> Complete([FromRoute] long executionId)
    {
        var touristId = User.PersonId();
        var execution = _tourExecutionService.Complete(touristId, executionId);

        return Ok(execution);
    }

    [HttpPost("{executionId}/abandon")]
    [ProducesResponseType(typeof(TourExecutionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<TourExecutionDto> Abandon([FromRoute] long executionId)
    {
        var touristId = User.PersonId();
        var execution = _tourExecutionService.Abandon(touristId, executionId);

        return Ok(execution);
    }
}
