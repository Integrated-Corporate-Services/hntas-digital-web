using HNTAS.Api.Client.Api;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class HeatNetworkControllerTests
    {
        private readonly Mock<ILogger<HeatNetworkController>> _loggerMock;
        private readonly Mock<ISessionHelper> _sessionHelperMock;
        private readonly Mock<IHeatNetworksApi> _heatNetworksApiMock;
        private readonly Mock<IUserService> _userServiceMock;

        public HeatNetworkControllerTests()
        {
            _loggerMock = new Mock<ILogger<HeatNetworkController>>();
            _sessionHelperMock = new Mock<ISessionHelper>();
            _heatNetworksApiMock = new Mock<IHeatNetworksApi>();
            _userServiceMock = new Mock<IUserService>();
        }

        private HeatNetworkController CreateController()
        {
            var controller = new HeatNetworkController(
                _loggerMock.Object,
                _heatNetworksApiMock.Object,
                _userServiceMock.Object,
                _sessionHelperMock.Object
            );
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new MockHttpSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns("/mocked-url");

            controller.Url = urlHelperMock.Object;
            return controller;
        }

        [Fact]
        public void EnterHNName_Get_ReturnsViewWithModelFromSession()
        {
            // Arrange
            var controller = CreateController();
            var model = new HeatNetworkNameModel { HeatNetworkName = "mock-hnname" };
            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkNameModel>(It.IsAny<HttpContext>(), It.IsAny<string>())).Returns(model);
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "UserAccount" && ctx.Controller == "Dashboard")))
                .Returns("Dashboard/UserAccount");
            controller.Url = urlHelperMock.Object;

            // Act
            var result = controller.EnterHNName();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ViewBag.ShowBackButton);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal("Dashboard/UserAccount", controller.ViewBag.BackLinkUrl);
        }

        [Fact]
        public void EnterHNName_Post_ValidUrl_RedirectsToEnterHNLocation()
        {
            // Arrange
            var controller = CreateController();
            var model = new HeatNetworkNameModel { HeatNetworkName = "mock-hnname" };
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "EnterHNLocation" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/EnterHNLocation");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.EnterHNName(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HeatNetworkNameModel>(controller.HttpContext, It.IsAny<string>(), model), Times.Once);
            Assert.Equal("EnterHNLocation", redirectResult.ActionName);
        }

        [Fact]
        public void EnterHNLocation_Get_ReturnsViewWithModelFromSession()
        {
            // Arrange
            var controller = CreateController();
            var model = new HeatNetworkLocationModel { HeatNetworkLocation = "https://what3words.com/word1.word2.word3" };
            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkLocationModel>(It.IsAny<HttpContext>(), It.IsAny<string>())).Returns(model);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "EnterHNName" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/EnterHNName");
            controller.Url = urlHelperMock.Object;

            // Act
            var result = controller.EnterHNLocation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal("HeatNetwork/EnterHNName", controller.ViewBag.BackLinkUrl);
        }

        [Fact]
        public void EnterHNLocation_Post_InvalidUrl_ReturnsViewWithModelError()
        {
            // Arrange
            var controller = CreateController();

            var model = new HeatNetworkLocationModel { HeatNetworkLocation = "https://invalid.com/word.word.word" };
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "EnterHNName" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/EnterHNName");
            controller.Url = urlHelperMock.Object;         

            // Act
            var result = controller.EnterHNLocation(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(controller.ModelState.ContainsKey("HeatNetworkLocation"));
            Assert.Contains("Invalid url", controller.ModelState["HeatNetworkLocation"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void EnterHNLocation_Post_ValidUrl_RedirectsToCheckYourAnswers()
        {
            // Arrange
            var controller = CreateController();

            var model = new HeatNetworkLocationModel { HeatNetworkLocation = "https://what3words.com/word1.word2.word3" };
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "EnterHNName" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/EnterHNName");
            controller.Url = urlHelperMock.Object;
            
            // Act
            var result = controller.EnterHNLocation(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HeatNetworkLocationModel>(controller.HttpContext, It.IsAny<string>(), model), Times.Once);
            Assert.Equal("CheckYourAnswers", redirectResult.ActionName);
            
        }
        

        [Fact]
        public void CheckYourAnswers_ReturnsViewWithPopulatedModel()
        {
            // Arrange
            var controller = CreateController();

            var nameModel = new HeatNetworkNameModel { HeatNetworkName = "Test Network" };
            var locationModel = new HeatNetworkLocationModel { HeatNetworkLocation = "https://what3words.com/word1.word2.word3" };

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkNameModel>(
                It.IsAny<HttpContext>(), SessionKeys.HeatNetworkNameModelKey)).Returns(nameModel);

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkLocationModel>(
                It.IsAny<HttpContext>(), SessionKeys.HeatNetworkLocationModelKey)).Returns(locationModel);

            // Act
            var result = controller.CheckYourAnswers();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CheckYourAnswersHeatNetworkModel>(viewResult.Model);

            Assert.Equal(nameModel, model.HeatNetworkNameModel);
            Assert.Equal(locationModel, model.HeatNetworkLocationModel);
            Assert.False(model.ConfirmedDeclaration);
            Assert.False((bool)controller.ViewBag.ShowBackButton);
        }

        [Fact]
        public void CheckYourAnswers_MissingSessionData_ReturnsViewWithNullModelProperties()
        {
            // Arrange
            var controller = CreateController();

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkNameModel>(
                It.IsAny<HttpContext>(), SessionKeys.HeatNetworkNameModelKey)).Returns((HeatNetworkNameModel)null);

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkLocationModel>(
                It.IsAny<HttpContext>(), SessionKeys.HeatNetworkLocationModelKey)).Returns((HeatNetworkLocationModel)null);

            // Act
            var result = controller.CheckYourAnswers();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CheckYourAnswersHeatNetworkModel>(viewResult.Model);

            Assert.Null(model.HeatNetworkNameModel);
            Assert.Null(model.HeatNetworkLocationModel);
            Assert.False(model.ConfirmedDeclaration);
            Assert.False((bool)controller.ViewBag.ShowBackButton);
        }

        // Add TC for testing SubmitAnswer method

        // Add TC to mock GetUserById and confirmation page


    }
}


// Test cases for testing the model validation in the HeatNetworkController - works in ui, does not work in tests
// TC passe upon adding the model state validation in the controller methods - code redundancy

//[Fact]
//public void EnterHNLocation_Post_InvalidModel_ReturnsViewWithModelError()
//{
//    // Arrange
//    var controller = CreateController();

//    var model = new HeatNetworkLocationModel { HeatNetworkLocation = "" };
//    controller.ModelState.AddModelError("HeatNetworkLocation", "Required");

//    var httpContext = new DefaultHttpContext();
//    httpContext.Request.Headers["Referer"] = "https://localhost/dashboard";
//    controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

//    // Act
//    var result = controller.EnterHNLocation(model);

//    // Assert
//    var viewResult = Assert.IsType<ViewResult>(result);
//    Assert.Equal(model, viewResult.Model);
//    Assert.True(controller.ModelState.ContainsKey("HeatNetworkLocation"));
//}

//[Fact]
//public void EnterHNName_Post_InvalidModel_ReturnsViewWithModelError()
//{
//    // Arrange
//    var controller = CreateController();
//    var model = new HeatNetworkNameModel { HeatNetworkName = "" }; // Invalid due to [Required]

//    // Simulate model validation
//    var validationContext = new ValidationContext(model, null, null);
//    var validationResults = new List<ValidationResult>();
//    Validator.TryValidateObject(model, validationContext, validationResults, true);

//    foreach (var validationResult in validationResults)
//    {
//        foreach (var memberName in validationResult.MemberNames)
//        {
//            controller.ModelState.AddModelError(memberName, validationResult.ErrorMessage);
//        }
//    }

//    var httpContext = new DefaultHttpContext();
//    httpContext.Request.Headers["Referer"] = "https://localhost/dashboard";
//    controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

//    // Act
//    var result = controller.EnterHNName(model);

//    // Assert
//    var viewResult = Assert.IsType<ViewResult>(result);
//    Assert.Equal(model, viewResult.Model);
//    Assert.False(controller.ModelState.IsValid);
//    Assert.True(controller.ModelState.ContainsKey("HeatNetworkName"));
//}