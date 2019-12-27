using System;
using System.Threading.Tasks;
using FleetTrack.Core.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FleetTrack.Api.Middleware
{
    /// <summary>
    /// Translates domain and argument exceptions into 400 responses and anything else into 500.
    /// </summary>
    /// <remarks>
    /// Once the response has started, status and headers are already on the wire:
    /// Response.Clear() would throw and mask the original exception. In that case the
    /// exception is logged and rethrown so the server aborts the connection.
    /// </remarks>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex) when (IsClientError(ex) && !context.Response.HasStarted)
            {
                _logger.LogInformation("Rejected {Method} {Path}: {Message}", context.Request.Method, context.Request.Path, ex.Message);
                await Write(context, StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

                if (context.Response.HasStarted)
                {
                    throw;
                }

                await Write(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        private static bool IsClientError(Exception ex)
        {
            return ex is DomainException || ex is ArgumentException;
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
