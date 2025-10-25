using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Core.Interfaces
{
    public interface IAccountManager
    {

        Task<bool> CheckPasswordAsync(Account user, string password);
        Task<(bool Succeeded, string[] Errors)> CreateRoleAsync(ApplicationRole role, IEnumerable<string> claims);
        Task<(bool Succeeded, string[] Errors)> CreateUserAsync(Account user, IEnumerable<string> roles, string password);
        Task<(bool Succeeded, string[] Errors)> DeleteRoleAsync(ApplicationRole role);
        Task<(bool Succeeded, string[] Errors)> DeleteRoleAsync(string roleName);
        Task<(bool Succeeded, string[] Errors)> DeleteUserAsync(Account user);
        Task<(bool Succeeded, string[] Errors)> DeleteUserAsync(string userId);
        Task<ApplicationRole> GetRoleByIdAsync(string roleId);
        Task<ApplicationRole> GetRoleByNameAsync(string roleName);
        Task<ApplicationRole> GetRoleLoadRelatedAsync(string roleName);
        Task<List<ApplicationRole>> GetRolesLoadRelatedAsync(int page, int pageSize);
        Task<(Account User, string[] Roles)?> GetUserAndRolesAsync(string userId, Account account);
        Task<Account> GetUserByEmailAsync(string email);
        Task<Account?> GetUserByIdAsync(string userId, Account account);
        Task<Account?> GetUserByUserNameAsync(string userName, Account account);
        Task<IList<string>> GetUserRolesAsync(Account user);
        Task<List<(Account User, string[] Roles)>> GetUsersAndRolesAsync(int page, int pageSize, Account account);
        Task<(bool Succeeded, string[] Errors)> ResetPasswordAsync(Account user, string newPassword);
        Task<bool> TestCanDeleteRoleAsync(string roleId);
        Task<bool> TestCanDeleteUserAsync(string userId);
        Task<(bool Succeeded, string[] Errors)> UpdatePasswordAsync(Account user, string currentPassword, string newPassword);
        Task<(bool Succeeded, string[] Errors)> UpdateRoleAsync(ApplicationRole role, IEnumerable<string> claims);
        Task<(bool Succeeded, string[] Errors)> UpdateUserAsync(Account user);
        Task<(bool Succeeded, string[] Errors)> UpdateUserAsync(Account user, IEnumerable<string> roles);
    }
}
