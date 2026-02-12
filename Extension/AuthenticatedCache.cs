using Microsoft.AspNetCore.OutputCaching;

namespace minimalAPI.Extension
{
    public class AuthenticatedCache : IOutputCachePolicy
    {

        public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
        {

            //validar que no sea nulo 
            ArgumentNullException.ThrowIfNull(context);

            var attemptoutputcache = AttemptOutputCaching(context);
            context.EnableOutputCaching = attemptoutputcache;
            context.AllowCacheLookup = attemptoutputcache;
            context.AllowCacheStorage = attemptoutputcache;

            context.CacheVaryByRules.QueryKeys = "*"; // variar por todos los query params
            context.CacheVaryByRules.HeaderNames = new[] { "Accept", "X-Tenant-ID" };

            return ValueTask.CompletedTask;
        }

        public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            //Validar si esta autenticado 
            var isAuthenticated = IsAuthenticated(context);

            if (!isAuthenticated)
            {
                context.AllowCacheLookup = false; // No permitir búsqueda en caché para usuarios no autenticados

                return ValueTask.CompletedTask;
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
        {

            //Validar si esta autenticado 
            var isAuthenticated = IsAuthenticated(context);

            //Solo almacenar respuestas sastifactorias
            var issuccessful = context.HttpContext.Response.StatusCode >= 200 && context.HttpContext.Response.StatusCode < 300;

            if (!isAuthenticated || !issuccessful)
            { 
                context.AllowCacheStorage = false;

                return ValueTask.CompletedTask;
            }

            return ValueTask.CompletedTask;
        }

        private bool AttemptOutputCaching(OutputCacheContext context)
        {
            // Verificar si la llamada es GET
            var request = context.HttpContext.Request;
            if (request.Method != HttpMethods.Get)
            {
                return false; // No cachear si no es GET
            }
            return true;
        }

        private bool IsAuthenticated(OutputCacheContext context)
        {
            return context.HttpContext.User?.Identity?.IsAuthenticated ?? false;
        }
    }
}
