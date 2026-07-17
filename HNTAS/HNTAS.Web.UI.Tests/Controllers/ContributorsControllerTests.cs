using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.Contributors;
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
    public class ContributorsControllerTests
    {
        private readonly Mock<ILogger<ContributorsController>> _loggerMock;
        private readonly Mock<IInvitationService> _invitationServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IInvitationTokenService> _invitationTokenService;
        private readonly Mock<ISessionHelper> _sessionHelperMock;

        private readonly ContributorsController _controller;

        public ContributorsControllerTests()
        {
            _loggerMock = new Mock<ILogger<ContributorsController>>();
            _invitationServiceMock = new Mock<IInvitationService>();
            _userServiceMock = new Mock<IUserService>();
            _invitationTokenService = new Mock<IInvitationTokenService>();
            _sessionHelperMock = new Mock<ISessionHelper>();
            _controller = CreateController();
        }

        private ContributorsController CreateController()
        {
            var controller = new ContributorsController(
                _sessionHelperMock.Object,
                _loggerMock.Object,
                _userServiceMock.Object,
                _invitationServiceMock.Object,
                _invitationTokenService.Object
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
        public async Task ManageContributors_Get_ReturnsViewResult()
        {
            // Arrange
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModelId");

            _userServiceMock.Setup(u => u.GetManagedUsers(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<ManagedUserResponse> { new ManagedUserResponse
                {
                    Id = "1", Name = "Test User", Roles = new List<string> { "Contributor" }, Status = "Active", HeatNetworks = new List<HeatNetworkInfo> { new HeatNetworkInfo { HnId = "HN1", Name = "Heat Network 1" } }
                }
            });

            // Act
            var result = await _controller.ManageContributors();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void NewContributorRole_Get_ReturnsViewResult()
        {
            _controller.Url = SetUpBackLink("ManageContributors", "ContributorsController").Object;
            var result = _controller.NewContributorRole();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void NewContributorRole_Post_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new NewContributorRoleViewModel();
            _controller.Url = SetUpBackLink("ManageContributors", "ContributorsController").Object;
            // Act
            var result = _controller.NewContributorRole(model);
            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AddContributor", redirectToActionResult.ActionName);
        }

        [Fact]
        public void NewContributorRole_Post_InvalidModelState()
        {
            // Arrange
            var model = new NewContributorRoleViewModel();
            _controller.Url = SetUpBackLink("ManageContributors", "ContributorsController").Object;
            _controller.ModelState.AddModelError("IsDDH", "The IsDDH field is required.");
            // Act
            var result = _controller.NewContributorRole(model);
            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void AddContributor_Get_DDHRole_ReturnsViewResult()
        {
            _controller.Url = SetUpBackLink("NewContributorRole", "ContributorsController").Object;
            _sessionHelperMock
                .Setup(x => x.GetFromSession<NewContributorRoleViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.NewContributorRoleViewModelSessionKey))
                .Returns(new NewContributorRoleViewModel { IsDDH = true });
            var result = _controller.AddContributor();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void AddContributor_Get_ContributorRole_ReturnsViewResult()
        {
            _controller.Url = SetUpBackLink("ManageContributors", "ContributorsController").Object;
            _sessionHelperMock
                .Setup(x => x.GetFromSession<NewContributorRoleViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.NewContributorRoleViewModelSessionKey))
                .Returns(new NewContributorRoleViewModel { IsDDH = false });
            var result = _controller.AddContributor();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void AddContributor_Get_AddContributorRole_ReturnsViewResult()
        {
            _controller.Url = SetUpBackLink("ManageContributors", "ContributorsController").Object;
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.WhoDoYouWantToAddSessionKey))
                .Returns("test");
            var result = _controller.AddContributor();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void AddContributor_Post_InviteNewContributor_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new AddContributorViewModel() { InviteNewContributor = true };
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.WhoDoYouWantToAddSessionKey))
                .Returns("test");
            _controller.Url = SetUpBackLink("NewContributorRole", "ContributorsController").Object;
            // Act
            var result = _controller.AddContributor(model);
            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("NewContributorDetails", redirectToActionResult.ActionName);
        }

        [Fact]
        public void AddContributor_Post_ExistingContributorsList_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new AddContributorViewModel() { InviteNewContributor = false };
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.WhoDoYouWantToAddSessionKey))
                .Returns("test");
            _controller.Url = SetUpBackLink("NewContributorRole", "ContributorsController").Object;
            // Act
            var result = _controller.AddContributor(model);
            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ExistingContributorsList", redirectToActionResult.ActionName);
        }

        [Fact]
        public void AddContributor_Post_InvalidModelState_ReturnsView()
        {
            // Arrange
            var model = new AddContributorViewModel() { InviteNewContributor = false };
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.WhoDoYouWantToAddSessionKey))
                .Returns("test");
            _controller.Url = SetUpBackLink("NewContributorRole", "ContributorsController").Object;
            _controller.ModelState.AddModelError("InviteNewContributor", "The InviteNewContributor field is required.");
            // Act
            var result = _controller.AddContributor(model);
            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void NewContributorDetails_Get_WhenWhoDoYouWantToAddExists_ReturnsViewWithSessionModel()
        {
            // Arrange
            var model = new NewContributorDetailsViewModel();

            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.WhoDoYouWantToAddSessionKey))
                .Returns("Contributor");

            _sessionHelperMock.Setup(x => x.GetFromSession<NewContributorDetailsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NewContributorDetailsViewModelSessionKey))
                .Returns(model);

            // Act
            var result = _controller.NewContributorDetails();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal("Contributor", _controller.ViewBag.WhoDoYouWantToAdd);
        }

        [Fact]
        public void NewContributorDetails_Get_WhenIsDDHTrue_SetsDesignatedDutyHolder()
        {
            // Arrange
            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.WhoDoYouWantToAddSessionKey))
                .Returns((string)null);

            _sessionHelperMock.Setup(x => x.GetFromSession<NewContributorRoleViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NewContributorRoleViewModelSessionKey))
                .Returns(new NewContributorRoleViewModel
                {
                    IsDDH = true
                });

            _sessionHelperMock.Setup(x => x.GetFromSession<NewContributorDetailsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NewContributorDetailsViewModelSessionKey))
                .Returns((NewContributorDetailsViewModel)null);

            // Act
            var result = _controller.NewContributorDetails();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsType<NewContributorDetailsViewModel>(viewResult.Model);
            Assert.Equal("Designated duty holder", _controller.ViewBag.WhoDoYouWantToAdd);
        }

        [Fact]
        public void NewContributorDetails_Get_WhenIsDDHFalse_SetsContributor()
        {
            // Arrange
            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.WhoDoYouWantToAddSessionKey))
                .Returns((string)null);

            _sessionHelperMock.Setup(x => x.GetFromSession<NewContributorRoleViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NewContributorRoleViewModelSessionKey))
                .Returns(new NewContributorRoleViewModel
                {
                    IsDDH = false
                });

            _sessionHelperMock.Setup(x => x.GetFromSession<NewContributorDetailsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NewContributorDetailsViewModelSessionKey))
                .Returns(new NewContributorDetailsViewModel());

            // Act
            var result = _controller.NewContributorDetails();

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.Equal("Contributor", _controller.ViewBag.WhoDoYouWantToAdd);
        }

        [Fact]
        public async Task NewContributorDetails_Post_InvalidModelState_ReturnsView()
        {
            // Arrange
            var model = new NewContributorDetailsViewModel();

            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;

            _controller.ModelState.AddModelError("EmailAddress", "Required");

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.WhoDoYouWantToAddSessionKey))
                .Returns("Contributor");

            // Act
            var result = await _controller.NewContributorDetails(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);

            _userServiceMock.Verify(x => x.IsRpUserAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task NewContributorDetails_Post_RpUser_ReturnsViewWithError()
        {
            // Arrange
            var model = new NewContributorDetailsViewModel
            {
                EmailAddress = "test@test.com"
            };

            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.WhoDoYouWantToAddSessionKey))
                .Returns("Contributor");

            _userServiceMock.Setup(x => x.IsRpUserAsync(model.EmailAddress))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.NewContributorDetails(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(model, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains(nameof(model.EmailAddress), _controller.ModelState.Keys);

            _userServiceMock.Verify(x => x.IsActiveUserAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task NewContributorDetails_Post_ExistingUser_ReturnsViewWithError()
        {
            // Arrange
            var model = new NewContributorDetailsViewModel
            {
                EmailAddress = "test@test.com"
            };

            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.WhoDoYouWantToAddSessionKey))
                .Returns("Contributor");

            _userServiceMock.Setup(x => x.IsRpUserAsync(model.EmailAddress))
                .ReturnsAsync(false);

            _userServiceMock.Setup(x => x.IsActiveUserAsync(model.EmailAddress))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.NewContributorDetails(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(model, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<NewContributorDetailsViewModel>()),
                Times.Never);
        }

        [Fact]
        public async Task NewContributorDetails_Post_ValidModel_RedirectsToNewContributorHeatNetwork()
        {
            // Arrange
            var model = new NewContributorDetailsViewModel
            {
                EmailAddress = "test@test.com"
            };

            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.WhoDoYouWantToAddSessionKey))
                .Returns("Contributor");

            _userServiceMock.Setup(x => x.IsRpUserAsync(model.EmailAddress))
                .ReturnsAsync(false);

            _userServiceMock.Setup(x => x.IsActiveUserAsync(model.EmailAddress))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.NewContributorDetails(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("NewContributorHeatNetwork", redirectResult.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.NewContributorDetailsViewModelSessionKey,
                model), Times.Once);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                "backAction",
                "NewContributorDetails"), Times.Once);
        }

        [Fact]
        public async Task ExistingContributorsList_Get_ReturnsViewResult()
        {
            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModelId");

            _userServiceMock.Setup(u => u.GetRegisteredUsersAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<UserResponse> { new UserResponse {
                FirstName = "John", LastName = "Doe", EmailId = "test", Roles = new List<UserRole> { UserRole.Contributor}
                } });
            var result = await _controller.ExistingContributorsList();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task ExistingContributorsList_Post_ReturnsRedirectToActionResult()
        {
            var model = new ExistingContributorsListViewModel
            {
                SelectedEmailAddress = "test"
            };
            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModelId");

            _userServiceMock.Setup(u => u.GetRegisteredUsersAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<UserResponse> { new UserResponse {
                FirstName = "John", LastName = "Doe", EmailId = "test", Roles = new List<UserRole> { UserRole.Contributor}
                } });

            var result = await _controller.ExistingContributorsList(model);
            var resultVal = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("NewContributorHeatNetwork", resultVal.ActionName);
        }

        [Fact]
        public async Task ExistingContributorsList_Post_InvalidModelState()
        {
            var model = new ExistingContributorsListViewModel
            {
                SelectedEmailAddress = "test"
            };
            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModelId");

            _userServiceMock.Setup(u => u.GetRegisteredUsersAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<UserResponse> { new UserResponse {
                FirstName = "John", LastName = "Doe", EmailId = "test", Roles = new List<UserRole> { UserRole.Contributor}
                } });

            _controller.ModelState.AddModelError("SelectedEmailAddress", "The SelectedEmailAddress field is required.");

            var result = await _controller.ExistingContributorsList(model);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task NewContributorHeatNetwork_Get_ReturnsViewResult()
        {
            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModelId");
            _userServiceMock.Setup(u => u.GetUserHeatNetworks(It.IsAny<string>()))
                .ReturnsAsync(new List<HeatNetworkUserResponse> { new HeatNetworkUserResponse {
                HnId = "HN1", Name = "Heat Network 1"
                } });
            var result = await _controller.NewContributorHeatNetwork();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task NewContributorHeatNetwork_Get_NoNetwork_RedirectToAction()
        {
            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModelId");
            _userServiceMock.Setup(u => u.GetUserHeatNetworks(It.IsAny<string>()))
                .Returns(Task.FromResult((List<HeatNetworkUserResponse>)null!));
            var result = await _controller.NewContributorHeatNetwork();
            var resultVal = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Error", resultVal.ActionName);
        }

        [Fact]
        public void NewContributorHeatNetwork_Post_ReturnsRedirectToActionResult()
        {
            var model = new NewContributorHeatNetworkViewModel
            {

            };
            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModelId");
            var result = _controller.NewContributorHeatNetwork(model);
            var resultVal = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkPhase", resultVal.ActionName);
        }

        [Fact]
        public void NewContributorHeatNetwork_Post_InvalidModelState_ReturnViewResult()
        {
            var model = new NewContributorHeatNetworkViewModel
            {

            };
            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModelId");
            _controller.ModelState.AddModelError("SelectedHeatNetworkId", "The SelectedHeatNetworkId field is required.");
            var result = _controller.NewContributorHeatNetwork(model);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void HeatNetworkPhase_Get_ReturnsViewResult()
        {
            _controller.Url = SetUpBackLink("NewContributorHeatNetwork", "ContributorsController").Object;
            var result = _controller.HeatNetworkPhase();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void HeatNetworkPhase_Post_RedirectToAction()
        {
            var model = new HeatNetworkPhaseViewModel();
            _controller.Url = SetUpBackLink("NewContributorHeatNetwork", "ContributorsController").Object;
            var result = _controller.HeatNetworkPhase(model);
            var resultVal = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CheckYourAnswers", resultVal.ActionName);
        }

        [Fact]
        public void HeatNetworkPhase_Post_InvalidModelState_ReturnsView()
        {
            var model = new HeatNetworkPhaseViewModel();
            _controller.Url = SetUpBackLink("NewContributorHeatNetwork", "ContributorsController").Object;
            _controller.ModelState.AddModelError("SelectedPhase", "The SelectedPhase field is required.");
            var result = _controller.HeatNetworkPhase(model);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void CheckYourAnswers_Get_ReturnsViewResult()
        {
            _controller.Url = SetUpBackLink("HeatNetworkPhase", "ContributorsController").Object;
            _sessionHelperMock
                .Setup(x => x.GetFromSession<CheckYourAnswersViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.CheckYourAnswersContributorsModelSessionKey))
                .Returns(new CheckYourAnswersViewModel());
            var result = _controller.CheckYourAnswers();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CheckYourAnswers_Post_RedirectToAction()
        {
            var model = new CheckYourAnswersViewModel { ConfirmedDeclaration = true, EmailAddress = "test", FirstName = "first", LastName = "last", RoleAssigned = "contributor", HeatNetwork = "HN1"};
            _controller.Url = SetUpBackLink("HeatNetworkPhase", "ContributorsController").Object;
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModelId");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NewContributorRoleViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.NewContributorRoleViewModelSessionKey))
                .Returns(new NewContributorRoleViewModel { IsDDH = true });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NewContributorDetailsViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.NewContributorDetailsViewModelSessionKey))
                .Returns(new NewContributorDetailsViewModel { FirstName = "first", LastName = "last", EmailAddress = "test" });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NewContributorHeatNetworkViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.NewContributorHeatNetworkViewModelSessionKey))
                .Returns(new NewContributorHeatNetworkViewModel { SelectedHeatNetwork = "HN1" });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<HeatNetworkPhaseViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.ContributorsHeatNetworkPhaseViewModelSessionKey))
                .Returns(new HeatNetworkPhaseViewModel { SelectedPhases = new List<string>{ "Design"} });

            _invitationServiceMock.Setup(i => i.AddInvitedUserAsync(It.IsAny<string>(), It.IsAny<AddInvitationRequest>()))
                .ReturnsAsync("invitationId");

            _invitationTokenService.Setup(i => i.GenerateToken(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("token");
            var result = await _controller.CheckYourAnswers(model);
            var resultVal = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UserConfirmation", resultVal.ActionName);
        }

        [Fact]
        public async Task CheckYourAnswers_Post_InvalidModelState_ViewResult()
        {
            var model = new CheckYourAnswersViewModel { ConfirmedDeclaration = true, EmailAddress = "test", FirstName = "first", LastName = "last", RoleAssigned = "contributor", HeatNetwork = "HN1" };
            _controller.Url = SetUpBackLink("HeatNetworkPhase", "ContributorsController").Object;
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModelId");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NewContributorRoleViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.NewContributorRoleViewModelSessionKey))
                .Returns(new NewContributorRoleViewModel { IsDDH = true });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NewContributorDetailsViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.NewContributorDetailsViewModelSessionKey))
                .Returns(new NewContributorDetailsViewModel { FirstName = "first", LastName = "last", EmailAddress = "test" });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NewContributorHeatNetworkViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.NewContributorHeatNetworkViewModelSessionKey))
                .Returns(new NewContributorHeatNetworkViewModel { SelectedHeatNetwork = "HN1" });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<HeatNetworkPhaseViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.ContributorsHeatNetworkPhaseViewModelSessionKey))
                .Returns(new HeatNetworkPhaseViewModel { SelectedPhases = new List<string> { "Design" } });

            _controller.ModelState.AddModelError("ConfirmedDeclaration", "The ConfirmedDeclaration field is required.");
            var result = await _controller.CheckYourAnswers(model);
            Assert.IsType<ViewResult>(result);            
        }

        [Fact]
        public async Task CheckYourAnswers_Post_NoInvitation_RedirectToAction()
        {
            var model = new CheckYourAnswersViewModel { ConfirmedDeclaration = true, EmailAddress = "test", FirstName = "first", LastName = "last", RoleAssigned = "contributor", HeatNetwork = "HN1" };
            _controller.Url = SetUpBackLink("HeatNetworkPhase", "ContributorsController").Object;
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModelId");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NewContributorRoleViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.NewContributorRoleViewModelSessionKey))
                .Returns(new NewContributorRoleViewModel { IsDDH = true });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NewContributorDetailsViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.NewContributorDetailsViewModelSessionKey))
                .Returns(new NewContributorDetailsViewModel { FirstName = "first", LastName = "last", EmailAddress = "test" });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NewContributorHeatNetworkViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.NewContributorHeatNetworkViewModelSessionKey))
                .Returns(new NewContributorHeatNetworkViewModel { SelectedHeatNetwork = "HN1" });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<HeatNetworkPhaseViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.ContributorsHeatNetworkPhaseViewModelSessionKey))
                .Returns(new HeatNetworkPhaseViewModel { SelectedPhases = new List<string> { "Design" } });

            _invitationServiceMock.Setup(i => i.AddInvitedUserAsync(It.IsAny<string>(), It.IsAny<AddInvitationRequest>()))
                .ReturnsAsync((string)null!);

            var result = await _controller.CheckYourAnswers(model);
            var resultVal = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CheckYourAnswers", resultVal.ActionName);
        }

        [Fact]
        public async Task CheckYourAnswers_Post_ThrowException()
        {
            var model = new CheckYourAnswersViewModel { ConfirmedDeclaration = true, EmailAddress = "test", FirstName = "first", LastName = "last", RoleAssigned = "contributor", HeatNetwork = "HN1" };
            _controller.Url = SetUpBackLink("HeatNetworkPhase", "ContributorsController").Object;
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModelId");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NewContributorRoleViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.NewContributorRoleViewModelSessionKey))
                .Returns(new NewContributorRoleViewModel { IsDDH = true });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NewContributorDetailsViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.NewContributorDetailsViewModelSessionKey))
                .Returns(new NewContributorDetailsViewModel { FirstName = "first", LastName = "last", EmailAddress = "test" });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NewContributorHeatNetworkViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.NewContributorHeatNetworkViewModelSessionKey))
                .Returns(new NewContributorHeatNetworkViewModel { SelectedHeatNetwork = "HN1" });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<HeatNetworkPhaseViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.ContributorsHeatNetworkPhaseViewModelSessionKey))
                .Returns(new HeatNetworkPhaseViewModel { SelectedPhases = new List<string> { "Design" } });

            _invitationServiceMock.Setup(i => i.AddInvitedUserAsync(It.IsAny<string>(), It.IsAny<AddInvitationRequest>()))
                .Throws(new Exception());

            var result = await _controller.CheckYourAnswers(model);
            
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error submitting new contributor details for email:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
