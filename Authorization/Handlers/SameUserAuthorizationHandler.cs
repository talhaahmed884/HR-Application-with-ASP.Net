using System.Security.Claims;
using HR_Application.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace HR_Application.Authorization.Handlers;

public class SameUserAuthorizationHandler : AuthorizationHandler<SameUserRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SameUserAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameUserRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Task.CompletedTask;
        }

        if (userRole == "HR")
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return Task.CompletedTask;
        }

        var routeId = httpContext.GetRouteValue("id")?.ToString();

        if (routeId == userIdClaim)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
