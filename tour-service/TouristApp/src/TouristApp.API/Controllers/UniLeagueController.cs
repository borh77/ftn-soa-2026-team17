using TouristApp.Tours.API.Public;
using Microsoft.AspNetCore.Mvc;

namespace TouristApp.API.Controllers;

/// <summary>
/// Placeholder controller for the Tours module.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ToursController : ControllerBase
{
    private readonly IHealthService _healthService;

    public ToursController(IHealthService healthService)
    {
        _healthService = healthService;
    }

    /// <summary>GET /api/unileague/ping – sanity check that the module is wired up.</summary>
    [HttpGet("ping")]
    public ActionResult<string> Ping() => Ok(_healthService.Ping());
}
