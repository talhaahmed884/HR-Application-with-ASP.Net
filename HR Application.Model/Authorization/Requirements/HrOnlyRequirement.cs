using Microsoft.AspNetCore.Authorization;

namespace HR_Application.Model.Authorization.Requirements;

public class HrOnlyRequirement : IAuthorizationRequirement
{
    public HrOnlyRequirement()
    {
    }
}
