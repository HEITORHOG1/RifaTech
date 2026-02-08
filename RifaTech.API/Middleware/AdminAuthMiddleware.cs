using System.Security.Claims;

namespace RifaTech.API.Middleware
{
    public class AdminAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if user is authenticated
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Check if user has Admin role
                bool isAdmin = context.User.IsInRole("Admin");

                // Add a custom claim to easily identify admin status
                var identity = context.User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    // Remove any existing IsAdmin claim to avoid duplicates
                    var existingClaim = identity.FindFirst("IsAdmin");
                    if (existingClaim != null)
                    {
                        identity.RemoveClaim(existingClaim);
                    }

                    // Add IsAdmin claim with current status
                    identity.AddClaim(new Claim("IsAdmin", isAdmin.ToString()));
                }
            }

            // Continue processing the request
            await _next(context);
        }
    }

    // Extension method to make it easier to register the middleware
    public static class AdminAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseAdminAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AdminAuthMiddleware>();
        }
    }
}