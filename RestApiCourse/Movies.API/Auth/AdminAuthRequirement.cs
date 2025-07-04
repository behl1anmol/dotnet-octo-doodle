using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Movies.API.Auth;

public class AdminAuthRequirement : IAuthorizationHandler, IAuthorizationRequirement
{
    private readonly string _apiKey;
    public AdminAuthRequirement(string apiKey)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
    }

    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.User.HasClaim(AuthConstants.AdminUserClaimName, "true"))
        {
            context.Succeed(this);
            return Task.CompletedTask;
        }

        var httpContext = context.Resource as Microsoft.AspNetCore.Http.HttpContext;
        if (httpContext is null)
        {
            return Task.CompletedTask;
        }

        if (!httpContext.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (!string.Equals(extractedApiKey, _apiKey))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var identity = (ClaimsIdentity)httpContext.User.Identity!;
        // we are just mimicing a user id in an api key so it can be captured at the level of controller. this is the most barebones way
        identity.AddClaim(new Claim("userid", Guid.Parse("74e20de1-8dd0-4bc2-a9f5-8aa3203ad209").ToString())); // should not be hardcoded handle in better way in real world
        context.Succeed(this);
        return Task.CompletedTask;
    }
}
