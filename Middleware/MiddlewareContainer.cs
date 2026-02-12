using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace minimalAPI.Middleware
{
    public class MiddlewareContainer
    {
        private readonly RequestDelegate _next;
        private readonly string headerKey = "X-Tenant-ID"; 
        public MiddlewareContainer(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            //obtener tenantId de keycloak
            var tenantFromToken = context.User.FindFirst("tenantid")?.Value;
             
            if (context.Request.Headers.TryGetValue(headerKey, out var tenantIdValues))
            {
                try
                { 

                    if (tenantFromToken == null) {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Tenant ID not found in token.");
                        return;
                    }

                    if (!context.Request.Headers.TryGetValue(headerKey, out var tenantIdHeader))
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new { error = $"Header {headerKey} is required." });
                        return;
                    }

                    if (!Guid.TryParse(tenantIdHeader.ToString(), out var tenantId))
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new { error = "Invalid UUID format in X-Tenant-ID header." });
                        return;
                    }


                    if (tenantId.ToString() != tenantFromToken)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(new { error = "Security Alert: Tenant mismatch between Token and Header." });
                        return;
                    }

                    context.Items["TenantId"] = tenantId;
                }
                catch (Exception e)
                { 
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync($"Invalid Tenant Info format. {e}");
                    return;
                }

                await _next(context);
          
        }
        }
    }
}
