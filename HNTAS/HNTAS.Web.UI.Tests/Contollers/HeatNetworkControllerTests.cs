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
        private readonly Mock<IHeatNetworkService> _heatNetworkServiceMock;
        private readonly Mock<IUserService> _userServiceMock;

        public HeatNetworkControllerTests()
        {
            _loggerMock = new Mock<ILogger<HeatNetworkController>>();
            _sessionHelperMock = new Mock<ISessionHelper>();
            _heatNetworkServiceMock = new Mock<IHeatNetworkService>();
            _userServiceMock = new Mock<IUserService>();
        }

        private HeatNetworkController CreateController()
        {
            var controller = new HeatNetworkController(
                _loggerMock.Object,
                _heatNetworkServiceMock.Object,
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
            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkNameModel>(It.IsAny<HttpContext>(), SessionKeys.HeatNetworkNameModelKey)).Returns(model);
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
            _sessionHelperMock.Verify(x => x.SaveToSession<HeatNetworkNameModel>(controller.HttpContext, SessionKeys.HeatNetworkNameModelKey, model), Times.Once);
            Assert.Equal("EnterHNLocation", redirectResult.ActionName);
        }

        [Fact]
        public void EnterHNLocation_Get_ReturnsViewWithModelFromSession()
        {
            // Arrange
            var controller = CreateController();
            var model = new HeatNetworkLocationModel { HeatNetworkLocation = "https://what3words.com/word1.word2.word3" };
            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkLocationModel>(It.IsAny<HttpContext>(), SessionKeys.HeatNetworkLocationModelKey)).Returns(model);

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

            var model = new HeatNetworkLocationModel { HeatNetworkLocation = "///word.word.word" };
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
            _sessionHelperMock.Verify(x => x.SaveToSession<HeatNetworkLocationModel>(controller.HttpContext, SessionKeys.HeatNetworkLocationModelKey, model), Times.Once);
            Assert.Equal("EnterHNPhase", redirectResult.ActionName);            
        }

        [Fact]
        public void EnterHNPhase_Get_ReturnsViewWithModelFromSession()
        {
            // Arrange
            var controller = CreateController();
            var model = new HeatNetworkPhaseModel ();
            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkPhaseModel>(It.IsAny<HttpContext>(), SessionKeys.HeatNetworkPhaseModelKey)).Returns(model);
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "EnterHNLocation" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/EnterHNLocation");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.EnterHNPhase();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal("HeatNetwork/EnterHNLocation", controller.ViewBag.BackLinkUrl);
        }

        [Fact]
        public void EnterHNPhase_Post_ValidModel_RedirectsToCheckYourAnswers()
        {
            // Arrange
            var controller = CreateController();
            var model = new HeatNetworkPhaseModel { HeatNetworkPhase = "design" };
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "EnterHNLocation" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/EnterHNLocation");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.EnterHNPhase(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HeatNetworkPhaseModel>(controller.HttpContext, SessionKeys.HeatNetworkPhaseModelKey, model), Times.Once);
            _sessionHelperMock.Verify(x => x.SaveToSession<PathwayModel>(controller.HttpContext, SessionKeys.PathwayModelKey, It.Is<PathwayModel>(p => p.Pathway == "1")), Times.Once);
            Assert.Equal("CheckYourAnswers", redirectResult.ActionName);
        }

        [Fact]
        public void EnterHNPhase_Post_ValidModel_RedirectsToHaveYouSignedMEContract()
        {
            // Arrange
            var controller = CreateController();
            var model = new HeatNetworkPhaseModel { HeatNetworkPhase = "construction" };
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "EnterHNLocation" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/EnterHNLocation");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.EnterHNPhase(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HeatNetworkPhaseModel>(controller.HttpContext, SessionKeys.HeatNetworkPhaseModelKey, model), Times.Once);
            Assert.Equal("HaveYouSignedMEContract", redirectResult.ActionName);
        }

        [Fact]
        public void EnterHNPhase_Post_ValidModel_RedirectsToHNInOperation()
        {
            // Arrange
            var controller = CreateController();
            var model = new HeatNetworkPhaseModel { HeatNetworkPhase = "operation" };
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "EnterHNLocation" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/EnterHNLocation");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.EnterHNPhase(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HeatNetworkPhaseModel>(controller.HttpContext, SessionKeys.HeatNetworkPhaseModelKey, model), Times.Once);
            Assert.Equal("HNInOperation", redirectResult.ActionName);
        }

        [Fact]
        public void EnterHNPhase_Post_InvalidModel_ReturnsViewWithModelError()
        {
            // Arrange
            var controller = CreateController();
            var model = new HeatNetworkPhaseModel { HeatNetworkPhase = null };
            controller.ModelState.AddModelError("HeatNetworkPhase", "The HeatNetworkPhase field is required.");
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "EnterHNLocation" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/EnterHNLocation");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.EnterHNPhase(model);
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(controller.ModelState.ContainsKey("HeatNetworkPhase"));
            Assert.Contains("The HeatNetworkPhase field is required.", controller.ModelState["HeatNetworkPhase"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void HaveYouSignedMEContract_Get_ReturnsViewWithModelFromSession()
        {
            // Arrange
            var controller = CreateController();
            var model = new HaveYouSignedMEContractModel();
            _sessionHelperMock.Setup(x => x.GetFromSession<HaveYouSignedMEContractModel>(It.IsAny<HttpContext>(), SessionKeys.HaveYouSignedMEContractModelKey)).Returns(model);
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "EnterHNPhase" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/EnterHNPhase");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.HaveYouSignedMEContract();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal("HeatNetwork/EnterHNPhase", controller.ViewBag.BackLinkUrl);
        }

        [Fact]
        public void HaveYouSignedMEContract_Post_ValidModel_RedirectsToMEContractIsSigned()
        {
            // Arrange
            var controller = CreateController();
            var model = new HaveYouSignedMEContractModel { HaveYouSignedMEContract = "yes" };
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "EnterHNPhase" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/EnterHNPhase");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.HaveYouSignedMEContract(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HaveYouSignedMEContractModel>(controller.HttpContext, SessionKeys.HaveYouSignedMEContractModelKey, model), Times.Once);
            Assert.Equal("MEContractIsSigned", redirectResult.ActionName);
        }

        [Fact]
        public void HaveYouSignedMEContract_Post_ValidModel_RedirectsToHasElementBeenRegistered()
        {
            // Arrange
            var controller = CreateController();
            var model = new HaveYouSignedMEContractModel { HaveYouSignedMEContract = "no" };
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "EnterHNPhase" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/EnterHNPhase");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.HaveYouSignedMEContract(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HaveYouSignedMEContractModel>(controller.HttpContext, SessionKeys.HaveYouSignedMEContractModelKey, model), Times.Once);
            Assert.Equal("HasElementBeenRegistered", redirectResult.ActionName);
        }

        [Fact]
        public void HaveYouSignedMEContract_Post_InvalidModel_ReturnsViewWithModelError()
        {
            // Arrange
            var controller = CreateController();
            var model = new HaveYouSignedMEContractModel { HaveYouSignedMEContract = null };
            controller.ModelState.AddModelError("HaveYouSignedMEContract", "The HaveYouSignedMEContract field is required.");
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "EnterHNPhase" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/EnterHNPhase");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.HaveYouSignedMEContract(model);
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(controller.ModelState.ContainsKey("HaveYouSignedMEContract"));
            Assert.Contains("The HaveYouSignedMEContract field is required.", controller.ModelState["HaveYouSignedMEContract"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void HasElementBeenRegistered_Get_ReturnsViewWithModelFromSession()
        {
            // Arrange
            var controller = CreateController();
            var model = new HasElementBeenRegisteredModel();
            _sessionHelperMock.Setup(x => x.GetFromSession<HasElementBeenRegisteredModel>(It.IsAny<HttpContext>(), SessionKeys.HasElementBeenRegisteredModelKey)).Returns(model);
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "HaveYouSignedMEContract" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/HaveYouSignedMEContract");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.HasElementBeenRegistered();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal("HeatNetwork/HaveYouSignedMEContract", controller.ViewBag.BackLinkUrl);
        }

        [Fact]
        public void HasElementBeenRegistered_Post_ValidModel_RedirectsToHasPlanningApplicationBeenSubmitted()
        {
            // Arrange
            var controller = CreateController();
            var model = new HasElementBeenRegisteredModel { HasElementBeenRegistered = "yes" };
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "HaveYouSignedMEContract" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/HaveYouSignedMEContract");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.HasElementBeenRegistered(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HasElementBeenRegisteredModel>(controller.HttpContext, SessionKeys.HasElementBeenRegisteredModelKey, model), Times.Once);
            Assert.Equal("HasPlanningApplicationBeenSubmitted", redirectResult.ActionName);
        }

        [Fact]
        public void HasElementBeenRegistered_Post_ValidModel_RedirectsToCheckYourAnswers()
        {
            // HasPlanningApplicationBeenSubmitted
            // Arrange
            var controller = CreateController();
            var model = new HasElementBeenRegisteredModel { HasElementBeenRegistered = "no" };
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "HaveYouSignedMEContract" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/HaveYouSignedMEContract");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.HasElementBeenRegistered(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HasElementBeenRegisteredModel>(controller.HttpContext, SessionKeys.HasElementBeenRegisteredModelKey, model), Times.Once);
            _sessionHelperMock.Verify(x => x.SaveToSession<PathwayModel>(controller.HttpContext, SessionKeys.PathwayModelKey, It.Is<PathwayModel>(p => p.Pathway == "1")), Times.Once);
            Assert.Equal("CheckYourAnswers", redirectResult.ActionName);
        }

        [Fact]
        public void HasElementBeenRegistered_Post_InvalidModel_ReturnsViewWithModelError()
        {
            // Arrange
            var controller = CreateController();
            var model = new HasElementBeenRegisteredModel { HasElementBeenRegistered = null };
            controller.ModelState.AddModelError("HasElementBeenRegistered", "The HasElementBeenRegistered field is required.");
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "HaveYouSignedMEContract" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/HaveYouSignedMEContract");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.HasElementBeenRegistered(model);
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(controller.ModelState.ContainsKey("HasElementBeenRegistered"));
            Assert.Contains("The HasElementBeenRegistered field is required.", controller.ModelState["HasElementBeenRegistered"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void HasPlanningApplicationBeenSubmitted_Get_ReturnsViewWithModelFromSession()
        {
            // Arrange
            var controller = CreateController();
            var model = new HasPlanningApplicationBeenSubmittedModel();
            _sessionHelperMock.Setup(x => x.GetFromSession<HasPlanningApplicationBeenSubmittedModel>(It.IsAny<HttpContext>(), SessionKeys.HasPlanningApplicationBeenSubmittedModelKey)).Returns(model);
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "HasElementBeenRegistered" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/HasElementBeenRegistered");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.HasPlanningApplicationBeenSubmitted();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal("HeatNetwork/HasElementBeenRegistered", controller.ViewBag.BackLinkUrl);
        }

        [Fact]
        public void HasPlanningApplicationBeenSubmitted_Post_ValidModel_RedirectsToCheckYourAnswers_Pathway_3to7()
        {
            // Arrange
            var controller = CreateController();
            var model = new HasPlanningApplicationBeenSubmittedModel { HasPlanningApplicationBeenSubmitted = "yes" };
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "HasElementBeenRegistered" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/HasElementBeenRegistered");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.HasPlanningApplicationBeenSubmitted(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HasPlanningApplicationBeenSubmittedModel>(controller.HttpContext, SessionKeys.HasPlanningApplicationBeenSubmittedModelKey, model), Times.Once);
            _sessionHelperMock.Verify(x => x.SaveToSession<PathwayModel>(controller.HttpContext, SessionKeys.PathwayModelKey, It.Is<PathwayModel>(p => p.Pathway == "3")), Times.Once);
            Assert.Equal("CheckYourAnswers", redirectResult.ActionName);
        }

        [Fact]
        public void HasPlanningApplicationBeenSubmitted_Post_ValidModel_RedirectsToCheckYourAnswers_Pathway_1to7()
        {
            // Arrange
            var controller = CreateController();
            var model = new HasPlanningApplicationBeenSubmittedModel { HasPlanningApplicationBeenSubmitted = "no" };
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "HasElementBeenRegistered" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/HasElementBeenRegistered");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.HasPlanningApplicationBeenSubmitted(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HasPlanningApplicationBeenSubmittedModel>(controller.HttpContext, SessionKeys.HasPlanningApplicationBeenSubmittedModelKey, model), Times.Once);
            _sessionHelperMock.Verify(x => x.SaveToSession<PathwayModel>(controller.HttpContext, SessionKeys.PathwayModelKey, It.Is<PathwayModel>(p => p.Pathway == "1")), Times.Once);
            Assert.Equal("CheckYourAnswers", redirectResult.ActionName);
        }

        [Fact]
        public void HasPlanningApplicationBeenSubmitted_Post_InvalidModel_ReturnsViewWithModelError()
        {
            // Arrange
            var controller = CreateController();
            var model = new HasPlanningApplicationBeenSubmittedModel { HasPlanningApplicationBeenSubmitted = null };
            controller.ModelState.AddModelError("HasPlanningApplicationBeenSubmitted", "The HasPlanningApplicationBeenSubmitted field is required.");
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == "HasElementBeenRegistered" && ctx.Controller == "HeatNetwork")))
                .Returns("HeatNetwork/HasElementBeenRegistered");
            controller.Url = urlHelperMock.Object;
            // Act
            var result = controller.HasPlanningApplicationBeenSubmitted(model);
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(controller.ModelState.ContainsKey("HasPlanningApplicationBeenSubmitted"));
            Assert.Contains("The HasPlanningApplicationBeenSubmitted field is required.", controller.ModelState["HasPlanningApplicationBeenSubmitted"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void CheckYourAnswers_ReturnsViewWithPopulatedModel()
        {
            // Arrange
            var controller = CreateController();

            var nameModel = new HeatNetworkNameModel { HeatNetworkName = "Test Network" };
            var locationModel = new HeatNetworkLocationModel { HeatNetworkLocation = "https://what3words.com/word1.word2.word3" };
            var phaseModel = new HeatNetworkPhaseModel { HeatNetworkPhase = "design" };
            var pathwayModel = new PathwayModel { Pathway = "1" };

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