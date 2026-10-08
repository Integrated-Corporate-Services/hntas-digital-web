using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class KpiDashboardControllerTests
    {
        private readonly Mock<ISessionHelper> _mockSessionHelper;
        private readonly Mock<IArmsDashboardService> _mockArmsDashboardService;

        private readonly KpiDashboardController _controller;

        public KpiDashboardControllerTests()
        {
            _mockSessionHelper = new Mock<ISessionHelper>();
            _mockArmsDashboardService = new Mock<IArmsDashboardService>();
            _controller = new KpiDashboardController(_mockSessionHelper.Object, _mockArmsDashboardService.Object);
        }

        private KpiDashboardController CreateController()
        {
            var controller = new KpiDashboardController(_mockSessionHelper.Object, _mockArmsDashboardService.Object);
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

        [Fact]
        public async Task Index_Post_ReturnsViewResult()
        {
            _controller.Url = SetUpBackLink("Index", "KpiDashboardController").Object;

            _mockSessionHelper
                .Setup(x => x.GetFromSession<bool>(
                    It.IsAny<HttpContext>(), SessionKeys.IsSuperUserKey))
                .Returns(false);

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModel_Id");

            _mockArmsDashboardService.Setup(x => x.GetKpiNetworksByRpUser(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new HeatNetworkDashboardResponse { CurrentPage = 1, TotalCount = 4, TotalPages = 1, Items = new List<HeatNetworkDashboardRow> { new HeatNetworkDashboardRow { HnId = "hn1", NetworkName = "testHn" } } });
            
            var result = await _controller.Index("testHn", 12, 2025, 1);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_Post_NoDashboardData_ReturnsViewResult()
        {
            _controller.Url = SetUpBackLink("Index", "KpiDashboardController").Object;

            _mockSessionHelper
                .Setup(x => x.GetFromSession<bool>(
                    It.IsAny<HttpContext>(), SessionKeys.IsSuperUserKey))
                .Returns(false);

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModel_Id");

            _mockArmsDashboardService.Setup(x => x.GetKpiNetworksByRpUser(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((HeatNetworkDashboardResponse)null!);

            var result = await _controller.Index("testHn", 12, 2025, 1);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_Post_SuperUser_NoDashboardData_ReturnsViewResult()
        {
            _controller.Url = SetUpBackLink("Index", "KpiDashboardController").Object;

            _mockSessionHelper
                .Setup(x => x.GetFromSession<bool?>(
                    It.IsAny<HttpContext>(), SessionKeys.IsSuperUserKey))
                .Returns(true);

            _mockSessionHelper
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModel_Id");

            _mockArmsDashboardService.Setup(x => x.GetKpiNetworksByRpUser(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((HeatNetworkDashboardResponse)null!);

            var result = await _controller.Index("testHn", 12, 2025, 1);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Details_Get_ReturnsViewResult()
        {
            _controller.Url = SetUpBackLink("Index", "KpiDashboardController").Object;

            _mockArmsDashboardService.Setup(a => a.GetKpiNetworkDetails(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<int>()))
                .ReturnsAsync(new HeatNetworkDetailsResponse { HnId = "hn1", NetworkName = "testHn", SelectedMonth = 12, SelectedYear = 2025, CurrentPage = 1, TotalPages = 4, TotalElements = 20, TotalCarbonEmission = 30, CarbonCalculationInputs = new Dictionary<string, CarbonInputUiDisplay> { { "key", new CarbonInputUiDisplay { Label = "lbl", Value = 20} } }, GroupedElements = new List<ElementGroupDto> { new ElementGroupDto { ElementType = "EC", ElementId = "EC1", Kpis = new List<KpiDetailDto> { new KpiDetailDto { KpiName = "kpiName", Value = 20, Status = "active"} } } }, AggregatedKpis = new List<AggregatedKpi> { new AggregatedKpi { KpiName = "kpiName", Value = 20, Status = "active" } } });

            var result = await _controller.Details(12, 2025, "submissionId", new List<string> { "statusFilter"},new List<string> { "typeFilter"}, 1 );

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Details_Get_NoSubmissionId_RedirectToActionResult()
        {
            _controller.Url = SetUpBackLink("Index", "KpiDashboardController").Object;

            _mockArmsDashboardService.Setup(a => a.GetKpiNetworkDetails(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<int>()))
                .ReturnsAsync(new HeatNetworkDetailsResponse { HnId = "hn1", NetworkName = "testHn", SelectedMonth = 12, SelectedYear = 2025, CurrentPage = 1, TotalPages = 4, TotalElements = 20, TotalCarbonEmission = 30, CarbonCalculationInputs = new Dictionary<string, CarbonInputUiDisplay> { { "key", new CarbonInputUiDisplay { Label = "lbl", Value = 20 } } }, GroupedElements = new List<ElementGroupDto> { new ElementGroupDto { ElementType = "EC", ElementId = "EC1", Kpis = new List<KpiDetailDto> { new KpiDetailDto { KpiName = "kpiName", Value = 20, Status = "active" } } } }, AggregatedKpis = new List<AggregatedKpi> { new AggregatedKpi { KpiName = "kpiName", Value = 20, Status = "active" } } });

            var result = await _controller.Details(12, 2025, "", new List<string> { "statusFilter" }, new List<string> { "typeFilter" }, 1);

            var resultVal = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", resultVal.ActionName);
        }

        [Fact]
        public async Task Details_Get_NetworkDetailsNotFound()
        {
            _controller.Url = SetUpBackLink("Index", "KpiDashboardController").Object;

            _mockArmsDashboardService.Setup(a => a.GetKpiNetworkDetails(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<int>()))
                .ReturnsAsync((HeatNetworkDetailsResponse)null!);

            var result = await _controller.Details(12, 2025, "subId", new List<string> { "statusFilter" }, new List<string> { "typeFilter" }, 1);

            Assert.IsType<NotFoundResult>(result);            
        }
    }
}
