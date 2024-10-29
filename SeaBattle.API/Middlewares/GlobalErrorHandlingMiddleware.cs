using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace SeaBattle.API.Middlewares;

public class GlobalErrorHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";

            var errorDetails = new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An unexpected error occurred.",
                Detail = ex.Message
            };
            
            var errorJson = JsonSerializer.Serialize(errorDetails);

            await context.Response.WriteAsync(errorJson);
        }
    }
}