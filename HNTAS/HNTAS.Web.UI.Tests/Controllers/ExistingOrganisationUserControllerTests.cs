using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Extensions;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.OrganisationRole;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Workflows;
using HNTAS.Web.UI.Workflows.Enums;
using HNTAS.Web.UI.Workflows.Models;
using HNTAS.Web.UI.Workflows.Models.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class ExistingOrganisationUserControllerTests
    {
        private readonly Mock<IWorkflowManager> _mockWorkflowManager;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IInvitationService> _mockInvitationService;
        private readonly Mock<IOrganisationUserService> _mockOrganisationUserService;
        private readonly Mock<IInvitationTokenService> _mockInvitationTokenService;
        private readonly Mock<ISessionHelper> _mockSessionHelper;
        private readonly Mock<ILogger<ExistingOrganisationUserController>> _mockLogger;

        private readonly ExistingOrganisationUserController _controller;

        public ExistingOrganisationUserControllerTests()
        {
            _mockWorkflowManager = new Mock<IWorkflowManager>();
            _mockUserService = new Mock<IUserService>();
            _mockInvitationService = new Mock<IInvitationService>();
            _mockOrganisationUserService = new Mock<IOrganisationUserService>();
            _mockInvitationTokenService = new Mock<IInvitationTokenService>();
            _mockSessionHelper = new Mock<ISessionHelper>();
            _mockLogger = new Mock<ILogger<ExistingOrganisationUserController>>();
            _controller = CreateController();

        }

        private ExistingOrganisationUserController CreateController()
        {
            var controller = new ExistingOrganisationUserController(
                _mockSessionHelper.Object,
                _mockWorkflowManager.Object,
                _mockUserService.Object,
                _mockLogger.Object,
                _mockInvitationService.Object,
                _mockOrganisationUserService.Object,
                _mockInvitationTokenService.Object
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
        public async Task ChooseUser_Get_ReturnsViewResult()
        {
            // Arrange
            _controller.Url = SetUpBackLink("ChangeOrganisationUser", "UserManagement").Object;

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModelId");

            _mockUserService.Setup(u => u.GetUsersByOrganisationIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<UserResponse>() { new UserResponse { Roles = new List<UserRole> { UserRole.NetworkManager }, Id = "test", FirstName = "test", FullName = "fullname" } });
            
            _mockUserService.Setup(u => u.GetUserRolesAsync())
                .ReturnsAsync(new List<EnumItemResponse> { new EnumItemResponse { Name = "fullname", Description = "desc" } });
            
            _mockWorkflowManager
                .Setup(w => w.GetState<AddExistingOrganisationUserWorkflowModel>())
                .Returns(new WorkflowState<AddExistingOrganisationUserWorkflowModel>
                {
                    Data = new AddExistingOrganisationUserWorkflowModel
                    {
                        CurrentStep = ExistingOrganisationUserWorkflowStep.ChooseRole,
                        ChooseUserModel = new ChooseUserModel
                        {                            
                            Users = new List<SelectItemOption> { new SelectItemOption { Text = "text", Hint = "hint" } }
                        }
                    }
                });
            // Act
            var result = await _controller.ChooseUser();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ChooseUserModel>(viewResult.ViewData.Model);
            Assert.Equal("ChooseUser", viewResult.ViewName);
        }

        [Fact]
        public void SaveChooseUser_Post_RedirectToActionResult()
        {
            var model = new ChooseUserModel
            {
                // Add necessary properties
            };

            var workflowState = new WorkflowState<AddExistingOrganisationUserWorkflowModel>
            {
                Data = new AddExistingOrganisationUserWorkflowModel()
            };
            
            _mockWorkflowManager
                .Setup(w => w.GetState<AddExistingOrganisationUserWorkflowModel>())
                .Returns(workflowState);
            
            _mockWorkflowManager
                .Setup(w => w.SaveState(It.IsAny<WorkflowState<AddExistingOrganisationUserWorkflowModel>>()))
                .Verifiable();

            var result = _controller.SaveChooseUser(model);

            Assert.IsType<RedirectToActionResult>(result);

            // Verify SaveState was called
            _mockWorkflowManager.Verify(w => w.SaveState(It.IsAny<WorkflowState<AddExistingOrganisationUserWorkflowModel>>()), Times.Once);
        }

        [Fact]
        public void SaveChooseUser_Post_InvalidModelState_ReturnsView()
        {
            var model = new ChooseUserModel
            {
                // Add necessary properties
            };

            var workflowState = new WorkflowState<AddExistingOrganisationUserWorkflowModel>
            {
                Data = new AddExistingOrganisationUserWorkflowModel()
            };

            _controller.ModelState.AddModelError("SelectedUserId", "Required");

            var result = _controller.SaveChooseUser(model);

            Assert.IsType<ViewResult>(result);

            // Verify SaveState was called
            _mockWorkflowManager.Verify(w => w.SaveState(It.IsAny<WorkflowState<AddExistingOrganisationUserWorkflowModel>>()), Times.Never);
        }

        [Fact]
        public async Task AssignRole_Get_ReturnsViewResult()
        {
            _controller.Url = SetUpBackLink("ChooseUser", "ExistingOrganisationUserController").Object;

            _mockOrganisationUserService.Setup(s => s.GetResponsiblePartyDetails(It.IsAny<string>()))
                .ReturnsAsync(new UserResponse { FullName = "fullname"});

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationId))
                .Returns("orgId");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("orgName");

            var workflowState = new WorkflowState<AddExistingOrganisationUserWorkflowModel>
            {
                Data = new AddExistingOrganisationUserWorkflowModel()
            };

            _mockWorkflowManager
                .Setup(w => w.GetState<AddExistingOrganisationUserWorkflowModel>())
                .Returns(workflowState);

            _mockUserService.Setup(u => u.GetUserById(It.IsAny<string>()))
                .ReturnsAsync(new UserResponse { FullName = "fullname" });

            var result = await _controller.AssignRole();
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Contributor/AssignRole", viewResult.ViewName);
        }

        [Fact]
        public async Task AssignRole_Get_BadRequest()
        {
            _controller.Url = SetUpBackLink("ChooseUser", "ExistingOrganisationUserController").Object;

            _mockOrganisationUserService.Setup(s => s.GetResponsiblePartyDetails(It.IsAny<string>()))
                .ReturnsAsync((UserResponse)null!);

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationId))
                .Returns("orgId");            

            var result = await _controller.AssignRole();
            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);            
        }

        [Fact]
        public async Task SaveAssignRole_Post_RedirectToActionResult()
        {
            var workflowState = new WorkflowState<AddExistingOrganisationUserWorkflowModel>
            {
                Data = new AddExistingOrganisationUserWorkflowModel()
            };

            _mockWorkflowManager
                .Setup(w => w.GetState<AddExistingOrganisationUserWorkflowModel>())
                .Returns(workflowState);

            _mockWorkflowManager
                .Setup(w => w.SaveState(It.IsAny<WorkflowState<AddExistingOrganisationUserWorkflowModel>>()))
                .Verifiable();

            var result = await _controller.SaveAssignRole(new RoleAssignmentModel());
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task SaveAssignRole_Post_InvalidModelState_ReturnViewResult()
        {
            _controller.ModelState.AddModelError("SelectedRoleName", "Required");
            _controller.Url = SetUpBackLink("ChooseUser", "ExistingOrganisationUserController").Object;
            var workflowState = new WorkflowState<AddExistingOrganisationUserWorkflowModel>
            {
                Data = new AddExistingOrganisationUserWorkflowModel()
            };

            _mockWorkflowManager
                .Setup(w => w.GetState<AddExistingOrganisationUserWorkflowModel>())
                .Returns(workflowState);

            _mockOrganisationUserService.Setup(s => s.GetResponsiblePartyDetails(It.IsAny<string>()))
                .ReturnsAsync(new UserResponse { FullName = "fullname" });

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationId))
                .Returns("orgId");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("orgName");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModel_Id");

            _mockUserService.Setup(u => u.GetUserById(It.IsAny<string>()))
                .ReturnsAsync(new UserResponse { FullName = "fullname" });

            var result = await _controller.SaveAssignRole(new RoleAssignmentModel());
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void ReplaceUserRoleConfirmation_Get_ReturnsViewResult()
        {
            _controller.Url = SetUpBackLink("ChooseUser", "ExistingOrganisationUserController").Object;
            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("orgName");

            var result = _controller.ReplaceUserRoleConfirmation();
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task SaveReplaceUserRoleConfirmation_Post__ReplaceExistingRole_RedirectToActionResult()
        {
            var model = new ReplaceUserRoleViewModel
            {
                ReplaceExistingRole = "Yes",
            };
            var workflowState = new WorkflowState<AddExistingOrganisationUserWorkflowModel>
            {
                Data = new AddExistingOrganisationUserWorkflowModel()
            };

            _mockWorkflowManager
                .Setup(w => w.GetState<AddExistingOrganisationUserWorkflowModel>())
                .Returns(workflowState);

            _mockWorkflowManager
                .Setup(w => w.SaveState(It.IsAny<WorkflowState<AddExistingOrganisationUserWorkflowModel>>()))
                .Verifiable();

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationId))
                .Returns("orgId");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("orgName");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModel_Id");

            _mockUserService.Setup(u => u.GetUserById(It.IsAny<string>()))
                .ReturnsAsync(new UserResponse { FullName = "fullname", FirstName = "firstname", LastName = "lastname", EmailId = "test", Roles = new List<UserRole> { UserRole.NetworkManager} });

            _mockOrganisationUserService.Setup(s => s.GetResponsiblePartyDetails(It.IsAny<string>()))
                .ReturnsAsync(new UserResponse { FullName = "fullname" });

            _mockInvitationService.Setup(i => i.AddInvitedUserAsync(It.IsAny<string>(), It.IsAny<AddInvitationRequest>()))
                .ReturnsAsync("invitationId");

            var result = await _controller.SaveReplaceUserRoleConfirmation(model);
            Assert.NotNull(result);
            var resultVal = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("RoleAssignmentConfirmation", resultVal.ActionName);
        }

        [Fact]
        public async Task SaveReplaceUserRoleConfirmation_Post_NotReplaceExistingRole_RedirectToActionResult()
        {
            var model = new ReplaceUserRoleViewModel
            {
                ReplaceExistingRole = "No",
            };
            var workflowState = new WorkflowState<AddExistingOrganisationUserWorkflowModel>
            {
                Data = new AddExistingOrganisationUserWorkflowModel()
            };

            _mockWorkflowManager
                .Setup(w => w.GetState<AddExistingOrganisationUserWorkflowModel>())
                .Returns(workflowState);

            _mockWorkflowManager
                .Setup(w => w.SaveState(It.IsAny<WorkflowState<AddExistingOrganisationUserWorkflowModel>>()))
                .Verifiable();

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationId))
                .Returns("orgId");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("orgName");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModel_Id");

            _mockUserService.Setup(u => u.GetUserById(It.IsAny<string>()))
                .ReturnsAsync(new UserResponse { FullName = "fullname", FirstName = "firstname", LastName = "lastname", EmailId = "test", Roles = new List<UserRole> { UserRole.NetworkManager } });

            _mockOrganisationUserService.Setup(s => s.GetResponsiblePartyDetails(It.IsAny<string>()))
                .ReturnsAsync(new UserResponse { FullName = "fullname" });

            _mockInvitationService.Setup(i => i.AddInvitedUserAsync(It.IsAny<string>(), It.IsAny<AddInvitationRequest>()))
                .ReturnsAsync("invitationId");

            var result = await _controller.SaveReplaceUserRoleConfirmation(model);
            Assert.NotNull(result);
            var resultVal = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CannotContinue", resultVal.ActionName);
        }

        [Fact]
        public async Task SaveReplaceUserRoleConfirmation_Post_ThrowException_RedirectToActionResult()
        {
            var model = new ReplaceUserRoleViewModel
            {
                ReplaceExistingRole = "Yes",
            };
            var workflowState = new WorkflowState<AddExistingOrganisationUserWorkflowModel>
            {
                Data = new AddExistingOrganisationUserWorkflowModel()
            };

            _mockWorkflowManager
                .Setup(w => w.GetState<AddExistingOrganisationUserWorkflowModel>())
                .Returns(workflowState);

            _mockWorkflowManager
                .Setup(w => w.SaveState(It.IsAny<WorkflowState<AddExistingOrganisationUserWorkflowModel>>()))
                .Verifiable();

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationId))
                .Returns("orgId");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("orgName");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModel_Id");

            _mockUserService.Setup(u => u.GetUserById(It.IsAny<string>()))
                .ReturnsAsync(new UserResponse { FullName = "fullname", FirstName = "firstname", LastName = "lastname", EmailId = "test", Roles = new List<UserRole> { UserRole.NetworkManager } });

            _mockOrganisationUserService.Setup(s => s.GetResponsiblePartyDetails(It.IsAny<string>()))
                .Throws(new Exception());

            _mockInvitationService.Setup(i => i.AddInvitedUserAsync(It.IsAny<string>(), It.IsAny<AddInvitationRequest>()))
                .ReturnsAsync("invitationId");

            var result = await _controller.SaveReplaceUserRoleConfirmation(model);
            Assert.NotNull(result);
            var resultVal = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ReplaceUserRoleConfirmation", resultVal.ActionName);
        }

        [Fact]
        public async Task SaveReplaceUserRoleConfirmation_Post_NoInvitationId_RedirectToActionResult()
        {
            var model = new ReplaceUserRoleViewModel
            {
                ReplaceExistingRole = "Yes",
            };
            var workflowState = new WorkflowState<AddExistingOrganisationUserWorkflowModel>
            {
                Data = new AddExistingOrganisationUserWorkflowModel()
            };

            _mockWorkflowManager
                .Setup(w => w.GetState<AddExistingOrganisationUserWorkflowModel>())
                .Returns(workflowState);

            _mockWorkflowManager
                .Setup(w => w.SaveState(It.IsAny<WorkflowState<AddExistingOrganisationUserWorkflowModel>>()))
                .Verifiable();

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationId))
                .Returns("orgId");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("orgName");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModel_Id");

            _mockUserService.Setup(u => u.GetUserById(It.IsAny<string>()))
                .ReturnsAsync(new UserResponse { FullName = "fullname", FirstName = "firstname", LastName = "lastname", EmailId = "test", Roles = new List<UserRole> { UserRole.NetworkManager } });

            _mockOrganisationUserService.Setup(s => s.GetResponsiblePartyDetails(It.IsAny<string>()))
                .Throws(new Exception());

            _mockInvitationService.Setup(i => i.AddInvitedUserAsync(It.IsAny<string>(), It.IsAny<AddInvitationRequest>()))
                .ReturnsAsync((string)null!);

            var result = await _controller.SaveReplaceUserRoleConfirmation(model);
            Assert.NotNull(result);
            var resultVal = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ReplaceUserRoleConfirmation", resultVal.ActionName);
        }

        [Fact]
        public async Task SaveReplaceUserRoleConfirmation_Post_BadRequest()
        {
            var model = new ReplaceUserRoleViewModel
            {
                ReplaceExistingRole = "No",
            };
            var workflowState = new WorkflowState<AddExistingOrganisationUserWorkflowModel>
            {
                Data = new AddExistingOrganisationUserWorkflowModel()
            };

            _mockWorkflowManager
                .Setup(w => w.GetState<AddExistingOrganisationUserWorkflowModel>())
                .Returns(workflowState);

            _mockWorkflowManager
                .Setup(w => w.SaveState(It.IsAny<WorkflowState<AddExistingOrganisationUserWorkflowModel>>()))
                .Verifiable();

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationId))
                .Returns("orgId");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("orgName");

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModel_Id");

            _mockUserService.Setup(u => u.GetUserById(It.IsAny<string>()))
                .ReturnsAsync((UserResponse)null!);

            _mockOrganisationUserService.Setup(s => s.GetResponsiblePartyDetails(It.IsAny<string>()))
                .Throws(new Exception());

            _mockInvitationService.Setup(i => i.AddInvitedUserAsync(It.IsAny<string>(), It.IsAny<AddInvitationRequest>()))
                .ReturnsAsync((string)null!);

            var result = await _controller.SaveReplaceUserRoleConfirmation(model);
            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);            
        }

        [Fact]
        public async Task SaveReplaceUserRoleConfirmation_Post_InvalidModelState_ReturnsViewResult()
        {
            _controller.ModelState.AddModelError("ReplaceExistingRole", "Required");
            _controller.Url = SetUpBackLink("ChooseUser", "ExistingOrganisationUserController").Object;
            var model = new ReplaceUserRoleViewModel
            {
                ReplaceExistingRole = "No",
            };
            var workflowState = new WorkflowState<AddExistingOrganisationUserWorkflowModel>
            {
                Data = new AddExistingOrganisationUserWorkflowModel()
            };

            _mockWorkflowManager
                .Setup(w => w.GetState<AddExistingOrganisationUserWorkflowModel>())
                .Returns(workflowState);            

            var result = await _controller.SaveReplaceUserRoleConfirmation(model);
            Assert.NotNull(result);
            var resultVal = Assert.IsType<ViewResult>(result);
            Assert.Equal("ReplaceUserRoleConfirmation", resultVal.ViewName);
        }

        [Fact]
        public async Task SaveReplaceUserRoleConfirmation_Post_InvalidWorkFlowState_RedirectsToActionResult()
        {
            _controller.ModelState.AddModelError("ReplaceExistingRole", "Required");
            var model = new ReplaceUserRoleViewModel
            {
                ReplaceExistingRole = "No",
            };
            var workflowState = new WorkflowState<AddExistingOrganisationUserWorkflowModel>
            {
                Data = null!
            };

            _mockWorkflowManager
                .Setup(w => w.GetState<AddExistingOrganisationUserWorkflowModel>())
                .Returns(workflowState);

            var result = await _controller.SaveReplaceUserRoleConfirmation(model);
            Assert.NotNull(result);
            var resultVal = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ReplaceUserRoleConfirmation", resultVal.ActionName);
        }

        [Fact]
        public void CannotContinue_Get_ReturnsViewResult()
        {
            _controller.Url = SetUpBackLink("ReplaceUserRoleConfirmation", "ExistingOrganisationUserController").Object;
            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("orgName");

            var result = _controller.CannotContinue();
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void RoleAssignmentConfirmation_Get_ReturnsViewResult()
        {
            var result = _controller.RoleAssignmentConfirmation();
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }
    }
}
