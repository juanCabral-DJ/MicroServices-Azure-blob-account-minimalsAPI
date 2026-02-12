using minimalAPI.Data;

namespace minimalAPI.Services.Interfaces
{
    public interface IBlobServices
    {
        Task<OperationResult> Get(Guid tenantid, string entityType);
        Task<OperationResult> Get(Guid tenantid, string entityType, string name);
        Task<OperationResult> createBlob(Guid tenantid, string entitytype, IFormFile file);
        Task<OperationResult> deleteBlob(Guid tenantid, string entitytype, string blobName);
    }
}
