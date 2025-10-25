using AutoMapper;
using DAL.Core;
using DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoListCore.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ToDoListApi.ViewModels
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            var readOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };
            readOptions.Converters.Add(new JsonStringEnumConverter());

            var writeOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.WriteAsString
            };
            writeOptions.Converters.Add(new JsonStringEnumConverter());

            CreateMap<Account, UserViewModel>()
                   .ForMember(d => d.Roles, map => map.Ignore());
            CreateMap<UserViewModel, Account>()
                .ForMember(d => d.Roles, map => map.Ignore())
                .ForMember(d => d.Id, map => map.Condition(src => src.Id != null));

            CreateMap<Account, UserEditViewModel>()
                .ForMember(d => d.Roles, map => map.Ignore());
            CreateMap<UserEditViewModel, Account>()
                .ForMember(d => d.Roles, map => map.Ignore())
                .ForMember(d => d.Id, map => map.Condition(src => src.Id != null));

            CreateMap<Account, UserPatchViewModel>()
                .ReverseMap();

            CreateMap<ApplicationRole, RoleViewModel>()
                .ForMember(d => d.Permissions, map => map.MapFrom(s => s.Claims))
                .ForMember(d => d.UsersCount, map => map.MapFrom(s => s.Users != null ? s.Users.Count : 0))
                .ReverseMap();
            CreateMap<RoleViewModel, ApplicationRole>()
                .ForMember(d => d.Id, map => map.Condition(src => src.Id != null));

            CreateMap<IdentityRoleClaim<string>, ClaimViewModel>()
                .ForMember(d => d.Type, map => map.MapFrom(s => s.ClaimType))
                .ForMember(d => d.Value, map => map.MapFrom(s => s.ClaimValue))
                .ReverseMap();

            CreateMap<ApplicationPermission, PermissionViewModel>()
                .ReverseMap();

            CreateMap<IdentityRoleClaim<string>, PermissionViewModel>()
                .ConvertUsing(s => (PermissionViewModel)ApplicationPermissions.GetPermissionByValue(s.ClaimValue));

            ////////////
            CreateMap<PriorityRequest, Priority>();
            CreateMap<Priority, PriorityRequest>();

            CreateMap<Account, AccountResponse>();
            CreateMap<Priority, PriorityResponse>();

            CreateMap<ToDoItemRequest, ToDoItem>();
            CreateMap<ToDoItem, ToDoItemResponse>()
                //.ForMember(x => x.Priority, map => map.MapFrom(s => s.Priority))
                //.ForMember(x => x.User, map => map.MapFrom(s => s.User))
                ;
   
        }

    }
}
