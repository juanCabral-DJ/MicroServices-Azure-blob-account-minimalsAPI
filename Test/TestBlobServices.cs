using Azure;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using minimalAPI.Data;
using minimalAPI.Services;
using minimalAPI.Services.Interfaces;
using Moq;
using Xunit;

namespace Test
{
    public class TestBlobServices
    {
        public readonly IBlobServices _blob;
        public readonly Mock<IConfiguration> _mockConfi;
        public readonly Mock<BlobServiceClient> _mockBlob;
        public TestBlobServices(IBlobServices blob)
        {
             _mockConfi = new Mock<IConfiguration>();
             _mockBlob = new Mock<BlobServiceClient>();
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            var _dbContext = new AppDbContext(options);

            var _services = new BlobServices(_dbContext, _mockBlob.Object, _mockConfi.Object);

                _blob = blob;
        }

        [Fact]
        public async void TestWhenTenantIdIsZero()
        {
            //Arrange
            Guid tenantId = Guid.Empty;
            string entityType = "invoices";

            //Act
            var result = await _blob.Get(tenantId, entityType);

            //Assert
            Assert.IsFalse(result.IsSuccess);
        }

        [Fact]
        public async void TestWhenEntityTypeIsNull()
        {
            //Arrange
            Guid tenantId = Guid.NewGuid();
            string entityType = null;

            //Act
            var result = await _blob.Get(tenantId, entityType);

            //Assert
            Assert.IsFalse(result.IsSuccess);
        }

        [Fact]
        public async void TestWhenBothParametersAreValid()
        {
            //Arrange
            var tenantId = "4e1b5f9b-82a0-439b-b550-5fdc1c94ad2d";
            var tenantGuid = Guid.Parse(tenantId);
            string entityType = "invoices";

            var mockContainer = new Mock<BlobContainerClient>();
            _mockBlob
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(mockContainer.Object);

            //Act
            var result = await _blob.Get(tenantGuid, entityType);

            //Assert
            Assert.IsTrue(result.IsSuccess);
        }

        [Fact]
        public async void TestWhenExtensionIsNotAllowed()
        {
            //Arrange
            Guid tenantId = Guid.NewGuid();
            string entityType = "invoices";
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.exe");
            fileMock.Setup(f => f.ContentType).Returns("application/x-msdownload");
            fileMock.Setup(f => f.Length).Returns(1024);

            //Act
            var result = await _blob.createBlob(tenantId, entityType, fileMock.Object);
            //Assert
            Assert.IsFalse(result.IsSuccess);
        }

        [Fact]
        public async void TestWhenFileAlreadyExists()
        {
            //Arrange
            Guid tenantId = Guid.NewGuid();
            string entityType = "invoices";
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");
            fileMock.Setup(f => f.Length).Returns(1024);
            var mockContainer = new Mock<BlobContainerClient>();
            var mockBlobClient = new Mock<BlobClient>();
            mockBlobClient.Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(true, null!));
            _mockBlob.Setup(x => x.GetBlobContainerClient(It.IsAny<string>())).Returns(mockContainer.Object);
            //Act
            var result = await _blob.createBlob(tenantId, entityType, fileMock.Object);
            //Assert
            Assert.IsFalse(result.IsSuccess);
        }

        [Fact]
        public async void TestWhenGetDocuments()
        {
            //Arrange
            Guid tenantId = Guid.NewGuid();
            string entityType = "invoices";
            string fileName = "test.pdf";

            //Act
            var result = await _blob.Get(tenantId, entityType, fileName);

            //Assert
            Assert.IsFalse(result.IsSuccess);
        }

        [Fact]
        public async void TestWhenGetDocumentsIsFailure()
        {
            //Arrange
            Guid tenantId = Guid.NewGuid();
            string entityType = "invoices";
            string fileName = "tes.pdf";

            //Act
            var result = await _blob.Get(tenantId, entityType, fileName);

            //Assert
            Assert.IsFalse(result.IsSuccess);
        }

        [Fact]
        public async void TestWhenDeleteBlob()
        {
            //Arrange
            Guid tenantId = Guid.NewGuid();
            string entityType = "invoices";
            string fileName = "test.pdf";
            //Act
            var result = await _blob.deleteBlob(tenantId, entityType, fileName);
            //Assert
            Assert.IsFalse(result.IsSuccess);
        }

        [Fact]

        public async void TestWhenDeleteBlobIsFailure()
        {
            //Arrange
            Guid tenantId = Guid.NewGuid();
            string entityType = "invoices";
            string fileName = "tes.pdf";
            //Act
            var result = await _blob.deleteBlob(tenantId, entityType, fileName);
            //Assert
            Assert.IsFalse(result.IsSuccess);
        }

    }
}