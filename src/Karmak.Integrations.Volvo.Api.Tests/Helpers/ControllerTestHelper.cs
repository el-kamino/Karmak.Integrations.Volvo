using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Karmak.Integrations.Volvo.Api.Test.Helpers;

public static class ControllerTestHelper
{
    public static ControllerContext CreateControllerContext(
        string? accountCode = "12345",
        string? branchCode = "01",
        string? username = "testUser",
        Dictionary<string, string>? headers = null)
    {
        var fusionClaims = new List<Claim>();
        if (accountCode != null) fusionClaims.Add(new Claim("fusionKarmakAccountNumber", accountCode));
        if (branchCode != null) fusionClaims.Add(new Claim("fusionBranchCode", branchCode));
        if (username != null) fusionClaims.Add(new Claim("fusionUserName", username));

        var fusionIdentity = new ClaimsIdentity(fusionClaims, "Test", null, null);
        fusionIdentity.Label = "fusion";

        var user = new ClaimsPrincipal(fusionIdentity);

        var httpContext = new DefaultHttpContext { User = user };

        if (headers != null)
        {
            foreach (var header in headers)
            {
                httpContext.Request.Headers[header.Key] = header.Value;
            }
        }

        return new ControllerContext { HttpContext = httpContext };
    }

    public static void SetRequestBody(ControllerContext context, string content)
    {
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        context.HttpContext.Request.Body = stream;
    }
}
