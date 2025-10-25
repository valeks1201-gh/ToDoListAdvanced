using AutoMapper;
using DAL;
using DAL.Models;
using System.Runtime.Serialization;
using ToDoListApi.Interfaces;
using ToDoListCore;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Json;
using System.Diagnostics.CodeAnalysis;

using System;
using System.Linq;
using System.Collections.Generic;
using DAL.Core;

using ToDoListCore.Helpers;
using ToDoListCore.Models;
using Properties = ToDoListCore.Properties;
using Org.BouncyCastle.Asn1.Pkcs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.VisualBasic;


namespace ToDoListApi.Services
{
    public class ToDoListService : IToDoListService
    {
        private readonly ILogger<ToDoListService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private IWebHostEnvironment _environment;

        public ToDoListService(
            ILogger<ToDoListService> logger,
            ApplicationDbContext context,
            IMapper mapper,
            IWebHostEnvironment environment
            )
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _environment = environment;
        }

        public async Task<Priority> AddPriority(PriorityRequest model, Account account, bool isCommitTransaction = true)
        {
            if (account.Organization != null)
            {
                model.Organization = (Organization)account.Organization;
            }

            ValidationUtilities.PriorityRequestValidate(ref model);

            Priority? tmpPriority = _context.Priorities.Where(p => p.Name.ToUpper().Equals(model.Name.ToUpper()) && p.Organization.Equals(model.Organization)).FirstOrDefault();

            if (tmpPriority != null)
            {
                throw new CustomException(703, Properties.CustomErrorCodes._703);
            }

            var result = new Priority();

            using var transaction = _context.Database.BeginTransaction();
            {
                try
                {
                    Priority priority = new Priority();
                    priority = _mapper.Map<Priority>(model);

                    if (account.Organization != null)
                    {
                        priority.Organization = (Organization)account.Organization;
                    }

                    priority.CreatedBy = account.UserName;
                    priority.CreatedDate = DateTime.UtcNow;
                    await _context.Priorities.AddAsync(priority);
                  
                    if (isCommitTransaction)
                    {
                        _context.SaveChanges();
                        transaction.Commit();
                        result = _context.Priorities.Where(p => p.Id.Equals(priority.Id)).FirstOrDefault();
                    }
                    else
                    {
                        result = priority;
                    }
                }
                catch (CustomException e)
                {
                    _logger.LogError($"ToDoListService.AddPriority. Exception message [{e.LastErrorMessage}]");
                    throw new CustomException(e.LastCustomErrorCode, e.LastErrorMessage);
                }
                catch (Exception e)
                {
                    _logger.LogError($"ToDoListService.AddPriority. Exception message [{e.Message}]");
                    throw new Exception($"ToDoListService.AddPriority. Exception message [{e.Message}]");
                }
            }

            return result;
        }

        public async Task<List<Priority>?> GetPriorities(Account account, DateTime? minDate = null, DateTime? maxDate = null)
        {
            List<Priority>? results = null;

            if (minDate == null)
            {
                minDate = DateTime.UtcNow.AddDays(-30);
            }

            if (maxDate == null)
            {
                maxDate = DateTime.UtcNow;
            }

            await Task.Run(() =>
            {
                try
                {
                    if (account.Organization == null)
                    {
                        results = _context.Priorities.Where(p => p.CreatedDate > minDate && p.CreatedDate < maxDate).ToList();
                    }
                    else
                    {
                        results = _context.Priorities
                        .Where(p => p.CreatedDate > minDate && p.CreatedDate < maxDate && p.Organization.Equals(account.Organization)).ToList();
                    }
                }
                catch (CustomException e)
                {
                    _logger.LogError($"ToDoListService.GetPriorities. Exception message [{e.LastErrorMessage}]");
                    throw new CustomException(e.LastCustomErrorCode, e.LastErrorMessage);
                }
                catch (Exception e)
                {
                    _logger.LogError($"ToDoListService.GetPriorities. Exception message [{e.Message}]");
                    throw new Exception($"ToDoListService.GetPriorities. Exception message [{e.Message}]");
                }
            });

            return results;
        }

        public async Task<Priority?> GetPriority(int priorityId, Account account)
        {
            Priority? result = null;

            await Task.Run(() =>
            {
                try
                {
                    if (account.Organization == null)
                    {
                        result = _context.Priorities.Where(p => p.Id.Equals(priorityId)).FirstOrDefault();
                    }
                    else
                    {
                        result = _context.Priorities.Where(p => p.Id.Equals(priorityId) && p.Organization.Equals(account.Organization)).FirstOrDefault();
                    }
                }
                catch (CustomException e)
                {
                    _logger.LogError($"ToDoListService.GetPriority. Exception message [{e.LastErrorMessage}]");
                    throw new CustomException(e.LastCustomErrorCode, e.LastErrorMessage);
                }
                catch (Exception e)
                {
                    _logger.LogError($"ToDoListService.GetPriority. Exception message [{e.Message}]");
                    throw new Exception($"ToDoListService.GetPriority. Exception message [{e.Message}]");
                }
            });

            return result;
        }

        public Task<Priority?> UpdatePriority(int priorityId, PriorityRequest model, Account account, bool isCommitTransaction = true)
        {
            ValidationUtilities.PriorityRequestValidate(ref model);

            return Task.Run(() =>
            {
                var result = new Priority();
                Priority? priority = null;

                if (account.Organization == null)
                {
                    priority = _context.Priorities.Where(p => p.Id.Equals(priorityId)).FirstOrDefault();
                }
                else
                {
                    priority = _context.Priorities.Where(p => p.Id.Equals(priorityId) && p.Organization.Equals(account.Organization)).FirstOrDefault();
                }

                if (priority == null)
                {
                    throw new CustomException(700, Properties.CustomErrorCodes._700);
                }

                using var transaction = _context.Database.BeginTransaction();
                {
                    try
                    {
                        if (IsNotNull(priority))
                        {
                            priority.Name = model.Name;
                            priority.Description = model.Description;

                            if (account.Organization != null)
                            {
                                priority.Organization = (Organization)account.Organization;
                            }

                            priority.UpdatedBy = account.UserName;
                            priority.UpdatedDate = DateTime.UtcNow;
                         
                            if (isCommitTransaction)
                            {
                                _context.Priorities.Update(priority);
                                _context.SaveChanges();
                                transaction.Commit();
                                result = _context.Priorities.Where(p => p.Id.Equals(priority.Id)).FirstOrDefault();
                            }
                            else
                            {
                                result = priority;
                            }
                        }

                        return result;
                    }
                    catch (CustomException e)
                    {
                        _logger.LogError($"ToDoListService.UpdatePriority. Exception message [{e.LastErrorMessage}]");
                        throw new CustomException(e.LastCustomErrorCode, e.LastErrorMessage);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"ToDoListService.UpdatePriority. Exception message [{e.Message}]");
                        throw new Exception($"ToDoListService.UpdatePriority. Exception message [{e.Message}]");
                    }
                }
            });

        }

        public Task<bool> DeletePriority(int priorityId, Account account)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var transaction = _context.Database.BeginTransaction();
                    {
                        Priority? priority = null;

                        if (account.Organization == null)
                        {
                            priority = _context.Priorities.Where(p => p.Id.Equals(priorityId)).Include(t => t.ToDoItems).FirstOrDefault();
                        }
                        else
                        {
                            priority = _context.Priorities.Where(p => p.Id.Equals(priorityId) && p.Organization.Equals(account.Organization))
                                                                                                            .Include(t => t.ToDoItems).FirstOrDefault();
                        }

                        if (priority == null)
                        {
                            throw new CustomException(700, Properties.CustomErrorCodes._700);
                        }

                        if ((priority.ToDoItems != null) && (priority.ToDoItems.Count > 0))
                        {
                            throw new CustomException(705, Properties.CustomErrorCodes._705);
                        }

                        _context.Priorities.Remove(priority);
                        _context.SaveChanges();
                        transaction.Commit();
                    }

                    return true;
                }
                catch (CustomException e)
                {
                    _logger.LogError($"ToDoListService.DeletePriority. Exception message [{e.LastErrorMessage}]");
                    throw new CustomException(e.LastCustomErrorCode, e.LastErrorMessage);
                }
                catch (Exception e)
                {
                    _logger.LogError($"ToDoListService.DeletePriority. Exception message [{e.Message}]");
                    throw new Exception($"ToDoListService.DeletePriority. Exception message [{e.Message}]");
                }
            });
        }

        public async Task<ToDoItemResponse> AddToDoItem(ToDoItemRequest model, Account account, bool isCommitTransaction = true)
        {
            ValidationUtilities.ToDoItemRequestValidate(ref model);

            Account refAccount = null;
            if (account.Organization == null)
            {
                refAccount = _context.Users.Where(u => u.Id.Equals(model.UserId)).FirstOrDefault();
            }
            else
            {
                refAccount = _context.Users.Where(u => u.Id.Equals(model.UserId) && u.Organization.Equals(account.Organization)).FirstOrDefault();
            }

            if (refAccount == null)
            {
                throw new CustomException(706, Properties.CustomErrorCodes._706);
            }

            Priority refPriority = null;
            if (account.Organization == null)
            {
                refPriority = _context.Priorities.Where(p => p.Id.Equals(model.PriorityId)).FirstOrDefault();
            }
            else
            {
                refPriority = _context.Priorities.Where(p => p.Id.Equals(model.PriorityId) && p.Organization.Equals(account.Organization)).FirstOrDefault();
            }

            if (refPriority == null)
            {
                throw new CustomException(707, Properties.CustomErrorCodes._707);
            }

            var result = new ToDoItemResponse();

            using var transaction = _context.Database.BeginTransaction();
            {
                try
                {
                    ToDoItem toDoItem = new ToDoItem();
                    toDoItem = _mapper.Map<ToDoItem>(model);
                    //toDoItem.PriorityId = model.PriorityId;
                    //toDoItem.UserId = model.UserId;

                    toDoItem.CreatedBy = account.UserName;
                    toDoItem.CreatedDate = DateTime.UtcNow;
                    await _context.ToDoItems.AddAsync(toDoItem);

                    if (isCommitTransaction)
                    {
                        _context.SaveChanges();
                        transaction.Commit();
                        var tmpToDoItem = _context.ToDoItems.Where(p => p.Id.Equals(toDoItem.Id))
                                                    .Include(p => p.Priority)
                                                    .Include(u => u.User)
                                                    .FirstOrDefault();
                        result = _mapper.Map<ToDoItemResponse>(tmpToDoItem);
                    }
                    else
                    {
                        result = _mapper.Map<ToDoItemResponse>(toDoItem); 
                    }
                }
                catch (CustomException e)
                {
                    _logger.LogError($"ToDoListService.AddToDoItem. Exception message [{e.LastErrorMessage}]");
                    throw new CustomException(e.LastCustomErrorCode, e.LastErrorMessage);
                }
                catch (Exception e)
                {
                    _logger.LogError($"ToDoListService.AddToDoItem. Exception message [{e.Message}]");
                    throw new Exception($"ToDoListService.AddToDoItem. Exception message [{e.Message}]");
                }
            }

            return result;
        }


        public async Task<List<ToDoItemResponse>?> GetToDoItems(Account account, DateTime? minDate = null, DateTime? maxDate = null, 
                                                   bool? isCompleted = null, DateTime? dueDate = null, int? priorityId = null, string? userId = null)
        {
            List<ToDoItemResponse>? results = null;

            if (minDate == null)
            {
                minDate = DateTime.UtcNow.AddDays(-30);
            }

            if (maxDate == null)
            {
                maxDate = DateTime.UtcNow;
            }

            await Task.Run(() =>
            {
                try
                {
                    List<ToDoItem>? tmpResults = null; ;
                    if (account.Organization == null)
                    {
                        tmpResults = _context.ToDoItems.Where(t => t.CreatedDate > minDate && t.CreatedDate < maxDate
                         && (isCompleted != null)?t.IsCompleted.Equals((bool)isCompleted):true
                         && (dueDate != null) ? t.DueDate >=(DateTime)dueDate : true
                         && (priorityId != null) ? t.PriorityId.Equals((int)priorityId) : true
                         && (userId != null) ? t.UserId.Equals((string)userId) : true
                        )
                        .Include(p => p.Priority)
                        .Include(u => u.User)
                        .ToList();
                    }
                    else
                    {
                        tmpResults = _context.ToDoItems
                        .Where(t => t.CreatedDate > minDate && t.CreatedDate < maxDate && t.User.Organization.Equals(account.Organization)
                        && (isCompleted != null) ? t.IsCompleted.Equals((bool)isCompleted) : true
                         && (dueDate != null) ? t.DueDate >= (DateTime)dueDate : true
                         && (priorityId != null) ? t.PriorityId.Equals((int)priorityId) : true
                         && (userId != null) ? t.UserId.Equals((string)userId) : true
                        )
                        .Include(p => p.Priority)
                        .Include(u => u.User)
                        .ToList();
                    }

                    results = _mapper.Map<List<ToDoItemResponse>?>(tmpResults); 

                }
                catch (CustomException e)
                {
                    _logger.LogError($"ToDoListService.GetToDoItems. Exception message [{e.LastErrorMessage}]");
                    throw new CustomException(e.LastCustomErrorCode, e.LastErrorMessage);
                }
                catch (Exception e)
                {
                    _logger.LogError($"ToDoListService.GetToDoItems. Exception message [{e.Message}]");
                    throw new Exception($"ToDoListService.GetToDoItems. Exception message [{e.Message}]");
                }
            });

            return results;
        }

        public async Task<ToDoItemResponse?> GetToDoItem(int toDoItemId, Account account)
        {
            ToDoItemResponse? result = null;

            await Task.Run(() =>
            {
                try
                {
                    ToDoItem? tmpResult = null; 
                    if (account.Organization == null)
                    {
                        tmpResult = _context.ToDoItems.Where(t => t.Id.Equals(toDoItemId))
                        .Include(p => p.Priority)
                        .Include(u => u.User)
                        .FirstOrDefault();
                    }
                    else
                    {
                        tmpResult = _context.ToDoItems
                        .Where(t => t.Id.Equals(toDoItemId) && t.User.Organization.Equals(account.Organization))
                        .Include(p => p.Priority)
                        .Include(u => u.User)
                         .FirstOrDefault();
                    }

                    result = _mapper.Map<ToDoItemResponse>(tmpResult);
                }
                catch (CustomException e)
                {
                    _logger.LogError($"ToDoListService.GetToDoItem. Exception message [{e.LastErrorMessage}]");
                    throw new CustomException(e.LastCustomErrorCode, e.LastErrorMessage);
                }
                catch (Exception e)
                {
                    _logger.LogError($"ToDoListService.GetToDoItem. Exception message [{e.Message}]");
                    throw new Exception($"ToDoListService.GetToDoItem. Exception message [{e.Message}]");
                }
            });

            return result;
        }

        public Task<ToDoItemResponse> UpdateToDoItem(int toDoItemId, ToDoItemRequest model, Account account, bool isCommitTransaction = true)
        {
            ValidationUtilities.ToDoItemRequestValidate(ref model);

            Account refAccount = null;
            if (account.Organization == null)
            {
                refAccount = _context.Users.Where(u => u.Id.Equals(model.UserId)).FirstOrDefault();
            }
            else
            {
                refAccount = _context.Users.Where(u => u.Id.Equals(model.UserId) && u.Organization.Equals(account.Organization)).FirstOrDefault();
            }

            if (refAccount == null)
            {
                throw new CustomException(706, Properties.CustomErrorCodes._706);
            }

            return Task.Run(() =>
            {
                var result = new ToDoItemResponse();
                ToDoItem? toDoItem = null;

                if (account.Organization == null)
                {
                    toDoItem = _context.ToDoItems.Where(t => t.Id.Equals(toDoItemId)).FirstOrDefault();
                }
                else
                {
                    toDoItem = _context.ToDoItems.Where(t => t.Id.Equals(toDoItemId) && t.User.Organization.Equals(account.Organization)).FirstOrDefault();
                }

                if (toDoItem == null)
                {
                    throw new CustomException(700, Properties.CustomErrorCodes._700);
                }

                using var transaction = _context.Database.BeginTransaction();
                {
                    try
                    {
                        if (IsNotNull(toDoItem))
                        {
                            toDoItem.Title = model.Title;
                            toDoItem.Description = model.Description;
                            toDoItem.DueDate = model.DueDate;
                            toDoItem.PriorityId = model.PriorityId;
                            toDoItem.UserId = model.UserId;

                            toDoItem.UpdatedBy = account.UserName;
                            toDoItem.UpdatedDate = DateTime.UtcNow;

                            ToDoItem? tmpResult = null;

                            if (isCommitTransaction)
                            {
                                _context.ToDoItems.Update(toDoItem);
                                _context.SaveChanges();
                                transaction.Commit();
                                tmpResult = _context.ToDoItems.Where(t => t.Id.Equals(toDoItemId))
                                                              .Include(p => p.Priority)
                                                              .Include(u => u.User)
                                                              .FirstOrDefault();
                                result = _mapper.Map<ToDoItemResponse>(tmpResult);
                            }
                            else
                            {
                                result = _mapper.Map<ToDoItemResponse>(toDoItem);
                            }
                        }

                        return result;
                    }
                    catch (CustomException e)
                    {
                        _logger.LogError($"ToDoListService.UpdateToDoItem. Exception message [{e.LastErrorMessage}]");
                        throw new CustomException(e.LastCustomErrorCode, e.LastErrorMessage);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"ToDoListService.UpdateToDoItem. Exception message [{e.Message}]");
                        throw new Exception($"ToDoListService.UpdateToDoItem. Exception message [{e.Message}]");
                    }
                }
            });

        }

        public Task<bool> DeleteToDoItem(int toDoItemId, Account account)
        {
            return Task.Run(() =>
            {
                try
                {
                    using var transaction = _context.Database.BeginTransaction();
                    {
                        ToDoItem? toDoItem = null;

                        if (account.Organization == null)
                        {
                            toDoItem = _context.ToDoItems.Where(t => t.Id.Equals(toDoItemId)).FirstOrDefault();
                        }
                        else
                        {
                            toDoItem = _context.ToDoItems.Where(t => t.Id.Equals(toDoItemId) && t.User.Organization.Equals(account.Organization))
                                                                                                                                .FirstOrDefault();
                        }

                        if (toDoItem == null)
                        {
                            throw new CustomException(700, Properties.CustomErrorCodes._700);
                        }

                        _context.ToDoItems.Remove(toDoItem);
                        _context.SaveChanges();
                        transaction.Commit();
                    }

                    return true;
                }
                catch (CustomException e)
                {
                    _logger.LogError($"ToDoListService.DeleteToDoItem. Exception message [{e.LastErrorMessage}]");
                    throw new CustomException(e.LastCustomErrorCode, e.LastErrorMessage);
                }
                catch (Exception e)
                {
                    _logger.LogError($"ToDoListService.DeleteToDoItem. Exception message [{e.Message}]");
                    throw new Exception($"ToDoListService.DeleteToDoItem. Exception message [{e.Message}]");
                }
            });
        }


        private static bool IsNotNull([NotNullWhen(true)] object? obj) => obj != null;
    }
}
