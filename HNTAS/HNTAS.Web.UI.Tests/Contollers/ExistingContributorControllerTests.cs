using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Extensions;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Tests.Helpers;
using HNTAS.Web.UI.Workflows;
using HNTAS.Web.UI.Workflows.Enums;
using HNTAS.Web.UI.Workflows.Models;
using HNTAS.Web.UI.Workflows.Models.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Moq;

namespace HNTAS.Web.UI.Tests.Contollers
{
    public class ExistingContributorControllerTests
    {
        private readonly Mock<IWorkflowManager> _mockWorkflowManager;
        private readonly Mock<ILogger<ExistingContributorController>> _mockLogger;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ISessionHelper> _mockSessionHelper;
        private readonly Mock<IInvitationTokenService> _mockIInvitationTokenService;
        private readonly Mock<IInvitationService> _mockInvitationService;
        private readonly ExistingContributorController _controller;

        public ExistingContributorControllerTests()
        {
            _mockWorkflowManager = new Mock<IWorkflowManager>();
            _mockLogger = new Mock<ILogger<ExistingContributorController>>();
            _mockUserService = new Mock<IUserService>();
            _mockSessionHelper = new Mock<ISessionHelper>();
            _mockIInvitationTokenService = new Mock<IInvitationTokenService>();
            _mockInvitationService = new Mock<IInvitationService>();
            _controller = CreateController();
        }

        private ExistingContributorController CreateController()
        {
            var controller = new ExistingContributorController(
                _mockWorkflowManager.Object,
                _mockLogger.Object,
                _mockUserService.Object,
                _mockSessionHelper.Object,
                _mockIInvitationTokenService.Object,
                _mockInvitationService.Object
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
        public async Task ChooseUserAsync_ReturnsView_WithContributors_WhenContributorsExist()
        {
            // Arrange
            var rpUserId = "User123";
            var organisationName = "Test Organisation";
            var workflowState = new WorkflowState<AddExistingContributorWorkflowModel>
            {
                Data = new AddExistingContributorWorkflowModel
                {
                    ChooseContributorModel = new ChooseContributorModel()
                }
            };

            _mockWorkflowManager.Setup(w => w.GetState<AddExistingContributorWorkflowModel>()).Returns(workflowState);
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey)).Returns(rpUserId);
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationName)).Returns(organisationName);            
            _mockUserService.Setup(u => u.GetRegisteredUsersAsync(rpUserId)).ReturnsAsync(TestingUtility.MockValid_UserService_GetRegisteredUsers(rpUserId));
            _controller.Url = TestingUtility.SetUpBackLink("AddContributor", "UserManagement").Object;

            // Act
            var result = await _controller.ChooseUserAsync();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ChooseContributorModel>(viewResult.Model);

            Assert.NotNull(model.Contributors);
            Assert.Equal(2, model.Contributors.Count);
            Assert.Equal("alicesmith@test.com", model.Contributors[0].Text);
            Assert.Equal(organisationName, _controller.ViewBag.OrganisationName);
        }        
    }
}
