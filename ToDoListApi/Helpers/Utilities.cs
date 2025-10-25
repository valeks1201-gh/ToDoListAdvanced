using IdentityModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DAL.Core;

namespace ToDoListApi.Helpers
{
    public static class Utilities
    {
        public static void QuickLog(string text, string logPath)
        {
            var dirPath = Path.GetDirectoryName(logPath);

            if ((dirPath != null) && (!Directory.Exists(dirPath)))
            {
                Directory.CreateDirectory(dirPath);
            }

            using (StreamWriter writer = File.AppendText(logPath))
            {
                writer.WriteLine($"{DateTime.Now} - {text}");
            }
        }

        public static string? GetUserId(ClaimsPrincipal? user)
        {
            if (user == null) { return null; }
            var result = user.FindFirst(JwtClaimTypes.Subject)?.Value?.Trim();

            if (result == null) 
            {
                result = user.FindFirst(@"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value?.Trim();
            }

            //var result2 = user.FindFirst(JwtClaimTypes.Name)?.Value?.Trim();
            //var result3 = user.FindFirst(JwtClaimTypes.Id)?.Value?.Trim();
            return result;
        }

        public static bool GetIsSameUser(ClaimsPrincipal user, string targetUserId)
        {
            if (string.IsNullOrWhiteSpace(targetUserId))
                return false;

            return GetUserId(user) == targetUserId;
        }

        public static string[] GetRoles(ClaimsPrincipal identity)
        {
            return identity.Claims
                .Where(c => c.Type == JwtClaimTypes.Role)
                .Select(c => c.Value)
                .ToArray();
        }

        public static List<int>? GetKvpListsKeys(List<KeyValuePair<int, List<int>>>? kvpList1, List<KeyValuePair<int, List<int>>>? kvpList2)
        {
            List<int>? kvpList1Keys = null;
            List<int>? kvpList2Keys = null;

            if (kvpList1 != null)
            {
                kvpList1Keys = kvpList1.Select(x => x.Key).Distinct().ToList();
            }

            if (kvpList2 != null)
            {
                kvpList2Keys = kvpList2.Select(x => x.Key).Distinct().ToList();
            }

            List<int>? kvpListsIds = null;

            if ((kvpList1Keys != null) && (kvpList2Keys == null))
            {
                kvpListsIds = kvpList1Keys;
            }
            else if ((kvpList1Keys == null) && (kvpList2Keys != null))
            {
                kvpListsIds = kvpList2Keys;
            }
            else if ((kvpList1Keys != null) && (kvpList2Keys != null))
            {
                kvpListsIds = kvpList1Keys.Union(kvpList2Keys).Distinct().Order().ToList();
            }

            return kvpListsIds;
        }

        public static Dictionary<int, List<int>>? GetKvpListsValuesIds(List<KeyValuePair<int, List<int>>>? kvpList1, List<KeyValuePair<int, List<int>>>? kvpList2)
        {
            Dictionary<int, List<int>>? result = null;
            var kvpListsIds = GetKvpListsKeys(kvpList1, kvpList2);

            if (kvpListsIds != null)
            {
                foreach (var kvpListsId in kvpListsIds)
                {
                    List<int>? kvpList1ValuesIds = null;
                    List<int>? kvpList2ValuesIds = null;
                    List<int>? kvpListValuesIds = null;

                    if (kvpList1 != null)
                    {
                        kvpList1ValuesIds = kvpList1.Where(y => y.Key == kvpListsId)
                                                                    .SelectMany(z => z.Value)
                                                                    .Distinct()
                                                                    .ToList();
                    }

                    if (kvpList2 != null)
                    {
                        kvpList2ValuesIds = kvpList2.Where(y => y.Key == kvpListsId)
                                                                    .SelectMany(z => z.Value)
                                                                    .Distinct()
                                                                    .ToList();
                    }

                    if ((kvpList1ValuesIds != null) && (kvpList2ValuesIds == null))
                    {
                        kvpListValuesIds = kvpList1ValuesIds;
                    }
                    else if ((kvpList1ValuesIds == null) && (kvpList2ValuesIds != null))
                    {
                        kvpListValuesIds = kvpList2ValuesIds;
                    }
                    else if ((kvpList1ValuesIds != null) && (kvpList2ValuesIds != null))
                    {
                        kvpListValuesIds = kvpList1ValuesIds.Union(kvpList2ValuesIds).Distinct().ToList();
                    }

                    if (result == null)
                    {
                        result = new Dictionary<int, List<int>>();
                    }

                    if (kvpListValuesIds != null)
                    {
                        result.Add(kvpListsId, kvpListValuesIds.Order().ToList());
                    }
                }
            }

            return result;
        }
    }
}
