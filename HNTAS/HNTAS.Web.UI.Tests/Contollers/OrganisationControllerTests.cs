using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace HNTAS.Web.UI.Tests.Contollers
{
    public class OrganisationControllerTests
    {
        private readonly Mock<ICompaniesHouseService> _companiesHouseServiceMock;
        private readonly Mock<ILogger<OrganisationController>> _loggerMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ISessionHelper> _sessionHelperMock;

        public OrganisationControllerTests()
        {
            _companiesHouseServiceMock = new Mock<ICompaniesHouseService>();
            _loggerMock = new Mock<ILogger<OrganisationController>>();
            _userServiceMock = new Mock<IUserService>();
            _sessionHelperMock = new Mock<ISessionHelper>();
        }

        private OrganisationController CreateController()
        {
            var controller = new OrganisationController(
                _companiesHouseServiceMock.Object,
                _loggerMock.Object,
                _userServiceMock.Object,
                _sessionHelperMock.Object
            );

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new MockHttpSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        [Fact]
        public void Start_RedirectsToOrganisationType_And_CallsSessionHelpers()
        {
            // Arrange
            var controller = CreateController();
            var httpContext = controller.ControllerContext.HttpContext;

            // Act
            var result = controller.Start();

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("OrganisationType", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.ClearAllFlowRelatedSessionData(httpContext), Times.Once);
            _sessionHelperMock.Verify(x => x.SetIsCheckAnswerFlow(httpContext, false), Times.Once);
        }
    }
}