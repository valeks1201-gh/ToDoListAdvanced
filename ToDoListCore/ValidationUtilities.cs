using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoListCore.Helpers;
using ToDoListCore.Models;

namespace ToDoListCore
{
    public static class ValidationUtilities
    {
        public static void PriorityRequestValidate(ref PriorityRequest? model)
        {
            if (model == null)
            {
                throw new CustomException(701, Properties.CustomErrorCodes._701);
            }

            if (model.Name.Length <= 0)
            {
                throw new CustomException(704, Properties.CustomErrorCodes._704);
            }
        }

        public static void ToDoItemRequestValidate(ref ToDoItemRequest? model)
        {
            if (model == null)
            {
                throw new CustomException(701, Properties.CustomErrorCodes._701);
            }

            if (model.Title.Length <= 0)
            {
                throw new CustomException(704, Properties.CustomErrorCodes._704);
            }

            if (model.UserId.Length <= 0)
            {
                throw new CustomException(704, Properties.CustomErrorCodes._704);
            }

            if (model.PriorityId <= 0)
            {
                throw new CustomException(704, Properties.CustomErrorCodes._704);
            }
        }
    }
}
