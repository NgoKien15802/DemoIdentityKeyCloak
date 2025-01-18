using DemoIdentity.Constant;
using DemoIdentity.Features.Users.Query;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace DemoIdentity.Security.ClaimsTransformation
{
    public class CustomClaimsTransformation : IClaimsTransformation
    {
        private readonly IServiceProvider _serviceProvider;

        public CustomClaimsTransformation(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.HasClaim(x => x.Type == CustomClaims.Permission))
            {
                return principal;
            }
            using IServiceScope scope = _serviceProvider.CreateScope();

            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var identityId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await sender.Send(
                     new GetUserPermissionsQuery(identityId));

            if (principal.Identity is not ClaimsIdentity identity)
            {
                return principal;
            }

            foreach (var permission in result.Permissions)
            {
                identity.AddClaim(new Claim(CustomClaims.Permission, permission));
            }
            return principal;
        }
    }
}
