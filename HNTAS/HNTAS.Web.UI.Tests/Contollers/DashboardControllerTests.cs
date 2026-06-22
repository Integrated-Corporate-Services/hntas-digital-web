using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class DashboardControllerTests
    {
        private readonly Mock<ILogger<DashboardController>> _loggerMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IHeatNetworkService> _heatNetworksApiMock;
        private readonly Mock<IOrganisationService> _organisationServiceMock;
        private readonly Mock<ISessionHelper> _sessionHelperMock;

        private readonly DashboardController _controller;

        public DashboardControllerTests()
        {
            _loggerMock = new Mock<ILogger<DashboardController>>();
            _userServiceMock = new Mock<IUserService>();
            _heatNetworksApiMock = new Mock<IHeatNetworkService>();
            _organisationServiceMock = new Mock<IOrganisationService>();
            _sessionHelperMock = new Mock<ISessionHelper>();
            _controller = CreateController();
        }

        private DashboardController CreateController()
        {
            var controller = new DashboardController(
                _loggerMock.Object,
                _userServiceMock.Object,
                _heatNetworksApiMock.Object,
                _organisationServiceMock.Object,
                _sessionHelperMock.Object
            );
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var tempData = new TempDataDictionary(controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;

            return controller;
        }

        private Mock<IUrlHelper> SetUpBackLink(string controller, string action)
        {
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == action && ctx.Controller == controller)))
                .Returns($"{controller}/{action}");
            return urlHelperMock;
        }

        [Fact]
        public async Task Get_UserAccount_UserNotFound_ReturnsViewWithErrorMessage()
        {
            // Arrange
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("nonexistent-user-id");
            _userServiceMock.Setup(x => x.GetUserDetails("nonexistent-user-id"))
                .ReturnsAsync((UserDetailsResponse)null);

            // Act
            var result = await _controller.UserAccount();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Unable to retrieve user information. Please try again later.", _controller.TempData["ErrorMessage"]);
            Assert.IsType<DashboardModel>(viewResult.Model);
            var model = (DashboardModel)viewResult.Model;
        }

        [Fact]
        public async Task Get_UserAccount_UserHasNoOrganisation_ReturnsViewWithErrorMessage()
        {
            // Arrange
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
                    HeatNetworks = new List<HeatNetworkUserResponse>(),
                    EmailId = "test@example.com",
                    Roles = new List<UserRole>() { UserRole.ResponsiblePerson }
                });

            // Act
            var result = await _controller.UserAccount();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Your account is not associated with any organisation. Please contact support.", _controller.TempData["ErrorMessage"]);
            Assert.IsType<DashboardModel>(viewResult.Model);
        }

        [Fact]
        public async Task OrganisationDetails_ReturnsViewWithModel_WhenUserDetailsRetrievedSuccessfully()
        {
            // Arrange
            var urlHelperMock = SetUpBackLink("Dashboard", "UserAccount");
            _controller.Url = urlHelperMock.Object; // Assign mock to controller.Url
            _sessionHelperMock.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                             .Returns("Test Org");
            _sessionHelperMock.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                             .Returns("user123");

            var userDetails = new UserDetailsResponse
            {
                Organisation = new OrganisationResponse
                {
                    Name = "Test Org",
                    RegisteredAddress = new RegisteredAddress2(addressLine1: "Line1", addressLine2: "Line2", town: "town", county: "county", postcode: "e23rt", country: "country"),
                },
                EmailId = "test@example.com"
            };

            _userServiceMock.Setup(s => s.GetUserDetails("user123"))
                       .ReturnsAsync(userDetails);

            // Act
            var result = await _controller.OrganisationDetails();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OrganisationDetailsModel>(viewResult.Model);
            Assert.Equal("Test Org", model.OrganisationName);
            Assert.Equal("test@example.com", model.RPEmail);
        }

        [Fact]
        public async Task OrganisationDetails_ReturnsViewWithErrorMessage_WhenUserDetailsRetrievalFails()
        {
            // Arrange
            var urlHelperMock = SetUpBackLink("Dashboard", "UserAccount");
            _controller.Url = urlHelperMock.Object; // Assign mock to controller.Url
            var errorMessage = "Some error occured.";
            _sessionHelperMock.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                             .Returns("user123");
            _userServiceMock.Setup(s => s.GetUserDetails("user123"))
                       .ThrowsAsync(new Exception(errorMessage));
            // Act
            var result = await _controller.OrganisationDetails();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(errorMessage, _controller.TempData["ErrorMessage"]);
            Assert.IsType<OrganisationDetailsModel>(viewResult.Model);
        }
    }
}