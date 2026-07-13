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
    public class NotificationControllerTests
    {
        private readonly Mock<INotificationHistoryService> _mockNotificationHistoryService;
        private readonly Mock<ILogger<NotificationController>> _loggerMock;
        private readonly NotificationController _controller;

        public NotificationControllerTests()
        {
            _mockNotificationHistoryService = new Mock<INotificationHistoryService>();
            _loggerMock = new Mock<ILogger<NotificationController>>();
            _controller = CreateController();
        }

        private NotificationController CreateController()
        {
            var controller = new NotificationController(
                _mockNotificationHistoryService.Object, _loggerMock.Object
                );
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var tempData = new TempDataDictionary(controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;
            return controller;
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithNotificationHistory()
        {
            // Arrange
            var userId = "testUser";
            var notificationHistoryResponse = new NotificationHistoryResponse
            {
                Items = new List<NotificationHistoryData>
                {
                    new NotificationHistoryData { Id = "1", Timestamp = DateTime.UtcNow },
                    
                },
            };
            _mockNotificationHistoryService.Setup(service => service.GetNotificationHistory(It.IsAny<NotificationHistoryRequest>()))
                .ReturnsAsync(notificationHistoryResponse);
            // Act
            var result = await _controller.Index(userId);
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<NotificationHistoryResponse>(viewResult.Model);
            Assert.Equal(1, model.Items.Count);
        }

        [Fact]
        public async Task Index_Exception_ReturnsViewResult_EmpbyNotificationHistory()
        {
            // Arrange
            var userId = "testUser";
            var notificationHistoryResponse = new NotificationHistoryResponse
            {
                Items = new List<NotificationHistoryData>
                {
                    new NotificationHistoryData { Id = "1", Timestamp = DateTime.UtcNow },

                },
            };
            _mockNotificationHistoryService.Setup(service => service.GetNotificationHistory(It.IsAny<NotificationHistoryRequest>()))
                .Throws(new Exception("Test exception"));
            // Act
            var result = await _controller.Index(userId);
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<NotificationHistoryResponse>(viewResult.Model);
            Assert.Equal(0, model.Items.Count);
        }

        [Theory]
        [InlineData("Heat network details", "AddNetworkDetails")]
        [InlineData("DDH and contributors", "ManageContributors")]
        [InlineData("Network managers", "ManageLeads")]        
        public async Task ExecuteAction_ReturnsRedirectResult_WhenActionDetailsAreValid(string action, string targetAction)
        {
            // Arrange
            var actionDetails = new Dictionary<string, string>
            {
                { "hnid", "testHnid" },
                { "action", action }
            };
            // Act
            var result = await _controller.ExecuteAction(actionDetails);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(targetAction, redirectResult.ActionName);
        }
    }
}
