using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer8.Extensions;
using IdentityServer8.Models;
using IdentityServer8.Services;
using Microsoft.AspNetCore.Identity;
using DAL.Core;
using DAL.Models;
//using ToDoListApi.Helpers;
using ToDoListCore;

namespace ToDoListApi.Authorization
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<Account> _userManager;
        private readonly IUserClaimsPrincipalFactory<Account> _claimsFactory;
        public ProfileService(UserManager<Account> userManager, IUserClaimsPrincipalFactory<Account> claimsFactory)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            var principal = await _claimsFactory.CreateAsync(user);

            var claims = principal.Claims.ToList();
            claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();

            if (user.JobTitle != null)
                claims.Add(new Claim(PropertyConstants.JobTitle, user.JobTitle));

            if (user.FullName != null)
                claims.Add(new Claim(PropertyConstants.FullName, user.FullName));

            if (user.Configuration != null)
                claims.Add(new Claim(PropertyConstants.Configuration, user.Configuration));

            if (user?.Organization != null)
                claims.Add(new Claim(PropertyConstants.Organization, user.Organization.ToString()));

            var organizationsProfile = (user?.Organization != null) ? CoreUtilities.OrganizationsProfiles()?[(Organization)user.Organization] : null;
            var methodTypes = organizationsProfile?.MethodTypes;

            if (methodTypes != null)
            {
               claims.Add(new Claim(PropertyConstants.MethodTypes, methodTypes.ToList().Select(i => i.ToString()).Aggregate((i, j) => i + "," + j)));
            }

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);

            context.IsActive = (user != null) && user.IsEnabled;
        }
    }
}