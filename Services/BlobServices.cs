using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using minimalAPI.Data;
using minimalAPI.Services.Interfaces;

namespace minimalAPI.Services
{
    public class BlobServices : IBlobServices
    {
        private readonly BlobServiceClient _client;
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        public BlobServices(AppDbContext context, BlobServiceClient client, IConfiguration configuration)
        {
            _dbContext = context;
            _client = client;
            _configuration = configuration;
        }
        public async Task<OperationResult> createBlob(Guid tenantid, string entitytype, IFormFile file)
        {
            try
            {
                OperationResult result = new OperationResult();

                var Containername = tenantid.ToString().ToLower(); //Usar el tenantid como nombre del contenedor
                var containerClient = _client.GetBlobContainerClient(Containername);
                await containerClient.CreateIfNotExistsAsync();

                string storageKey = $"{entitytype}/{file.FileName}";
                var blobClient = await containerClient.GetBlobClient(storageKey).ExistsAsync();

                if (blobClient)
                {
                    return OperationResult.Failure("El archivo ya existe en el contenedor");
                }

                var blob = await containerClient.UploadBlobAsync(storageKey, file.OpenReadStream());

                //Extraccion de datos del documento para guardarlos en Postgres
                var doc = new Doc
                {
                    id = Guid.NewGuid(),
                    tenant_id = tenantid,
                    doc_tyoe_code = entitytype,
                    file_name = file.FileName,
                    content_type = file.ContentType,
                    content_length = file.Length,
                    checksum_sha256 = "", // Calcular el checksum si es necesario
                    storage_provider = "AzureBlobStorage",
                    storage_key = storageKey,
                    row_status_id = 1,
                    create_at = DateTime.UtcNow,
                    update_at = DateTime.UtcNow
                };

                var doclink = new DocLinks
                {
                    id = Guid.NewGuid(),
                    tenant_id = tenantid,
                    doc_id = doc.id,
                    entity_type = entitytype,
                    entity_id = Guid.NewGuid(), // Asignar el ID de la entidad correspondiente
                    role = "default",
                    notes = "Documento subido",
                    row_status_id = 1,
                    create_at = DateTime.UtcNow,
                    update_at = DateTime.UtcNow
                };

                // Guardar en la base de datos PostgreSQL
                _dbContext.doc.Add(doc);
                _dbContext.doc_link.Add(doclink);
                await _dbContext.SaveChangesAsync();

                return OperationResult.Success("Archivo subido con éxito", "id =" + doc.id );
            }
            catch (Exception e)
            {
                return OperationResult.Failure("Error al subir el archivo", e);
            }

        }

        public async Task<OperationResult> deleteBlob(Guid tenantid, string entityType, string blobName)
        {
            try
            {
                OperationResult result = new OperationResult();

                var container = _client.GetBlobContainerClient(tenantid.ToString());
                var blob = container.GetBlobClient($"{entityType}/{blobName}");

                if (!await blob.ExistsAsync()) return OperationResult.Failure("el archivo no existe en el contenedor");

                await blob.DeleteIfExistsAsync();

                return OperationResult.Success("Archivo eliminado con exito");
            }
            catch (Exception e)
            {
                return OperationResult.Failure("Error al eliminar el archivo", e);
            }
        }

        public async Task<OperationResult> Get(Guid tenantid, string entityType)
        {
            try
            {
                OperationResult result = new OperationResult();

                var container = _client.GetBlobContainerClient(tenantid.ToString());
                var blobs = container.GetBlobsAsync(traits: BlobTraits.None, states: BlobStates.None, prefix: $"{entityType}/", default);


                result = OperationResult.Success("Devolviendo blobs exitosamente", blobs);

                return result;
            }
            catch (Exception e)
            {
                return OperationResult.Failure("Error devolviendo blobs", e);
            }

        }

        public async Task<OperationResult> Get(Guid tenantid, string entityType, string name)
        {
            try
            {
                OperationResult result = new OperationResult();
                var container = _client.GetBlobContainerClient(tenantid.ToString().ToLower());
                var blob = container.GetBlobClient($"{entityType}/{name}");

                if (!await blob.ExistsAsync())
                {
                    return OperationResult.Failure("el archivo no existe");
                }

                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = container.Name,
                    BlobName = blob.Name,
                    Resource = _configuration["SasToken:resource"],
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(_configuration.GetValue<int>("SasToken:timeToExpireInMinutes")).DateTime
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);
                Uri sasUri = blob.GenerateSasUri(sasBuilder);

                //Devolver los datos
                var response = new 
                {
                    url = sasUri.ToString(),
                    expiration = DateTimeOffset.UtcNow.AddMinutes(_configuration.GetValue<int>("SasToken:timeToExpireInMinutes")).DateTime,
                    message = "La URL expirará en 5 minutos"
                };

                result = OperationResult.Success("Blob devuelto exitosamente", response);

                return result;
            }
            catch (Exception e)
            {
                return OperationResult.Failure("Error devolviendo blob", e);
            }
        }
    }
}
