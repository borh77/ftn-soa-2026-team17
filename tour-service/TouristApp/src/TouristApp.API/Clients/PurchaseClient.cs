using System.Net.Http.Headers;
using System.Text.Json;

namespace TouristApp.API.Clients;

public class PurchaseClient
{
    private readonly HttpClient _httpClient;

    public PurchaseClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> HasPurchasedAsync(long tourId, string bearerToken)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/purchases/tours/{tourId}/purchased");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", bearerToken);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<HasPurchasedResponse>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return result?.Purchased == true;
    }

    private class HasPurchasedResponse
    {
        public bool Purchased { get; set; }
    }
}