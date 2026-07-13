//using HNTAS.Web.UI.Controllers;
//using HNTAS.Web.UI.Helpers;
//using HNTAS.Web.UI.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Routing;
//using Microsoft.Extensions.Logging;
//using Moq;

//namespace HNTAS.Web.UI.Tests.Contollers
//{
//    public class RegistrationEligibilityControllerTests
//    {
//        private readonly Mock<ILogger<HeatNetworkController>> _loggerMock;
//        private readonly Mock<ISessionHelper> _sessionHelperMock;

//        public RegistrationEligibilityControllerTests()
//        {
//            _loggerMock = new Mock<ILogger<HeatNetworkController>>();
//            _sessionHelperMock = new Mock<ISessionHelper>();
//        }
//        private RegistrationEligibilityController CreateController()
//        {
//            var controller = new RegistrationEligibilityController(
//                _sessionHelperMock.Object
//            );
//            var httpContext = new DefaultHttpContext();
//            httpContext.Session = new MockHttpSession();
//            controller.ControllerContext = new ControllerContext
//            {
//                HttpContext = httpContext
//            };

//            return controller;
//        }

//        private Mock<IUrlHelper> SetUpURL(string controller, string action)
//        {
//            var urlHelperMock = new Mock<IUrlHelper>();
//            urlHelperMock
//                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
//                    ctx.Action == action && ctx.Controller == controller)))
//                .Returns($"{controller}/{action}");
//            return urlHelperMock;
//        }

//        [Fact]
//        public void AreYouTheRP_Get_ReturnsViewWithModelFromSession()
//        {
//            // Arrange
//            var controller = CreateController();
//            var model = new AreYouTheRPModel { AreYouTheRP = "yes" };
//            _sessionHelperMock.Setup(x => x.GetFromSession<AreYouTheRPModel>(It.IsAny<HttpContext>(), SessionKeys.AreYouTheRPModelKey)).Returns(model);
//            var urlHelperMock = new Mock<IUrlHelper>();
//            controller.Url = SetUpURL("Home", "WhatDoYouWantToDo").Object;
//            // Act
//            var result = controller.AreYouTheRP() as ViewResult;
//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            Assert.True(controller.ViewBag.ShowBackButton);
//            Assert.Equal(model, viewResult.Model);
//            Assert.Equal("Home/WhatDoYouWantToDo", controller.ViewBag.BackLinkUrl);
//        }

//        [Fact]
//        public void AreYouTheRP_Post_ValidUrl_RedirectsToIsYourOrgWorkingOnANewHN()
//        {
//            // Arrange
//            var controller = CreateController();
//            var model = new AreYouTheRPModel { AreYouTheRP = "yes" };

//            controller.Url = SetUpURL("RegistrationEligibility", "IsYourOrgWorkingOnANewHN").Object;
//            // Act
//            var result = controller.AreYouTheRP(model);
//            // Assert
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            _sessionHelperMock.Verify(x => x.SaveToSession<AreYouTheRPModel>(controller.HttpContext, SessionKeys.AreYouTheRPModelKey, model), Times.Once);
//            Assert.Equal("IsYourOrgWorkingOnANewHN", redirectResult.ActionName);
//        }

//        [Fact]
//        public void AreYouTheRP_Post_ValidUrl_RedirectsToUserIsNotRP()
//        {
//            // Arrange
//            var controller = CreateController();
//            var model = new AreYouTheRPModel { AreYouTheRP = "no" };

//            controller.Url = SetUpURL("RegistrationEligibility", "UserIsNotRP").Object;
//            // Act
//            var result = controller.AreYouTheRP(model);
//            // Assert
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            _sessionHelperMock.Verify(x => x.SaveToSession<AreYouTheRPModel>(controller.HttpContext, SessionKeys.AreYouTheRPModelKey, model), Times.Once);
//            Assert.Equal("UserIsNotRP", redirectResult.ActionName);
//        }

//        [Fact]
//        public void AreYouTheRP_Post_InvalidValue_ReturnsViewWithModelError()
//        {
//            // Arrange
//            var controller = CreateController();
//            var model = new AreYouTheRPModel { AreYouTheRP = "maybe" }; // Invalid value
//            controller.ModelState.Clear(); // Ensure model state is valid
//            controller.Url = SetUpURL("RegistrationEligibility", "AreYouTheRP").Object;

//            // Act
//            var result = controller.AreYouTheRP(model) as ViewResult;

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(model, result.Model);
//            Assert.True(controller.ModelState.ContainsKey(nameof(model.AreYouTheRP)));
//            var error = controller.ModelState[nameof(model.AreYouTheRP)].Errors.FirstOrDefault();
//            Assert.NotNull(error);
//            Assert.Equal("Please select a valid option.", error.ErrorMessage);
//        }

//        [Fact]
//        public void IsYourOrgWorkingOnANewHN_Get_ReturnsViewWithModelFromSession()
//        {
//            // Arrange
//            var controller = CreateController();
//            var model = new IsYourOrgWorkingOnANewHNModel { IsYourOrgWorkingOnANewHN = "yes" };
//            _sessionHelperMock.Setup(x => x.GetFromSession<IsYourOrgWorkingOnANewHNModel>(It.IsAny<HttpContext>(), SessionKeys.IsYourOrgWorkingOnANewHNModelKey)).Returns(model);
//            var urlHelperMock = new Mock<IUrlHelper>();
//            controller.Url = SetUpURL("RegistrationEligibility", "AreYouTheRP").Object;
//            // Act
//            var result = controller.IsYourOrgWorkingOnANewHN() as ViewResult;
//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            Assert.True(controller.ViewBag.ShowBackButton);
//            Assert.Equal(model, viewResult.Model);
//            Assert.Equal("RegistrationEligibility/AreYouTheRP", controller.ViewBag.BackLinkUrl);
//        }

//        [Fact]
//        public void IsYourOrgWorkingOnANewHN_Post_ValidUrl_RedirectsToIsHNLocatedInEnglandScotlandWales()
//        {
//            // Arrange
//            var controller = CreateController();
//            var model = new IsYourOrgWorkingOnANewHNModel { IsYourOrgWorkingOnANewHN = "yes" };
//            controller.Url = SetUpURL("RegistrationEligibility", "IsHNLocatedInEnglandScotlandWales").Object;
//            // Act
//            var result = controller.IsYourOrgWorkingOnANewHN(model);
//            // Assert
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            _sessionHelperMock.Verify(x => x.SaveToSession<IsYourOrgWorkingOnANewHNModel>(controller.HttpContext, SessionKeys.IsYourOrgWorkingOnANewHNModelKey, model), Times.Once);
//            Assert.Equal("IsHNLocatedInEnglandScotlandWales", redirectResult.ActionName);
//        }

//        [Fact]
//        public void IsYourOrgWorkingOnANewHN_Post_ValidUrl_RedirectsToUserOrgNotEligible()
//        {
//            // Arrange
//            var controller = CreateController();
//            var model = new IsYourOrgWorkingOnANewHNModel { IsYourOrgWorkingOnANewHN = "no" };
//            controller.Url = SetUpURL("EndOfJourney", "HNIsOperationalRegisterLater").Object;
//            // Act
//            var result = controller.IsYourOrgWorkingOnANewHN(model);
//            // Assert
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            _sessionHelperMock.Verify(x => x.SaveToSession<IsYourOrgWorkingOnANewHNModel>(controller.HttpContext, SessionKeys.IsYourOrgWorkingOnANewHNModelKey, model), Times.Once);
//            Assert.Equal("HNIsOperationalRegisterLater", redirectResult.ActionName);
//        }

//        [Fact]
//        public void IsYourOrgWorkingOnANewHN_Post_InvalidValue_ReturnsViewWithModelError()
//        {
//            // Arrange
//            var controller = CreateController();
//            var model = new IsYourOrgWorkingOnANewHNModel { IsYourOrgWorkingOnANewHN = "maybe" }; // Invalid value
//            controller.ModelState.Clear(); // Ensure model state is valid
//            controller.Url = SetUpURL("RegistrationEligibility", "IsYourOrgWorkingOnANewHN").Object;
//            // Act
//            var result = controller.IsYourOrgWorkingOnANewHN(model) as ViewResult;
//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(model, result.Model);
//            Assert.True(controller.ModelState.ContainsKey(nameof(model.IsYourOrgWorkingOnANewHN)));
//            var error = controller.ModelState[nameof(model.IsYourOrgWorkingOnANewHN)].Errors.FirstOrDefault();
//            Assert.NotNull(error);
//            Assert.Equal("Please select a valid option.", error.ErrorMessage);
//        }

//        public void IsHNLocatedInEnglandScotlandWales_Get_ReturnsViewWithModelFromSession()
//        {
//            // Arrange
//            var controller = CreateController();
//            var model = new IsHNLocatedInEnglandScotlandWalesModel { IsHNLocatedInEnglandScotlandWales = "yes" };
//            _sessionHelperMock.Setup(x => x.GetFromSession<IsHNLocatedInEnglandScotlandWalesModel>(It.IsAny<HttpContext>(), SessionKeys.IsHNLocatedInEnglandScotlandWalesModelKey)).Returns(model);
//            var urlHelperMock = new Mock<IUrlHelper>();
//            controller.Url = SetUpURL("RegistrationEligibility", "IsYourOrgWorkingOnANewHN").Object;
//            // Act
//            var result = controller.IsHNLocatedInEnglandScotlandWales() as ViewResult;
//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            Assert.True(controller.ViewBag.ShowBackButton);
//            Assert.Equal(model, viewResult.Model);
//            Assert.Equal("RegistrationEligibility/IsYourOrgWorkingOnANewHN", controller.ViewBag.BackLinkUrl);
//        }

//        [Fact]
//        public void IsHNLocatedInEnglandScotlandWales_Post_ValidUrl_RedirectsToHowManyDwellingsIncluded()
//        {
//            // Arrange
//            var controller = CreateController();
//            var model = new IsHNLocatedInEnglandScotlandWalesModel { IsHNLocatedInEnglandScotlandWales = "yes" };
//            controller.Url = SetUpURL("RegistrationEligibility", "HowManyDwellingsIncluded").Object;
//            // Act
//            var result = controller.IsHNLocatedInEnglandScotlandWales(model);
//            // Assert
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            _session_helperMock.Verify(x => x.SaveToSession<IsHNLocatedInEnglandScotlandWalesModel>(controller.HttpContext, SessionKeys.IsHNLocatedInEnglandScotlandWalesModelKey, model), Times.Once);
//            Assert.Equal("HowManyDwellingsIncluded", redirectResult.ActionName);
//        }

//        [Fact]
//        public void IsHNLocatedInEnglandScotlandWales_Post_ValidUrl_RedirectsToHNNotINEnglandScotlandWales()
//        {
//            // Arrange
//            var controller = CreateController();
//            var model = new IsHNLocatedInEnglandScotlandWalesModel { IsHNLocatedInEnglandScotlandWales = "no" };
//            controller.Url = SetUpURL("EndOfJourney", "HNNotINEnglandScotlandWales").Object;
//            // Act
//            var result = controller.IsHNLocatedInEnglandScotlandWales(model);
//            // Assert
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            _sessionHelperMock.Verify(x => x.SaveToSession<IsHNLocatedInEnglandScotlandWalesModel>(controller.HttpContext, SessionKeys.IsHNLocatedInEnglandScotlandWalesModelKey, model), Times.Once);
//            Assert.Equal("HNNotINEnglandScotlandWales", redirectResult.ActionName);
//        }

//        [Fact]

//        public void IsHNLocatedInEnglandScotlandWales_Post_InvalidValue_ReturnsViewWithModelError()
//        {
//            // Arrange
//            var controller = CreateController();
//            var model = new IsHNLocatedInEnglandScotlandWalesModel { IsHNLocatedInEnglandScotlandWales = "maybe" }; // Invalid value
//            controller.ModelState.Clear(); // Ensure model state is valid
//            controller.Url = SetUpURL("RegistrationEligibility", "IsHNLocatedInEnglandScotlandWales").Object;
//            // Act
//            var result = controller.IsHNLocatedInEnglandScotlandWales(model) as ViewResult;
//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(model, result.Model);
//            Assert.True(controller.ModelState.ContainsKey(nameof(model.IsHNLocatedInEnglandScotlandWales)));
//            var error = controller.ModelState[nameof(model.IsHNLocatedInEnglandScotlandWales)].Errors.FirstOrDefault();
//            Assert.NotNull(error);
//            Assert.Equal("Please select a valid option.", error.ErrorMessage);
//        }

//        [Fact]
//        public void HowManyDwellingsIncluded_Get_ReturnsViewWithModelFromSession()
//        {
//            // Arrange
//            var controller = CreateController();
//            var model = new HowManyDwellingsIncludedModel { HowManyDwellingsIncluded = "yes" };
//            _session_helperMock.Setup(x => x.GetFromSession<HowManyDwellingsIncludedModel>(It.IsAny<HttpContext>(), SessionKeys.HowManyDwellingsIncludedModelKey)).Returns(model);
//            var urlHelperMock = new Mock<IUrlHelper>();
//            controller.Url = SetUpURL("RegistrationEligibility", "IsHNLocatedInEnglandScotlandWales").Object;
//            // Act
//            var result = controller.HowManyDwellingsIncluded() as ViewResult;
//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            Assert.True(controller.ViewBag.ShowBackButton);
//            Assert.Equal(model, viewResult.Model);
//            Assert.Equal("RegistrationEligibility/IsHNLocatedInEnglandScotlandWales", controller.ViewBag.BackLinkUrl);
//        }

//        [Fact]
//        public void HowManyDwellingsIncluded_Post_ValidUrl_RedirectsToEnterYourDetails()
//        {
//            // Arrange
//            var controller = CreateController();
//            var model = new HowManyDwellingsIncludedModel { HowManyDwellingsIncluded = "yes" };
//            controller.Url = SetUpURL("UserDetails", "YouAreEligible").Object;
//            // Act
//            var result = controller.HowManyDwellingsIncluded(model);
//            // Assert
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            _session_helperMock.Verify(x => x.SaveToSession<HowManyDwellingsIncludedModel>(controller.HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey, model), Times.Once);
//            Assert.Equal("YouAreEligible", redirectResult.ActionName);
//        }

//        [Fact]
//        public void HowManyDwellingsIncluded_Post_ValidUrl_RedirectsToLessThan10Dwellings()
//        {
//            // Arrange
//            var controller = CreateController();
//            var model = new HowManyDwellingsIncludedModel { HowManyDwellingsIncluded = "no" };
//            controller.Url = SetUpURL("EndOfJourney", "LessThan10Dwellings").Object;
//            // Act
//            var result = controller.HowManyDwellingsIncluded(model);
//            // Assert
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            _session_helperMock.Verify(x => x.SaveToSession<HowManyDwellingsIncludedModel>(controller.HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey, model), Times.Once);
//            Assert.Equal("LessThan10Dwellings", redirectResult.ActionName);
//        }

//        [Fact]
//        public void HowManyDwellingsIncluded_Post_InvalidValue_ReturnsViewWithModelError()
//        {
//            // Arrange
//            var controller = CreateController();
//            var model = new HowManyDwellingsIncludedModel { HowManyDwellingsIncluded = "maybe" }; // Invalid value
//            controller.ModelState.Clear(); // Ensure model state is valid
//            controller.Url = SetUpURL("RegistrationEligibility", "HowManyDwellingsIncluded").Object;
//            // Act
//            var result = controller.HowManyDwellingsIncluded(model) as ViewResult;
//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(model, result.Model);
//            Assert.True(controller.ModelState.ContainsKey(nameof(model.HowManyDwellingsIncluded)));
//            var error = controller.ModelState[nameof(model.HowManyDwellingsIncluded)].Errors.FirstOrDefault();
//            Assert.NotNull(error);
//            Assert.Equal("Please select a valid option.", error.ErrorMessage);
//        }
//    }
//}
