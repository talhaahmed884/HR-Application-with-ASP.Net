using Microsoft.AspNetCore.Authorization;

namespace HR_Application.Authorization.Requirements;

public class HrOnlyRequirement : IAuthorizationRequirement
{
    public HrOnlyRequirement()
    {
    }
}
