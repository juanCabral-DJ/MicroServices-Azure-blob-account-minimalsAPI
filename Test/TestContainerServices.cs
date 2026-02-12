using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using minimalAPI.Services;
using minimalAPI.Services.Interfaces;
using Moq;
using Xunit;

namespace minimalAPI.Test
{
    public class TestContainerServices
    {
        public readonly IContainerServices _container;
        public readonly Mock<BlobServiceClient> _mockBlob;

        public TestContainerServices()
        {
            _mockBlob = new Mock<BlobServiceClient>();
            _container = new ContainerServices(_mockBlob.Object);
        }

        [Fact]
        public async void TestWhenContainerNameIsInvalid()
        {
            //Arrange
            string containerName = "Invalid_Name!";

            _mockBlob.Setup(x => x.CreateBlobContainerAsync(
            It.Is<string>(s => s == containerName),
            It.IsAny<PublicAccessType>(),
            It.IsAny<IDictionary<string, string>>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException("The specified container name is invalid."));

            //Act
            var result = await _container.createContainer(containerName);

            //Assert
            Assert.IsFalse(result.IsSuccess);
        }

        [Fact]
        public async void TestWhenContainerAlreadyExists()
        {
            //Arrange
            string containerName = "existingcontainer";
            _mockBlob.Setup(x => x.CreateBlobContainerAsync(containerName, PublicAccessType.None, default, default)).ThrowsAsync(new Azure.RequestFailedException("ContainerAlreadyExists"));
            //Act
            var result = await _container.createContainer(containerName);
            //Assert
            Assert.IsFalse(result.IsSuccess);
        }

        [Fact]
        public async void TestWhenContainerIsCreatedSuccessfully()
        {
            //Arrange
            string containerName = "newcontainer";
            _mockBlob.Setup(x => x.CreateBlobContainerAsync(
        It.IsAny<string>(),
        It.IsAny<PublicAccessType>(),
        It.IsAny<IDictionary<string, string>>(),
        It.IsAny<CancellationToken>()))
        .ReturnsAsync(Response.FromValue(Mock.Of<BlobContainerClient>(), Mock.Of<Response>()));
            //Act
            var result = await _container.createContainer(containerName);
            //Assert
            Assert.IsTrue(result.IsSuccess);
        }

        [Fact]
        public async void TestWhenGetContainersIsSuccessful()
        {
            //Arrange
            var mockContainerItem = new Mock<BlobContainerItem>();
            var mockAsyncPageable = new Mock<AsyncPageable<BlobContainerItem>>();
            mockAsyncPageable.Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(Mock.Of<IAsyncEnumerator<BlobContainerItem>>());
            _mockBlob.Setup(x => x.GetBlobContainersAsync(It.IsAny<BlobContainerTraits>(), It.IsAny<BlobContainerStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(mockAsyncPageable.Object);
            //Act
            var result = await _container.Get();
            //Assert
            Assert.IsTrue(result.IsSuccess);
        }

        [Fact]
        public async void TestWhenDeleteContainerIsSuccessful()
        {
            //Arrange
            string containerName = "existingcontainer";
            _mockBlob.Setup(x => x.GetBlobContainerClient(containerName)).Returns(Mock.Of<BlobContainerClient>());
            _mockBlob.Setup(x => x.GetBlobContainerClient(containerName).DeleteAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue(Mock.Of<Response>(), Mock.Of<Response>()));
            //Act
            var result = await _container.deleteContainer(containerName);
            //Assert
            Assert.IsTrue(result.IsSuccess);
        }

        [Fact]
        public async void TestWhenDeleteContainerThatDoesNotExist()
        {
            //Arrange
            string containerName = "nonexistentcontainer";
            _mockBlob.Setup(x => x.GetBlobContainerClient(containerName)).Returns(Mock.Of<BlobContainerClient>());
            _mockBlob.Setup(x => x.GetBlobContainerClient(containerName).DeleteAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>())).ThrowsAsync(new RequestFailedException("ContainerNotFound"));
            //Act
            var result = await _container.deleteContainer(containerName);
            //Assert
            Assert.IsFalse(result.IsSuccess);
        }

        [Fact]
        public async void TestWhenGetContainerNotExist()
        {
            //Arrange
            var mockAsyncPageable = new Mock<AsyncPageable<BlobContainerItem>>();
            mockAsyncPageable.Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(Mock.Of<IAsyncEnumerator<BlobContainerItem>>());
            _mockBlob.Setup(x => x.GetBlobContainersAsync(It.IsAny<BlobContainerTraits>(), It.IsAny<BlobContainerStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(mockAsyncPageable.Object);
            //Act
            var result = await _container.Get();
            //Assert
            Assert.IsTrue(result.IsSuccess);
        }

    }

}
