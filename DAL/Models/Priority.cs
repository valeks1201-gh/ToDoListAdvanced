using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Core;

namespace DAL.Models
{
    public class Priority: AuditableEntity
    {
        List<ToDoItem> toDoItems;
        public Priority()
        {
            toDoItems = new List<ToDoItem>();
        } 

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Organization Organization { get; set; }
        public List<ToDoItem> ToDoItems
        {
            get
            {
                return toDoItems;
            }

            set
            {
                toDoItems = value;
            }
        }

    }
}
