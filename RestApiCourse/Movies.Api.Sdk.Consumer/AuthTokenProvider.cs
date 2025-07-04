using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;

namespace Movies.Api.Sdk.Consumer;

public class AuthTokenProvider
{
    private readonly HttpClient _httpClient;
    private string _cachedToken = string.Empty;
    //using a semaphore to ensure thread safety when accessing the token
    //this is important in a multi-threaded environment to prevent multiple threads from trying to refresh
    // the token at the same time, which could lead to multiple requests being made to the authentication service
    //and potentially invalidating the token.
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    public AuthTokenProvider(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<string> GetTokenAsync()
    {
        if (!string.IsNullOrEmpty(_cachedToken))
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(_cachedToken);
            var expiryTimeText = jwt.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            var expiryDateTime = UnixTimeStampToDateTime(int.Parse(expiryTimeText));

            if (expiryDateTime > DateTime.UtcNow)
            {
                return _cachedToken;
            }
        }
         await _semaphore.WaitAsync();

        var response = await _httpClient.PostAsJsonAsync("https://localhost:5003/token", new {
            userId = "d8566de3-b1a6-4a9b-b842-8e3887a82e41",
            email="nick@nickchapsas.com",
            customClaims = new Dictionary<string, object>
            {
                {"admin", true},
                {"trusted_member", true}
            }
        });
        var newToken = await response.Content.ReadAsStringAsync();
        _cachedToken = newToken;
        _semaphore.Release();
        return newToken;
    }

    private static DateTime UnixTimeStampToDateTime(int unixTimeStamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dateTime;
    }

}
