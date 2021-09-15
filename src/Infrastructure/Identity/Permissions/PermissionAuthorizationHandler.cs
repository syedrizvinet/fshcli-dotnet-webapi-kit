using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DN.WebApi.Application.Abstractions.Services.Identity;
using DN.WebApi.Domain.Constants;
using DN.WebApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;

namespace DN.WebApi.Infrastructure.Identity.Permissions
{
    internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService;
        public PermissionAuthorizationHandler(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User == null)
            {
                await Task.CompletedTask;
            }

            var userId = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (await _permissionService.HasPermissionAsync(userId, requirement.Permission))
            {
                context.Succeed(requirement);
                await Task.CompletedTask;
            }
        }
    }
}