using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Workflows;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;

namespace HNTAS.Web.UI.Tests.Contollers
{
    public class UserManagementControllerTests
    {

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
        public async Task HeatNetworksAsync_ReturnsViewWithModel_WhenUserHasHeatNetworks()
        {
            // Arrange
            var userId = "user-1";

            var mockUserService = new Mock<IUserService>();
            var mockSessionHelper = new Mock<ISessionHelper>();
            var mockLogger = new Mock<ILogger<UserManagementController>>();
            var mockWorkflowManager = new Mock<IWorkflowManager>();
            var mockHeatNetworkService = new Mock<IHeatNetworkService>();

            // session returns the user id
            mockSessionHelper
                .Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns(userId);

            // no declaration in session
            mockSessionHelper
                .Setup(s => s.GetFromSession<DeclationOfImpartialityModel>(It.IsAny<HttpContext>(), SessionKeys.DeclarationOfImpartialityModelKey))
                .Returns((DeclationOfImpartialityModel?)null);

            // build a UserDetailsResponse with one heat network and regulatory contact role
            var userDetails = new UserDetailsResponse
            {
                Id = userId,
                Roles = new List<UserRole> { UserRole.RegulatoryContact },
                HeatNetworks = new List<HeatNetworkUserResponse>
                {
                    new HeatNetworkUserResponse { HnId = "hn-1", Name = "Network 1" }
                },
                Organisation = new OrganisationResponse { Name = "Org Ltd" }
            };

            mockUserService
                .Setup(u => u.GetUserDetails(userId))
                .ReturnsAsync(userDetails);

            var controller = new UserManagementController(
                mockUserService.Object,
                mockLogger.Object,
                mockSessionHelper.Object,
                mockWorkflowManager.Object,
                mockHeatNetworkService.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            controller.Url = SetUpBackLink("UserAccount", "Dashboard").Object;

            // Act
            var result = await controller.HeatNetworksAsync();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HeatNetworksViewModel>(viewResult.Model);

            Assert.NotNull(model.HeatNetworks);
            Assert.Single(model.HeatNetworks);
            Assert.Equal("hn-1", model.HeatNetworks[0].HnId);
            Assert.Equal("Network 1", model.HeatNetworks[0].Name);
            Assert.True(model.IsRegulatoryContact);

            // controller ViewBag should have the user role string set
            Assert.Equal("RegulatoryContact", controller.ViewBag.UserRole);
        }
    }
}
