namespace RifaTech.API.Exceptions
{
    public static class ExceptionHandlingMiddleware
    {
        public static void UseCustomExceptionHandling(this IApplicationBuilder app, ILogger logger)
        {
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred processing the request");
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new { Error = "An internal error occurred" });
                }
            });
        }
    }
}