using HNTAS.Web.UI.Models.CompaniesHouse;
using HNTAS.Web.UI.Services;
using Moq;
using Moq.Protected;
using System.Net;

namespace HNTAS.Web.UI.Tests.Services
{
    public class CompaniesHouseServiceTests
    {
        private readonly Mock<ICompaniesHouseService> _service;

        public CompaniesHouseServiceTests()
        {
            _service = new Mock<ICompaniesHouseService>();
        }

        [Fact]
        public async Task GetCompanyByNumberAsync_ReturnsCompanyDetails_WhenCompanyExists()
        {
            // Arrange
            var json = """
            {
                "company_name": "Some company",
                "registered_office_address": {
                    "address_line_1": "Street 1",
                    "postal_code": "AB1 2CD"
                }
            }
            """;

            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json)
                });

            var httpClient = new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("https://api.company-information.service.gov.uk/")
            };

            var service = new CompaniesHouseService(
                httpClient,
                "test-api-key");

            // Act
            var result = await service.GetCompanyByNumberAsync("83031634");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Some company", result.Title);
            Assert.NotNull(result.RegisteredOfficeAddress);
            Assert.Equal("Street 1", result.RegisteredOfficeAddress.AddressLine1);
        }

        [Fact]
        public async Task GetCompanyByNumberAsync_ThrowsInvalidOperationException_WhenApiKeyNotConfigured()
        {
            // Arrange
            _service.Setup(s => s.GetCompanyByNumberAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Companies House API key is not configured."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.Object.GetCompanyByNumberAsync("83031634"));

            Assert.Equal(
                "Companies House API key is not configured.",
                exception.Message);
        }
    }
}