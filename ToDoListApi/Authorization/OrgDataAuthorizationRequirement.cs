using DAL.Core;
using Microsoft.AspNetCore.Authorization;
using ToDoListApi.Helpers;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListApi.Authorization
{
    public class OrgDataAuthorizationRequirement : IAuthorizationRequirement
    {
        public OrgDataAuthorizationRequirement(string operationName)
        {
            this.OperationName = operationName;
        }
        public string OperationName { get; private set; }
    }

    public class ViewOrgDataAuthorizationHandler : AuthorizationHandler<UserAccountAuthorizationRequirement, string>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserAccountAuthorizationRequirement requirement, string targetUserId)
        {
            if (context.User == null || requirement.OperationName != OrgDataManagementOperations.ReadOperationName)
                return Task.CompletedTask;

            if (context.User.HasClaim(ClaimConstants.Permission, ApplicationPermissions.ViewOrgData) || Utilities.GetIsSameUser(context.User, targetUserId))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class ManageOrgDataAuthorizationHandler : AuthorizationHandler<UserAccountAuthorizationRequirement, string>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserAccountAuthorizationRequirement requirement, string targetUserId)
        {
            if (context.User == null ||
                (requirement.OperationName != OrgDataManagementOperations.CreateOperationName &&
                 requirement.OperationName != OrgDataManagementOperations.UpdateOperationName &&
                 requirement.OperationName != OrgDataManagementOperations.DeleteOperationName))
                return Task.CompletedTask;

            if (context.User.HasClaim(ClaimConstants.Permission, ApplicationPermissions.ManageOrgData) || Utilities.GetIsSameUser(context.User, targetUserId))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}