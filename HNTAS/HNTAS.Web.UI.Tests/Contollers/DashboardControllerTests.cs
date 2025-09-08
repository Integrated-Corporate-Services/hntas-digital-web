using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class DashboardControllerTests
    {
        private readonly Mock<ILogger<DashboardController>> _loggerMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IHeatNetworksApi> _heatNetworksApiMock;
        private readonly Mock<ISessionHelper> _sessionHelperMock;

        internal class OrganisationAddressResponse 
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
    }

        public DashboardControllerTests()
        {
            _loggerMock = new Mock<ILogger<DashboardController>>();
            _userServiceMock = new Mock<IUserService>();
            _heatNetworksApiMock = new Mock<IHeatNetworksApi>();
            _sessionHelperMock = new Mock<ISessionHelper>();
        }

        private DashboardController CreateController()
        {
            var controller = new DashboardController(
                _loggerMock.Object,
                _userServiceMock.Object,
                _heatNetworksApiMock.Object,
                _sessionHelperMock.Object
            );
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns("/mocked-url");
            controller.Url = urlHelperMock.Object;

            // Setup TempData for error message assertions
            var tempData = new TempDataDictionary(controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;

            return controller;
        }

        [Fact]
        public async Task Get_UserAccount_UserNotFound_ReturnsViewWithErrorMessage()
        {
            // Arrange
            var controller = CreateController();
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("nonexistent-user-id");
            _userServiceMock
                .Setup(x => x.GetUserDetails("nonexistent-user-id"))
                .ReturnsAsync((UserDetailsResponse)null);

            // Act
            var result = await controller.UserAccount();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Unable to retrieve user information. Please try again later.", controller.TempData["ErrorMessage"]);
            Assert.IsType<DashboardModel>(viewResult.Model);
            var model = (DashboardModel)viewResult.Model;
            Assert.Empty(model.HeatNetworks);
        }

        [Fact]
        public async Task Get_UserAccount_UserHasNoOrganisation_ReturnsViewWithErrorMessage()
        {
            // Arrange
            var controller = CreateController();
            var userId = "user-without-org";
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns(userId);

            _userServiceMock
                .Setup(x => x.GetUserDetails(userId))
                .ReturnsAsync(new UserDetailsResponse
                {
                    Organisation = null,
                    HeatNetworks = new List<HeatNetworkResponse>(),
                    EmailId = "test@example.com"
                });

            // Act
            var result = await controller.UserAccount();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Your account is not associated with any organisation. Please contact support.", controller.TempData["ErrorMessage"]);
            Assert.IsType<DashboardModel>(viewResult.Model);
        }

        [Fact]
        public async Task OrganisationDetails_ReturnsViewWithModel_WhenUserDetailsRetrievedSuccessfully()
        {
            // Arrange
            var controller = CreateController();
            _sessionHelperMock.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                             .Returns("Test Org");
            _sessionHelperMock.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                             .Returns("user123");

            var userDetails = new UserDetailsResponse
            {
                Organisation = new OrganisationResponse
                {
                    Name = "Test Org",
                    RegisteredAddress = new RegisteredAddress(addressLine1: "Line1", addressLine2: "Line2", town: "town", county:"county", postcode:"e23rt", country:"country"),
                },
                EmailId = "test@example.com"
            };

            _userServiceMock.Setup(s => s.GetUserDetails("user123"))
                       .ReturnsAsync(userDetails);

            // Act
            var result = await controller.OrganisationDetails();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OrganisationDetailsModel>(viewResult.Model);
            Assert.Equal("Test Org", model.OrganisationName);
            Assert.Equal("test@example.com", model.RPEmail);
        }
    }
}