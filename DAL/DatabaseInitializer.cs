using DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Core;
using DAL.Core.Interfaces;
using IdentityServer8.EntityFramework;
using IdentityServer8.EntityFramework.DbContexts;
using IdentityServer8.EntityFramework.Mappers;
using IdentityServer8.EntityFramework.Entities;
using AutoMapper;
using IdentityServer8.Models;
using Microsoft.VisualBasic;
using IdentityServer8.EntityFramework.Interfaces;

namespace DAL
{
    public interface IDatabaseInitializer
    {
        Task SeedAsync();
    }

    public class DatabaseInitializer : IDatabaseInitializer
    {
        readonly ApplicationDbContext _context;
        readonly ConfigurationDbContext _configurationDbContext;
        readonly PersistedGrantDbContext _persistedGrantDbContext;
        readonly IAccountManager _accountManager;
        readonly ILogger _logger;

        //public DatabaseInitializer(ApplicationDbContext context, IAccountManager accountManager, ILogger<DatabaseInitializer> logger)
        //{
        //    _accountManager = accountManager;
        //    _context = context;
        //    _logger = logger;
        //}

        public DatabaseInitializer(ApplicationDbContext context, ConfigurationDbContext configurationDbContext, PersistedGrantDbContext persistedGrantDbContext, IAccountManager accountManager, ILogger<DatabaseInitializer> logger)
        {
            _accountManager = accountManager;
            _context = context;
            _configurationDbContext = configurationDbContext;
            _persistedGrantDbContext = persistedGrantDbContext;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync().ConfigureAwait(false);
            await _configurationDbContext.Database.MigrateAsync().ConfigureAwait(false);
            await _persistedGrantDbContext.Database.MigrateAsync().ConfigureAwait(false);
            await SeedDefaultUsersAsync();
            await SeedDemoDataAsync();
        }

        private async Task SeedDefaultUsersAsync()
        {
            if (!await _configurationDbContext.Clients.AnyAsync())
            {
                _logger.LogInformation("Seeding IdentityServer Clients");
                foreach (var client in IdentityServerConfig.GetClients().ToList())
                {
                    _configurationDbContext.Clients.Add(client.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!await _configurationDbContext.IdentityResources.AnyAsync())
            {
                _logger.LogInformation("Seeding IdentityServer Identity Resources");
                foreach (var resource in IdentityServerConfig.GetIdentityResources().ToList())
                {
                    _configurationDbContext.IdentityResources.Add(resource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!await _configurationDbContext.ApiResources.AnyAsync())
            {
                _logger.LogInformation("Seeding IdentityServer API Resources");
                foreach (var resource in IdentityServerConfig.GetApiResources().ToList())
                {
                    _configurationDbContext.ApiResources.Add(resource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!await _configurationDbContext.ApiScopes.AnyAsync())
            {
                _logger.LogInformation("Seeding IdentityServer Scopes");
                foreach (var scope in IdentityServerConfig.GetApiScopes().ToList())
                {
                    _configurationDbContext.ApiScopes.Add(scope.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }


            if (!await _context.Users.AnyAsync())
            {
                _logger.LogInformation("Generating inbuilt accounts");

                const string adminRoleName = Roles.Administrator;
                const string userRoleName = Roles.User;

                await EnsureRoleAsync(adminRoleName, "Default administrator", ApplicationPermissions.GetAllPermissionValues());
                await EnsureRoleAsync(userRoleName, "Default user", new string[] { });

                await CreateUserAsync("admin", "Password-123", "Inbuilt Administrator", String.Empty, String.Empty, "admin@test.com", "+7 (123) 000-0000", null, new string[] { adminRoleName });
                await CreateUserAsync("user", "Password-123", "Inbuilt Standard User", String.Empty, String.Empty, "user@test.com", "+7 (123) 000-0001", null, new string[] { userRoleName });

                //Organization admins
                const string adminRoleNameOrg = Roles.AdministratorOrg;
                await EnsureRoleAsync(adminRoleNameOrg, "Default Organization administrator", ApplicationPermissions.GetOrgAdminPermissionValues());
                await CreateUserAsync("adminOrg1", "PasswordORG1-123", "Inbuilt ORG1 Administrator", String.Empty, String.Empty, "adminorg1@test.com", "+7 (123) 000-0000", Organization.ORG1, new string[] { adminRoleNameOrg });
                await CreateUserAsync("adminOrg2", "PasswordORG2-123", "Inbuilt ORG2 Administrator", String.Empty, String.Empty, "adminorg2@test.com", "+7 (123) 000-0000", Organization.ORG1, new string[] { adminRoleNameOrg });
               
                //Organization data roles
                const string orgDataManagerRoleName = Roles.OrgDataManager;
                const string orgDataViewerRoleName = Roles.OrgDataViewer;
                await EnsureRoleAsync(orgDataManagerRoleName, "Default Organization Data Manager", ApplicationPermissions.GetOrgDataManagerPermissionValues());
                await EnsureRoleAsync(orgDataViewerRoleName, "Default Organization Data Viewer", ApplicationPermissions.GetOrgDataViewerPermissionValues());
                _logger.LogInformation("Inbuilt accounts generation completed");
            }
        }

        private async Task EnsureRoleAsync(string roleName, string description, string[] claims)
        {
            ApplicationRole applicationRole = await _accountManager.GetRoleByNameAsync(roleName);

            if (applicationRole == null)
            {
                _logger.LogInformation($"Generating default role: {roleName}");
                applicationRole = new ApplicationRole(roleName, description);
                var result = await this._accountManager.CreateRoleAsync(applicationRole, claims);

                if (!result.Succeeded)
                    throw new Exception($"Seeding \"{description}\" role failed. Errors: {string.Join(Environment.NewLine, result.Errors)}");
            }
            else
            {
                _logger.LogInformation($"Updating default role: {roleName}");
                var result = await this._accountManager.UpdateRoleAsync(applicationRole, claims);

                if (!result.Succeeded)
                    throw new Exception($"Seeding \"{description}\" role failed. Errors: {string.Join(Environment.NewLine, result.Errors)}");
            }
        }

        private async Task<Account> CreateUserAsync(string userName, string password, string fullName, string firstName, string lastName, string email, string phoneNumber, Organization? organization, string[] roles)
        {
            _logger.LogInformation($"Generating default user: {userName}");

            Account applicationUser = new Account
            {
                UserName = userName,
                FullName = fullName,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNumber = phoneNumber,
                Organization = organization,
                EmailConfirmed = true,
                IsEnabled = true
            };

            var result = await _accountManager.CreateUserAsync(applicationUser, roles, password);

            if (!result.Succeeded)
                throw new Exception($"Seeding \"{userName}\" user failed. Errors: {string.Join(Environment.NewLine, result.Errors)}");

            return applicationUser;
        }

        private async Task SeedDemoDataAsync()
        {

        }

    }
}
