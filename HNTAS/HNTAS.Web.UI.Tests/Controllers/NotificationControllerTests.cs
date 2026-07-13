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
    }
}
