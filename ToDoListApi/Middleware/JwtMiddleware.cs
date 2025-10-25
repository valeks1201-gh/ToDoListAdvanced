using DAL;
using IdentityModel;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;
using Microsoft.AspNetCore.Identity;
using DAL.Core.Interfaces;
using DAL.Core;
using ToDoListApi.Helpers;

namespace ToDoListApi.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;
    
        public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
        {
            _next = next;
            _appSettings = appSettings.Value;
        }

        public async Task Invoke(HttpContext context, ApplicationDbContext dataContext, UserManager<Account> _userManager)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                await AttachAccountToContext(context, dataContext, token, _userManager);

            await _next(context);
        }

        private async Task AttachAccountToContext(HttpContext context, ApplicationDbContext dataContext, string token, UserManager<Account> _userManager)
        {
            try
            {
                var openIdConfig = await IdentityServerUtilities.GetOpenIdConfig();
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKeys = openIdConfig.SigningKeys
                }, out SecurityToken validatedToken);

                // attach account to context on successful jwt validation
                var jwtToken = (JwtSecurityToken)validatedToken;
                var accountId = jwtToken.Claims.First(x => x.Type == "sub").Value;
                var account = await _userManager.FindByIdAsync(accountId);
                context.Items["Account"] = account;
            }
            catch
            {
                // do nothing if jwt validation fails
                // account is not attached to context so request won't have access to secure routes
            }
        }
    }
}
