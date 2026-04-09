using TouristApp.Blog.API.Public;
using Microsoft.AspNetCore.Mvc;

namespace TouristApp.API.Controllers;

/// <summary>
/// Placeholder controller for the Blog module.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BlogController : ControllerBase
{
    private readonly IHealthService _healthService;

    public BlogController(IHealthService healthService)
    {
        _healthService = healthService;
    }

    /// <summary>GET /api/unileague/ping – sanity check that the module is wired up.</summary>
    [HttpGet("ping")]
    public ActionResult<string> Ping() => Ok(_healthService.Ping());
}
