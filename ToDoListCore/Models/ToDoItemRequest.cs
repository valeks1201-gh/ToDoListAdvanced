using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using DAL.Core;
using System.ComponentModel.DataAnnotations;

namespace ToDoListCore.Models
{
    public class ToDoItemRequest
    {
        [Required]
        public string Title { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;

        public bool IsCompleted { get; set; }

        public DateTime DueDate { get; set; }

        public int PriorityId { get; set; }

        [Required]
        public string UserId { get; set; }
    }
}
