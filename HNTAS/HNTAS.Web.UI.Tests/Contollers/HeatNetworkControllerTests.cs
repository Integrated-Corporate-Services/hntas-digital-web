using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
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


        //[Fact]
        //public async Task Details_WhenResponseIsNull_ReturnsBadRequest()
        //{
        //    // Arrange
        //    var hnid = "hn123";

        //    _heatNetworkServiceMock.Setup(x => x.GetAsync(hnid.ToUpper()))
        //        .ReturnsAsync((HeatNetworkResponse)null);
        //    _controller.Url = SetUpBackLink("HeatNetworks", "UserManagement").Object;

        //    // Act
        //    var result = await _controller.Details(hnid);

        //    // Assert
        //    Assert.IsType<BadRequestResult>(result);
        //}
    }
}