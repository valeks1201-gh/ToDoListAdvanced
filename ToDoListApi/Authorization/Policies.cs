using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListApi.Authorization
{
    public class Policies
    {
        ///<summary>Policy to allow viewing all user records.</summary>
        public const string ViewAllUsersPolicy = "View All Users";

        ///<summary>Policy to allow adding, removing and updating all user records.</summary>
        public const string ManageAllUsersPolicy = "Manage All Users";

        /// <summary>Policy to allow viewing details of all roles.</summary>
        public const string ViewAllRolesPolicy = "View All Roles";

        /// <summary>Policy to allow viewing details of all or specific roles (Requires roleName as parameter).</summary>
        public const string ViewRoleByRoleNamePolicy = "View Role by RoleName";

        /// <summary>Policy to allow adding, removing and updating all roles.</summary>
        public const string ManageAllRolesPolicy = "Manage All Roles";

        /// <summary>Policy to allow assigning roles the user has access to (Requires new and current roles as parameter).</summary>
        public const string AssignAllowedRolesPolicy = "Assign Allowed Roles";

        /// <summary>Policy to allow viewing all organization data.</summary>
        public const string ViewOrgDataPolicy = "View organization data";

        /// <summary>Policy to allow adding, removing and updating all organization data.</summary>
        public const string ManageOrgDataPolicy = "Manage organization data";

    }

    /// <summary>
    /// Operation Policy to allow adding, viewing, updating and deleting general or specific user records.
    /// </summary>
    public static class AccountManagementOperations
    {
        public const string CreateOperationName = "Create";
        public const string ReadOperationName = "Read";
        public const string UpdateOperationName = "Update";
        public const string DeleteOperationName = "Delete";

        public static UserAccountAuthorizationRequirement Create = new UserAccountAuthorizationRequirement(CreateOperationName);
        public static UserAccountAuthorizationRequirement Read = new UserAccountAuthorizationRequirement(ReadOperationName);
        public static UserAccountAuthorizationRequirement Update = new UserAccountAuthorizationRequirement(UpdateOperationName);
        public static UserAccountAuthorizationRequirement Delete = new UserAccountAuthorizationRequirement(DeleteOperationName);
    }

    /// <summary>
    /// Operation Policy to allow adding, viewing, updating and deleting general or specific organization data records.
    /// </summary>
    public static class OrgDataManagementOperations
    {
        public const string CreateOperationName = "Create";
        public const string ReadOperationName = "Read";
        public const string UpdateOperationName = "Update";
        public const string DeleteOperationName = "Delete";

        public static OrgDataAuthorizationRequirement Create = new OrgDataAuthorizationRequirement(CreateOperationName);
        public static OrgDataAuthorizationRequirement Read = new OrgDataAuthorizationRequirement(ReadOperationName);
        public static OrgDataAuthorizationRequirement Update = new OrgDataAuthorizationRequirement(UpdateOperationName);
        public static OrgDataAuthorizationRequirement Delete = new OrgDataAuthorizationRequirement(DeleteOperationName);
    }
}
