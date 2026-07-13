using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.OrganisationRole;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Workflows;
using HNTAS.Web.UI.Workflows.Models;
using HNTAS.Web.UI.Workflows.Models.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class AddOrganisationUserControllerTests
    {
        private readonly Mock<ILogger<AddOrganisationUserController>> _mockLogger;
        private readonly Mock<ISessionHelper> _mockSessionHelper;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IOrganisationUserService> _mockOrganisationUserService;
        private readonly Mock<IInvitationService> _mockInitiativeService;
        private readonly Mock<IInvitationTokenService> _mockInvitationTokenService;
        private readonly Mock<IWorkflowManager> _mockWorkflowManager;
        private readonly Mock<ITempDataProvider> _mockTempDataProvider;
        private TempDataDictionary? _mockTempData;

        public AddOrganisationUserControllerTests()
        {
            _mockLogger = new Mock<ILogger<AddOrganisationUserController>>();
            _mockSessionHelper = new Mock<ISessionHelper>();
            _mockUserService = new Mock<IUserService>();
            _mockOrganisationUserService = new Mock<IOrganisationUserService>();
            _mockInitiativeService = new Mock<IInvitationService>();
            _mockInvitationTokenService = new Mock<IInvitationTokenService>();
            _mockWorkflowManager = new Mock<IWorkflowManager>();            
            _mockTempDataProvider = new Mock<ITempDataProvider>();
        }

        private AddOrganisationUserController CreateController()
        {
            var httpContext = new DefaultHttpContext();
            _mockTempData = new TempDataDictionary(httpContext, _mockTempDataProvider.Object);
            var controller = new AddOrganisationUserController(
                _mockWorkflowManager.Object,
                _mockSessionHelper.Object,
                _mockUserService.Object,
                _mockLogger.Object,
                _mockOrganisationUserService.Object,
                _mockInitiativeService.Object,
                _mockInvitationTokenService.Object)
            { TempData = _mockTempData };

            
            httpContext.Session = new MockHttpSession();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        #region AddEmailAddress Tests
        [Fact]
        public void AddEmailAddress_ReturnsViewResult_WithCorrectViewNameAndModel()
        {
            // Arrange
            var controller = CreateController();
            var expectedModel = new AddUserEmailAddressModel { EmailAddress = "test@example.com" };
            var workflowModel = new AddOrganisationUserWorkflowModel
            {
                AddUserEmailAddressModel = expectedModel
            };

            _mockWorkflowManager
                .Setup(x => x.GetState<AddOrganisationUserWorkflowModel>())
                .Returns(new WorkflowState<AddOrganisationUserWorkflowModel> { Data = workflowModel });

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("Test Organisation");

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "ChangeOrganisationUser" && ctx.Controller == "UserManagement")))
                .Returns("UserManagement/ChangeOrganisationUser");
            controller.Url = urlHelperMock.Object;

            // Act
            var result = controller.AddEmailAddress();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Contributor/AddEmailAddress", viewResult.ViewName);
            Assert.IsType<AddUserEmailAddressModel>(viewResult.Model);
            Assert.Equal(expectedModel.EmailAddress, ((AddUserEmailAddressModel)viewResult.Model).EmailAddress);
            Assert.Equal("Test Organisation", controller.ViewBag.OrganisationName);
        }
        #endregion

        #region SaveEmailAddress Tests
        [Fact]
        public async Task SaveEmailAddress_WithValidEmail_UpdatesWorkflowAndRedirects()
        {
            // Arrange
            var controller = CreateController();
            var model = new AddUserEmailAddressModel { EmailAddress = "newuser@example.com" };

            _mockUserService
                .Setup(x => x.IsRpUserAsync(model.EmailAddress))
                .ReturnsAsync(false);

            _mockUserService
                .Setup(x => x.IsActiveUserAsync(model.EmailAddress))
                .ReturnsAsync(false);

            var workflowState = new WorkflowState<AddOrganisationUserWorkflowModel>
            {
                Data = new AddOrganisationUserWorkflowModel
                {
                    AddUserEmailAddressModel = new AddUserEmailAddressModel()
                }
            };

            _mockWorkflowManager.Setup(w => w.GetState<AddOrganisationUserWorkflowModel>())
                               .Returns(workflowState);

            // Act
            var result = await controller.SaveEmailAddress(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ContactDetails", redirectResult.ActionName);

            _mockWorkflowManager.Verify(m =>
                m.SaveState(It.IsAny<WorkflowState<AddOrganisationUserWorkflowModel>>()),
                Times.Once);
        }

        [Fact]
        public async Task SaveEmailAddress_WithInvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var controller = CreateController();
            var model = new AddUserEmailAddressModel { EmailAddress = "invalid-email" };
            controller.ModelState.AddModelError("EmailAddress", "Invalid email format.");

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "AddContributor" && ctx.Controller == "UserManagement")))
                .Returns("UserManagement/AddContributor");
            controller.Url = urlHelperMock.Object;

            // Act
            var result = await controller.SaveEmailAddress(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Contributor/AddEmailAddress", viewResult.ViewName);
            Assert.Same(model, viewResult.Model);
            Assert.False(controller.ModelState.IsValid);

            _mockUserService.Verify(x => x.IsRpUserAsync(It.IsAny<string>()), Times.Never);
            _mockUserService.Verify(x => x.IsActiveUserAsync(It.IsAny<string>()), Times.Never);
            _mockWorkflowManager.Verify(m =>
                m.SaveState(It.IsAny<WorkflowState<AddOrganisationUserWorkflowModel>>()),
                Times.Never);
        }

        [Fact]
        public async Task SaveEmailAddress_WhenUserIsRpUser_AddsModelErrorAndReturnsView()
        {
            // Arrange
            var controller = CreateController();
            var model = new AddUserEmailAddressModel { EmailAddress = "rpuser@example.com" };

            _mockUserService
                .Setup(x => x.IsRpUserAsync(model.EmailAddress))
                .ReturnsAsync(true);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "AddContributor" && ctx.Controller == "UserManagement")))
                .Returns("UserManagement/AddContributor");
            controller.Url = urlHelperMock.Object;

            // Act
            var result = await controller.SaveEmailAddress(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Contributor/AddEmailAddress", viewResult.ViewName);
            Assert.Same(model, viewResult.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(nameof(model.EmailAddress)));
            Assert.Equal("This user is already registered as a Responsible Party (RP). Go back and use Add an existing user to give them access.",
                controller.ModelState[nameof(model.EmailAddress)].Errors[0].ErrorMessage);

            _mockUserService.Verify(x => x.IsRpUserAsync(model.EmailAddress), Times.Once);
            _mockUserService.Verify(x => x.IsActiveUserAsync(It.IsAny<string>()), Times.Never);
            _mockWorkflowManager.Verify(m =>
                m.SaveState(It.IsAny<WorkflowState<AddOrganisationUserWorkflowModel>>()),
                Times.Never);
        }

        [Fact]
        public async Task SaveEmailAddress_WhenUserIsActiveUser_AddsModelErrorAndReturnsView()
        {
            // Arrange
            var controller = CreateController();
            var model = new AddUserEmailAddressModel { EmailAddress = "existinguser@example.com" };

            _mockUserService
                .Setup(x => x.IsRpUserAsync(model.EmailAddress))
                .ReturnsAsync(false);

            _mockUserService
                .Setup(x => x.IsActiveUserAsync(model.EmailAddress))
                .ReturnsAsync(true);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "AddContributor" && ctx.Controller == "UserManagement")))
                .Returns("UserManagement/AddContributor");
            controller.Url = urlHelperMock.Object;

            // Act
            var result = await controller.SaveEmailAddress(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Contributor/AddEmailAddress", viewResult.ViewName);
            Assert.Same(model, viewResult.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(nameof(model.EmailAddress)));
            Assert.Equal("This user already has an active account. Go back and use Add an existing user to give them access.",
                controller.ModelState[nameof(model.EmailAddress)].Errors[0].ErrorMessage);

            _mockUserService.Verify(x => x.IsRpUserAsync(model.EmailAddress), Times.Once);
            _mockUserService.Verify(x => x.IsActiveUserAsync(model.EmailAddress), Times.Once);
            _mockWorkflowManager.Verify(m =>
                m.SaveState(It.IsAny<WorkflowState<AddOrganisationUserWorkflowModel>>()),
                Times.Never);
        }

        [Fact]
        public async Task SaveEmailAddress_WhenIsRpUserReturnsNull_ContinuesValidation()
        {
            // Arrange
            var controller = CreateController();
            var model = new AddUserEmailAddressModel { EmailAddress = "newuser@example.com" };

            _mockUserService
                .Setup(x => x.IsRpUserAsync(model.EmailAddress))
                .ReturnsAsync((bool?)null);

            _mockUserService
                .Setup(x => x.IsActiveUserAsync(model.EmailAddress))
                .ReturnsAsync(false);

            var workflowState = new WorkflowState<AddOrganisationUserWorkflowModel>
            {
                Data = new AddOrganisationUserWorkflowModel
                {
                    AddUserEmailAddressModel = new AddUserEmailAddressModel()
                }
            };

            _mockWorkflowManager.Setup(w => w.GetState<AddOrganisationUserWorkflowModel>())
                               .Returns(workflowState);

            // Act
            var result = await controller.SaveEmailAddress(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ContactDetails", redirectResult.ActionName);

            _mockUserService.Verify(x => x.IsRpUserAsync(model.EmailAddress), Times.Once);
            _mockUserService.Verify(x => x.IsActiveUserAsync(model.EmailAddress), Times.Once);
            _mockWorkflowManager.Verify(m =>
                m.SaveState(It.IsAny<WorkflowState<AddOrganisationUserWorkflowModel>>()),
                Times.Once);
        }
        #endregion

        #region ContactDetails Tests
        [Fact]
        public void ContactDetails_ReturnsViewResult_WithCorrectViewNameAndModel()
        {
            // Arrange
            var controller = CreateController();
            var expectedModel = new ContributorContactDetailsModel
            {
                FirstName = "John",
                LastName = "Doe",
            };
            var workflowModel = new AddOrganisationUserWorkflowModel
            {
                ContributorContactDetailsModel = expectedModel
            };

            _mockWorkflowManager
                .Setup(x => x.GetState<AddOrganisationUserWorkflowModel>())
                .Returns(new WorkflowState<AddOrganisationUserWorkflowModel> { Data = workflowModel });

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("Test Organisation");

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "AddEmailAddress")))
                .Returns("AddEmailAddress");
            controller.Url = urlHelperMock.Object;

            // Act
            var result = controller.ContactDetails();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Contributor/ContactDetails", viewResult.ViewName);
            var model = Assert.IsType<ContributorContactDetailsModel>(viewResult.Model);
            Assert.Equal(expectedModel.FirstName, model.FirstName);
            Assert.Equal(expectedModel.LastName, model.LastName);
            Assert.Equal("Test Organisation", controller.ViewBag.OrganisationName);
        }

        [Fact]
        public void ContactDetails_ReturnsViewResult_WithNewModel_WhenWorkflowModelIsNull()
        {
            // Arrange
            var controller = CreateController();
            var workflowModel = new AddOrganisationUserWorkflowModel
            {
                ContributorContactDetailsModel = null
            };

            _mockWorkflowManager
                .Setup(x => x.GetState<AddOrganisationUserWorkflowModel>())
                .Returns(new WorkflowState<AddOrganisationUserWorkflowModel> { Data = workflowModel });

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("Test Organisation");

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "AddEmailAddress")))
                .Returns("AddEmailAddress");
            controller.Url = urlHelperMock.Object;

            // Act
            var result = controller.ContactDetails();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Contributor/ContactDetails", viewResult.ViewName);
            var model = Assert.IsType<ContributorContactDetailsModel>(viewResult.Model);
            Assert.Null(model.FirstName);
            Assert.Null(model.LastName);
        }
        #endregion

        #region SaveContactDetails Tests
        [Fact]
        public void SaveContactDetails_WithValidModel_TrimsNamesUpdatesWorkflowAndRedirects()
        {
            // Arrange
            var controller = CreateController();
            var model = new ContributorContactDetailsModel
            {
                FirstName = "  John  ",
                LastName = "  Doe  ",
            };

            var workflowState = new WorkflowState<AddOrganisationUserWorkflowModel>
            {
                Data = new AddOrganisationUserWorkflowModel
                {
                    ContributorContactDetailsModel = new ContributorContactDetailsModel()
                }
            };

            _mockWorkflowManager.Setup(w => w.GetState<AddOrganisationUserWorkflowModel>())
                               .Returns(workflowState);

            // Act
            var result = controller.SaveContactDetails(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AssignRole", redirectResult.ActionName);
            Assert.Equal("John", model.FirstName);
            Assert.Equal("Doe", model.LastName);

            _mockWorkflowManager.Verify(m =>
                m.SaveState(It.IsAny<WorkflowState<AddOrganisationUserWorkflowModel>>()),
                Times.Once);
        }

        [Fact]
        public void SaveContactDetails_WithInvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var controller = CreateController();
            var model = new ContributorContactDetailsModel
            {
                FirstName = "",
                LastName = "Doe",
            };
            controller.ModelState.AddModelError("FirstName", "First name is required.");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("Test Organisation");

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "ContactDetails")))
                .Returns("ContactDetails");
            controller.Url = urlHelperMock.Object;

            // Act
            var result = controller.SaveContactDetails(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Contributor/ContactDetails", viewResult.ViewName);
            Assert.Same(model, viewResult.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.Equal("Test Organisation", controller.ViewBag.OrganisationName);

            _mockWorkflowManager.Verify(m =>
                m.SaveState(It.IsAny<WorkflowState<AddOrganisationUserWorkflowModel>>()),
                Times.Never);
        }

        #endregion

        #region AssignRole Tests

        [Fact]
        public async Task AssignRole_ReturnsViewResult_WithCorrectViewNameAndModel()
        {
            // Arrange
            var controller = CreateController();
            var contactDetails = new ContributorContactDetailsModel
            {
                FirstName = "John",
                LastName = "Doe"
            };
            var workflowModel = new AddOrganisationUserWorkflowModel
            {
                ContributorContactDetailsModel = contactDetails,
                RoleAssignmentModel = null
            };

            _mockWorkflowManager
                .Setup(x => x.GetState<AddOrganisationUserWorkflowModel>())
                .Returns(new WorkflowState<AddOrganisationUserWorkflowModel> { Data = workflowModel });

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("Test Organisation");

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "ContactDetails")))
                .Returns("ContactDetails");
            controller.Url = urlHelperMock.Object;

            // Act
            var result = await controller.AssignRole();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Contributor/AssignRole", viewResult.ViewName);
            var model = Assert.IsType<RoleAssignmentModel>(viewResult.Model);
            Assert.NotNull(model.AvailableRoles);
            Assert.Single(model.AvailableRoles);
            Assert.Equal("NetworkManager", model.AvailableRoles[0].Value);
            Assert.Equal("Assign as a Network manager", model.AvailableRoles[0].Text);
            Assert.Equal("John Doe", model.InvitedUserName);
            Assert.Equal("Test Organisation", controller.ViewBag.OrganisationName);
        }       

        #endregion

        #region SaveAssignRole Tests

        [Fact]
        public async Task SaveAssignRole_WithValidModel_AddsInvitedUserAndRedirects()
        {
            // Arrange
            var controller = CreateController();
            var model = new RoleAssignmentModel
            {
                SelectedRoleName = "Coordinator",
                AvailableRoles = new List<SelectItemOption>()
            };

            var workflowModel = new AddOrganisationUserWorkflowModel
            {
                AddUserEmailAddressModel = new AddUserEmailAddressModel { EmailAddress = "test@example.com" },
                ContributorContactDetailsModel = new ContributorContactDetailsModel
                {
                    FirstName = "John",
                    LastName = "Doe"
                }
            };

            _mockWorkflowManager
                .Setup(x => x.GetState<AddOrganisationUserWorkflowModel>())
                .Returns(new WorkflowState<AddOrganisationUserWorkflowModel> { Data = workflowModel });

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("user-123");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationId))
                .Returns("org-123");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("Test Organisation");

            _mockInitiativeService
                .Setup(x => x.AddInvitedUserAsync(It.IsAny<string>(), It.IsAny<AddInvitationRequest>()))
                .ReturnsAsync("invitation-123");

            _mockInvitationTokenService
                .Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("test-token");

            _mockInitiativeService
                .Setup(x => x.SendInvitationEmailAsync(It.IsAny<string>(), It.IsAny<SendInvitationEmailRequest>()))
                .Returns(Task.CompletedTask);

            var workflowState = new WorkflowState<AddOrganisationUserWorkflowModel>
            {
                Data = workflowModel
            };

            _mockWorkflowManager.Setup(w => w.GetState<AddOrganisationUserWorkflowModel>())
                               .Returns(workflowState);

            // Act
            var result = await controller.SaveAssignRole(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("RoleAssignmentConfirmation", redirectResult.ActionName);

            _mockInitiativeService.Verify(x => x.AddInvitedUserAsync(
                "user-123",
                It.Is<AddInvitationRequest>(r =>
                    r.EmailAddress == "test@example.com" &&
                    r.FirstName == "John" &&
                    r.LastName == "Doe" &&
                    r.OrgId == "org-123" &&
                    r.Status == InvitationStatus.Invited
                )), Times.Once);

            _mockInitiativeService.Verify(x => x.SendInvitationEmailAsync(
                "invitation-123",
                It.IsAny<SendInvitationEmailRequest>()), Times.Once);

            _mockWorkflowManager.Verify(m =>
                m.SaveState(It.IsAny<WorkflowState<AddOrganisationUserWorkflowModel>>()),
                Times.Once);
        }

        [Fact]
        public async Task SaveAssignRole_WithInvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var controller = CreateController();
            var model = new RoleAssignmentModel
            {
                SelectedRoleName = null
            };
            controller.ModelState.AddModelError("SelectedRoleName", "Please select role.");

            var workflowModel = new AddOrganisationUserWorkflowModel
            {
                ContributorContactDetailsModel = new ContributorContactDetailsModel
                {
                    FirstName = "John",
                    LastName = "Doe"
                }
            };

            _mockWorkflowManager
                .Setup(x => x.GetState<AddOrganisationUserWorkflowModel>())
                .Returns(new WorkflowState<AddOrganisationUserWorkflowModel> { Data = workflowModel });

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("Test Organisation");

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "AssignRole")))
                .Returns("AssignRole");
            controller.Url = urlHelperMock.Object;

            // Act
            var result = await controller.SaveAssignRole(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Contributor/AssignRole", viewResult.ViewName);
            Assert.Same(model, viewResult.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.Single(model.AvailableRoles);
            Assert.Equal("John Doe", model.InvitedUserName);
            Assert.Equal("Test Organisation", controller.ViewBag.OrganisationName);

            _mockInitiativeService.Verify(x => x.AddInvitedUserAsync(
                It.IsAny<string>(),
                It.IsAny<AddInvitationRequest>()), Times.Never);
        }

        [Fact]
        public async Task SaveAssignRole_WhenWorkflowStateIsNull_LogsErrorAndRedirects()
        {
            // Arrange
            var controller = CreateController();
            var model = new RoleAssignmentModel
            {
                SelectedRoleName = "Coordinator"
            };

            _mockWorkflowManager
                .Setup(x => x.GetState<AddOrganisationUserWorkflowModel>())
                .Returns((WorkflowState<AddOrganisationUserWorkflowModel>)null);

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("user-123");

            // Act
            var result = await controller.SaveAssignRole(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AssignRole", redirectResult.ActionName);
            Assert.Equal("Unable to submit your details. Please try again later.", controller.TempData["ErrorMessage"]);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Workflow state or data is null")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SaveAssignRole_WhenAddInvitedUserReturnsNull_SetsErrorAndRedirects()
        {
            // Arrange
            var controller = CreateController();
            var model = new RoleAssignmentModel
            {
                SelectedRoleName = "Coordinator"
            };

            var workflowModel = new AddOrganisationUserWorkflowModel
            {
                AddUserEmailAddressModel = new AddUserEmailAddressModel { EmailAddress = "test@example.com" },
                ContributorContactDetailsModel = new ContributorContactDetailsModel
                {
                    FirstName = "John",
                    LastName = "Doe"
                }
            };

            _mockWorkflowManager
                .Setup(x => x.GetState<AddOrganisationUserWorkflowModel>())
                .Returns(new WorkflowState<AddOrganisationUserWorkflowModel> { Data = workflowModel });

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("user-123");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationId))
                .Returns("org-123");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("Test Organisation");

            _mockInitiativeService
                .Setup(x => x.AddInvitedUserAsync(It.IsAny<string>(), It.IsAny<AddInvitationRequest>()))
                .ReturnsAsync((string)null);

            // Act
            var result = await controller.SaveAssignRole(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AssignRole", redirectResult.ActionName);
            Assert.Equal("There was an error submitting your details. Please try again later.", controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task SaveAssignRole_WhenExceptionThrown_LogsErrorAndRedirects()
        {
            // Arrange
            var controller = CreateController();
            var model = new RoleAssignmentModel
            {
                SelectedRoleName = "Coordinator"
            };

            var workflowModel = new AddOrganisationUserWorkflowModel
            {
                AddUserEmailAddressModel = new AddUserEmailAddressModel { EmailAddress = "test@example.com" },
                ContributorContactDetailsModel = new ContributorContactDetailsModel
                {
                    FirstName = "John",
                    LastName = "Doe"
                }
            };

            _mockWorkflowManager
                .Setup(x => x.GetState<AddOrganisationUserWorkflowModel>())
                .Returns(new WorkflowState<AddOrganisationUserWorkflowModel> { Data = workflowModel });

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("user-123");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationId))
                .Returns("org-123");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("Test Organisation");

            var exception = new Exception("Test exception");
            _mockInitiativeService
                .Setup(x => x.AddInvitedUserAsync(It.IsAny<string>(), It.IsAny<AddInvitationRequest>()))
                .ThrowsAsync(exception);

            // Act
            var result = await controller.SaveAssignRole(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AssignRole", redirectResult.ActionName);
            Assert.Equal("There was an error submitting your details. Please try again later.", controller.TempData["ErrorMessage"]);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error submitting new contributor details")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        //[Fact]
        //public async Task SaveAssignRole_SetsTempDataCorrectly()
        //{
        //    // Arrange
        //    var controller = CreateController();
        //    var model = new RoleAssignmentModel
        //    {
        //        SelectedRoleName = "Network manager"
        //    };

        //    var workflowModel = new AddOrganisationUserWorkflowModel
        //    {
        //        AddUserEmailAddressModel = new AddUserEmailAddressModel { EmailAddress = "test@example.com" },
        //        ContributorContactDetailsModel = new ContributorContactDetailsModel
        //        {
        //            FirstName = "Jane",
        //            LastName = "Smith"
        //        }
        //    };

        //    _mockWorkflowManager
        //        .Setup(x => x.GetState<AddOrganisationUserWorkflowModel>())
        //        .Returns(new WorkflowState<AddOrganisationUserWorkflowModel> { Data = workflowModel });

        //    _mockSessionHelper
        //        .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
        //        .Returns("user-123");

        //    _mockSessionHelper
        //        .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationId))
        //        .Returns("org-123");

        //    _mockSessionHelper
        //        .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
        //        .Returns("My Organisation");

        //    _mockInitiativeService
        //        .Setup(x => x.AddInvitedUserAsync(It.IsAny<string>(), It.IsAny<AddInvitationRequest>()))
        //        .ReturnsAsync("invitation-123");

        //    _mockInvitationTokenService
        //        .Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>()))
        //        .Returns("test-token");

        //    var workflowState = new WorkflowState<AddOrganisationUserWorkflowModel>
        //    {
        //        Data = workflowModel
        //    };

        //    _mockWorkflowManager.Setup(w => w.GetState<AddOrganisationUserWorkflowModel>())
        //                       .Returns(workflowState);

        //    // Act
        //    var result = await controller.SaveAssignRole(model);

        //    // Assert
        //    Assert.Equal("Jane Smith", controller.TempData["UserName"]);
        //    Assert.Equal("My Organisation", controller.TempData["OrganisationName"]);
        //    Assert.Equal("Network Manager", controller.TempData["AssignedRole"]);
        //}

        #endregion

        #region RoleAssignmentConfirmation Tests

        [Fact]
        public void RoleAssignmentConfirmation_ReturnsViewWithCorrectData()
        {
            // Arrange
            var controller = CreateController();
            _mockTempData["UserName"] = "John Doe";
            _mockTempData["AssignedRole"] = "HNTAS Coordinator";
            _mockTempData["OrganisationName"] = "Test Organisation";

            // Act
            var result = controller.RoleAssignmentConfirmation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Contributor/RoleAssignmentConfirmation", viewResult.ViewName);
            Assert.Equal("John Doe", controller.ViewData["FullName"]);
            Assert.Equal("HNTAS Coordinator", controller.ViewData["AssignedRole"]);
            Assert.Equal("Test Organisation", controller.ViewData["OrganisationName"]);
        }       

        #endregion
    }
}
