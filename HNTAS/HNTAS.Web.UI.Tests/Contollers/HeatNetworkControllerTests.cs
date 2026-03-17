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
    public class HeatNetworkControllerTests
    {
        private readonly Mock<ILogger<HeatNetworkController>> _loggerMock;
        private readonly Mock<ISessionHelper> _sessionHelperMock;
        private readonly Mock<IHeatNetworkService> _heatNetworkServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IAddressLookupService> _addressLookupServiceMock;
        private readonly Mock<IOrganisationService> _organisationServiceMock;
        private readonly HeatNetworkController _controller;

        public HeatNetworkControllerTests()
        {
            _loggerMock = new Mock<ILogger<HeatNetworkController>>();
            _sessionHelperMock = new Mock<ISessionHelper>();
            _heatNetworkServiceMock = new Mock<IHeatNetworkService>();
            _userServiceMock = new Mock<IUserService>();
            _addressLookupServiceMock = new Mock<IAddressLookupService>();
            _organisationServiceMock = new Mock<IOrganisationService>();
            _controller = CreateController();
        }

        private HeatNetworkController CreateController()
        {
            var _controller = new HeatNetworkController(
                _loggerMock.Object,
                _heatNetworkServiceMock.Object,
                _userServiceMock.Object,
                _sessionHelperMock.Object,
                _organisationServiceMock.Object,
                _addressLookupServiceMock.Object
            );
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new MockHttpSession();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var tempDataProvider = new Mock<ITempDataProvider>();
            _controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);

            return _controller;
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
        public void EnterHNName_Get_ReturnsViewWithModelFromSession()
        {
            // Arrange
            var model = new HeatNetworkNameModel { HeatNetworkName = "mock-hnname" };
            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkNameModel>(It.IsAny<HttpContext>(), SessionKeys.HeatNetworkNameModelKey)).Returns(model);
            _controller.Url = SetUpBackLink("UserAccount", "Dashboard").Object;

            // Act
            var result = _controller.EnterHNName();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void EnterHNName_Post_ValidUrl_RedirectsToDoesHNHaveAPostcode()
        {
            // Arrange
            var model = new HeatNetworkNameModel { HeatNetworkName = "mock-hnname" };
            _controller.Url = SetUpBackLink("UserAccount", "Dashboard").Object;
            // Act
            var result = _controller.EnterHNName(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HeatNetworkNameModel>(_controller.HttpContext, SessionKeys.HeatNetworkNameModelKey, model), Times.Once);
            Assert.Equal("DoesHNHaveAPostcode", redirectResult.ActionName);
        }

        [Fact]
        public void EnterHNPhase_Get_ReturnsViewWithModelFromSession()
        {
            // Arrange
            var model = new HeatNetworkPhaseModel();
            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkPhaseModel>(It.IsAny<HttpContext>(), SessionKeys.HeatNetworkPhaseModelKey)).Returns(model);
            _controller.Url = SetUpBackLink("EnterHNLocation", "HeatNetwork").Object;
            // Act
            var result = _controller.EnterHNPhase();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void EnterHNPhase_Post_ValidModel_RedirectsToCheckYourAnswers()
        {
            // Arrange
            var model = new HeatNetworkPhaseModel { HeatNetworkPhase = "Design" };
            _controller.Url = SetUpBackLink("EnterHNLocation", "HeatNetwork").Object;
            // Act
            var result = _controller.EnterHNPhase(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HeatNetworkPhaseModel>(_controller.HttpContext, SessionKeys.HeatNetworkPhaseModelKey, model), Times.Once);
            _sessionHelperMock.Verify(x => x.SaveToSession<PathwayModel>(_controller.HttpContext, SessionKeys.PathwayModelKey, It.Is<PathwayModel>(p => p.Pathway == "2")), Times.Once);
            Assert.Equal("CheckYourAnswers", redirectResult.ActionName);
        }

        [Fact]
        public void EnterHNPhase_Post_ValidModel_RedirectsToHaveYouSignedMEContract()
        {
            // Arrange
            var model = new HeatNetworkPhaseModel { HeatNetworkPhase = "Construction" };
            _controller.Url = SetUpBackLink("EnterHNLocation", "HeatNetwork").Object;
            // Act
            var result = _controller.EnterHNPhase(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HeatNetworkPhaseModel>(_controller.HttpContext, SessionKeys.HeatNetworkPhaseModelKey, model), Times.Once);
            Assert.Equal("HaveYouSignedMEContract", redirectResult.ActionName);
        }

        [Fact]
        public void EnterHNPhase_Post_ValidModel_RedirectsToHNInOperation()
        {
            // Arrange
            var model = new HeatNetworkPhaseModel { HeatNetworkPhase = "Operation" };
            _controller.Url = SetUpBackLink("EnterHNLocation", "HeatNetwork").Object;
            // Act
            var result = _controller.EnterHNPhase(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HeatNetworkPhaseModel>(_controller.HttpContext, SessionKeys.HeatNetworkPhaseModelKey, model), Times.Once);
            Assert.Equal("HNInOperation", redirectResult.ActionName);
        }

        [Fact]
        public void EnterHNPhase_Post_InvalidModel_ReturnsViewWithModelError()
        {
            // Arrange
            var model = new HeatNetworkPhaseModel { HeatNetworkPhase = null };
            _controller.ModelState.AddModelError("HeatNetworkPhase", "The HeatNetworkPhase field is required.");
            _controller.Url = SetUpBackLink("EnterHNLocation", "HeatNetwork").Object;
            // Act
            var result = _controller.EnterHNPhase(model);
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(_controller.ModelState.ContainsKey("HeatNetworkPhase"));
            Assert.Contains("The HeatNetworkPhase field is required.", _controller.ModelState["HeatNetworkPhase"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void HaveYouSignedMEContract_Get_ReturnsViewWithModelFromSession()
        {
            // Arrange
            var model = new HaveYouSignedMEContractModel();
            _sessionHelperMock.Setup(x => x.GetFromSession<HaveYouSignedMEContractModel>(It.IsAny<HttpContext>(), SessionKeys.HaveYouSignedMEContractModelKey)).Returns(model);
            _controller.Url = SetUpBackLink("EnterHNPhase", "HeatNetwork").Object;
            // Act
            var result = _controller.HaveYouSignedMEContract();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void HaveYouSignedMEContract_Post_ValidModel_RedirectsToMEContractIsSigned()
        {
            // Arrange
            var model = new HaveYouSignedMEContractModel { HaveYouSignedMEContract = "yes" };
            _controller.Url = SetUpBackLink("MEContractIsSigned", "HeatNetwork").Object;
            // Act
            var result = _controller.HaveYouSignedMEContract(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HaveYouSignedMEContractModel>(_controller.HttpContext, SessionKeys.HaveYouSignedMEContractModelKey, model), Times.Once);
            Assert.Equal("MEContractIsSigned", redirectResult.ActionName);
        }

        [Fact]
        public void HaveYouSignedMEContract_Post_ValidModel_RedirectsToHasElementBeenRegistered()
        {
            // Arrange
            var model = new HaveYouSignedMEContractModel { HaveYouSignedMEContract = "no" };
            _controller.Url = SetUpBackLink("HasElementBeenRegistered", "HeatNetwork").Object;
            // Act
            var result = _controller.HaveYouSignedMEContract(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HaveYouSignedMEContractModel>(_controller.HttpContext, SessionKeys.HaveYouSignedMEContractModelKey, model), Times.Once);
            Assert.Equal("HasElementBeenRegistered", redirectResult.ActionName);
        }

        [Fact]
        public void HaveYouSignedMEContract_Post_InvalidModel_ReturnsViewWithModelError()
        {
            // Arrange
            var model = new HaveYouSignedMEContractModel { HaveYouSignedMEContract = null };
            _controller.ModelState.AddModelError("HaveYouSignedMEContract", "The HaveYouSignedMEContract field is required.");
            _controller.Url = SetUpBackLink("EnterHNPhase", "HeatNetwork").Object;
            // Act
            var result = _controller.HaveYouSignedMEContract(model);
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(_controller.ModelState.ContainsKey("HaveYouSignedMEContract"));
            Assert.Contains("The HaveYouSignedMEContract field is required.", _controller.ModelState["HaveYouSignedMEContract"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void HasElementBeenRegistered_Get_ReturnsViewWithModelFromSession()
        {
            // Arrange
            var model = new HasElementBeenRegisteredModel();
            _sessionHelperMock.Setup(x => x.GetFromSession<HasElementBeenRegisteredModel>(It.IsAny<HttpContext>(), SessionKeys.HasElementBeenRegisteredModelKey)).Returns(model);
            _controller.Url = SetUpBackLink("HaveYouSignedMEContract", "HeatNetwork").Object;
            // Act
            var result = _controller.HasElementBeenRegistered();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void HasElementBeenRegistered_Post_ValidModel_RedirectsToHasPlanningApplicationBeenSubmitted()
        {
            // Arrange
            var model = new HasElementBeenRegisteredModel { HasElementBeenRegistered = "yes" };
            _controller.Url = SetUpBackLink("HaveYouSignedMEContract", "HeatNetwork").Object;
            // Act
            var result = _controller.HasElementBeenRegistered(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HasElementBeenRegisteredModel>(_controller.HttpContext, SessionKeys.HasElementBeenRegisteredModelKey, model), Times.Once);
            Assert.Equal("HasPlanningApplicationBeenSubmitted", redirectResult.ActionName);
        }

        [Fact]
        public void HasElementBeenRegistered_Post_ValidModel_RedirectsToCheckYourAnswers()
        {
            // Arrange
            var model = new HasElementBeenRegisteredModel { HasElementBeenRegistered = "no" };
            _controller.Url = SetUpBackLink("HaveYouSignedMEContract", "HeatNetwork").Object;
            // Act
            var result = _controller.HasElementBeenRegistered(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HasElementBeenRegisteredModel>(_controller.HttpContext, SessionKeys.HasElementBeenRegisteredModelKey, model), Times.Once);
            _sessionHelperMock.Verify(x => x.SaveToSession<PathwayModel>(_controller.HttpContext, SessionKeys.PathwayModelKey, It.Is<PathwayModel>(p => p.Pathway == "1")), Times.Once);
            Assert.Equal("CheckYourAnswers", redirectResult.ActionName);
        }

        [Fact]
        public void HasElementBeenRegistered_Post_InvalidModel_ReturnsViewWithModelError()
        {
            // Arrange
            var model = new HasElementBeenRegisteredModel { HasElementBeenRegistered = null };
            _controller.ModelState.AddModelError("HasElementBeenRegistered", "The HasElementBeenRegistered field is required.");
            _controller.Url = SetUpBackLink("HaveYouSignedMEContract", "HeatNetwork").Object;
            // Act
            var result = _controller.HasElementBeenRegistered(model);
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(_controller.ModelState.ContainsKey("HasElementBeenRegistered"));
            Assert.Contains("The HasElementBeenRegistered field is required.", _controller.ModelState["HasElementBeenRegistered"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void HasPlanningApplicationBeenSubmitted_Get_ReturnsViewWithModelFromSession()
        {
            // Arrange
            var model = new HasPlanningApplicationBeenSubmittedModel();
            _sessionHelperMock.Setup(x => x.GetFromSession<HasPlanningApplicationBeenSubmittedModel>(It.IsAny<HttpContext>(), SessionKeys.HasPlanningApplicationBeenSubmittedModelKey)).Returns(model);
            _controller.Url = SetUpBackLink("HasElementBeenRegistered", "HeatNetwork").Object;

            // Act
            var result = _controller.HasPlanningApplicationBeenSubmitted();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void HasPlanningApplicationBeenSubmitted_Post_ValidModel_RedirectsToCheckYourAnswers_Pathway_3to7()
        {
            // Arrange
            var model = new HasPlanningApplicationBeenSubmittedModel { HasPlanningApplicationBeenSubmitted = "yes" };
            _controller.Url = SetUpBackLink("HasElementBeenRegistered", "HeatNetwork").Object;
            // Act
            var result = _controller.HasPlanningApplicationBeenSubmitted(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HasPlanningApplicationBeenSubmittedModel>(_controller.HttpContext, SessionKeys.HasPlanningApplicationBeenSubmittedModelKey, model), Times.Once);
            _sessionHelperMock.Verify(x => x.SaveToSession<PathwayModel>(_controller.HttpContext, SessionKeys.PathwayModelKey, It.Is<PathwayModel>(p => p.Pathway == "3")), Times.Once);
            Assert.Equal("CheckYourAnswers", redirectResult.ActionName);
        }

        [Fact]
        public void HasPlanningApplicationBeenSubmitted_Post_ValidModel_RedirectsToCheckYourAnswers_Pathway_1to7()
        {
            // Arrange
            var model = new HasPlanningApplicationBeenSubmittedModel { HasPlanningApplicationBeenSubmitted = "no" };
            _controller.Url = SetUpBackLink("HasElementBeenRegistered", "HeatNetwork").Object;
            // Act
            var result = _controller.HasPlanningApplicationBeenSubmitted(model);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            _sessionHelperMock.Verify(x => x.SaveToSession<HasPlanningApplicationBeenSubmittedModel>(_controller.HttpContext, SessionKeys.HasPlanningApplicationBeenSubmittedModelKey, model), Times.Once);
            _sessionHelperMock.Verify(x => x.SaveToSession<PathwayModel>(_controller.HttpContext, SessionKeys.PathwayModelKey, It.Is<PathwayModel>(p => p.Pathway == "1")), Times.Once);
            Assert.Equal("CheckYourAnswers", redirectResult.ActionName);
        }

        [Fact]
        public void HasPlanningApplicationBeenSubmitted_Post_InvalidModel_ReturnsViewWithModelError()
        {
            // Arrange
            var model = new HasPlanningApplicationBeenSubmittedModel { HasPlanningApplicationBeenSubmitted = null };
            _controller.ModelState.AddModelError("HasPlanningApplicationBeenSubmitted", "The HasPlanningApplicationBeenSubmitted field is required.");
            _controller.Url = SetUpBackLink("HasElementBeenRegistered", "HeatNetwork").Object;
            // Act
            var result = _controller.HasPlanningApplicationBeenSubmitted(model);
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(_controller.ModelState.ContainsKey("HasPlanningApplicationBeenSubmitted"));
            Assert.Contains("The HasPlanningApplicationBeenSubmitted field is required.", _controller.ModelState["HasPlanningApplicationBeenSubmitted"].Errors[0].ErrorMessage);
        }

        //[Fact]
        //public void CheckYourAnswers_ReturnsViewWithPopulatedModel()
        //{
        //    // Arrange
        //    var nameModel = new HeatNetworkNameModel { HeatNetworkName = "Test Network" };
        //    var locationModel = new HeatNetworkLocationModel { LatitudeLongitude = "56.123, -14.90" };
        //    var phaseModel = new HeatNetworkPhaseModel { HeatNetworkPhase = "design" };
        //    var pathwayModel = new PathwayModel { Pathway = "1" };

        //    _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkNameModel>(
        //        It.IsAny<HttpContext>(), SessionKeys.HeatNetworkNameModelKey)).Returns(nameModel);

        //    _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkLocationModel>(
        //        It.IsAny<HttpContext>(), SessionKeys.HeatNetworkLocationModelKey)).Returns(locationModel);

        //    // Act
        //    var result = _controller.CheckYourAnswers();

        //    // Assert
        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    var model = Assert.IsType<CheckYourAnswersHeatNetworkModel>(viewResult.Model);

        //    Assert.Equal(nameModel, model.HeatNetworkNameModel);
        //    Assert.Equal(locationModel, model.HeatNetworkLocationModel);
        //    Assert.False(model.ConfirmedDeclaration);
        //    Assert.False((bool)_controller.ViewBag.ShowBackButton);
        //}

        //[Fact]
        //public void CheckYourAnswers_MissingSessionData_ReturnsViewWithNullModelProperties()
        //{
        //    // Arrange
        //    _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkNameModel>(
        //        It.IsAny<HttpContext>(), SessionKeys.HeatNetworkNameModelKey)).Returns((HeatNetworkNameModel)null);

        //    _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkLocationModel>(
        //        It.IsAny<HttpContext>(), SessionKeys.HeatNetworkLocationModelKey)).Returns((HeatNetworkLocationModel)null);

        //    // Act
        //    var result = _controller.CheckYourAnswers();

        //    // Assert
        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    var model = Assert.IsType<CheckYourAnswersHeatNetworkModel>(viewResult.Model);

        //    Assert.Null(model.HeatNetworkNameModel);
        //    Assert.Null(model.HeatNetworkLocationModel);
        //    Assert.False(model.ConfirmedDeclaration);
        //    Assert.False((bool)_controller.ViewBag.ShowBackButton);
        //}

        // Add TC for testing SubmitAnswer method
        //[Fact]
        //public async Task SubmitAnswers_ValidModelState_ReturnsRedirectToConfirmation()
        //{
        //    // Arrange            
        //    var viewModel = new CheckYourAnswersHeatNetworkModel();

        //    var hnId = "user123";
        //    var heatNetworkNameModel = new HeatNetworkNameModel { HeatNetworkName = "Test Network" };
        //    var heatNetworkLocationModel = new HeatNetworkLocationModel { LatitudeLongitude = "Test Location" };
        //    var pathwayModel = new PathwayModel { Pathway = "Test Pathway" };

        //    _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkNameModel>(It.IsAny<HttpContext>(), SessionKeys.HeatNetworkNameModelKey))
        //        .Returns(heatNetworkNameModel);
        //    _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkLocationModel>(It.IsAny<HttpContext>(), SessionKeys.HeatNetworkLocationModelKey))
        //        .Returns(heatNetworkLocationModel);
        //    _sessionHelperMock.Setup(x => x.GetFromSession<PathwayModel>(It.IsAny<HttpContext>(), SessionKeys.PathwayModelKey))
        //        .Returns(pathwayModel);
        //    _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
        //        .Returns(hnId);

        //    _heatNetworkServiceMock.Setup(x => x.AddHeatNetwork(It.IsAny<HeatNetwork>()))
        //        .ReturnsAsync(new HeatNetworkResponse { HnId = "hn456", Name = "Test Network" });

        //    _organisationServiceMock.Setup(x => x.UpdateOrgHeatNetworkId(hnId, It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        //    // Act
        //    var result = await _controller.SubmitAnswers(viewModel);

        //    // Assert
        //    var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal("Confirmation", redirectResult.ActionName);
        //    Assert.Equal("HeatNetwork", redirectResult.ControllerName);
        //}

        //[Fact]
        //public async Task SubmitAnswers_UserResponseHasNoHnId_ReturnsCheckYourAnswersViewWithErrorMessage()
        //{
        //    // Arrange
        //    var viewModel = new CheckYourAnswersHeatNetworkModel();

        //    var hnId = "user123";
        //    var heatNetworkNameModel = new HeatNetworkNameModel { HeatNetworkName = "Test Network" };
        //    var heatNetworkLocationModel = new HeatNetworkLocationModel { LatitudeLongitude = "Test Location" };
        //    var pathwayModel = new PathwayModel { Pathway = "Test Pathway" };

        //    _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkNameModel>(It.IsAny<HttpContext>(), SessionKeys.HeatNetworkNameModelKey))
        //        .Returns(heatNetworkNameModel);
        //    _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkLocationModel>(It.IsAny<HttpContext>(), SessionKeys.HeatNetworkLocationModelKey))
        //        .Returns(heatNetworkLocationModel);
        //    _sessionHelperMock.Setup(x => x.GetFromSession<PathwayModel>(It.IsAny<HttpContext>(), SessionKeys.PathwayModelKey))
        //        .Returns(pathwayModel);
        //    _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
        //        .Returns(hnId);

        //    // Simulate AddHeatNetwork returning a response with null HnId
        //    _heatNetworkServiceMock.Setup(x => x.AddHeatNetwork(It.IsAny<HeatNetwork>()))
        //        .ReturnsAsync(new HeatNetworkResponse { HnId = null, Name = "Test Network" });

        //    // Act
        //    var result = await _controller.SubmitAnswers(viewModel);

        //    // Assert
        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    Assert.Equal("CheckYourAnswers", viewResult.ViewName);
        //    Assert.Equal(viewModel, viewResult.Model);

        //    // Verify TempData contains error message
        //    Assert.Equal("An error occurred while submitting your heat network details. Please try again later.",
        //                 _controller.TempData["ErrorMessage"]);
        //}


        // Add TC to mock GetUserById and confirmation page
        [Fact]
        public async Task Confirmation_ReturnsViewWithExpectedViewBagValues()
        {
            // Arrange
            var hnId = "user123";
            var userResponse = new UserDetailsResponse
            {
                FullName = "John Doe",
                Organisation = new OrganisationResponse { Name = "Test Company" }
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns(hnId);

            _userServiceMock.Setup(x => x.GetUserDetails(hnId))
                .ReturnsAsync(userResponse);

            // Initialize TempData
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new Mock<ITempDataProvider>();
            _controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);
            _controller.TempData["Confirmation_HN_Id"] = "hn456";
            _controller.TempData["HNName"] = "Test Network";

            // Act
            var result = await _controller.Confirmation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Confirmation", viewResult.ViewName);

            Assert.Equal("Test Company", _controller.ViewBag.CompanyName);
            Assert.Equal("John Doe", _controller.ViewBag.ContactName);
            Assert.Equal("hn456", _controller.ViewBag.HNId);
            Assert.Equal("Test Network", _controller.ViewBag.HNName);
        }

        [Fact]
        public async Task Confirmation_WhenTempDataIsEmpty_ViewBagHNValuesAreNull()
        {
            // Arrange
            var hnId = "user123";
            var userResponse = new UserDetailsResponse
            {
                FullName = "John Doe",
                Organisation = new OrganisationResponse { Name = "Test Company" }
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns(hnId);

            _userServiceMock.Setup(x => x.GetUserDetails(hnId))
                .ReturnsAsync(userResponse);

            // Initialize TempData but leave it empty
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new Mock<ITempDataProvider>();
            _controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);

            // Act
            var result = await _controller.Confirmation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Confirmation", viewResult.ViewName);

            Assert.Equal("Test Company", _controller.ViewBag.CompanyName);
            Assert.Equal("John Doe", _controller.ViewBag.ContactName);
            Assert.Null(_controller.ViewBag.HNId);
            Assert.Null(_controller.ViewBag.HNName);
        }

        //[Fact]
        //public async Task Details_ValidHnid_ReturnsViewWithModel()
        //{
        //    // Arrange
        //    var hnid = "hn123";
        //    var heatNetworkResponse = new HeatNetworkResponse
        //    {
        //        HnId = "HN123",
        //        Name = "Test Network",
        //        Location = "http://location.com",
        //        Pathway = "Test Pathway"
        //    };

        //    _heatNetworkServiceMock.Setup(x => x.GetAsync(hnid.ToUpper()))
        //        .ReturnsAsync(heatNetworkResponse);

        //    _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationName))
        //        .Returns("Test Organisation");
        //    _controller.Url = SetUpBackLink("HeatNetworks", "UserManagement").Object;

        //    // Act
        //    var result = await _controller.Details(hnid);

        //    // Assert
        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    var model = Assert.IsType<HNDetailsViewModel>(viewResult.Model);

        //    Assert.Equal("Test Network", model.Name);
        //    Assert.Equal("http://location.com", model.LocationUrl);
        //    Assert.Equal("Test Organisation", model.OrganisationName);
        //    Assert.Equal("Test Pathway", model.PathWay);
        //    Assert.Equal("HN123", model.UHNID);
        //}

        [Fact]
        public async Task Details_WhenResponseIsNull_ReturnsBadRequest()
        {
            // Arrange
            var hnid = "hn123";

            _heatNetworkServiceMock.Setup(x => x.GetAsync(hnid.ToUpper()))
                .ReturnsAsync((HeatNetworkResponse)null);
            _controller.Url = SetUpBackLink("HeatNetworks", "UserManagement").Object;

            // Act
            var result = await _controller.Details(hnid);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }
    }
}