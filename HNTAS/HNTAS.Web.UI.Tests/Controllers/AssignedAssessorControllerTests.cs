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
    public class AssignedAssessorControllerTests
    {
        private readonly Mock<IAssignedAssessorService> _mockAssignedAssessorService;
        private readonly Mock<ILogger<AssignedAssessorController>> _loggerMock;
        private readonly AssignedAssessorController _controller;

        public AssignedAssessorControllerTests()
        {
            _mockAssignedAssessorService = new Mock<IAssignedAssessorService>();
            _loggerMock = new Mock<ILogger<AssignedAssessorController>>();
            _controller = CreateController();
        }

        private AssignedAssessorController CreateController()
        {
            var controller = new AssignedAssessorController(
                _mockAssignedAssessorService.Object, _loggerMock.Object
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
        public async Task Index_ReturnsViewResult_WithAssignedAssessor()
        {
            // Arrange
            var userId = "testUser";
            var assignedAssessorResponse = new AssignedAssessorResponse
            {
                Items = new List<AssignedAssessor>
                {
                    new AssignedAssessor { },                    
                },
                TotalCount = 4,
                TotalPages = 1
            };
            _mockAssignedAssessorService.Setup(service => service.GetAssignedAssessor(It.IsAny<AssignedAssessorRequest>()))
                .ReturnsAsync(assignedAssessorResponse);
            // Act
            var result = await _controller.Index(userId);
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<AssignedAssessorResponse>(viewResult.Model);
            Assert.Equal(1, model.Items.Count);
        }

        [Fact]
        public async Task Index_Exception_ReturnsViewResult_EmptyAssignedAssessor()
        {
            // Arrange
            var userId = "testUser";
            var assignedAssessorResponse = new AssignedAssessorResponse
            {
                Items = new List<AssignedAssessor>
                {
                    new AssignedAssessor { },
                },
                TotalCount = 4,
                TotalPages = 1
            };
            _mockAssignedAssessorService.Setup(service => service.GetAssignedAssessor(It.IsAny<AssignedAssessorRequest>()))
                .Throws(new Exception("Test exception"));
            // Act
            var result = await _controller.Index(userId);
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<AssignedAssessorResponse>(viewResult.Model);
            Assert.Equal(0, model.Items.Count);
        }        
    }
}
