using HNTAS.Web.UI.Services;

namespace HNTAS.Web.UI.Tests.Services
{
    public class CompaniesHouseServiceTests
    {
        private readonly CompaniesHouseService _service;
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;

        public CompaniesHouseServiceTests()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api-sandbox.company-information.service.gov.uk")
            };

            _apiKey = Environment.GetEnvironmentVariable("COMPANIES_HOUSE_API_KEY");

            _service = new CompaniesHouseService(_httpClient, _apiKey);
          
        }

        [Fact]
        public async Task GetCompanyByNumberAsync_ReturnsCompanyDetails_WhenCompanyExists()
        {
            // Arrange
            var companyNumber = "48850136"; // Sandbox test company number

            // Act
            var result = await _service.GetCompanyByNumberAsync(companyNumber);            

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.Title));
        }

        [Fact]
        public async Task GetCompanyByNumberAsync_ThrowsException_WhenApiKeyIsMissing()
        {
            // Arrange
            Environment.SetEnvironmentVariable("COMPANIES_HOUSE_API_KEY", null); // Simulate missing key
            var httpClient = new HttpClient();
            var service = new CompaniesHouseService(httpClient, _apiKey);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetCompanyByNumberAsync("48850136"));

        }
    }
}