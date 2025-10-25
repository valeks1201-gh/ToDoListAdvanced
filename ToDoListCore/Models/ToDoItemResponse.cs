using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using DAL.Core;
using DAL.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ToDoListCore.Models
{
    public class ToDoItemResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public bool IsCompleted { get; set; }
        public DateTime DueDate { get; set; }
        public PriorityResponse Priority { get; set; }
        public AccountResponse User { get; set; }
    }
}
