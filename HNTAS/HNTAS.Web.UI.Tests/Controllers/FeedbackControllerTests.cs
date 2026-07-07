using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNTAS.Web.UI.Tests.Contollers
{
    public class FeedbackControllerTests
    {
        private readonly Mock<IFeedbackApi> _feedbackApiMock;
        private readonly FeedbackController _controller;

        public FeedbackControllerTests()
        {
            _feedbackApiMock = new Mock<IFeedbackApi>();
            _controller = new FeedbackController(_feedbackApiMock.Object);
        }        

        [Fact]
        public void Index_Clear_ModelState_And_Return_View_With_New_FeedbackFormModel()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Error");
            _controller.Url = TestingUtility.SetUpBackLink("StartPage", "Home").Object;
            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Empty(_controller.ModelState);

            var model = Assert.IsType<FeedbackFormModel>(viewResult.Model);
            Assert.NotNull(model);
        }

        [Fact]
        public async Task Index_Post_Returns_View_When_ModelState_Is_Invalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Feedback", "Required");
            _controller.Url = TestingUtility.SetUpBackLink("StartPage", "Home").Object;
            var model = new FeedbackFormModel();

            // Act
            var result = await _controller.Index(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);

            _feedbackApiMock.Verify(
                x => x.ApiFeedbackPostAsync(It.IsAny<CreateFeedbackRequest>(), default),
                Times.Never);
        }

        [Fact]
        public async Task Index_Post_Redirects_To_FeedbackReceived_When_Api_Call_Succeeds()
        {
            // Arrange
            _controller.Url = TestingUtility.SetUpBackLink("StartPage", "Home").Object;
            var model = new FeedbackFormModel
            {
                SatisfactionLevel = "Satisfied",
                Feedback = "Test feedback"
            };

            _feedbackApiMock
                .Setup(x => x.ApiFeedbackPostAsync(It.IsAny<CreateFeedbackRequest>(), default))
                .ReturnsAsync(new Mock<IApiFeedbackPostApiResponse>().Object);

            // Act
            var result = await _controller.Index(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("FeedbackReceived", redirectResult.ActionName);

            _feedbackApiMock.Verify(
                x => x.ApiFeedbackPostAsync(It.Is<CreateFeedbackRequest>(r =>
                    r.SatisfactionLevel == model.SatisfactionLevel &&
                    r.FeedbackText == model.Feedback), default),
                Times.Once);
        }

        [Fact]
        public async Task Index_Post_Returns_View_With_Error_When_ApiException_Is_Thrown()
        {
            // Arrange
            _controller.Url = TestingUtility.SetUpBackLink("StartPage", "Home").Object;
            var model = new FeedbackFormModel();

            _feedbackApiMock
                .Setup(x => x.ApiFeedbackPostAsync(It.IsAny<CreateFeedbackRequest>(), default))
                .ThrowsAsync(new Api.Client.Client.ApiException(
                    reasonPhrase: "Internal Server Error",
                    statusCode: System.Net.HttpStatusCode.InternalServerError,
                    rawContent: "Test error"));

            // Act
            var result = await _controller.Index(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);

            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Index_Post_Returns_View_With_Error_When_Unexpected_Exception_Is_Thrown()
        {
            // Arrange
            _controller.Url = TestingUtility.SetUpBackLink("StartPage", "Home").Object;
            var model = new FeedbackFormModel();

            _feedbackApiMock
                .Setup(x => x.ApiFeedbackPostAsync(It.IsAny<CreateFeedbackRequest>(), default))
                .ThrowsAsync(new Exception("Test"));

            // Act
            var result = await _controller.Index(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);

            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public void FeedbackReceived_Returns_View()
        {
            //Arrange
            _controller.Url = TestingUtility.SetUpBackLink("Feedback" ,"Index").Object;

            // Act
            var result = _controller.FeedbackReceived();

            // Assert
            Assert.IsType<ViewResult>(result);
        }
    }
}
