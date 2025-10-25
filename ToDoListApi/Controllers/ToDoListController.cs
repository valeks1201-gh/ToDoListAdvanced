using AutoMapper;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToDoListApi.Helpers;
using ToDoListApi.Interfaces;
using ToDoListCore.Models;
using ToDoListApi.ViewModels;
using DAL.Core;
using Azure;
using ToDoListApi.Services;
using System.Collections.Generic;
using Microsoft.Identity.Client;
using System.Data;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ToDoListApi.Controllers
{
    // [Authorize]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ToDoListController : BaseController //ControllerBase
    {
        private readonly IToDoListService _toDoListService;
        private readonly IMapper _autoMapper;
        private readonly ILogger<ToDoListController> _logger;
        public ToDoListController(IToDoListService toDoListService, IMapper autoMapper)
        {
            _toDoListService = toDoListService;
            _autoMapper = autoMapper;
        }

        /// <summary>
        /// Endpoint for testing Returns "Test"
        /// </summary>
        /// <returns></returns>
        [HttpGet("uncertainty/test")]
        [ProducesResponseType(200, Type = typeof(string))]
        public string Test()
        {
            return "Test";
        }

        /// <summary>
        /// Add priority
        /// </summary>
        /// <param name="model"></param>
        /// <param name="isCommitTransaction"></param>
        /// <returns></returns>
        [Authorize(Authorization.Policies.ManageOrgDataPolicy)]
        [HttpPost("Priority")]
        public async Task<IActionResult> AddPriority(PriorityRequest model, bool isCommitTransaction = true)
        {
            var response = await _toDoListService.AddPriority(model, Account, isCommitTransaction);
            return Ok(response);
        }

        /// <summary>
        /// Get list of priorities
        /// </summary>
        /// <param name="minDate"></param>
        /// <param name="maxDate"></param>
        /// <returns></returns>
        [Authorize(Authorization.Policies.ViewOrgDataPolicy)]
        [HttpGet("Priorities")]
        public async Task<IActionResult> GetPriorities(DateTime? minDate = null, DateTime? maxDate = null)
        {
            var response = await _toDoListService.GetPriorities(Account, minDate, maxDate);
            return Ok(response);
        }

        /// <summary>
        /// Get priority by Id
        /// </summary>
        /// <param name="methodSpecId"></param>
        /// <returns></returns>
        [Authorize(Authorization.Policies.ViewOrgDataPolicy)]
        [HttpGet("Priority/{priorityId:int}")]
        public async Task<IActionResult> GetPriority(int priorityId)
        {
            var response = await _toDoListService.GetPriority(priorityId, Account);
            return Ok(response);
        }

        /// <summary>
        /// Update priority object
        /// </summary>
        /// <param name="priorityId"></param>
        /// <param name="model"></param>
        /// <param name="isCommitTransaction"></param>
        /// <returns></returns>
        [Authorize(Authorization.Policies.ManageOrgDataPolicy)]
        [HttpPut("Priority/{priorityId:int}/{isCommitTransaction:bool}")]
        public async Task<IActionResult> UpdatePriority(int priorityId, [FromBody] PriorityRequest model, bool isCommitTransaction = true)
        {
            var response = await _toDoListService.UpdatePriority(priorityId, model, Account, isCommitTransaction);
            return Ok(response);
        }

        /// <summary>
        /// Delete priority object
        /// </summary>
        /// <param name="priorityId"></param>
        /// <returns></returns>
        [Authorize(Authorization.Policies.ManageOrgDataPolicy)]
        [HttpDelete("Priority/{priorityId:int}")]
        public async Task<IActionResult> DeletePriority(int priorityId)
        {
            var response = await _toDoListService.DeletePriority(priorityId, Account);
            return Ok(response);
        }

        /// <summary>
        /// Add ToDoListItem
        /// </summary>
        /// <param name="model"></param>
        /// <param name="isCommitTransaction"></param>
        /// <returns></returns>
        [Authorize(Authorization.Policies.ManageOrgDataPolicy)]
        [HttpPost("ToDoItem")]
        public async Task<IActionResult> AddToDoItem(ToDoItemRequest model, bool isCommitTransaction = true)
        {
            var response = await _toDoListService.AddToDoItem(model, Account, isCommitTransaction);
            return Ok(response);
        }

        /// <summary>
        /// Get list of ToDoListItem
        /// </summary>
        /// <param name="minDate"></param>
        /// <param name="maxDate"></param>
        /// <returns></returns>
        [Authorize(Authorization.Policies.ViewOrgDataPolicy)]
        [HttpGet("ToDoItems")]
        public async Task<IActionResult> GetToDoItems(DateTime? minDate = null, DateTime? maxDate = null,
            bool? isCompleted = null, DateTime? dueDate = null, int? priorityId = null, string? userId = null)
        {
            var response = await _toDoListService.GetToDoItems(Account, minDate, maxDate, isCompleted, dueDate, priorityId, userId);
            return Ok(response);
        }

        /// <summary>
        /// Get ToDoListItem by Id
        /// </summary>
        /// <param name="toDoItemId"></param>
        /// <returns></returns>
        [Authorize(Authorization.Policies.ViewOrgDataPolicy)]
        [HttpGet("ToDoItem/{toDoItemId:int}")]
        public async Task<IActionResult> GetToDoItem(int toDoItemId)
        {
            var response = await _toDoListService.GetToDoItem(toDoItemId, Account);
            return Ok(response);
        }

        /// <summary>
        /// Update ToDoListItem
        /// </summary>
        /// <param name="toDoItemId"></param>
        /// <param name="model"></param>
        /// <param name="isCommitTransaction"></param>
        /// <returns></returns>
        [Authorize(Authorization.Policies.ManageOrgDataPolicy)]
        [HttpPut("ToDoItem/{toDoItemId:int}/{isCommitTransaction:bool}")]
        public async Task<IActionResult> UpdateToDoItem(int toDoItemId, [FromBody] ToDoItemRequest model, bool isCommitTransaction = true)
        {
            var response = await _toDoListService.UpdateToDoItem(toDoItemId, model, Account, isCommitTransaction);
            return Ok(response);
        }

        /// <summary>
        /// Delete ToDoListItem
        /// </summary>
        /// <param name="toDoItemId"></param>
        /// <returns></returns>
        [Authorize(Authorization.Policies.ManageOrgDataPolicy)]
        [HttpDelete("ToDoItem/{toDoItemId:int}")]
        public async Task<IActionResult> DeleteToDoItem(int toDoItemId)
        {
            var response = await _toDoListService.DeleteToDoItem(toDoItemId, Account);
            return Ok(response);
        }

    }
}
