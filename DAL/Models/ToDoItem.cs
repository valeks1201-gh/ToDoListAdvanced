using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class ToDoItem : AuditableEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public bool IsCompleted { get; set; }
        public DateTime DueDate { get; set; }
        public int PriorityId { get; set; }
        public Priority Priority { get; set; }
        public string UserId { get; set; }
        //public User User { get; set; }
        public Account User { get; set; }
    }
}
