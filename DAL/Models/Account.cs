using DAL.Models.Interfaces;
using DAL.Core;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Account : IdentityUser, IAuditableEntity
    {
        List<ToDoItem> toDoItems;

        public Account()
        {
            toDoItems = new List<ToDoItem>();
        }
        public virtual string FriendlyName
        {
            get
            {
                string friendlyName = string.IsNullOrWhiteSpace(FullName) ? UserName : FullName;

                if (!string.IsNullOrWhiteSpace(JobTitle))
                    friendlyName = $"{JobTitle} {friendlyName}";

                return friendlyName;
            }
        }

        public string? JobTitle { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Configuration { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsLockedOut => this.LockoutEnabled && this.LockoutEnd >= DateTimeOffset.UtcNow;

        public Organization? Organization { get; set; }

        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        /// <summary>
        /// Navigation property for the roles this user belongs to.
        /// </summary>
        public virtual ICollection<IdentityUserRole<string>> Roles { get; set; }

        /// <summary>
        /// Navigation property for the claims this user possesses.
        /// </summary>
        public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }

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
