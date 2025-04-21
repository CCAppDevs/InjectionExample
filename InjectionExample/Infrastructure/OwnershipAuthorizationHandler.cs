using InjectionExample.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
            // currently failing
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == resource.UserId)
            {
                // switch to success
                context.Succeed(requirement);
            }

            // return the task whether or not the task failed or succeeded
            return Task.CompletedTask;
        }
    }

    public class SameAuthorRequirement : IAuthorizationRequirement { }
}
