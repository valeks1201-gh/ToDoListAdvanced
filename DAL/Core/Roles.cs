using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Core
{
    public static class Roles
    {
        public const string Administrator = "Administrator"; //Global admimistrator
        public const string AdministratorOrg = "AdministratorOrg"; //Organization admimistrator (manages organization users nd has a full access to all organization data)
        public const string User = "User"; //User (for role model testing)
        public const string OrgDataManager = "OrgDataManager"; //Organization manager (has a full access to all organization data)
        public const string OrgDataViewer = "OrgDataViewer"; //Organization viewer (has a read only access to all organization data)
    }
}
