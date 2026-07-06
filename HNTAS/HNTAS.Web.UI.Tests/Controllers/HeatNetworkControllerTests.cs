using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.Enums;
using HNTAS.Web.UI.Models.HeatNetwork;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class HeatNetworkControllerTests
    {
        private readonly Mock<IHeatNetworkService> _heatNetworkServiceMock = new();
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<ISessionHelper> _sessionHelperMock = new();
        private readonly Mock<IOrganisationService> _organisationServiceMock = new();
        private readonly Mock<IAddressLookupService> _addressLookupServiceMock = new();
        private readonly Mock<ILogger<HeatNetworkController>> _logger = new();

        private readonly HeatNetworkController _controller;

        public HeatNetworkControllerTests()
        {
            _controller = new HeatNetworkController(
                _logger.Object,
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
            _controller.Url = new Mock<IUrlHelper>().Object;
        }

        [Fact]
        public void Index_Should_ClearSession_AndRedirect()
        {
            // Act
            var result = _controller.Index();

            // Assert session cleared
            _sessionHelperMock.Verify(x =>
                x.ClearAllHNRegistrationFlowRelatedSessionData(It.IsAny<HttpContext>()),
                Times.Once);

            // Assert redirect
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkDwellingsCheck", redirect.ActionName);
            Assert.Equal("HeatNetworkRegistration", redirect.ControllerName);
        }

        [Fact]
        public void Index_Should_StillRedirect_EvenIfSessionHelperDoesNothing()
        {
            // No setup needed ? mock does nothing
            var result = _controller.Index();
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkDwellingsCheck", redirect.ActionName);
            Assert.Equal("HeatNetworkRegistration", redirect.ControllerName);
        }

        [Fact]
        public void SubmitDetails_Should_SaveSession_AndRedirect()
        {
            // Arrange
            var model = new HNDetailsViewModel
            {
                UHNID = "HN123",
                Name = "Test Network"
            };

            // Act
            var result = _controller.SubmitDetails(model);

            // Assert session saves
            _sessionHelperMock.Verify(x =>
                x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.HnId, "HN123"),
                Times.Once);

            _sessionHelperMock.Verify(x =>
                x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.HnName, "Test Network"),
                Times.Once);

            // Assert redirect
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("SOAIntro", redirect.ActionName);
            Assert.Equal("SOA", redirect.ControllerName);
        }

        [Fact]
        public void SubmitDetails_WithNullValues_ShouldStillSaveAndRedirect()
        {
            // Arrange
            var model = new HNDetailsViewModel
            {
                UHNID = null,
                Name = null
            };

            // Act
            var result = _controller.SubmitDetails(model);

            // Assert it still tries to save
            _sessionHelperMock.Verify(x =>
                x.SaveToSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId, null),
                Times.Once);

            _sessionHelperMock.Verify(x =>
                x.SaveToSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnName, null),
                Times.Once);

            // Assert redirect still happens
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("SOAIntro", redirect.ActionName);
            Assert.Equal("SOA", redirect.ControllerName);
        }

        [Fact]
        public async Task AddNetworkDetails_ReturnsView_WhenModelExists()
        {
            // Arrange
            var hnid = "ABC123";
            var response = new HNTAS.Api.Client.Model.HeatNetworkResponse
            {
                Name = "Test Network",
                Address = new HNTAS.Api.Client.Model.RegisteredAddress("Line1", "PC1"),
                Pathway = "Pathway1",
                HnId = hnid,
                Phase = "Phase1"
            };
            _heatNetworkServiceMock.Setup(s => s.GetAsync(hnid)).ReturnsAsync(response);
            _sessionHelperMock.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), It.IsAny<string>())).Returns(hnid);
            TestingUtility.SetUpBackLink("HeatNetworks", "UserManagement");

            // Act
            var result = await _controller.AddNetworkDetails(hnid);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("AddNetworkDetails", viewResult.ViewName);
            var model = Assert.IsType<HNDetailsViewModel>(viewResult.Model);
            Assert.Equal("Test Network", model.Name);
            Assert.Equal("Pathway1", model.PathWay);
            Assert.Equal("Phase1", model.Phase);
            Assert.Equal(hnid, model.UHNID);
        }

        [Fact]
        public async Task AddNetworkDetails_ReturnsBadRequest_WhenModelIsNull()
        {
            // Arrange
            var hnid = "NOTFOUND";
            _heatNetworkServiceMock.Setup(s => s.GetAsync(hnid)).ReturnsAsync((HNTAS.Api.Client.Model.HeatNetworkResponse?)null);
            _sessionHelperMock.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), It.IsAny<string>())).Returns(hnid);

            // Act
            var result = await _controller.AddNetworkDetails(hnid);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task NetworkDetails_ReturnsViewWithModel()
        {
            // Arrange
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId)).Returns("H1");
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnName)).Returns("HN");
            _heatNetworkServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(new HNTAS.Api.Client.Model.HeatNetworkResponse
            {
                NetworkElements = new HNTAS.Api.Client.Model.NetworkElementsResponse()
            });

            // Act
            var result = await _controller.NetworkDetails();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("NetworkDetails", view.ViewName);
            Assert.IsType<NetworkDetailsViewModel>(view.Model);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.HnId, "H1"), Times.Once);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.HnName, "HN"), Times.Once);
        }

        [Fact]
        public async Task SelectNetworkDetail_NetworkElements_Redirects()
        {

            // Act
            var result = await _controller.SelectNetworkDetail("H1", NetworkDetailsType.NetworkElements);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("SelectNetworkElements", redirect.ActionName);
            Assert.Equal("NetworkElements", redirect.ControllerName);
        }

        [Fact]
        public async Task SelectNetworkDetail_Soa_Redirects()
        {

            // Act
            var result = await _controller.SelectNetworkDetail("H1", NetworkDetailsType.Soa);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UnderstandingSoa", redirect.ActionName);
            Assert.Equal("ElementSoa", redirect.ControllerName);
        }

        [Fact]
        public async Task SelectNetworkDetail_Default_ReturnsBadRequest()
        {

            // Act
            var result = await _controller.SelectNetworkDetail("H1", (NetworkDetailsType)999);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

    }

}