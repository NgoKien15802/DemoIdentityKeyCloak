using DemoIdentity.Security.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace DemoIdentity.Security.PolicyDynamic
{
    public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
     : DefaultAuthorizationPolicyProvider(options)
    {
        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            var policy = await base.GetPolicyAsync(policyName);
            // kiểm tra nếu có policy rồi thì sẽ ko cần add requirement nữa
            if (policy is not null)
            {
                return policy;
            }
            // Nếu policy không tồn tại, tạo một policy mới với yêu cầu PermissionRequirement
            return new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();
        }
    }
}
