using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class AuditHistoryControllerTests
    {
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly Mock<ILogger<AuditHistoryController>> _loggerMock;
        private readonly AuditHistoryController _controller;

        public AuditHistoryControllerTests()
        {
            _mockAuditService = new Mock<IAuditService>();
            _loggerMock = new Mock<ILogger<AuditHistoryController>>();
            _controller = CreateController();
        }

        private AuditHistoryController CreateController()
        {
            var controller = new AuditHistoryController(
                _mockAuditService.Object, _loggerMock.Object
                );
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
        public async Task Index_ReturnsViewResult_WithAuditHistory()
        {
            // Arrange
            _controller.Url = SetUpBackLink("HeatNetworks", "UserManagement").Object;
            var userId = "testUser";
            var auditLogResponse = new AuditLogResponse
            {
                Items = new List<AuditLog>
                {
                    new AuditLog { },                    
                },
                TotalCount = 4,
                TotalPages = 1
            };
            _mockAuditService.Setup(service => service.GetAuditHistoryByHnId(It.IsAny<AuditLogRequest>()))
                .ReturnsAsync(auditLogResponse);
            // Act
            var result = await _controller.Index(userId);
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<AuditLogResponse>(viewResult.Model);
            Assert.Equal(1, model.Items.Count);
        }

        [Fact]
        public async Task Index_Exception_ReturnsViewResult_EmptyAuditHistory()
        {
            // Arrange
            _controller.Url = SetUpBackLink("HeatNetworks", "UserManagement").Object;
            var userId = "testUser";           
            _mockAuditService.Setup(service => service.GetAuditHistoryByHnId(It.IsAny<AuditLogRequest>()))
                .Throws(new Exception("Test exception"));
            // Act
            var result = await _controller.Index(userId);
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<AuditLogResponse>(viewResult.Model);
            Assert.Equal(0, model.Items.Count);
        }        
    }
}
