using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Memory;
using minimalAPI.Data;
using minimalAPI.Services.Interfaces;
using System.Text;

namespace minimalAPI.EndPoints
{
    public static class BlobEndPoint
    {
        public static void MapBlobEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/blob").RequireAuthorization("TenantAccess");

            group.MapGet("/type/{entitytype}", async ([FromRoute]string entitytype, ILogger<Program> logger, HttpContext context, IBlobServices blobServices) =>
            {
                var tenantid = (Guid)context.Items["TenantId"]; 

                var blobs = await blobServices.Get(tenantid, entitytype);

                return blobs;

            }).RequireAuthorization("ClienteCanRead").CacheOutput("AuthenticatedPolicy");
            


            group.MapGet("/Get/type/{entitytype}/name/{name}", async ([FromRoute]string entitytype, HttpContext context, [FromRoute] string name, IBlobServices blobServices) =>
            {  

                var tenantid = (Guid)context.Items["TenantId"]; 

                var blob = await blobServices.Get(tenantid, entitytype, name);

                return blob;

            }).RequireAuthorization("ClienteDownLoadAccess")
            .CacheOutput("AuthenticatedPolicy");
            
             

             
            group.MapPost("/create/type/{entitytype}", async ([FromRoute]string entitytype, IOutputCacheStore cacheStore, HttpContext context,
                IFormFile file, IBlobServices blobServices) =>
            { 
                OperationResult result = new OperationResult();

                var tenantid = (Guid)context.Items["TenantId"]; 

                // Extraemos el tenant_id del token de Keycloak
                var userTenantId = context.User.FindFirst("tenantid")?.Value;

                if (userTenantId == null || Guid.Parse(userTenantId) != tenantid)
                {
                   return OperationResult.Failure("Cliente no autorizado");  
                }

                var allowedExtensions = new[] { ".pdf", ".docx" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                    return OperationResult.Failure("Tipo de archivo no permitido.");

                var blob = await blobServices.createBlob(tenantid, entitytype, file);

                  
                await cacheStore.EvictByTagAsync("documents", default); // Evict cache 
                 
                return blob;

            }).DisableAntiforgery().RequireAuthorization("CreateClientAccess"); ;

            group.MapDelete("/delete/type/{entitytype}/name/{blobName}", async ([FromRoute]string entitytype, IOutputCacheStore cacheStore, HttpContext context, [FromRoute] string blobName, IBlobServices blobServices) =>
            { 
                var tenantid = (Guid)context.Items["TenantId"]; 

                await cacheStore.EvictByTagAsync("documents", default); // Evict cache 

                return await blobServices.deleteBlob(tenantid, entitytype,blobName);
            }).RequireAuthorization("ClienteDeleteAccess"); ;
        }
 
    }
}
