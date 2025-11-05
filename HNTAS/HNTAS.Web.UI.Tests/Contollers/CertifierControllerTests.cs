using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Soa;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;


namespace HNTAS.Web.UI.Tests.Contollers
{
    public class CertifierControllerTests
    {
        private readonly Mock<ILogger<CertifierController>> _mockLogger;
        private readonly Mock<ISessionHelper> _mockSessionHelper;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IHeatNetworkService> _mockHeatNetworkService;
        private readonly Mock<ISoaService> _mockSoaService;
        private readonly Mock<IS3UploadService> _mockS3UploadService;
        private readonly CertifierController _controller;

        public CertifierControllerTests()
        {
            _mockLogger = new Mock<ILogger<CertifierController>>();
            _mockSessionHelper = new Mock<ISessionHelper>();
            _mockUserService = new Mock<IUserService>();
            _mockHeatNetworkService = new Mock<IHeatNetworkService>();
            _mockSoaService = new Mock<ISoaService>();
            _mockS3UploadService = new Mock<IS3UploadService>();
            _controller = CreateController();
        }

        private CertifierController CreateController() {
            var controller = new CertifierController(_mockLogger.Object, _mockSessionHelper.Object, _mockUserService.Object, _mockHeatNetworkService.Object, _mockSoaService.Object, _mockS3UploadService.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new MockHttpSession();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            return controller;
        }

        [Fact]
        public async Task HeatNetworkDetails_ValidHnid_ReturnsViewWithModel()
        {
            // Arrange
            var hnid = "hn123";
            var userResponse = TestingUtility.MockValid_UserService_GetUserDetails("user123");

            var hnDetails = TestingUtility.MockValid_HNService_GetAsync(hnid.ToUpper());

            _mockSessionHelper.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("user123");
            _mockSessionHelper.Setup(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.HnId, hnid.ToUpper()));
            _mockSessionHelper.Setup(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.HnName, hnDetails.Name));
            _mockSessionHelper.Setup(x => x.SaveToSession(It.IsAny<HttpContext>(), "PhaseData", It.IsAny<object>()));

            _mockUserService.Setup(x => x.GetUserDetails("user123")).ReturnsAsync(userResponse);
            _mockHeatNetworkService.Setup(x => x.GetAsync(hnid.ToUpper())).ReturnsAsync(hnDetails);

            _controller.Url = TestingUtility.SetUpBackLink("HeatNetworks", "UserManagement").Object;

            // Act
            var result = await _controller.HeatNetworkDetails(hnid);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HeatNetworkDetailsViewModel>(viewResult.Model);

            Assert.Equal("HN123", model.HnId);
            Assert.Equal("Test Network", model.HnName);
            Assert.Equal("///pretty.nice.stuff", model.HnLocation);
            Assert.Equal("Test Organisation", model.OrganisationName);
            Assert.Equal("123 Test St", model.OrganisationAddress.AddressLine1);
            Assert.Equal("1", model.Pathway);
            Assert.NotNull(model.Phases); // Ensure phases are populated
        }

        [Fact]
        public async Task HeatNetworkDetails_ReturnsBadRequest_WhenUserOrHnDetailsAreNull()
        {
            // Arrange
            var hnid = "invalidHnid";

            // Simulate session returning a user ID
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey)).Returns("userId");

            // Simulate user service returning null
            _mockUserService.Setup(u => u.GetUserDetails("userId")).ReturnsAsync(TestingUtility.MockValid_UserService_GetUserDetails("userId"));

            // Simulate heat network service returning null
            _mockHeatNetworkService.Setup(h => h.GetAsync(hnid.ToUpper())).ReturnsAsync((HeatNetworkResponse)null);
            _controller.Url = TestingUtility.SetUpBackLink("HeatNetworks", "UserManagement").Object;

            // Act
            var result = await _controller.HeatNetworkDetails(hnid);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DownloadTheDocuments_ReturnsViewResult_WithValidModel()
        {
            // Arrange
            var phase = 1;
            var hnId = "HN123";
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId)).Returns(hnId);
            _mockHeatNetworkService.Setup(h => h.GetAsync(hnId)).ReturnsAsync(TestingUtility.MockValid_HNService_GetAsync(hnId));
            _controller.Url = TestingUtility.SetUpBackLink("HeatNetworkDetails", "Certifier").Object;

            // Act
            var result = await _controller.DownloadTheDocuments(phase);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SOAReviewSummaryViewModel>(viewResult.Model);
            Assert.Equal("1", model.Phase);
            Assert.NotEmpty(model.Elements);
            Assert.NotEmpty(model.ElementDocuments);
            Assert.NotNull(model.AssessmentPlanDocument);
            Assert.NotNull(model.AssessorDocument);
        }

        [Fact]
        public async Task DownloadTheDocuments_ReturnsViewResult_WithEmptyModel_WhenHeatNetworkDataIsNull()
        {
            // Arrange
            var phase = 1;
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId))
                             .Returns("HN123");

            _mockHeatNetworkService.Setup(h => h.GetAsync(It.IsAny<string>()))
                                  .ReturnsAsync((HeatNetworkResponse)null); // Simulate null response
            _controller.Url = TestingUtility.SetUpBackLink("HeatNetworkDetails", "Certifier").Object;

            // Act
            var result = await _controller.DownloadTheDocuments(phase);

            // Assert

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(phase, viewResult.Model); // Since you're returning `View(phase)`
            var modelState = _controller.ModelState;
            Assert.False(modelState.IsValid);
            Assert.True(modelState.ContainsKey("HeatNetworkDetailsNotFound"));
            var error = modelState["HeatNetworkDetailsNotFound"].Errors.FirstOrDefault();
            Assert.NotNull(error);
            Assert.Equal("We couldn't retrieve the Heat Network details for the provided ID. Please try again or contact support.", error.ErrorMessage);

        }

        [Fact]
        public void SubmitDownloadTheDocuments_RedirectsToUploadCertificate_WithValidModel()
        {
            // Arrange
            var phase = 1;
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/Soa/DownloadTemplate?Phase=1");
            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = _controller.SubmitDownloadTheDocuments(phase);

            // Assert

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UploadCertificate", redirectResult.ActionName);
            Assert.Equal("Certifier", redirectResult.ControllerName);

            // ✅ Check RouteValues instead of casting
            Assert.True(redirectResult.RouteValues.ContainsKey("PhaseNumber"));
            Assert.Equal(phase, redirectResult.RouteValues["PhaseNumber"]);
            Assert.True(redirectResult.RouteValues.ContainsKey("TemplateDownloadUrl"));
            Assert.Equal("/Soa/DownloadTemplate?Phase=1", redirectResult.RouteValues["TemplateDownloadUrl"]);

        }

        [Fact]
        public void UploadCertificate_ReturnsViewResult_WithValidModel()
        {
            // Arrange
            var phase = 2;
            var hnName = "Test Heat Network";
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnName)).Returns(hnName);
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/Soa/DownloadTemplate?phase=2");
            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = _controller.UploadCertificate(phase);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UploadSOCViewModel>(viewResult.Model);

            Assert.Equal(phase, model.PhaseNumber);
            Assert.Equal("/Soa/DownloadTemplate?phase=2", model.TemplateDownloadUrl);
            Assert.Equal(hnName, _controller.ViewBag.HeatNetworkName);
        }

        [Fact]
        public void UploadCertificate_ReturnsViewResult_WithNullHeatNetworkName()
        {
            // Arrange
            var phase = 2;            
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnName)).Returns((string)null); // Simulate missing session data
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/Soa/DownloadTemplate?phase=2");
            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = _controller.UploadCertificate(phase);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UploadSOCViewModel>(viewResult.Model);

            Assert.Equal(phase, model.PhaseNumber);
            Assert.Equal("/Soa/DownloadTemplate?phase=2", model.TemplateDownloadUrl);
            Assert.Null(_controller.ViewBag.HeatNetworkName); // HeatNetworkName should be null
        }

        [Fact]
        public async Task SaveUploadCertificateAsync_RedirectsToDeclaration_WhenFileIsValid()
        {
            // Arrange
            var phase = 1;
            var hnId = "HN123";
            var userId = "User123";
            var s3Key = "soa/HN123/2/certifierSOC";            
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId)).Returns(hnId);
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey)).Returns(userId);
            var fileMock = new Mock<IFormFile>();
            var content = "Fake file content";
            var fileName = "test.pdf";
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            _mockS3UploadService.Setup(s => s.UploadFileAsync(fileMock.Object, It.IsAny<string>())).ReturnsAsync(s3Key);

            // Act
            var result = await _controller.SaveUploadCertificateAsync(phase, fileMock.Object);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Declaration", redirectResult.ActionName);
            _mockS3UploadService.Verify(s => s.UploadFileAsync(fileMock.Object, It.Is<string>(key => key.Contains(hnId))), Times.Once);
            _mockSoaService.Verify(s => s.UpdateDocument(It.IsAny<UpdateDocumentRequest>()), Times.Once);
        }

        [Fact]
        public async Task SaveUploadCertificateAsync_ReturnsView_WhenFileIsMissing()
        {
            // Arrange
            var phase = 1;
            var hnId = "HN123";            
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId)).Returns(hnId);
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnName)).Returns("Test Heat Network");
            _controller.Url = Mock.Of<IUrlHelper>(u => u.Action(It.IsAny<UrlActionContext>()) == "/Soa/DownloadTemplate?phase=2");

            // Act
            var result = await _controller.SaveUploadCertificateAsync(phase, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("UploadCertificate", viewResult.ViewName);

            var model = Assert.IsType<UploadSOCViewModel>(viewResult.Model);
            Assert.Equal(phase + 1, model.PhaseNumber); // Phase incremented in method
            Assert.Equal("/Soa/DownloadTemplate?phase=2", model.TemplateDownloadUrl);

            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("certifier"));
            var error = _controller.ModelState["certifier"].Errors.FirstOrDefault();
            Assert.Equal("Please select a file to upload.", error.ErrorMessage);
        }

        [Fact]
        public void Declaration_ReturnsViewResult_WithValidModelAndHeatNetworkName()
        {
            // Arrange
            var hnName = "Test Heat Network";
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnName)).Returns(hnName);
            _controller.Url = TestingUtility.SetUpBackLink("UploadCertificate", "Certifier").Object;

            // Act
            var result = _controller.Declaration();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CertifierConfirmationViewModel>(viewResult.Model);

            Assert.Equal(hnName, _controller.ViewBag.HeatNetworkName);
            Assert.NotNull(model); // Model should not be null
        }

        [Fact]
        public void Declaration_ReturnsViewResult_WithNullHeatNetworkName()
        {
            // Arrange
            var mockSessionHelper = new Mock<ISessionHelper>();
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnName)).Returns((string)null); // Simulate missing session data
            _controller.Url = TestingUtility.SetUpBackLink("UploadCertificate", "Certifier").Object;

            // Act
            var result = _controller.Declaration();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CertifierConfirmationViewModel>(viewResult.Model);

            Assert.Null(_controller.ViewBag.HeatNetworkName); // HeatNetworkName should be null
            Assert.NotNull(model); // Model should still be created
        }

        [Fact]
        public async Task SubmitDeclarationAsync_RedirectsToConfirmation_WhenIsConfirmedTrue()
        {
            // Arrange
            var hnName = "Test Heat Network";
            var hnId = "HN123";            
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnName)).Returns(hnName);
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId)).Returns(hnId);

            var model = new CertifierConfirmationViewModel { IsConfirmed = true };

            // Act
            var result = await _controller.SubmitDeclarationAsync(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Confirmation", redirectResult.ActionName);
            _mockSoaService.Verify(s => s.SendCertificationCompleteEmail(hnName, hnId), Times.Once);
        }

        [Fact]
        public async Task SubmitDeclarationAsync_ReturnsDeclarationView_WhenIsConfirmedFalse()
        {
            // Arrange
            var hnName = "Test Heat Network";            
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnName)).Returns(hnName);
            var model = new CertifierConfirmationViewModel { IsConfirmed = false };

            // Act
            var result = await _controller.SubmitDeclarationAsync(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Declaration", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);

            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(string.Empty));
            var error = _controller.ModelState[string.Empty].Errors.FirstOrDefault();
            Assert.Equal("You must confirm and accept the declaration to continue.", error.ErrorMessage);

            Assert.Equal(hnName, _controller.ViewBag.HeatNetworkName);
            _mockSoaService.Verify(s => s.SendCertificationCompleteEmail(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Confirmation_ReturnsViewResult_WithHeatNetworkName()
        {
            // Arrange
            var hnName = "Test Heat Network";
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnName)).Returns(hnName);

            // Act
            var result = _controller.Confirmation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(hnName, _controller.ViewBag.HeatNetworkName);
        }

        [Fact]
        public void Confirmation_ReturnsViewResult_WithNullHeatNetworkName()
        {
            // Arrange
            var mockSessionHelper = new Mock<ISessionHelper>();
            mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnName)).Returns((string)null); // Simulate missing session data

            // Act
            var result = _controller.Confirmation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(_controller.ViewBag.HeatNetworkName); // Should be null if session is missing
        }
    }
}
