using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace HNTAS.Web.UI.Tests.Controllers;
public class HomeControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ILogger<HomeController>> _loggerMock;
    private readonly Mock<ISessionHelper> _sessionHelperMock;
    private readonly Mock<IInvitationService> _invitationServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;

    public HomeControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _loggerMock = new Mock<ILogger<HomeController>>();
        _sessionHelperMock = new Mock<ISessionHelper>();
        _invitationServiceMock = new Mock<IInvitationService>();
        _configurationMock = new Mock<IConfiguration>();
    }

    private HomeController CreateController(ClaimsPrincipal user = null)
    {
        var controller = new HomeController(
            _userServiceMock.Object,
            _loggerMock.Object,
            _sessionHelperMock.Object,
            _invitationServiceMock.Object,
            _configurationMock.Object
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

    private IUrlHelper SetUpBackLink(string controller, string action)
    {
        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock
            .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                ctx.Action == action && ctx.Controller == controller)))
            .Returns($"{controller}/{action}");
        return urlHelperMock.Object;
    }

    [Fact(Skip = "To be fixed")]
    public async Task Index_ReturnsView_WhenClaimsMissing()
    {
        // Arrange
        var controller = CreateController(new ClaimsPrincipal());
        _userServiceMock.Setup(s => s.IsSuperUser(It.IsAny<string>())).ReturnsAsync(false);
        _configurationMock.Setup(c => c.GetSection("SuperUserLogin:Enabled").Value).Returns("false");

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<BadRequestResult>(result);
        Assert.Equal("Unable to retrieve essential user info. Please try again.", controller.TempData["ErrorMessage"]);
    }

    [Fact(Skip = "To be fixed")]
    public async Task Index_ReturnsView_WhenUserServiceThrowsException()
    {
        // Arrange
        var controller = CreateController(CreateUser());
        _userServiceMock.Setup(s => s.GetUserByOneLoginId(It.IsAny<string>())).ThrowsAsync(new System.Exception("fail"));
        _userServiceMock.Setup(s => s.IsSuperUser(It.IsAny<string>())).ReturnsAsync(false);
        _configurationMock.Setup(c => c.GetSection("SuperUserLogin:Enabled").Value).Returns("false");

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<BadRequestResult>(result);
        Assert.Equal("Error during account setup. Please contact support.", controller.TempData["ErrorMessage"]);
    }

    [Fact(Skip = "To be fixed")]
    public async Task Index_CreatesUser_WhenUserNotFound()
    {
        // Arrange
        var controller = CreateController(CreateUser());
        _userServiceMock.Setup(s => s.GetUserByOneLoginId(It.IsAny<string>())).ReturnsAsync((UserResponse)null);
        _userServiceMock.Setup(s => s.CreateUser(It.IsAny<InitialUserRegistrationRequest>())).ReturnsAsync("new-user-id");
        _userServiceMock.Setup(s => s.IsSuperUser(It.IsAny<string>())).ReturnsAsync(false);
        _configurationMock.Setup(c => c.GetSection("SuperUserLogin:Enabled").Value).Returns("false");

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<RedirectToActionResult>(result);
        // Optionally verify session helper was called
        _sessionHelperMock.Verify(x =>
            x.SaveToSession<string>(
                It.IsAny<HttpContext>(),
                SessionKeys.UserModel_Id_SessionKey,
                "new-user-id"),
            Times.Once);
    }

    [Fact(Skip = "To be fixed")]
    public async Task Index_SavesUserId_WhenUserFoundWithoutOrganisation()
    {
        // Arrange
        var controller = CreateController(CreateUser());
        var userResponse = new UserResponse(id: "user-123");
        _userServiceMock.Setup(s => s.GetUserByOneLoginId(It.IsAny<string>())).ReturnsAsync(userResponse);
        _userServiceMock.Setup(s => s.IsSuperUser(It.IsAny<string>())).ReturnsAsync(false);
        _configurationMock.Setup(c => c.GetSection("SuperUserLogin:Enabled").Value).Returns("false");

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<BadRequestResult>(result);

        // Verify the session helper was called with the correct user id
        _sessionHelperMock.Verify(x =>
            x.SaveToSession<string>(
                It.IsAny<HttpContext>(),
                SessionKeys.UserModel_Id_SessionKey,
                "user-123"),
            Times.Once);
    }

    [Fact(Skip = "To be fixed")]
    public async Task Index_Redirects_WhenUserHasOrganisation()
    {
        // Arrange
        var controller = CreateController(CreateUser());
        var userResponse = new UserResponse() { Id = "user-123", OrgId = "org123" };
        _userServiceMock.Setup(s => s.GetUserByOneLoginId(It.IsAny<string>())).ReturnsAsync(userResponse);
        _userServiceMock.Setup(s => s.IsSuperUser(It.IsAny<string>())).ReturnsAsync(false);
        _configurationMock.Setup(c => c.GetSection("SuperUserLogin:Enabled").Value).Returns("false");

        // Act
        var result = await controller.Index();

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("UserAccount", redirect.ActionName);
        Assert.Equal("Dashboard", redirect.ControllerName);

        // Verify the session helper was called with the correct user id
        _sessionHelperMock.Verify(x =>
            x.SaveToSession<string>(
                It.IsAny<HttpContext>(),
                SessionKeys.UserModel_Id_SessionKey,
                "user-123"),
            Times.Once);

        // Verify organisation details were saved
        if (userResponse.OrgId != null)
        {
            var Redirect = Assert.IsType<RedirectToActionResult>(result);
        }
    }

    [Fact]
    public void Error_WhenCodeIs404_ReturnsNotFoundView()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Error(404);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("NotFound", viewResult.ViewName);
    }

    [Fact]
    public void Error_WhenCodeIs500_ReturnsErrorView()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Error(500);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);
    }

    [Fact]
    public void StartPage_WithInvitedEmail_SetsBackButtonAndNavigateUrlToHomeIndex()
    {
        // Arrange        
        var invitedEmail = "test@example.com";
        _sessionHelperMock.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.InvitedTokenEmail)).Returns(invitedEmail);
        var controller = CreateController(CreateUser());
        controller.Url = (invitedEmail != null
            ? SetUpBackLink("Home", "Index")
            : SetUpBackLink("Home", "WhatDoYouWantToDo"));

        // Act
        var result = controller.StartPage();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Home/Index", controller.ViewBag.NavigateUrl);
    }

    [Fact]
    public void StartPage_ReturnsViewResult_WithNavigateUrlForMissingInvitedEmail()
    {
        // Arrange
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/WhatDoYouWantToDo");
        _sessionHelperMock.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.InvitedTokenEmail)).Returns((string)null); // Simulate missing invited email
        var controller = CreateController();

        controller.Url = mockUrlHelper.Object;

        // Act
        var result = controller.StartPage();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("/WhatDoYouWantToDo", controller.ViewBag.NavigateUrl);
    }

    [Fact]
    public void WhatDoYouWantToDo_WithModelInSession_ReturnsViewWithModel()
    {
        // Arrange
        var expectedModel = new WhatDoYouWantToDoViewModel { UserPathToday = "TestOption" };
        _configurationMock.Setup(c => c.GetSection("ExistingNetworks:EnableFeature").Value).Returns("true");
        _sessionHelperMock.Setup(s => s.GetFromSession<WhatDoYouWantToDoViewModel>(
            It.IsAny<HttpContext>(), SessionKeys.WhatDoYouWantToDoViewModelKey))
            .Returns(expectedModel);
        var controller = CreateController(CreateUser());
        controller.Url = SetUpBackLink("Home", "StartPage");

        // Act
        var result = controller.WhatDoYouWantToDo();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<WhatDoYouWantToDoViewModel>(viewResult.Model);
        Assert.Equal("TestOption", model.UserPathToday);
    }

    [Fact]
    public void WhatDoYouWantToDo_WithoutModelInSession_ReturnsViewWithNewModel()
    {
        // Arrange
        _configurationMock.Setup(c => c.GetSection("ExistingNetworks:EnableFeature").Value).Returns("true");
        _sessionHelperMock.Setup(s => s.GetFromSession<WhatDoYouWantToDoViewModel>(
            It.IsAny<HttpContext>(), SessionKeys.WhatDoYouWantToDoViewModelKey))
            .Returns((WhatDoYouWantToDoViewModel)null);
        var controller = CreateController(CreateUser());
        controller.Url = SetUpBackLink("Home", "StartPage");

        // Act
        var result = controller.WhatDoYouWantToDo();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<WhatDoYouWantToDoViewModel>(viewResult.Model);
        Assert.Empty(model.UserPathToday);
    }

    [Fact]
    public void WhatDoYouWantToDo_InvalidModelState_ReturnsViewWithModel()
    {
        // Arrange
        var model = new WhatDoYouWantToDoViewModel { UserPathToday = "doSomething" };
        var controller = CreateController();
        controller.Url = SetUpBackLink("Home", "StartPage");
        var errorMessage = "Invalid selection. Please try again.";
        controller.ModelState.AddModelError("UserPathToday", errorMessage);


        // Act
        var result = controller.WhatDoYouWantToDo(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.Equal(errorMessage, controller.ModelState["UserPathToday"].Errors[0].ErrorMessage);
    }

    [Fact]
    public void WhatDoYouWantToDo_RegisterNewHN_RedirectsToAreYouTheRP()
    {
        // Arrange
        var model = new WhatDoYouWantToDoViewModel { UserPathToday = "registerNewHN" };
        var controller = CreateController(CreateUser());
        controller.Url = SetUpBackLink("Home", "StartPage");

        // Act
        var result = controller.WhatDoYouWantToDo(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AreYouTheRP", redirectResult.ActionName);
        Assert.Equal("RegistrationEligibility", redirectResult.ControllerName);
        _sessionHelperMock.Verify(s => s.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.WhatDoYouWantToDoViewModelKey, model), Times.Once);
    }

    [Fact]
    public void WhatDoYouWantToDo_UpdateExistingHN_RedirectsToHomeIndex()
    {
        // Arrange
        var model = new WhatDoYouWantToDoViewModel { UserPathToday = "updateExistingHN" };
        var controller = CreateController(CreateUser());
        controller.Url = SetUpBackLink("Home", "StartPage");

        // Act
        var result = controller.WhatDoYouWantToDo(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);
        _sessionHelperMock.Verify(s => s.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.WhatDoYouWantToDoViewModelKey, model), Times.Once);
    }

}