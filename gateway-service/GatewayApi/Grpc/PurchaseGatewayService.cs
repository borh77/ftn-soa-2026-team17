using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Purchase;
using PurchaseProto = Purchase.PurchaseGrpcService;

namespace GatewayApi.Grpc;

[ApiController]
[Route("grpc/purchases")]
[Authorize]
public class PurchaseGatewayService : ControllerBase
{
    private readonly PurchaseProto.PurchaseGrpcServiceClient _client;

    public PurchaseGatewayService(
        PurchaseProto.PurchaseGrpcServiceClient client)
    {
        _client = client;
    }

    [HttpGet("cart")]
    public async Task<IActionResult> GetCart()
    {
        var touristId = long.Parse(
            User.Claims.First(c => c.Type == "personId").Value);

        var result = await _client.GetCartAsync(
            new PurchaseUserRequest
            {
                TouristId = touristId
            });

        return Ok(result);
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var touristId = long.Parse(
            User.Claims.First(c => c.Type == "personId").Value);

        var result = await _client.CheckoutCartAsync(
            new PurchaseUserRequest
            {
                TouristId = touristId
            });

        return Ok(result);
    }
}