using DAL.Core;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoListCore.Enums;
using ToDoListCore.Models;
using Newtonsoft.Json;
using DAL.Models;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ToDoListCore
{
    public static class CoreUtilities
    {
        public static string? GetWebApiUrl(IConfiguration configuration)
        {
            var hostType = GetHostType();

            if (hostType.Equals(HostType.VisualStudio))
            {
                return (string)configuration["ApiUrlInVS"].TrimEnd('/');  //Visual Studio
            }
            else if (hostType.Equals(HostType.IIS))
            {
                return (string)configuration["ApiUrlInIIS"].TrimEnd('/');  //IIS
            }
            else if (hostType.Equals(HostType.Kestrel)) //Kestrel
            {
                return (string)configuration["ApiUrlInKestrel"].TrimEnd('/');
            }
            else
            {
                return (string)configuration["ApiUrlInDocker"].TrimEnd('/'); //Docker
            }
        }

        public static string? GetCurrentProjectUrl(IConfiguration configuration)
        {
            var hostType = GetHostType();

            if (hostType.Equals(HostType.VisualStudio))
            {
                return Environment.GetEnvironmentVariable("ASPNETCORE_URLS")?.Split(';').Where(x => x.ToLower().StartsWith("https"))?.FirstOrDefault()?.TrimEnd('/');  //Visual Studio
            }
            else if (hostType.Equals(HostType.IIS))
            {
                return (string)configuration["CurrentProjectUrlInIIS"].TrimEnd('/');  //IIS
            }
            else if (hostType.Equals(HostType.Kestrel))
            {
                return (string)configuration["CurrentProjectUrlInKestrel"].TrimEnd('/');
            }
            else
            {
                return (string)configuration["CurrentProjectUrlInDocker"].TrimEnd('/'); //Docker
            }
        }

        public static HostType GetHostType()
        {
            var aspNetCoreUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")?.Split(';').Where(x => x.ToLower().StartsWith("https"))?.FirstOrDefault()?.TrimEnd('/');
            var isRunningInProcessIIS = CoreUtilities.IsRunningInProcessIIS();

            if (aspNetCoreUrl != null && aspNetCoreUrl.ToLower().Contains("localhost"))
            {
                return HostType.VisualStudio;  //Visual Studio
            }
            else if (aspNetCoreUrl != null)
            {
                return HostType.DockerContainer;  //Docker
            }
            if (!(isRunningInProcessIIS) && (aspNetCoreUrl == null))
            {
                return HostType.Kestrel; ;  //Kestrel
            }
            else
            {
                return HostType.IIS; //IIS
            }
        }

        public static Dictionary<Organization, OrganizationProfile>? OrganizationsProfiles()
        {
            Dictionary<Organization, OrganizationProfile>? organizationsProfiles = new Dictionary<Organization, OrganizationProfile>();
            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var organizationsProfilesJSON = File.ReadAllText(sCurrentDirectory + @"JSON\OrganizationProfiles.json");

            if (organizationsProfilesJSON != null)
            {
                organizationsProfiles = JsonConvert.DeserializeObject<Dictionary<Organization, OrganizationProfile>>(organizationsProfilesJSON);
            }

            return organizationsProfiles;
        }

        /// <summary>
        /// Check if this process is running on Windows in an in process instance in IIS
        /// </summary>
        /// <returns>True if Windows and in an in process instance on IIS, false otherwise</returns>
        public static bool IsRunningInProcessIIS()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return false;
            }

            string processName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);
            return (processName.Contains("w3wp", StringComparison.OrdinalIgnoreCase) ||
                processName.Contains("iisexpress", StringComparison.OrdinalIgnoreCase));
        }

    }
}
