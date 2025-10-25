using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Org.BouncyCastle.Tls;
using ToDoListCore;

namespace ToDoListApi.Helpers
{
    public class IdentityServerUtilities
    {
        private static OpenIdConnectConfiguration? _openIdConfig;

        public async static Task<OpenIdConnectConfiguration> GetOpenIdConfig()
        {
            {
                if (_openIdConfig != null) return _openIdConfig;
                var authorityEndpoint = CoreUtilities.GetCurrentProjectUrl(Configuration.AppSettings) + '/';
                var openIdConfigurationEndpoint = $"{authorityEndpoint}.well-known/openid-configuration";
                IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(openIdConfigurationEndpoint, new OpenIdConnectConfigurationRetriever());
                _openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);
                return _openIdConfig;
            }
        }
        
    }
}
