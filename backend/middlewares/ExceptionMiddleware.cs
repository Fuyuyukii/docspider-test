using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using backend.Common;

namespace middlewares.ExceptionMiddleware 
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext); 
            }
            catch (ArgumentException ex)
            {
                await HandleExceptionAsync(httpContext, 400, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await HandleExceptionAsync(httpContext, 400, ex.Message);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, 500, "An unexpected error occurred.");
            }
        }

        private Task HandleExceptionAsync(HttpContext context, int statusCode, string errorMessage)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ApiResponse<string>(errorMessage);

            return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}
