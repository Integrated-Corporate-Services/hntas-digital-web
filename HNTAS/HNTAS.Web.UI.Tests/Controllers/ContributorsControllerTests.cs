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
        public void NewContributorDetails_Get_ReturnsViewResult()
        {
            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;
            var result = _controller.NewContributorDetails();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task NewContributorDetails_NonRpUser_NewUser_Post_ReturnsRedirectToActionResult()
        {
            var model = new NewContributorDetailsViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "test"
            };

            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;

            _userServiceMock.Setup(u => u.IsRpUserAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _userServiceMock.Setup(u => u.IsActiveUserAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            var result = await _controller.NewContributorDetails(model);
            var resultVal = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("NewContributorHeatNetwork", resultVal.ActionName);
        }

        [Fact]
        public async Task NewContributorDetails_RpUser_Post_ReturnsView()
        {
            var model = new NewContributorDetailsViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "test"
            };

            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;

            _userServiceMock.Setup(u => u.IsRpUserAsync(It.IsAny<string>()))
                .ReturnsAsync(true);


            var result = await _controller.NewContributorDetails(model);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task NewContributorDetails_NonRpUser_ExistingUserUser_Post_ReturnsView()
        {
            var model = new NewContributorDetailsViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "test"
            };

            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;

            _userServiceMock.Setup(u => u.IsRpUserAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _userServiceMock.Setup(u => u.IsActiveUserAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var result = await _controller.NewContributorDetails(model);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task NewContributorDetails_Post_InvalidModelState_ReturnsView()
        {
            var model = new NewContributorDetailsViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "test"
            };

            _controller.Url = SetUpBackLink("AddContributor", "ContributorsController").Object;

            _controller.ModelState.AddModelError("FirstName", "The FirstName field is required.");

            var result = await _controller.NewContributorDetails(model);
            Assert.IsType<ViewResult>(result);
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
    }
}
