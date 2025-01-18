using Microsoft.AspNetCore.Authorization;

namespace DemoIdentity.Security.Requirements
{
    public class PermissionRequirement(string pemission) : IAuthorizationRequirement
    {
        public string Permission { get; set; } = pemission;
    }

    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var permissions = context.User.GetPermissions();
            if (permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
