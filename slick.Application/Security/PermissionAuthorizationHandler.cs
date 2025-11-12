using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace slick.Application.Security
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // Check if the user has a "permission" claim matching the required permission
            if (context.User.HasClaim(c =>
                c.Type == "permission" &&
                string.Equals(c.Value, requirement.PermissionTitle, StringComparison.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
