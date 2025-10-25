using DAL.Core;
using Microsoft.AspNetCore.Authorization;
using ToDoListApi.Helpers;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListApi.Authorization
{
    public class UserAccountAuthorizationRequirement : IAuthorizationRequirement
    {
        public UserAccountAuthorizationRequirement(string operationName)
        {
            this.OperationName = operationName;
        }
        public string OperationName { get; private set; }
    }

    public class ViewUserAuthorizationHandler : AuthorizationHandler<UserAccountAuthorizationRequirement, string>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserAccountAuthorizationRequirement requirement, string targetUserId)
        {
            if (context.User == null || requirement.OperationName != AccountManagementOperations.ReadOperationName)
                return Task.CompletedTask;

            if (context.User.HasClaim(ClaimConstants.Permission, ApplicationPermissions.ViewUsers) || Utilities.GetIsSameUser(context.User, targetUserId))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class ManageUserAuthorizationHandler : AuthorizationHandler<UserAccountAuthorizationRequirement, string>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserAccountAuthorizationRequirement requirement, string targetUserId)
        {
            if (context.User == null ||
                (requirement.OperationName != AccountManagementOperations.CreateOperationName &&
                 requirement.OperationName != AccountManagementOperations.UpdateOperationName &&
                 requirement.OperationName != AccountManagementOperations.DeleteOperationName))
                return Task.CompletedTask;

            if (context.User.HasClaim(ClaimConstants.Permission, ApplicationPermissions.ManageUsers) || Utilities.GetIsSameUser(context.User, targetUserId))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}