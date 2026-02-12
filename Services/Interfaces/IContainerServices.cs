using minimalAPI.Data;

namespace minimalAPI.Services.Interfaces
{
    public interface IContainerServices
    {
        Task<OperationResult> Get(); 
        Task<OperationResult> createContainer(string containerName);
        Task<OperationResult> deleteContainer(string containerName);
    }
}
