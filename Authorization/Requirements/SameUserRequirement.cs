using Microsoft.AspNetCore.Authorization;

namespace HR_Application.Authorization.Requirements;

public class SameUserRequirement : IAuthorizationRequirement
{
    public SameUserRequirement()
    {
    }
}
