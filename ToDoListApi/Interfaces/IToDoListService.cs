using DAL.Models;
using DAL.Core;
using ToDoListCore.Models;

namespace ToDoListApi.Interfaces
{
    public interface IToDoListService
    {
        Task<Priority> AddPriority(PriorityRequest model, Account account, bool isCommitTransaction = true);
        Task<List<Priority>?> GetPriorities(Account account, DateTime? minDate = null, DateTime? maxDate = null);
        Task<Priority?> GetPriority(int priorityId, Account account);
        Task<Priority> UpdatePriority(int priorityId, PriorityRequest model, Account account, bool isCommitTransaction = true);
        Task<bool> DeletePriority(int priorityId, Account account);
        Task<ToDoItemResponse> AddToDoItem(ToDoItemRequest model, Account account, bool isCommitTransaction = true);
        Task<List<ToDoItemResponse>?> GetToDoItems(Account account, DateTime? minDate = null, DateTime? maxDate = null,
           bool? isCompleted = null, DateTime? dueDate = null, int? priorityId = null, string? userId = null);
        Task<ToDoItemResponse?> GetToDoItem(int toDoItemId, Account account);
        Task<ToDoItemResponse> UpdateToDoItem(int toDoItemId, ToDoItemRequest model, Account account, bool isCommitTransaction = true);
        Task<bool> DeleteToDoItem(int toDoItemId, Account account);
    }
}
