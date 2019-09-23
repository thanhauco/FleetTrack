using System;
using System.Threading.Tasks;
using FleetTrack.Core.Common;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace FleetTrack.Api.Middleware
{
    /// <summary>
    /// Translates domain exceptions into 400 responses and anything else into 500.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainException ex)
            {
                await Write(context, StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (ArgumentException ex)
            {
                await Write(context, StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception)
            {
                await Write(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        private static Task Write(HttpContext context, int statusCode, string message)
        {
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = message }));
        }
    }
}
