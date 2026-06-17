using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Models.CompaniesHouse;
using Moq;

namespace HNTAS.Web.UI.Tests.Services
{
    public class CompaniesHouseServiceTests
    {
        private readonly Mock<ICompaniesHouseService> _service;

        public CompaniesHouseServiceTests()
        {
            _service = new Mock<ICompaniesHouseService>();
        }

        [Fact(Skip = "TODO: API key null issue to be fixed")]
        public async Task GetCompanyByNumberAsync_ReturnsCompanyDetails_WhenCompanyExists()
        {
            // Arrange
            var companyNumber = "83031634";

            var expected = new CompanyDetailsModel
            {
                Title = "Some company"
            };

            _service
                .Setup(s => s.GetCompanyByNumberAsync(companyNumber))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.Object.GetCompanyByNumberAsync(companyNumber);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Some company", result.Title);

            _service.Verify(s => s.GetCompanyByNumberAsync(companyNumber), Times.Once);
        }

        [Fact(Skip = "TODO: API key null issue to be fixed")]
        public async Task GetCompanyByNumberAsync_ThrowsException_WhenApiKeyIsMissing()
        {
            // Arrange
            var companyNumber = "48850136";

            _service
                .Setup(s => s.GetCompanyByNumberAsync(companyNumber))
                .ThrowsAsync(new InvalidOperationException("API key missing"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.Object.GetCompanyByNumberAsync(companyNumber));

            _service.Verify(s => s.GetCompanyByNumberAsync(companyNumber), Times.Once);
        }
    }
}