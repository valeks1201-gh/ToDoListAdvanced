using DAL.Core;
using DAL.Models;
using ToDoListCore.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ToDoListCore.Models
{
    public class OrganizationProfile
    {
        public OrganizationProfile()
        {
            MethodTypes = new List<MethodType>();
        }

        public OrganizationProfile(string? externalApiUrl, List<MethodType> methodTypes)
        {
            ExternalApiUrl = externalApiUrl;
            MethodTypes = methodTypes;
        }
        public string? ExternalApiUrl
        {
            get; set;
        }

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<MethodType> MethodTypes
        {
            get; set;
        }
    }
}
