using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using DAL.Core;
using System.ComponentModel.DataAnnotations;
using DAL.Models;
using Microsoft.AspNetCore.Identity;

namespace ToDoListCore.Models
{
    public class AccountResponse
    {
        public string Id { get; set; }
        public string FriendlyName { get; set; }
        public string? JobTitle { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Configuration { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsLockedOut { get; set; }

        public Organization? Organization { get; set; }
    }
}
