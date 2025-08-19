using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;

namespace HNTAS.Web.UI.Tests.Controllers {
    public class DutyHolderControllerTests
    {
        private readonly Mock<ILogger<DutyHolderController>> _loggerMock;
        private readonly Mock<ISessionHelper> _sessionHelperMock;
        private readonly DutyHolderController _controller;

        public DutyHolderControllerTests()
        {
            //_loggerMock = new Mock<ILogger<DutyHolderController>>();
            _sessionHelperMock = new Mock<ISessionHelper>();
            //_controller = new DutyHolderController(_sessionHelperMock.Object);
            //_controller.ControllerContext = new ControllerContext{HttpContext = new DefaultHttpContext()};
        }

        private DutyHolderController CreateController()
        {
            var controller = new DutyHolderController(_sessionHelperMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns("/mocked-url");

            controller.Url = urlHelperMock.Object;
            return controller;
        }



        [Fact]
        public void Get_YouHaveBeenInvited_WithSessionData_ReturnsViewWithModel()
        {
            var expectedModel = new YouHaveBeenInvitedModel { AcceptInvitation = "accept" };
            _sessionHelperMock
                .Setup(x => x.GetFromSession<YouHaveBeenInvitedModel>(
                    It.IsAny<HttpContext>(), SessionKeys.YouHaveBeenInvitedModelKey))
                .Returns(expectedModel);
            var controller = CreateController();
            var result = controller.YouHaveBeenInvited() as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<YouHaveBeenInvitedModel>(result.Model);
            Assert.Equal("accept", ((YouHaveBeenInvitedModel)result.Model).AcceptInvitation);
        }

        [Fact]
        public void Get_YouHaveBeenInvited_WithoutSessionData_ReturnsViewWithNewModel()
        {
            _sessionHelperMock
                .Setup(x => x.GetFromSession<YouHaveBeenInvitedModel>(
                    It.IsAny<HttpContext>(), SessionKeys.YouHaveBeenInvitedModelKey))
                .Returns((YouHaveBeenInvitedModel)null);
            var controller = CreateController();
            var result = controller.YouHaveBeenInvited() as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<YouHaveBeenInvitedModel>(result.Model);
        }

        [Fact]
        public void Post_YouHaveBeenInvited_InvalidModel_ReturnsView()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("AcceptInvitation", "Required");

            var result = controller.YouHaveBeenInvited(new YouHaveBeenInvitedModel()) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Post_YouHaveBeenInvited_Accept_RedirectsToStartPage()
        {
            var model = new YouHaveBeenInvitedModel { AcceptInvitation = "accept" };
            var controller = CreateController();
            var result = controller.YouHaveBeenInvited(model) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("StartPage", result.ActionName);
            Assert.Equal("DutyHolder", result.ControllerName);
        }

        [Fact]
        public void Post_YouHaveBeenInvited_Decline_RedirectsToYouHaveDeclined()
        {
            var model = new YouHaveBeenInvitedModel { AcceptInvitation = "decline" };
            var controller = CreateController();
            var result = controller.YouHaveBeenInvited(model) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("YouHaveDeclined", result.ActionName);
            Assert.Equal("DutyHolder", result.ControllerName);
        }

        [Fact]
        public void Post_YouHaveBeenInvited_InvalidChoice_ReturnsViewWithError()
        {
            var model = new YouHaveBeenInvitedModel { AcceptInvitation = "maybe" };
            var controller = CreateController();
            var result = controller.YouHaveBeenInvited(model) as ViewResult;

            Assert.NotNull(result);
            Assert.True(controller.ModelState.ContainsKey(nameof(model.AcceptInvitation)));
        }

        [Fact]
        public void Get_YouHaveDeclined_ReturnsView()
        {
            var controller = CreateController();
            var result = controller.YouHaveDeclined() as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void Get_StartPage_ReturnsView()
        {
            var controller = CreateController();

            var result = controller.StartPage() as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Get_Dashboard_ReturnsViewWithModel()
        {
            var controller = CreateController();

            var result = controller.Dashboard() as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<DHDashboardModel>(result.Model);

            var model = result.Model as DHDashboardModel;
            Assert.Equal("ABC Org", model.OrganisationName);
            Assert.Equal("XyZ HN", model.HeatNetwork);
            Assert.Equal("Active", model.HNStatus);
        }


        [Fact]
        public void Get_Dashboard_ModelValuesAreUnexpected_FailsAssertion()
        {
            var controller = CreateController();

            var result = controller.Dashboard() as ViewResult;
            var model = result?.Model as DHDashboardModel;

            Assert.NotNull(model);
            Assert.NotEqual("Wrong Org", model.OrganisationName); // Negative check
            Assert.NotEqual("Inactive", model.HNStatus);          // Negative check
        }

    }
}
