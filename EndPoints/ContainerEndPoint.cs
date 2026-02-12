using Microsoft.AspNetCore.Mvc;
using minimalAPI.Data;
using minimalAPI.Services.Interfaces;
using System.Text.RegularExpressions;

namespace minimalAPI.EndPoints
{
    public static class ContainerEndPoint
    {
        public static void MapContainerEndPoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/containers").RequireAuthorization("TenantAccess");

            group.MapGet("/", (IContainerServices containerService) =>
            {
                var container = containerService.Get();
                return container is not null ? Results.Ok(container) : Results.NotFound();
            }).RequireAuthorization("CanRead");


            group.MapPost("/create", async ([FromBody] string containerName, IContainerServices containerService) =>
            {
                OperationResult result = new OperationResult();

                if (!Regex.IsMatch(containerName, "^[a-z0-9-]{3,63}$"))
                    return OperationResult.Failure("El nombre no cumple con las reglas de Azure");

                return await containerService.createContainer(containerName);

            }).RequireAuthorization("CreateAccess");

            group.MapDelete("/delete/name/{containerName}", async ([FromRoute] string containerName, IContainerServices containerServices) =>
            {

                return await containerServices.deleteContainer(containerName);

            }).RequireAuthorization("DeleteAccess");
        }
    }
}