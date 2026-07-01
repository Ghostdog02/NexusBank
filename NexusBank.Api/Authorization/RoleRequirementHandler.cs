using Microsoft.AspNetCore.Authorization;
using NexusBank.Application.Common.Authorization;
using NexusBank.Application.Common.Services;

namespace NexusBank.Api.Authorization;

public class RoleRequirementHandler(ICurrentUserService currentUserService)
    : AuthorizationHandler<RoleRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        try
        {
            var user = await currentUserService.GetCurrentUserAsync();
            if (user.Role == requirement.Role)
                context.Succeed(requirement);
        }
        catch
        {
            context.Fail();
        }
    }
}
