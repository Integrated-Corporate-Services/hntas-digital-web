using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
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
    public class ContributorControllerTests
    {
        private readonly Mock<ILogger<ContributorController>> _loggerMock;
        private readonly Mock<IInvitationService> _invitationServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IInvitationTokenService> _invitationTokenService;
        private readonly Mock<ISessionHelper> _sessionHelperMock;

        private readonly ContributorController _controller;

        public ContributorControllerTests()
        {
            _loggerMock = new Mock<ILogger<ContributorController>>();
            _invitationServiceMock = new Mock<IInvitationService>();
            _userServiceMock = new Mock<IUserService>();
            _invitationTokenService = new Mock<IInvitationTokenService>();
            _sessionHelperMock = new Mock<ISessionHelper>();
            _controller = CreateController();
        }

        private ContributorController CreateController()
        {
            var controller = new ContributorController(
                _userServiceMock.Object,
                _invitationServiceMock.Object,
                _loggerMock.Object,
                _sessionHelperMock.Object,
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

        private UserDetailsResponse MockGetUserDetailsResponse(string userId)
        {
            var userDetails = new HNTAS.Api.Client.Model.UserDetailsResponse
            {
                Id = userId,
                OneLoginId = "one-login-id",
                FirstName = "Test",
                LastName = "User",
                FullName = "Test User",
                EmailId = "test@email.com",
                JobTitle = "Assessor",
                MobileNumber = "1234567890",
                Status = UserStatus.Active,
                Roles = new List<UserRole> { UserRole.Assessor },
                Organisation = new OrganisationResponse
                {
                    OrgId = "org-id",
                    Name = "Test Organisation",
                    CompaniesHouseNumber = "12345678",
                    Type = OrganisationType.UkCompaniesHouse,
                    RegisteredAddress = new RegisteredAddress2("123 Test St", "TE1 1ST", "Test Area", "Test Town", "Test County", "Test Country")
                },
                HeatNetworks = new List<HeatNetworkUserResponse>()
                {
                    new HeatNetworkUserResponse
                    {
                        HnId = "hn-1",
                        Name = "Heat Network 1",
                       // Location = "Location 1"
                    },
                    new HeatNetworkUserResponse
                    {
                        HnId = "hn-2",
                        Name = "Heat Network 2",
                        //Location = "Location 2"
                    }
                }
            };

            return userDetails;
        }

        private InvitedUserResponse MockGetInvitationByIdAsync(string invitationId)
        {
            var invitedUser = new InvitedUserResponse
            {
                Id = invitationId,
                InviterUserId = "valid-user-id",
                Email = "",
                FirstName = "Invited",
                LastName = "User",
                FullName = "Invited User",
                Status = InvitationStatus.Invited,
                InvitedHnId = "",
                Roles = new List<ContributorRole> { ContributorRole.Assessor },
                InvitedAt = DateTimeOffset.UtcNow,
                AcceptedAt = null,
                RejectedAt = null
            };
            return invitedUser;
        }

        [Fact]
        public async Task Get_YouHaveBeenInvited_ValidToken_ReturnsViewWithModel()
        {
            // Arrange
            var token = "validToken";
            var invitationId = "validid";
            var invitationEmail = "test@mailinator.com";
            var invitation = MockGetInvitationByIdAsync(invitationId);
            var inviterUser = MockGetUserDetailsResponse(invitation.InviterUserId);

            _invitationTokenService.Setup(s => s.DecryptToken(token))
                .Returns((invitationId, invitationEmail));
            _invitationServiceMock.Setup(s => s.GetInvitationByIdAsync(invitationId)).ReturnsAsync(invitation);
            _userServiceMock.Setup(s => s.GetUserDetails(invitation.InviterUserId)).ReturnsAsync(inviterUser);

            _controller.ControllerContext.HttpContext.Request.QueryString = new QueryString($"?token={token}");

            // Act
            var result = await _controller.YouHaveBeenInvited();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            _sessionHelperMock.Verify(s => s.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.InvitedTokenEmail, invitationEmail), Times.Once);
        }

        [Fact]
        public async Task Get_YouHaveBeenInvited_MissingToken_SetsErrorMessageAndReturnsView()
        {
            // Token is missing, so no setup needed for DecryptToken or service calls as they won't be invoked

            // Act
            var result = await _controller.YouHaveBeenInvited();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.TempData.ContainsKey("ErrorMessage"));
            Assert.Equal("The invitation token is missing from your request. Please use the link provided in the invitation email to proceed.", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Post_YouHaveBeenInvitedAsync_ValidModel_AcceptInvitation_RedirectsToStartPage()
        {
            // Arrange
            var model = new YouHaveBeenInvitedModel { AcceptInvitation = "accept" };
            var invitationId = "validid";
            _sessionHelperMock.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.InvitationId)).Returns(invitationId);

            // Act
            var result = await _controller.YouHaveBeenInvitedAsync(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("StartPage", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Post_YouHaveBeenInvitedAsync_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var model = new YouHaveBeenInvitedModel { AcceptInvitation = "accept" };
            _controller.ModelState.AddModelError("AcceptInvitation", "Required");

            // Act
            var result = await _controller.YouHaveBeenInvitedAsync(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task Post_YouHaveBeenInvitedAsync_DeclineInvitation_RedirectsToYouHaveDeclined()
        {
            // Arrange
            var model = new YouHaveBeenInvitedModel { AcceptInvitation = "decline" };
            var invitationId = "validid";
            _sessionHelperMock.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.InvitationId)).Returns(invitationId);
            _invitationServiceMock.Setup(s => s.RejectInvitationAsync(invitationId)).Returns(Task.CompletedTask);


            // Act
            var result = await _controller.YouHaveBeenInvitedAsync(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("YouHaveDeclined", redirectResult.ActionName);
            Assert.Equal("Contributor", redirectResult.ControllerName);
            _invitationServiceMock.Verify(s => s.RejectInvitationAsync(invitationId), Times.Once);
        }

        [Fact]
        public async Task YouHaveBeenInvitedAsync_DeclineInvitation_Exception_ReturnsViewWithErrorMessage()
        {
            // Arrange
            var model = new YouHaveBeenInvitedModel { AcceptInvitation = "decline" };
            var invitationId = Guid.NewGuid().ToString();
            _invitationServiceMock.Setup(s => s.RejectInvitationAsync(invitationId)).ThrowsAsync(new Exception("Database error"));
            _sessionHelperMock.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.InvitationId)).Returns(invitationId);

            // Act
            var result = await _controller.YouHaveBeenInvitedAsync(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(_controller.TempData.ContainsKey("ErrorMessage"));
            Assert.Equal("An error occurred while declining the invitation. Please try again later.", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Post_YouHaveBeenInvited_InvalidChoice_ReturnsViewWithError()
        {
            var model = new YouHaveBeenInvitedModel { AcceptInvitation = "maybe" };
            var invitationId = "validid";
            _sessionHelperMock.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.InvitationId)).Returns(invitationId);
            _invitationServiceMock.Setup(s => s.RejectInvitationAsync(invitationId)).Returns(Task.CompletedTask);
            var result = await _controller.YouHaveBeenInvitedAsync(model) as ViewResult;

            Assert.NotNull(result);
            Assert.True(_controller.ModelState.ContainsKey(nameof(model.AcceptInvitation)));
        }

        [Fact]
        public void Get_YouHaveDeclined_ReturnsView()
        {
            var result = _controller.YouHaveDeclined() as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void StartPage_ReturnsViewResult()
        {
            // Arrange
            _controller.Url = SetUpBackLink("Contributor", "YouHaveBeenInvited").Object; // Assign mock to controller.Url

            // Act
            var result = _controller.StartPage();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }
    }
}