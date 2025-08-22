using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Moq;

namespace HNTAS.Web.UI.Tests.Contollers
{
    public class OrganisationControllerTests
    {
        private readonly Mock<ICompaniesHouseService> _companiesHouseServiceMock;
        private readonly Mock<ILogger<OrganisationController>> _loggerMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ISessionHelper> _sessionHelperMock;

        public OrganisationControllerTests()
        {
            _companiesHouseServiceMock = new Mock<ICompaniesHouseService>();
            _loggerMock = new Mock<ILogger<OrganisationController>>();
            _userServiceMock = new Mock<IUserService>();
            _sessionHelperMock = new Mock<ISessionHelper>();
        }

        private OrganisationController CreateController()
        {
            var controller = new OrganisationController(
                _companiesHouseServiceMock.Object,
                _loggerMock.Object,
                _userServiceMock.Object,
                _sessionHelperMock.Object
            );

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new MockHttpSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        [Fact]
        public void Start_RedirectsToOrganisationType_And_CallsSessionHelpers()
        {
            // Arrange
            var controller = CreateController();
            var httpContext = controller.ControllerContext.HttpContext;

            // Act
            var result = controller.Start();

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("OrganisationType", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.ClearAllFlowRelatedSessionData(httpContext), Times.Once);
            _sessionHelperMock.Verify(x => x.SetIsCheckAnswerFlow(httpContext, false), Times.Once);
        }

        [Fact]
        public void OrganisationType_ReturnsView_WithExpectedModelAndViewBag()
        {
            var controller = CreateController();
            var httpContext = controller.ControllerContext.HttpContext;

            var expectedModel = new OrganisationModel
            {
                OrganisationTypes = new List<SelectListItem>(),
                SelectedOrganisationType = "UkCompaniesHouse"
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<OrganisationModel>(httpContext, SessionKeys.OrganisationCreation_SessionKey))
                .Returns(expectedModel);

            _sessionHelperMock
                .Setup(x => x.GetIsCheckAnswerFlow(httpContext))
                .Returns(false);

            var result = controller.OrganisationType();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OrganisationModel>(viewResult.Model);

            Assert.Equal(expectedModel.SelectedOrganisationType, model.SelectedOrganisationType);
            Assert.NotNull(model.OrganisationTypes);
            Assert.True(controller.ViewBag.ShowBackButton);
        }

        [Fact]
        public void Type_InvalidModel_ReturnsViewWithModel()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("SelectedOrganisationType", "Required");

            var model = new OrganisationModel();
            var result = controller.OrganisationType(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("OrganisationType", viewResult.ViewName);
            Assert.IsType<OrganisationModel>(viewResult.Model);
        }

        [Fact]
        public void Type_ValidModel_RedirectsToCompanyNumberOrOrganisationName()
        {
            var controller = CreateController();
            var model = new OrganisationModel
            {
                SelectedOrganisationType = "UkCompaniesHouse"
            };
            controller.ModelState.Clear();

            var result = controller.OrganisationType(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.True(redirect.ActionName == "CompanyNumber" || redirect.ActionName == "OrganisationName");
        }
    }
}