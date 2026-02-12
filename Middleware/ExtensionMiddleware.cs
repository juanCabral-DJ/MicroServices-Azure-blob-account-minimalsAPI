namespace minimalAPI.Middleware
{
    public static class ExtensionMiddleware
    {
        public static IApplicationBuilder UseExtensionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MiddlewareContainer>();
        }
    }
}
