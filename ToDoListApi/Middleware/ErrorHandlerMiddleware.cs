using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Net;
using ToDoListCore.Helpers;

namespace ToDoListApi.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                string result;

                var serializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                switch (error)
                {
                    case CustomException e:
                        // custom error
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        result = JsonConvert.SerializeObject(e.LastError, serializerSettings);
                        break;
                    //case AppException e:
                    //    // custom application error
                    //    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    //    result = System.Text.Json.JsonSerializer.Serialize(new { message = error?.Message });
                    //    break;
                    case KeyNotFoundException e:
                        // not found error
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        result = System.Text.Json.JsonSerializer.Serialize(new { message = error?.Message });
                        break;
                    default:
                        // unhandled error
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        result = System.Text.Json.JsonSerializer.Serialize(new { message = error?.Message });
                        break;
                }

                await response.WriteAsync(result);
            }
        }
    }
}
