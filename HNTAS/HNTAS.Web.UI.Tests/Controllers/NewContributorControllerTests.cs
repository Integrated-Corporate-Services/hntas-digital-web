using HNTAS.Api.Client.Api;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Workflows;
using HNTAS.Web.UI.Workflows.Models;
using HNTAS.Web.UI.Workflows.Models.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class NewContributorControllerTests
    {
        private readonly Mock<IHeatNetworksApi> _mockHeatNetworksApi;
        private readonly Mock<IInvitationTokenService> _mockInvitationTokenService;
        private readonly Mock<IInvitationService> _mockInvitationService;

        private readonly Mock<ILogger<NewContributorController>> _mockLogger;
        private readonly Mock<IWorkflowManager> _mockWorkflowManager;
        private readonly Mock<ISessionHelper> _mockSessionHelper;
        private readonly Mock<IUserService> _mockUserService;
        private readonly NewContributorController _controller;
        private readonly Mock<IUrlHelper> _mockUrlHelper;


        public NewContributorControllerTests()
        {
            // Set up all the mock objects once in the constructor
            _mockLogger = new Mock<ILogger<NewContributorController>>();
            _mockWorkflowManager = new Mock<IWorkflowManager>();
            _mockSessionHelper = new Mock<ISessionHelper>();
            _mockUserService = new Mock<IUserService>();
            _mockHeatNetworksApi = new Mock<IHeatNetworksApi>();
            _mockInvitationTokenService = new Mock<IInvitationTokenService>();
            _mockInvitationService = new Mock<IInvitationService>();

            // We also need to mock HttpContext for the session helper call
            var mockHttpContext = new Mock<HttpContext>();

            _controller = new NewContributorController(
                _mockLogger.Object,
                _mockWorkflowManager.Object,
                _mockSessionHelper.Object, _mockUserService.Object, _mockHeatNetworksApi.Object, _mockInvitationTokenService.Object, _mockInvitationService.Object);

            // Assign the mocked HttpContext to the controller
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext.Object
            };

            _mockUrlHelper = new Mock<IUrlHelper>();
            _mockUrlHelper
               .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
               .Returns("/mocked-url");

            _controller.Url = _mockUrlHelper.Object;
        }

        [Fact]
        public void AddEmailAddress_WithExistingModel_ReturnsViewModel()
        {
            // Arrange
            // Set up a pre-existing AddUserEmailAddressModel to be returned by the mock.
            var existingModel = new AddUserEmailAddressModel { EmailAddress = "test@example.com" };
            var workflowState = new WorkflowState<AddNewContributorWorkflowModel>
            {
                Data = new AddNewContributorWorkflowModel
                {
                    AddUserEmailAddressModel = existingModel
                }
            };

            // Configure the mocks to return our pre-defined objects.
            _mockWorkflowManager.Setup(m => m.GetState<AddNewContributorWorkflowModel>()).Returns(workflowState);
            _mockSessionHelper.Setup(m => m.GetFromSession<string>(It.IsAny<HttpContext>(), It.IsAny<string>())).Returns("Test Organisation");
            _mockUrlHelper.Setup(u => u.Action(It.Is<UrlActionContext>(ctx => ctx.Action == "AddContributor" && ctx.Controller == "UserManagement")))
                .Returns("UserManagement/AddContributor");
            _controller.Url = _mockUrlHelper.Object;

            // Act
            var result = _controller.AddEmailAddress() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AddUserEmailAddressModel>(result.Model);
            Assert.Same(existingModel, result.Model); // Check if the exact same instance is returned
            Assert.Equal("Test Organisation", _controller.ViewBag.OrganisationName);
            Assert.True(_controller.ViewBag.ShowBackButton);
            Assert.Equal("UserManagement/AddContributor", _controller.ViewBag.BackLinkUrl);

            // Verify that GetState and GetFromSession were called exactly once.
            _mockWorkflowManager.Verify(m => m.GetState<AddNewContributorWorkflowModel>(), Times.Once);
            _mockSessionHelper.Verify(m => m.GetFromSession<string>(It.IsAny<HttpContext>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void AddEmailAddress_WithNullModel_ReturnsNewViewModel()
        {
            // Arrange
            // Set up the workflow state to return a null AddUserEmailAddressModel.
            var workflowState = new WorkflowState<AddNewContributorWorkflowModel>
            {
                Data = new AddNewContributorWorkflowModel
                {
                    AddUserEmailAddressModel = null
                }
            };

            // Configure the mocks.
            _mockWorkflowManager.Setup(m => m.GetState<AddNewContributorWorkflowModel>()).Returns(workflowState);
            _mockSessionHelper.Setup(m => m.GetFromSession<string>(It.IsAny<HttpContext>(), It.IsAny<string>())).Returns("Test Organisation");
            _mockUrlHelper.Setup(u => u.Action(It.Is<UrlActionContext>(ctx => ctx.Action == "AddContributor" && ctx.Controller == "UserManagement")))
               .Returns("UserManagement/AddContributor");
            _controller.Url = _mockUrlHelper.Object;

            // Act
            var result = _controller.AddEmailAddress() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AddUserEmailAddressModel>(result.Model);
            Assert.NotNull(result.Model); // Check that a new instance was created.
            Assert.Equal("Test Organisation", _controller.ViewBag.OrganisationName);

            // The returned model should be a *new* instance, not the same as anything we set up.
            var returnedModel = result.Model as AddUserEmailAddressModel;
            Assert.Equal(null, returnedModel.EmailAddress); // Check a property to ensure it's a default, new object.

            // Verify that GetState and GetFromSession were called exactly once.
            _mockWorkflowManager.Verify(m => m.GetState<AddNewContributorWorkflowModel>(), Times.Once);
            _mockSessionHelper.Verify(m => m.GetFromSession<string>(It.IsAny<HttpContext>(), It.IsAny<string>()), Times.Once);
        }

        //[Fact]
        //public void ChooseRole_ReturnsViewWithRoles_WhenRolesExist()
        //{
        //    // Arrange
        //    var roles = new List<SelectListItem> { new SelectListItem { Value = "1", Text = "Role1" } };
        //    _mockSessionHelper.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), "UserRole")).Returns("RegulatoryContact");
        //    _mockWorkflowManager.Setup(x => x.GetState<AddNewContributorWorkflowModel>())
        //        .Returns(new WorkflowState<AddNewContributorWorkflowModel>
        //        {
        //            Data = new AddNewContributorWorkflowModel { ChooseRoleModel = new ChooseRoleModel { SelectedRoleId = "1" } }
        //        });

        //    // Act
        //    var result = _controller.ChooseRole();

        //    // Assert
        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    var model = Assert.IsType<ChooseRoleModel>(viewResult.Model);
        //    Assert.Equal("1", model.SelectedRoleId);
        //    Assert.NotNull(model.Roles);
        //}

        //[Fact]
        //public void ChooseRole_ReturnsErrorView_WhenRolesAreNull()
        //{
        //    // Arrange
        //    _mockSessionHelper.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), "UserRole")).Returns("UnknownRole");

        //    // Act
        //    var result = _controller.ChooseRole() as ViewResult;

        //    // Assert
        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    var model = Assert.IsType<ChooseRoleModel>(viewResult.Model);
        //    Assert.Null(model.Roles);
        //    Assert.Equal("Contributor/ChooseRole", viewResult.ViewName);
        //    Assert.Equal("Unable to retrieve contributor roles. Please try again later.", _controller.TempData["ErrorMessage"]);
        //}

        //[Fact]
        //public async Task SaveChosenRoleAsync_RedirectsToCheckYourAnswers_WhenModelIsValid()
        //{
        //    // Arrange
        //    var model = new ChooseRoleModel
        //    {
        //        SelectedRoleId = "1",
        //        Roles = new List<SelectListItem> { new SelectListItem { Value = "1", Text = "Role1" } }
        //    };
        //    _mockWorkflowManager.Setup(x => x.UpdateStep<AddNewContributorWorkflowModel, ContributorWorkflowStep>(
        //        It.IsAny<System.Action<AddNewContributorWorkflowModel>>(),
        //        ContributorWorkflowStep.Review
        //    ));

        //    // Act
        //    var result = await _controller.SaveChosenRoleAsync(model) as ViewResult;

        //    // Assert
        //    var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal("CheckYourAnswers", redirectResult.ActionName);
        //    Assert.Equal("Role1", model.SelectedRoleName);
        //}

        //[Fact]
        //public async Task SaveChosenRoleAsync_ReturnsView_WhenModelIsInvalid()
        //{
        //    // Arrange
        //    var model = new ChooseRoleModel
        //    {
        //        SelectedRoleId = null,
        //        Roles = new List<SelectListItem> { new SelectListItem { Value = "1", Text = "Role1" } }
        //    };
        //    _controller.ModelState.AddModelError("SelectedRoleId", "Required");

        //    // Act
        //    var result = await _controller.SaveChosenRoleAsync(model);

        //    // Assert
        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    var returnedModel = Assert.IsType<ChooseRoleModel>(viewResult.Model);
        //    Assert.Equal("Contributors/ChooseRole", viewResult.ViewName);
        //    Assert.Equal(model, returnedModel);
        //}

        //[Fact]
        //public async Task SaveChosenRoleAsync_ReturnsView_WhenRolesAreNull()
        //{
        //    // Arrange
        //    var model = new ChooseRoleModel
        //    {
        //        SelectedRoleId = "1",
        //        Roles = null
        //    };
        //    _controller.ModelState.AddModelError("SelectedRoleId", "Required");

        //    // Act
        //    var result = await _controller.SaveChosenRoleAsync(model);

        //    // Assert
        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    var returnedModel = Assert.IsType<ChooseRoleModel>(viewResult.Model);
        //    Assert.Null(returnedModel.Roles);
        //}

    }
}
