using HNTAS.Web.UI.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                BaseAddress = new Uri("https://api.company-information.service.gov.uk/")
            };

            _apiKey = Environment.GetEnvironmentVariable("COMPANIES_HOUSE_API_KEY");

            var config = new ConfigurationBuilder().Build();
            _service = new CompaniesHouseService(_httpClient, config);
        }

        [Fact]
        public async Task GetCompanyByNumberAsync_ReturnsCompanyDetails_WhenCompanyExists()
        {
            // Arrange
            var companyNumber = "08811254"; // Valid company number for testing

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
            var config = new ConfigurationBuilder().Build();
            var service = new CompaniesHouseService(httpClient, config);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetCompanyByNumberAsync("08811254"));
        }
    }

}
