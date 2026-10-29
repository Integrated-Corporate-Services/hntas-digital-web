using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.NetworkLeads;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class NetworkLeadsControllerTests
    {
        private readonly Mock<ILogger<NetworkLeadsController>> _logger = new();
        private readonly Mock<ISessionHelper> _sessionHelper = new();
        private readonly Mock<IInvitationService> _invitationService = new();
        private readonly Mock<IUserService> _userService = new();
        private readonly Mock<IInvitationTokenService> _invitationTokenService = new();
        private readonly Mock<ITempDataProvider> _tempDataProvider = new();

        private NetworkLeadsController CreateController()
        {
            var httpContext = new DefaultHttpContext
            {
                Session = new MockHttpSession()
            };

            var controller = new NetworkLeadsController(
                _logger.Object,
                _sessionHelper.Object,
                _invitationService.Object,
                _userService.Object,
                _invitationTokenService.Object)
            {
                TempData = new TempDataDictionary(httpContext, _tempDataProvider.Object),
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };

            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("/mocked-url");
            controller.Url = urlHelper.Object;

            return controller;
        }

        [Fact]
        public async Task ManageLeads_ReturnsMappedNetworkLeads()
        {
            var controller = CreateController();
            _sessionHelper.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
                .Returns("Test Organisation");
            _sessionHelper.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("user-id");
            _userService.Setup(x => x.GetNetworkLeads("user-id")).ReturnsAsync(new List<InvitedUserResponse>
            {
                new() { FullName = "Jane Doe", Email = "jane@example.com", Status = InvitationStatus.Invited }
            });

            var result = Assert.IsType<ViewResult>(await controller.ManageLeads());

            var model = Assert.IsType<ManageLeadsModel>(result.Model);
            var lead = Assert.Single(model.NetworkLeads);
            Assert.Equal("Jane Doe", lead.Name);
            Assert.Equal("jane@example.com", lead.EmailAddress);
            Assert.Equal(InvitationStatus.Invited.ToString(), lead.Status);
            Assert.Equal("Test Organisation", controller.ViewBag.OrganisationName);
        }

        [Fact]
        public void Start_ClearsInvitationSessionAndRedirects()
        {
            var controller = CreateController();

            var result = Assert.IsType<RedirectToActionResult>(controller.Start());

            Assert.Equal(nameof(NetworkLeadsController.InviteLeadByEmail), result.ActionName);
            _sessionHelper.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.NewLeadInviteEmailViewModelSessionKey), Times.Once);
            _sessionHelper.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.NewLeadDetailsViewModelSessionKey), Times.Once);
            _sessionHelper.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.SameOrganisationViewModelSessionKey), Times.Once);
        }

        [Fact]
        public void InviteLeadByEmail_UsesSavedModelOrCreatesNewModel()
        {
            var controller = CreateController();
            var savedModel = new InviteLeadViewModel { EmailAddress = "saved@example.com" };
            _sessionHelper.Setup(x => x.GetFromSession<InviteLeadViewModel>(It.IsAny<HttpContext>(), SessionKeys.NewLeadInviteEmailViewModelSessionKey))
                .Returns(savedModel);

            var result = Assert.IsType<ViewResult>(controller.InviteLeadByEmail());

            Assert.Same(savedModel, result.Model);
            Assert.True(controller.ViewBag.ShowBackButton);
            Assert.Equal("/mocked-url", controller.ViewBag.BackLinkUrl);
        }

        [Fact]
        public async Task SaveLeadEmail_WithInvalidModelState_ReturnsSameView()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("EmailAddress", "Invalid");
            var model = new InviteLeadViewModel { EmailAddress = "invalid" };

            var result = Assert.IsType<ViewResult>(await controller.SaveLeadEmailAsync(model));

            Assert.Equal(nameof(NetworkLeadsController.InviteLeadByEmail), result.ViewName);
            Assert.Same(model, result.Model);
            _userService.Verify(x => x.GetUserByEmailIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SaveLeadEmail_WithNewEmail_SavesModelAndRedirectsToDetails()
        {
            var controller = CreateController();
            var model = new InviteLeadViewModel { EmailAddress = "new@example.com" };
            _userService.Setup(x => x.GetUserByEmailIdAsync(model.EmailAddress)).ReturnsAsync((UserResponse?)null);

            var result = Assert.IsType<RedirectToActionResult>(await controller.SaveLeadEmailAsync(model));

            Assert.Equal(nameof(NetworkLeadsController.NewLeadDetails), result.ActionName);
            _sessionHelper.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.NewLeadInviteEmailViewModelSessionKey, model), Times.Once);
        }

        [Fact]
        public async Task SaveLeadEmail_WhenExistingResponsibleParty_ReturnsValidationError()
        {
            var controller = CreateController();
            var model = new InviteLeadViewModel { EmailAddress = "user@example.com" };
            _userService.Setup(x => x.GetUserByEmailIdAsync(model.EmailAddress)).ReturnsAsync(new UserResponse
            {
                Roles = new List<UserRole> { UserRole.ResponsibleParty }
            });

            var result = Assert.IsType<ViewResult>(await controller.SaveLeadEmailAsync(model));

            Assert.Equal(nameof(NetworkLeadsController.InviteLeadByEmail), result.ViewName);
            Assert.Equal("This user cannot be added as a Network Manager", controller.ModelState["EmailAddress"]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task SaveLeadEmail_WhenExistingNetworkManagerForOrganisation_ReturnsValidationError()
        {
            var controller = CreateController();
            var model = new InviteLeadViewModel { EmailAddress = "user@example.com" };
            _sessionHelper.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationId)).Returns("org-id");
            _userService.Setup(x => x.GetUserByEmailIdAsync(model.EmailAddress)).ReturnsAsync(new UserResponse
            {
                Roles = new List<UserRole> { UserRole.NetworkManager },
                ContributingOrganisations = new List<string> { "org-id" }
            });

            var result = Assert.IsType<ViewResult>(await controller.SaveLeadEmailAsync(model));

            Assert.Equal("This user is already a Network Manager for this organisation", controller.ModelState["EmailAddress"]!.Errors[0].ErrorMessage);
            Assert.Same(model, result.Model);
        }

        [Fact]
        public async Task NewLeadAlreadyExists_WhenSessionEmailIsMissing_RedirectsToManageLeads()
        {
            var controller = CreateController();

            var result = Assert.IsType<RedirectToActionResult>(await controller.NewLeadAlreadyExistsAsync());

            Assert.Equal(nameof(NetworkLeadsController.ManageLeads), result.ActionName);
            Assert.NotNull(controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task SaveNewLeadAlreadyExists_WhenCorrectPerson_SavesDetailsAndRedirects()
        {
            var controller = CreateController();
            var emailModel = new InviteLeadViewModel { EmailAddress = "existing@example.com" };
            var model = new NewLeadAlreadyExistsViewModel { IsCorrectPerson = true };
            _sessionHelper.Setup(x => x.GetFromSession<InviteLeadViewModel>(It.IsAny<HttpContext>(), SessionKeys.NewLeadInviteEmailViewModelSessionKey)).Returns(emailModel);
            _userService.Setup(x => x.GetUserByEmailIdAsync(emailModel.EmailAddress)).ReturnsAsync(new UserResponse
            {
                FirstName = "Jane", LastName = "Doe", JobTitle = "Manager"
            });

            var result = Assert.IsType<RedirectToActionResult>(await controller.SaveNewLeadAlreadyExistsAsync(model));

            Assert.Equal(nameof(NetworkLeadsController.CheckYourAnswers), result.ActionName);
            _sessionHelper.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.NewLeadDetailsViewModelSessionKey,
                It.Is<NewLeadDetailsViewModel>(m => m.FirstName == "Jane" && !m.InviteForFirstTime)), Times.Once);
        }

        [Fact]
        public async Task NewLeadDetails_WithMissingEmailSession_RedirectsToManageLeads()
        {
            var controller = CreateController();
            var model = new NewLeadDetailsViewModel { FirstName = "Jane", LastName = "Doe", JobTitle = "Manager" };

            var result = Assert.IsType<RedirectToActionResult>(await controller.NewLeadDetails(model));

            Assert.Equal(nameof(NetworkLeadsController.ManageLeads), result.ActionName);
            Assert.NotNull(controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task SaveSameOrganisation_WithoutOrganisationName_ReturnsValidationError()
        {
            var controller = CreateController();
            var model = new SameOrganisationViewModel { WorksForSameOrganisation = false };

            var result = Assert.IsType<ViewResult>(await controller.SaveSameOrganisationAsync(model));

            Assert.Equal(nameof(NetworkLeadsController.SameOrganisation), result.ViewName);
            Assert.Equal("Enter the organisation name", controller.ModelState[nameof(model.OrganisationName)]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task SaveSameOrganisation_WhenSameOrganisation_SavesNullNameAndRedirects()
        {
            var controller = CreateController();
            var model = new SameOrganisationViewModel { WorksForSameOrganisation = true, OrganisationName = "Ignored" };

            var result = Assert.IsType<RedirectToActionResult>(await controller.SaveSameOrganisationAsync(model));

            Assert.Equal(nameof(NetworkLeadsController.CheckYourAnswers), result.ActionName);
            Assert.Null(model.OrganisationName);
            _sessionHelper.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.SameOrganisationViewModelSessionKey, model), Times.Once);
        }

        [Fact]
        public void CheckYourAnswers_BuildsModelFromSession()
        {
            var controller = CreateController();
            _sessionHelper.Setup(x => x.GetFromSession<NewLeadDetailsViewModel>(It.IsAny<HttpContext>(), SessionKeys.NewLeadDetailsViewModelSessionKey))
                .Returns(new NewLeadDetailsViewModel { FirstName = "Jane", LastName = "Doe", JobTitle = "Manager", InviteForFirstTime = true });
            _sessionHelper.Setup(x => x.GetFromSession<InviteLeadViewModel>(It.IsAny<HttpContext>(), SessionKeys.NewLeadInviteEmailViewModelSessionKey))
                .Returns(new InviteLeadViewModel { EmailAddress = "jane@example.com" });
            _sessionHelper.Setup(x => x.GetFromSession<SameOrganisationViewModel>(It.IsAny<HttpContext>(), SessionKeys.SameOrganisationViewModelSessionKey))
                .Returns(new SameOrganisationViewModel { WorksForSameOrganisation = false, OrganisationName = "Another Organisation" });

            var result = Assert.IsType<ViewResult>(controller.CheckYourAnswers());
            var model = Assert.IsType<CheckYourAnswersViewModel>(result.Model);

            Assert.Equal("Jane", model.FirstName);
            Assert.Equal("jane@example.com", model.EmailAddress);
            Assert.Equal("Another Organisation", model.WorkingOrganisationName);
            Assert.Equal("Network Manager", model.InvitedRole);
        }

        [Fact]
        public async Task CheckYourAnswers_WhenInvitationSucceeds_SendsEmailAndRedirectsToConfirmation()
        {
            var controller = CreateController();
            SetupCheckYourAnswersSession(controller, inviteForFirstTime: true);
            _userService.Setup(x => x.GetUserByEmailIdAsync("jane@example.com")).ReturnsAsync((UserResponse?)null);
            _invitationService.Setup(x => x.AddInvitedUserAsync("user-id", It.IsAny<AddInvitationRequest>())).ReturnsAsync("invitation-id");
            _invitationTokenService.Setup(x => x.GenerateToken("invitation-id", "jane@example.com")).Returns("token");

            var result = Assert.IsType<RedirectToActionResult>(await controller.CheckYourAnswers(new CheckYourAnswersViewModel()));

            Assert.Equal(nameof(NetworkLeadsController.NewLeadConfirmation), result.ActionName);
            _invitationService.Verify(x => x.SendInvitationEmailAsync("invitation-id", It.IsAny<SendInvitationEmailRequest>()), Times.Once);
        }

        [Fact]
        public async Task CheckYourAnswers_WhenInvitationIdIsEmpty_RedirectsToManageLeadsWithError()
        {
            var controller = CreateController();
            SetupCheckYourAnswersSession(controller, inviteForFirstTime: false);
            _invitationService.Setup(x => x.AddInvitedUserAsync(It.IsAny<string>(), It.IsAny<AddInvitationRequest>())).ReturnsAsync((string?)null);

            var result = Assert.IsType<RedirectToActionResult>(await controller.CheckYourAnswers(new CheckYourAnswersViewModel()));

            Assert.Equal(nameof(NetworkLeadsController.ManageLeads), result.ActionName);
            Assert.NotNull(controller.TempData["ErrorMessage"]);
            _invitationService.Verify(x => x.SendInvitationEmailAsync(It.IsAny<string>(), It.IsAny<SendInvitationEmailRequest>()), Times.Never);
        }

        [Fact]
        public void NewLeadConfirmation_ClearsInvitationSessionAndReturnsView()
        {
            var controller = CreateController();

            var result = Assert.IsType<ViewResult>(controller.NewLeadConfirmation());

            Assert.Null(result.ViewName);
            _sessionHelper.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.NewLeadInviteEmailViewModelSessionKey), Times.Once);
            _sessionHelper.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.NewLeadDetailsViewModelSessionKey), Times.Once);
            _sessionHelper.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.SameOrganisationViewModelSessionKey), Times.Once);
        }

        private void SetupCheckYourAnswersSession(NetworkLeadsController controller, bool inviteForFirstTime)
        {
            _sessionHelper.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey)).Returns("user-id");
            _sessionHelper.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationId)).Returns("org-id");
            _sessionHelper.Setup(x => x.GetFromSession<InviteLeadViewModel>(It.IsAny<HttpContext>(), SessionKeys.NewLeadInviteEmailViewModelSessionKey))
                .Returns(new InviteLeadViewModel { EmailAddress = "jane@example.com" });
            _sessionHelper.Setup(x => x.GetFromSession<NewLeadDetailsViewModel>(It.IsAny<HttpContext>(), SessionKeys.NewLeadDetailsViewModelSessionKey))
                .Returns(new NewLeadDetailsViewModel { FirstName = "Jane", LastName = "Doe", JobTitle = "Manager", InviteForFirstTime = inviteForFirstTime });
            _sessionHelper.Setup(x => x.GetFromSession<SameOrganisationViewModel>(It.IsAny<HttpContext>(), SessionKeys.SameOrganisationViewModelSessionKey))
                .Returns(new SameOrganisationViewModel { WorksForSameOrganisation = true });
        }
    }
}
