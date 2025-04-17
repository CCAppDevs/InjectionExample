using InjectionExample.Models;
using Microsoft.AspNetCore.Authorization;

namespace InjectionExample.Infrastructure
{
    public class OwnershipAuthorizationHandler :
        AuthorizationHandler<SameAuthorRequirement, Address>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            SameAuthorRequirement requirement,
            Address resource
        )
        {
            if (context.User.Identity?.Name == resource.UserId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    public class SameAuthorRequirement : IAuthorizationRequirement { }
}
