using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Core
{
    public static class ApplicationPermissions
    {
        public static ReadOnlyCollection<ApplicationPermission> AllPermissions;
        public static ReadOnlyCollection<ApplicationPermission> OrgAdminPermissions;
        public static ReadOnlyCollection<ApplicationPermission> OrgDataManagerPermissions;
        public static ReadOnlyCollection<ApplicationPermission> OrgDataViewerPermissions;

        public const string UsersPermissionGroupName = "User Permissions";
        public static ApplicationPermission ViewUsers = new ApplicationPermission("View Users", "users.view", UsersPermissionGroupName, "Permission to view other users account details");
        public static ApplicationPermission ManageUsers = new ApplicationPermission("Manage Users", "users.manage", UsersPermissionGroupName, "Permission to create, delete and modify other users account details");

        public const string RolesPermissionGroupName = "Role Permissions";
        public static ApplicationPermission ViewRoles = new ApplicationPermission("View Roles", "roles.view", RolesPermissionGroupName, "Permission to view available roles");
        public static ApplicationPermission ManageRoles = new ApplicationPermission("Manage Roles", "roles.manage", RolesPermissionGroupName, "Permission to create, delete and modify roles");
        public static ApplicationPermission AssignRoles = new ApplicationPermission("Assign Roles", "roles.assign", RolesPermissionGroupName, "Permission to assign roles to users");

        public const string OrgDataPermissionGroupName = "Organization Data Permissions";
        public static ApplicationPermission ViewOrgData = new ApplicationPermission("View Organization Data", "Orgdata.view", OrgDataPermissionGroupName, "Permission to view organization data");
        public static ApplicationPermission ManageOrgData = new ApplicationPermission("Manage Organization Data", "Orgdata.manage", OrgDataPermissionGroupName, "Permission to create, delete and modify organization data");

        static ApplicationPermissions()
        {
            List<ApplicationPermission> allPermissions = new List<ApplicationPermission>()
            {
                ViewUsers,
                ManageUsers,

                ViewRoles,
                ManageRoles,
                AssignRoles,

                ViewOrgData,
                ManageOrgData
            };
            AllPermissions = allPermissions.AsReadOnly();

            List<ApplicationPermission> orgAdminPermissions = new List<ApplicationPermission>()
            {
                ViewUsers,
                ManageUsers,

                ViewOrgData,
                ManageOrgData
            };
            OrgAdminPermissions = orgAdminPermissions.AsReadOnly();

            List<ApplicationPermission> orgDataManagerPermissions = new List<ApplicationPermission>()
            {
                ViewOrgData,
                ManageOrgData
            };
            OrgDataManagerPermissions = orgDataManagerPermissions.AsReadOnly();


            List<ApplicationPermission> orgDataViewerPermissions = new List<ApplicationPermission>()
            {
                ViewOrgData
            };
            OrgDataViewerPermissions = orgDataViewerPermissions.AsReadOnly();
        }

        public static ApplicationPermission GetPermissionByName(string permissionName)
        {
            return AllPermissions.Where(p => p.Name == permissionName).SingleOrDefault();
        }

        public static ApplicationPermission GetPermissionByValue(string permissionValue)
        {
            return AllPermissions.Where(p => p.Value == permissionValue).SingleOrDefault();
        }

        public static string[] GetAllPermissionValues()
        {
            return AllPermissions.Select(p => p.Value).ToArray();
        }

        public static string[] GetAdministrativePermissionValues()
        {
            return new string[] { ManageUsers, ManageRoles, AssignRoles };
        }

        public static string[] GetOrgAdminPermissionValues()
        {
            return OrgAdminPermissions.Select(p => p.Value).ToArray();
        }

        public static string[] GetOrgDataManagerPermissionValues()
        {
            return OrgDataManagerPermissions.Select(p => p.Value).ToArray();
        }

        public static string[] GetOrgDataViewerPermissionValues()
        {
            return OrgDataViewerPermissions.Select(p => p.Value).ToArray();
        }
    }



    public class ApplicationPermission
    {
        public ApplicationPermission()
        { }

        public ApplicationPermission(string name, string value, string groupName, string description = null)
        {
            Name = name;
            Value = value;
            GroupName = groupName;
            Description = description;
        }



        public string Name { get; set; }
        public string Value { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }


        public override string ToString()
        {
            return Value;
        }


        public static implicit operator string(ApplicationPermission permission)
        {
            return permission.Value;
        }
    }
}
