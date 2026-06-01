using GatewayApi.Protos;
using Microsoft.AspNetCore.Mvc;

namespace GatewayApi.Grpc;

[ApiController]
[Route("grpc/stakeholders")]
public class StakeholdersGatewayService : ControllerBase
{
    private readonly StakeholdersGrpcService.StakeholdersGrpcServiceClient _client;

    public StakeholdersGatewayService(StakeholdersGrpcService.StakeholdersGrpcServiceClient client)
    {
        _client = client;
    }

    [HttpGet("profile/{username}")]
    public async Task<IActionResult> GetMyProfile(string username)
    {
        var response = await _client.GetMyProfileAsync(new GetMyProfileRequest
        {
            Username = username
        });

        return Ok(response);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var response = await _client.GetAllUsersAsync(new GetAllUsersRequest());

        return Ok(response.Users);
    }
}