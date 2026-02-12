using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using minimalAPI.Data;
using minimalAPI.Services.Interfaces;
using System.Buffers;

namespace minimalAPI.Services
{
    public class ContainerServices : IContainerServices
    {
        private readonly BlobServiceClient _client;

        public ContainerServices(BlobServiceClient client)
        {
            _client = client;
        }

        public async Task<OperationResult> createContainer(string containerName)
        {
            try
            {
                BlobContainerClient containerClient = await _client.CreateBlobContainerAsync(containerName);
                return OperationResult.Success("Contenedor creado exitosamente");
            }
            catch (Exception ex) 
            {
                // El contenedor ya existe 
                return OperationResult.Failure("El contenedor ya existe");
            }
        }

        public async Task<OperationResult> deleteContainer(string containerName)
        {
            try
            {
                OperationResult result = new OperationResult();

                BlobContainerClient containerClient =  _client.GetBlobContainerClient(containerName);
                await containerClient.DeleteAsync();

                return OperationResult.Success("contenedor eliminado exitosamente");
            }
            catch (Exception e) 
            {
                return OperationResult.Failure("el contenedor no existe");
            }
        }

        public async Task<OperationResult> Get()
        {
            try
            {
                OperationResult result = new OperationResult();

                var containers = _client.GetBlobContainersAsync();

                return OperationResult.Success("Contenedores obtenidos exitosamente", containers);

            }
            catch (Exception e)
            {
                return OperationResult.Failure("Error al obtener los contenedores");
            } 
        }
 
    }
}
