using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using System.Linq;
using System.Threading.Tasks;
using ToDoListApi.Interfaces;
using ToDoListApi.Services;
using Todolist;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using DAL.Models;



public class ToDoListGRPCImpl : ToDoListGRPC.ToDoListGRPCBase
{
    private readonly ILogger<ToDoListGRPCImpl> _logger;
    readonly IToDoListService _toDoListService;
    public ToDoListGRPCImpl(IToDoListService toDoListService, ILogger<ToDoListGRPCImpl> logger)
    {
        _toDoListService = toDoListService;
        _logger = logger;
    }

    [AllowAnonymous]
    public override async Task<TestResponse> Test(Dummy request, ServerCallContext context)
    {
        var result =  await _toDoListService.Test();

        return new TestResponse()
        {
            Title = result
        };
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public override async Task<TestResponse> TestAuth(Dummy request, ServerCallContext context)
    {
        var result = await _toDoListService.Test();

        return new TestResponse()
        {
            Title = "Auth " + result
        };
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public override async Task<ToDoItemsResponse> GetToDoList(Dummy request, ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        var account = (Account)httpContext.Items["Account"];

        var toDoItems = await _toDoListService.GetToDoItems(account);
        ToDoItemsResponse result = new ToDoItemsResponse();

        foreach (var toDoItem in toDoItems)
        {
            ToDoItemResponse toDoItemResponse = new ToDoItemResponse() { Id = toDoItem.Id, Title = toDoItem.Title, Description=toDoItem.Description, Iscompleted = toDoItem.IsCompleted };
            result.TodoItemsList.Add(toDoItemResponse);
        }

        return result;
    }



}