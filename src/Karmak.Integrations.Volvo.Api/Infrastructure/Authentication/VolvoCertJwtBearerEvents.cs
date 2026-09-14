using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Karmak.Integrations.Volvo.Api.Infrastructure.Authentication;

public class VolvoCertJwtBearerEvents : JwtBearerEvents
{
    private const string BearerPrefix = "Bearer ";
    private const string FusionKarmakAccountNumber = "fusionKarmakAccountNumber";
    private const string FusionIdentityKey = "fusion_identity";

    private static readonly TimeSpan DefaultCacheTokenExpiration = TimeSpan.FromMinutes(30);

    private readonly HttpClient _client;
    private readonly VolvoCertJwtBearerEventOptions _options;
    private readonly HybridCache _cache;
    private readonly ILogger _logger;

    public VolvoCertJwtBearerEvents(
        HttpClient client,
        HybridCache cache,
        IOptions<VolvoCertJwtBearerEventOptions> options,
        ILogger<VolvoCertJwtBearerEvents> logger)
    {
        _client = client;
        _cache = cache;
        _options = options.Value;
        _logger = logger;

        OnMessageReceived += OnKarmakMessageReceived;
        OnTokenValidated += OnKarmakTokenValidated;
    }

    private async Task OnKarmakMessageReceived(MessageReceivedContext messageReceivedContext)
    {
        //This is adapted from ElkAuthentication.cs in the Karmak.Gateway.Core project.  A fusion instance
        //will create a jwt token and sign it with a certificate.  We recognize that it is a fusion signed 
        //jwt by the presence of the fusionKarmakAccountNumber claim.  We send this token to the karmak (elk)
        //identity provider.  It has a copy of the same certificate fusion used and will validate the token
        //for us.  If the token is valid, it will issue a new jwt signed by itself (the karmak identity
        //provider) that we can then authenticate via the normal flow.  So essentially, we're swapping out
        //a custom signed jwt for a jwt signed by the Karmak IdP.

        string? authorizationHeader = messageReceivedContext.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(authorizationHeader) || authorizationHeader.Length < BearerPrefix.Length)
        {
            messageReceivedContext.Fail("Invalid bearer token");
            return;
        }

        string fusionToken = authorizationHeader.Substring(BearerPrefix.Length);
        JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(fusionToken);
        Claim? kanClaim = jwt.Claims.FirstOrDefault(c => c.Type == FusionKarmakAccountNumber);

        if (kanClaim == null)
        {
            messageReceivedContext.Fail("No KAN claim present");
            return;
        }

        try
        {
            string cacheKey = GetCacheKey(kanClaim.Value, fusionToken);
            TimeSpan expiration = GetCacheExpirationMinutes(
                jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp));

            var apiToken = await _cache.GetOrCreateAsync(
                fusionToken,
                async cancel => await GetApiTokenAsync(kanClaim.Value, fusionToken, cancel),
                new HybridCacheEntryOptions
                {
                    Expiration = expiration,

                    //Using hybrid cache primarily for the built in stampede protection.  Don't want
                    //auth keys stored in a remote cache where they could potentially be accessed.
                    Flags = HybridCacheEntryFlags.DisableDistributedCache
                },
                cancellationToken: messageReceivedContext.HttpContext.RequestAborted);

            messageReceivedContext.Token = apiToken;

            //If we got to this point we know the jwt presented by Fusion is valid.  Store it
            //so we can add it as a separate identity after the elk id token is validated
            //(see OnKarmakTokenValidated method) since downstream code needs data from both
            //the Fusion jwt and the Elk identity jwt.
            var fusionIdentity = new ClaimsIdentity(jwt.Claims, "Bearer")
            {
                Label = "fusion"
            };

            messageReceivedContext.Request.HttpContext.Items[FusionIdentityKey] = fusionIdentity;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error authenticating fusion token");
            messageReceivedContext.Fail(e);
        }
    }

    private static TimeSpan GetCacheExpirationMinutes(Claim? expClaim)
    {
        //If the fusion token is due to expire soon, only cache the identity
        //server token for the time for which the fusion token remains valid.
        //That is we don't want the fusion token to expire and continue using
        //the corresponding identity server jwt.

        if (expClaim == null)
        {
            return DefaultCacheTokenExpiration;
        }

        if (!int.TryParse(expClaim.Value, out var expSeconds))
        {
            return DefaultCacheTokenExpiration;
        }

        var exp = DateTime.UnixEpoch.AddSeconds(expSeconds);
        TimeSpan timeToExpiration = exp - DateTime.UtcNow;

        if (timeToExpiration > DefaultCacheTokenExpiration)
        {
            return DefaultCacheTokenExpiration;
        }

        return timeToExpiration;
    }

    private static string GetCacheKey(string kan, string fusionToken)
    {
        if (kan.Length > 40)
        {
            //This should never be hit.  If it is that means that there is an error in the
            //code calling the api or that it is malicious.  This should not matter as if
            //the KAN is invalid the factory method will throw but I'd like to have this
            //check for completeness.
            kan = kan.Substring(0, 40);
        }

        //Hash the token to enforce consistent cache key sizes.  For hybrid cache
        //if the key size is over 1024 it will bypass the cache and go directly to
        //the factory method.
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(fusionToken));
        string? hash = Convert.ToHexStringLower(hashBytes);
        string key = $"{kan}_{hash}";
        return key;
    }

    private async ValueTask<string> GetApiTokenAsync(string kan, string fusionToken, CancellationToken cancellationToken)
    {
        var requestParameters = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("client_id", "fusion_" + kan),
            new KeyValuePair<string, string>("grant_type", "fusion_delegation"),
            new KeyValuePair<string, string>("scope", "fusion"),
            new KeyValuePair<string, string>("token", fusionToken)
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_options.AuthorityUrl + "/connect/token"));
        request.Content = new FormUrlEncodedContent(requestParameters);

        using HttpResponseMessage response = await _client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception(
                $"Error retrieving api token for KAN {kan}.  Status: {response.StatusCode}. Reason: {response.ReasonPhrase}.  Error: {errorContent}");
        }

        Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        string? apiToken = document.RootElement.GetProperty("access_token").GetString();

        if (apiToken == null)
        {
            throw new Exception($"No access token returned for KAN: {kan}");
        }

        return apiToken;
    }

    private Task OnKarmakTokenValidated(TokenValidatedContext tokenValidatedContext)
    {
        ClaimsIdentity? fusionIdentity = tokenValidatedContext.HttpContext.Items[FusionIdentityKey] as ClaimsIdentity;

        if (fusionIdentity == null)
        {
            return Task.CompletedTask;
        }

        tokenValidatedContext.Principal?.AddIdentity(fusionIdentity);
        return Task.CompletedTask;
    }
}
