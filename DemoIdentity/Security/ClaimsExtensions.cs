using DemoIdentity.Constant;
using System.Security.Claims;

namespace DemoIdentity.Security
{
    public static class ClaimsExtensions
    {
        public static HashSet<string> GetPermissions(this ClaimsPrincipal user)
        {
            // get permission from calims principal of user
            IEnumerable<Claim> permissionClaims = user?.FindAll(CustomClaims.Permission) ?? new List<Claim>();
            // Extract the values into a HashSet to ensure uniqueness
            var permissions = new HashSet<string>();
            foreach (var claim in permissionClaims)
            {
                if (!string.IsNullOrWhiteSpace(claim.Value))
                {
                    permissions.Add(claim.Value);
                }
            }

            return permissions;
        }
    }
}
