using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using ToDoListApi.ViewModels;
using AutoMapper;
using DAL.Models;
using DAL.Core.Interfaces;
using ToDoListApi.Authorization;
using ToDoListApi.Helpers;
using Microsoft.AspNetCore.JsonPatch;
using DAL.Core;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using IdentityModel.Client;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ToDoListApi.Controllers
{
    // [Authorize]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class AccountController : BaseController //ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IAccountManager _accountManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<AccountController> _logger;
        private const string GetUserByIdActionName = "GetUserById";
        private const string GetRoleByIdActionName = "GetRoleById";

        public AccountController(IMapper mapper, IAccountManager accountManager, IAuthorizationService authorizationService,
            ILogger<AccountController> logger)
        {
            _mapper = mapper;
            _accountManager = accountManager;
            _authorizationService = authorizationService;
            _logger = logger;
        }
       
        /// <summary>
        /// Get current user info
        /// </summary>
        /// <returns></returns>
        [HttpGet("users/me")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[Authorize(Authorization.Policies.ViewAllUsersPolicy)]
        [ProducesResponseType(200, Type = typeof(UserViewModel))]
        public async Task<IActionResult> GetCurrentUser()
        {
           

            var result= await GetUserById(Utilities.GetUserId(this.User));
            return result;
        }

        /// <summary>
        /// Get user by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("users/{id}", Name = GetUserByIdActionName)]
        [ProducesResponseType(200, Type = typeof(UserViewModel))]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserById(string id)
        {
            if (!(await _authorizationService.AuthorizeAsync(this.User, id, AccountManagementOperations.Read)).Succeeded)
                return new ChallengeResult();


            UserViewModel userVM = await GetUserViewModelHelper(id);

            if (userVM != null)
                return Ok(userVM);
            else
                return NotFound(id);
        }

        /// <summary>
        /// Get user by name
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpGet("users/username/{userName}")]
        [ProducesResponseType(200, Type = typeof(UserViewModel))]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserByUserName(string userName)
        {
            Account? appUser = await _accountManager.GetUserByUserNameAsync(userName, Account);

            if (!(await _authorizationService.AuthorizeAsync(this.User, appUser?.Id ?? "", AccountManagementOperations.Read)).Succeeded)
                return new ChallengeResult();

            if (appUser == null)
                return NotFound(userName);

            return await GetUserById(appUser.Id);
        }

        /// <summary>
        /// Get users list
        /// </summary>
        /// <returns></returns>
        [HttpGet("users")]
        [Authorize(Authorization.Policies.ViewAllUsersPolicy)]
        [ProducesResponseType(200, Type = typeof(List<UserViewModel>))]
        public async Task<IActionResult> GetUsers()
        {
          return await GetUsers(-1, -1);
        }

        /// <summary>
        /// Get users list. Paging is used
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("users/{pageNumber:int}/{pageSize:int}")]
        [Authorize(Authorization.Policies.ViewAllUsersPolicy)]
        [ProducesResponseType(200, Type = typeof(List<UserViewModel>))]
        public async Task<IActionResult> GetUsers(int pageNumber, int pageSize)
        {
            var usersAndRoles = await _accountManager.GetUsersAndRolesAsync(pageNumber, pageSize, Account);

            List<UserViewModel> usersVM = new List<UserViewModel>();

            foreach (var item in usersAndRoles)
            {
                var userVM = _mapper.Map<UserViewModel>(item.User);
                userVM.Roles = item.Roles;

                usersVM.Add(userVM);
            }
            return Ok(usersVM);
        }

        /// <summary>
        /// Update current user's properties
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPut("users/me")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UserEditViewModel user)
        {
           var userId = Utilities.GetUserId(this.User);
           return await UpdateUser(userId, user);
        }

        /// <summary>
        /// Update user's properties
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPut("users/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserEditViewModel user)
        {
            Account appUser = await _accountManager.GetUserByIdAsync(id, Account);
            string[] currentRoles = appUser != null ? (await _accountManager.GetUserRolesAsync(appUser)).ToArray() : null;

            var manageUsersPolicy = _authorizationService.AuthorizeAsync(this.User, id, AccountManagementOperations.Update);
            var assignRolePolicy = _authorizationService.AuthorizeAsync(this.User, (user.Roles, currentRoles), Authorization.Policies.AssignAllowedRolesPolicy);

            if (Account.Organization == null)
            {
                if ((await Task.WhenAll(manageUsersPolicy, assignRolePolicy)).Any(r => !r.Succeeded))
                    return new ChallengeResult();
            }
            else
            {
                if ((await Task.WhenAll(manageUsersPolicy)).Any(r => !r.Succeeded))
                    return new ChallengeResult();

                if (user.Roles.Where(x => !(x.Equals(Roles.OrgDataViewer) || (x.Equals(Roles.OrgDataManager))) ).Any())
                    return new ChallengeResult();
            }

            if (ModelState.IsValid)
            {
                if (user == null)
                    return BadRequest($"{nameof(user)} cannot be null");

                if (!string.IsNullOrWhiteSpace(user.Id) && id != user.Id)
                    return BadRequest("Conflicting user id in parameter and model data");

                if (appUser == null)
                    return NotFound(id);

                bool isPasswordChanged = !string.IsNullOrWhiteSpace(user.NewPassword);
                bool isUserNameChanged = !appUser.UserName.Equals(user.UserName, StringComparison.OrdinalIgnoreCase);

                if (Utilities.GetUserId(this.User) == id)
                {
                    if (string.IsNullOrWhiteSpace(user.CurrentPassword))
                    {
                        if (isPasswordChanged)
                            AddError("Current password is required when changing your own password", "Password");

                        if (isUserNameChanged)
                            AddError("Current password is required when changing your own username", "Username");
                    }
                    else if (isPasswordChanged || isUserNameChanged)
                    {
                        if (!await _accountManager.CheckPasswordAsync(appUser, user.CurrentPassword))
                            AddError("The username/password couple is invalid.");
                    }
                }

                if (ModelState.IsValid)
                {
                    _mapper.Map<UserEditViewModel, Account>(user, appUser);

                    var result = await _accountManager.UpdateUserAsync(appUser, user.Roles);
                    if (result.Succeeded)
                    {
                        if (isPasswordChanged)
                        {
                            if (!string.IsNullOrWhiteSpace(user.CurrentPassword))
                                result = await _accountManager.UpdatePasswordAsync(appUser, user.CurrentPassword, user.NewPassword);
                            else
                                result = await _accountManager.ResetPasswordAsync(appUser, user.NewPassword);
                        }

                        if (result.Succeeded)
                            return NoContent();
                    }

                    AddError(result.Errors);
                }
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// Update current user's properties by patching
        /// </summary>
        /// <param name="patch"></param>
        /// <returns></returns>
        [HttpPatch("users/me")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] JsonPatchDocument<UserPatchViewModel> patch)
        {
            return await UpdateUser(Utilities.GetUserId(this.User), patch);
        }

        /// <summary>
        /// Update user's properties by patching
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patch"></param>
        /// <returns></returns>
        [HttpPatch("users/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] JsonPatchDocument<UserPatchViewModel> patch)
        {
            if (!(await _authorizationService.AuthorizeAsync(this.User, id, AccountManagementOperations.Update)).Succeeded)
                return new ChallengeResult();

            if (ModelState.IsValid)
            {
                if (patch == null)
                    return BadRequest($"{nameof(patch)} cannot be null");

                Account appUser = await _accountManager.GetUserByIdAsync(id, Account);

                if (appUser == null)
                    return NotFound(id);


                UserPatchViewModel userPVM = _mapper.Map<UserPatchViewModel>(appUser);
                patch.ApplyTo(userPVM, (e) => AddError(e.ErrorMessage));

                if (ModelState.IsValid)
                {
                    _mapper.Map<UserPatchViewModel, Account>(userPVM, appUser);

                    var result = await _accountManager.UpdateUserAsync(appUser);
                    if (result.Succeeded)
                        return NoContent();
                    AddError(result.Errors);
                }
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// Register user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("users")]
        [Authorize(Authorization.Policies.ManageAllUsersPolicy)]
        [ProducesResponseType(201, Type = typeof(UserViewModel))]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Register([FromBody] UserEditViewModel user)
        {
            if (Account.Organization == null)
            {
                if (!(await _authorizationService.AuthorizeAsync(this.User, (user.Roles, new string[] { }), Authorization.Policies.AssignAllowedRolesPolicy)).Succeeded)
                    return new ChallengeResult();
            }
            else
            {
                if (user.Roles.Where(x => !(x.Equals(Roles.OrgDataViewer) || (x.Equals(Roles.OrgDataManager)))).Any())
                    return new ChallengeResult();
            }

            Guid guid;
            if (Guid.TryParse(user.Id, out guid))
            {
                user.Id = guid.ToString();
            }
            else
            {
                user.Id = Guid.NewGuid().ToString();
            }

            if (ModelState.IsValid)
            {
                if (user == null)
                    return BadRequest($"{nameof(user)} cannot be null");
                Account appUser = _mapper.Map<Account>(user);

                if (Account.Organization != null)
                {
                    appUser.Organization = Account.Organization;
                }

                var result = await _accountManager.CreateUserAsync(appUser, user.Roles, user.NewPassword);
                if (result.Succeeded)
                {
                    UserViewModel userVM = await GetUserViewModelHelper(appUser.Id);
                    return CreatedAtAction(GetUserByIdActionName, new { id = userVM.Id }, userVM);
                }

                AddError(result.Errors);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpDelete("users/{id}")]
        [ProducesResponseType(200, Type = typeof(UserViewModel))]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (!(await _authorizationService.AuthorizeAsync(this.User, id, AccountManagementOperations.Delete)).Succeeded)
                return new ChallengeResult();

            Account? appUser = await _accountManager.GetUserByIdAsync(id, Account);

            if (appUser == null)
                return NotFound(id);

            if (!await _accountManager.TestCanDeleteUserAsync(id))
                return BadRequest("User cannot be deleted. Delete all orders associated with this user and try again");

            UserViewModel userVM = await GetUserViewModelHelper(appUser.Id);

            var result = await _accountManager.DeleteUserAsync(appUser);
            if (!result.Succeeded)
                throw new Exception("The following errors occurred whilst deleting user: " + string.Join(", ", result.Errors));
            return Ok(userVM);
        }

        /// <summary>
        /// Unblock user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("users/unblock/{id}")]
        [Authorize(Authorization.Policies.ManageAllUsersPolicy)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UnblockUser(string id)
        {
            Account? appUser = await _accountManager.GetUserByIdAsync(id, Account);

            if (appUser == null)
                return NotFound(id);

            appUser.LockoutEnd = null;
            var result = await _accountManager.UpdateUserAsync(appUser);
            if (!result.Succeeded)
                throw new Exception("The following errors occurred whilst unblocking user: " + string.Join(", ", result.Errors));
            return NoContent();
        }

        /// <summary>
        /// Get user's properties by id
        /// </summary>
        /// <returns></returns>
        [HttpGet("users/me/preferences")]
        [ProducesResponseType(200, Type = typeof(string))]
        public async Task<IActionResult> UserPreferences()
        {
            var userId = Utilities.GetUserId(this.User);
            Account? appUser = await _accountManager.GetUserByIdAsync(userId, Account);
            return Ok(appUser.Configuration);
        }

        /// <summary>
        /// Update current user's preferences
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("users/me/preferences")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> UserPreferences([FromBody] string data)
        {
            var userId = Utilities.GetUserId(this.User);
            if (userId == null) return NoContent();
            Account? appUser = await _accountManager.GetUserByIdAsync(userId, Account);
            if (appUser == null) return NoContent();
            if (appUser != null) appUser.Configuration = data;
            var result = await _accountManager.UpdateUserAsync(appUser);
            if (!result.Succeeded)
                throw new Exception("The following errors occurred whilst updating User Configurations: " + string.Join(", ", result.Errors));
            return NoContent();
        }

        /// <summary>
        /// Get role by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("roles/{id}", Name = GetRoleByIdActionName)]
        [ProducesResponseType(200, Type = typeof(RoleViewModel))]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var appRole = await _accountManager.GetRoleByIdAsync(id);

            if (!(await _authorizationService.AuthorizeAsync(this.User, appRole?.Name ?? "", Authorization.Policies.ViewRoleByRoleNamePolicy)).Succeeded)
                return new ChallengeResult();

            if (appRole == null)
                return NotFound(id);

            return await GetRoleByName(appRole.Name);
        }

        /// <summary>
        /// Get role by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("roles/name/{name}")]
        [ProducesResponseType(200, Type = typeof(RoleViewModel))]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetRoleByName(string name)
        {
            if (!(await _authorizationService.AuthorizeAsync(this.User, name, Authorization.Policies.ViewRoleByRoleNamePolicy)).Succeeded)
                return new ChallengeResult();

            RoleViewModel roleVM = await GetRoleViewModelHelper(name);

            if (roleVM == null)
                return NotFound(name);

            return Ok(roleVM);
        }

        /// <summary>
        /// Get roles list
        /// </summary>
        /// <returns></returns>
        [HttpGet("roles")]
        [Authorize(Authorization.Policies.ViewAllRolesPolicy)]
        [ProducesResponseType(200, Type = typeof(List<RoleViewModel>))]
        public async Task<IActionResult> GetRoles()
        {
            return await GetRoles(-1, -1);
        }

        /// <summary>
        /// Get roles list using paging
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("roles/{pageNumber:int}/{pageSize:int}")]
        [Authorize(Authorization.Policies.ViewAllRolesPolicy)]
        [ProducesResponseType(200, Type = typeof(List<RoleViewModel>))]
        public async Task<IActionResult> GetRoles(int pageNumber, int pageSize)
        {
            var roles = await _accountManager.GetRolesLoadRelatedAsync(pageNumber, pageSize);
            return Ok(_mapper.Map<List<RoleViewModel>>(roles));
        }

        /// <summary>
        /// Update role
        /// </summary>
        /// <param name="id"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPut("roles/{id}")]
        [Authorize(Authorization.Policies.ManageAllRolesPolicy)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] RoleViewModel role)
        {
            if (ModelState.IsValid)
            {
                if (role == null)
                    return BadRequest($"{nameof(role)} cannot be null");

                if (!string.IsNullOrWhiteSpace(role.Id) && id != role.Id)
                    return BadRequest("Conflicting role id in parameter and model data");

                ApplicationRole appRole = await _accountManager.GetRoleByIdAsync(id);

                if (appRole == null)
                    return NotFound(id);

                _mapper.Map<RoleViewModel, ApplicationRole>(role, appRole);

                var result = await _accountManager.UpdateRoleAsync(appRole, role.Permissions?.Select(p => p.Value).ToArray());
                if (result.Succeeded)
                    return NoContent();

                AddError(result.Errors);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// Create role
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPost("roles")]
        [Authorize(Authorization.Policies.ManageAllRolesPolicy)]
        [ProducesResponseType(201, Type = typeof(RoleViewModel))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateRole([FromBody] RoleViewModel role)
        {
            if (ModelState.IsValid)
            {
                if (role == null)
                    return BadRequest($"{nameof(role)} cannot be null");

                ApplicationRole appRole = _mapper.Map<ApplicationRole>(role);

                var result = await _accountManager.CreateRoleAsync(appRole, role.Permissions?.Select(p => p.Value).ToArray());
                if (result.Succeeded)
                {
                    RoleViewModel roleVM = await GetRoleViewModelHelper(appRole.Name);
                    return CreatedAtAction(GetRoleByIdActionName, new { id = roleVM.Id }, roleVM);
                }

                AddError(result.Errors);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// Delete role
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpDelete("roles/{id}")]
        [Authorize(Authorization.Policies.ManageAllRolesPolicy)]
        [ProducesResponseType(200, Type = typeof(RoleViewModel))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteRole(string id)
        {
            ApplicationRole appRole = await _accountManager.GetRoleByIdAsync(id);

            if (appRole == null)
                return NotFound(id);

            if (!await _accountManager.TestCanDeleteRoleAsync(id))
                return BadRequest("Role cannot be deleted. Remove all users from this role and try again");

            RoleViewModel roleVM = await GetRoleViewModelHelper(appRole.Name);

            var result = await _accountManager.DeleteRoleAsync(appRole);
            if (!result.Succeeded)
                throw new Exception("The following errors occurred whilst deleting role: " + string.Join(", ", result.Errors));
            return Ok(roleVM);
        }

        /// <summary>
        /// Get all oermissions
        /// </summary>
        /// <returns></returns>
        [HttpGet("permissions")]
        [Authorize(Authorization.Policies.ViewAllRolesPolicy)]
        [ProducesResponseType(200, Type = typeof(List<PermissionViewModel>))]
        public IActionResult GetAllPermissions()
        {
            return Ok(_mapper.Map<List<PermissionViewModel>>(ApplicationPermissions.AllPermissions));
        }



        private async Task<UserViewModel> GetUserViewModelHelper(string userId)
        {
            var userAndRoles = await _accountManager.GetUserAndRolesAsync(userId, Account);
            if (userAndRoles == null)
                return null;

            var userVM = _mapper.Map<UserViewModel>(userAndRoles.Value.User);
            userVM.Roles = userAndRoles.Value.Roles;
            return userVM;
        }


        private async Task<RoleViewModel> GetRoleViewModelHelper(string roleName)
        {
            var role = await _accountManager.GetRoleLoadRelatedAsync(roleName);
            if (role != null)
                return _mapper.Map<RoleViewModel>(role);
            return null;
        }


        private void AddError(IEnumerable<string> errors, string key = "")
        {
            foreach (var error in errors)
            {
                AddError(error, key);
            }
        }

        private void AddError(string error, string key = "")
        {
            ModelState.AddModelError(key, error);
        }

    }
}
