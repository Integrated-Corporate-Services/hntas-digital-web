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

        [Fact(Skip = "// TODO(Sushree) - this test case doesn't pass yet, mocking needs to be fixed")]
        public async Task ChooseUserAsync_ReturnsView_WithError_WhenNoContributorsFound()
        {
            // Arrange
            var userId = "User123";
            var workflowState = new WorkflowState<AddExistingContributorWorkflowModel>
            {
                Data = new AddExistingContributorWorkflowModel
                {
                    ChooseContributorModel = new ChooseContributorModel()
                }
            };
            _mockWorkflowManager.Setup(w => w.GetState<AddExistingContributorWorkflowModel>()).Returns(workflowState);
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey)).Returns(userId);
            _mockUserService.Setup(u => u.GetRegisteredUsersAsync(userId)).ReturnsAsync(new List<UserResponse>()); // Mock API response as empty
            //_controller.Url = TestingUtility.SetUpBackLink("AddContributor", "UserManagement").Object;

            // Act
            var result = await _controller.ChooseUserAsync();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ChooseContributorModel>(viewResult.Model);

            Assert.True(viewResult.ViewData.ContainsKey("ErrorMessage"));
            Assert.Equal("No users found. Please contact support.", viewResult.ViewData["ErrorMessage"]);
            Assert.Empty(model.Contributors);
            _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No contributors found for the current user.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        }        

        [Fact]
        public async Task SaveChooseUserAsync_RedirectsToChooseHeatNetwork_WhenModelIsValid()
        {
            // Arrange
            var userId = "User123";
            var selectedContributorId = "user-1";
            //var contributors = new List<SelectListItem>
            //{
            //    new SelectListItem { Value = "C1", Text = "Contributor One" }
            //};
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                             .Returns(userId);

            _mockUserService.Setup(u => u.GetUserById(selectedContributorId))
                           .ReturnsAsync(TestingUtility.MockValid_UserService_GetUserById(selectedContributorId));
            _mockUserService.Setup(u => u.GetRegisteredUsersAsync(userId)).ReturnsAsync(TestingUtility.MockValid_UserService_GetRegisteredUsers(userId));

            var workflowState = new WorkflowState<AddExistingContributorWorkflowModel>
            {
                Data = new AddExistingContributorWorkflowModel
                {
                    ChooseContributorModel = new ChooseContributorModel()
                }
            };

            _mockWorkflowManager.Setup(w => w.GetState<AddExistingContributorWorkflowModel>())
                               .Returns(workflowState);

            var model = new ChooseContributorModel
            {
                SelectedContributorId = selectedContributorId,
                SelectedContributorEmail = "testuser@test.com",
                Contributors = new List<SelectItemOption>() { 
                    new SelectItemOption() {
                        Value = "test",
                        Text = "test",
                        Hint = "test"
                    }
                }
            };

            _controller.Url = Mock.Of<IUrlHelper>();

            // Mock GetContributorSelectListAsync via partial mock or helper
            _controller.GetType().GetMethod("GetContributorSelectListAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                      ?.Invoke(_controller, new object[] { userId });

            // Act
            var result = await _controller.SaveChooseUserAsync(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ChooseHeatNetwork", redirectResult.ActionName);            
            _mockWorkflowManager.Verify(m =>
                m.SaveState(It.IsAny<WorkflowState<AddExistingContributorWorkflowModel>>()),
                Times.Exactly(2));

        }

        //[Fact]
        //public async Task SaveChooseUserAsync_ShouldPopulateContributors_WhenApiReturnsData()
        //{
        //    // Arrange
        //    var userId = "User123";
        //    var selectedContributorId = "test";

        //    var model = new ChooseContributorModel
        //    {
        //        SelectedContributorId = selectedContributorId,
        //        SelectedContributorEmail = "testuser@test.com"
        //    };

        //    //var contributorsFromApi = new List<UserDto>
        //    //{
        //    //    new UserDto { Id = "test", EmailId = "testuser@test.com" }
        //    //};

        //    _mockUserService.Setup(s => s.GetRegisteredUsersAsync(userId))
        //                   .ReturnsAsync(contributorsFromApi);

        //    var mockSessionHelper = new Mock<ISessionHelper>();
        //    mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
        //                     .Returns(userId);

        //    // Mock workflow state
        //    var workflowState = new WorkflowState<AddExistingContributorWorkflowModel>
        //    {
        //        Data = new AddExistingContributorWorkflowModel
        //        {
        //            ChooseContributorModel = new ChooseContributorModel()
        //        }
        //    };

        //    _mockWorkflowManager.Setup(w => w.GetState<AddExistingContributorWorkflowModel>())
        //                       .Returns(workflowState);

        //    _controller.Url = Mock.Of<IUrlHelper>();

        //    // Act
        //    var result = await _controller.SaveChooseUserAsync(model);

        //    // Assert
        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    var returnedModel = Assert.IsType<ChooseContributorModel>(viewResult.Model);

        //    Assert.NotNull(returnedModel.Contributors);
        //    Assert.Single(returnedModel.Contributors);
        //    Assert.Equal("test", returnedModel.Contributors.First().Value);
        //    Assert.Equal("testuser@test.com", returnedModel.Contributors.First().Text);
        //}
    }
}
