using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace HNTAS.Web.UI.Tests.Controllers;
public class HomeControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ILogger<HomeController>> _loggerMock;
    private readonly Mock<ISessionHelper> _sessionHelperMock;

    public HomeControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _loggerMock = new Mock<ILogger<HomeController>>();
        _sessionHelperMock = new Mock<ISessionHelper>();
    }

    private HomeController CreateController(ClaimsPrincipal user = null)
    {
        var controller = new HomeController(
            _userServiceMock.Object,
            _loggerMock.Object,
            _sessionHelperMock.Object
        );

        var httpContext = new DefaultHttpContext();
        if (user != null)
            httpContext.User = user;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
            httpContext,
            Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()
        );

        return controller;
    }

    private ClaimsPrincipal CreateUser(string email = "test@example.com", string sub = "user-123")
    {
        var claims = new[]
        {
            new Claim("email", email),
            new Claim("sub", sub)
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));
    }

    [Fact]
    public async Task Index_ReturnsView_WhenClaimsMissing()
    {
        // Arrange
        var controller = CreateController(new ClaimsPrincipal());

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Unable to retrieve essential user info. Please try again.", controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Index_ReturnsView_WhenUserServiceThrows()
    {
        // Arrange
        var controller = CreateController(CreateUser());
        _userServiceMock.Setup(s => s.GetUserByOneLoginId(It.IsAny<string>())).ThrowsAsync(new System.Exception("fail"));

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error during account setup. Please contact support.", controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Index_CreatesUser_WhenUserNotFound()
    {
        // Arrange
        var controller = CreateController(CreateUser());
        _userServiceMock.Setup(s => s.GetUserByOneLoginId(It.IsAny<string>())).ReturnsAsync((UserResponse)null);
        _userServiceMock.Setup(s => s.CreateUser(It.IsAny<InitialUserRegistrationRequest>())).ReturnsAsync("new-user-id");

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        // Optionally verify session helper was called
        _sessionHelperMock.Verify(x =>
            x.SaveToSession<string>(
                It.IsAny<HttpContext>(),
                SessionKeys.UserModel_Id_SessionKey,
                "new-user-id"),
            Times.Once);
    }

    [Fact]
    public async Task Index_SavesUserId_WhenUserFoundWithoutOrganisation()
    {
        // Arrange
        var controller = CreateController(CreateUser());
        var userResponse = new UserResponse(id: "user-123");
        _userServiceMock.Setup(s => s.GetUserByOneLoginId(It.IsAny<string>())).ReturnsAsync(userResponse);

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);

        // Verify the session helper was called with the correct user id
        _sessionHelperMock.Verify(x =>
            x.SaveToSession<string>(
                It.IsAny<HttpContext>(),
                SessionKeys.UserModel_Id_SessionKey,
                "user-123"),
            Times.Once);
    }

    //[Fact]
    //public async Task Index_Redirects_WhenUserHasOrganisation()
    //{
    //    // Arrange
    //    var controller = CreateController(CreateUser());
    //    var userResponse = new UserResponse(id: "user-123", organisation: new Organisation());
    //    _userServiceMock.Setup(s => s.GetUserByOneLoginId(It.IsAny<string>())).ReturnsAsync(userResponse);

    //    // Act
    //    var result = await controller.Index();

    //    // Assert
    //    var redirect = Assert.IsType<RedirectToActionResult>(result);
    //    Assert.Equal("UserAccount", redirect.ActionName);
    //    Assert.Equal("Dashboard", redirect.ControllerName);

    //    // Verify the session helper was called with the correct user id
    //    _sessionHelperMock.Verify(x =>
    //        x.SaveToSession<string>(
    //            It.IsAny<HttpContext>(),
    //            SessionKeys.UserModel_Id_SessionKey,
    //            "user-123"),
    //        Times.Once);

    //    // Verify organisation details were saved
    //    _sessionHelperMock.Verify(x =>
    //        x.SaveToSession<string>(
    //            It.IsAny<HttpContext>(),
    //            SessionKeys.OrganisationName,
    //            userResponse.Organisation.Name),
    //        Times.Once);
    //}
}