using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using DAL.Core;
using System.ComponentModel.DataAnnotations;

namespace ToDoListCore.Models
{
    public class PriorityRequest
    {
        [Required]
        public string Name { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
       
        [JsonConverter(typeof(StringEnumConverter))]
        public Organization Organization { get; set; } = Organization.ORG1; //Для пользователя с account.Organization=null, то есть глобального админа.
                                                                            //Для остальных переписывается значением account.Organization 
    }
}
