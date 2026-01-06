using System.Security.Claims;
using HR_Application.Model.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace HR_Application.Service.Authorization.Handlers;

public class HrOnlyAuthorizationHandler : AuthorizationHandler<HrOnlyRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HrOnlyRequirement requirement)
    {
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (userRole == "HR")
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
