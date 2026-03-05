using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.ElementSoa;
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
    public class ElementSoaControllerTests
    {
        private readonly Mock<ILogger<ElementSoaController>> _loggerMock;
        private readonly Mock<ISessionHelper> _sessionHelperMock;
        private readonly Mock<IHeatNetworkService> _heatNetworkServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IAddressLookupService> _addressLookupServiceMock;
        private readonly Mock<IOrganisationService> _organisationServiceMock;
        private readonly Mock<ISoaService> _soaServiceMock;
        private readonly Mock<IS3UploadService> _s3UploadServiceMock;
        private readonly ElementSoaController _controller;

        public ElementSoaControllerTests()
        {
            _loggerMock = new Mock<ILogger<ElementSoaController>>();
            _sessionHelperMock = new Mock<ISessionHelper>();
            _heatNetworkServiceMock = new Mock<IHeatNetworkService>();
            _userServiceMock = new Mock<IUserService>();
            _addressLookupServiceMock = new Mock<IAddressLookupService>();
            _organisationServiceMock = new Mock<IOrganisationService>();
            _soaServiceMock = new Mock<ISoaService>();
            _s3UploadServiceMock = new Mock<IS3UploadService>();
            _controller = CreateController();
        }

        private ElementSoaController CreateController()
        {
            var _controller = new ElementSoaController(
                _sessionHelperMock.Object,
                _soaServiceMock.Object,
                _loggerMock.Object,
                _s3UploadServiceMock.Object,
                _heatNetworkServiceMock.Object

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

        [Fact]
        public void UnderstandingSoa_Get_ReturnsView()
        {
            // Arrange

            //_sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkNameModel>(It.IsAny<HttpContext>(), SessionKeys.HeatNetworkNameModelKey)).Returns(model);
            _controller.Url = SetUpBackLink("NetworkDetails", "HeatNetwork").Object;

            // Act
            var result = _controller.UnderstandingSoa();

            // Assert
            Assert.IsType<ViewResult>(result);

        }

        [Fact]
        public void SubmitUnderstandingSoa_Post_RedirectsToSoaStages()
        {
            // Arrange

            // Act
            var result = _controller.SubmitUnderstandingSoa();
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("SoaStages", redirectResult.ActionName);
        }

        [Fact]
        public async Task SoaStages_Get_ReturnsViewWithModelAsync()
        {
            // Arrange
            var hnId = "HN0000001";
            var phase = "Design";
            var currentStageIndex = 1;

            var networkElements = new List<Element>
            {
                new Element
                {
                    ElementId = "00001",
                    Type = HeatNetworkElementDisplayType.EnergyCentre,
                    SoaStages = new List<SoaStages>
                    {
                        new SoaStages
                        {
                            StageId = (NullableOfSoaStage)SoaStage.Stage1,
                            Document = new NetworkDetailsUploadedDocument
                            {
                                FileName = "test.pdf",
                                S3Key = "path/to/test.pdf",
                                UploadedBy = "user123"
                            }
                        }
                    }
                }
            };

            var heatNetworkData = new HeatNetworkResponse
            {
                HnId = hnId,
                Phase = phase,
                NetworkElements = new NetworkElementsResponse
                {
                    Elements = networkElements
                }
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId))
                .Returns(hnId);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<int?>(It.IsAny<HttpContext>(), SessionKeys.CurrentStageIndexSessionKey))
                .Returns(currentStageIndex);

            _heatNetworkServiceMock
                .Setup(x => x.GetAsync(hnId.ToUpper()))
                .ReturnsAsync(heatNetworkData);

            _sessionHelperMock
                .Setup(x => x.SaveToSession(
                    It.IsAny<HttpContext>(),
                    SessionKeys.ElementSoaIncompleteSoaSessionKey,
                    It.IsAny<ElementSoaProgressStatusTracking>()));

            _controller.Url = SetUpBackLink("UnderstandingSoa", "ElementSoa").Object;

            // Act
            var result = await _controller.SoaStages();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("SoaStages", viewResult.ViewName);

            var model = Assert.IsType<ElementSoaViewModel>(viewResult.Model);
            Assert.NotNull(model);
            Assert.Equal(1, model.EligibleStageIndex);
        }

        [Fact]
        public async Task SoaStagesToUpload_Get_WithDocument_ReturnsViewWithUploadedDocument()
        {
            // Arrange
            var hnId = "HN0000001";
            var elementId = "00001";
            var stage = SoaStage.Stage1;
            var elementType = HeatNetworkElementDisplayType.EnergyCentre;
            var fileName = "test.pdf";
            var s3Key = "path/to/test.pdf";
            var uploadedBy = "user123";
            var hnName = "Test Heat Network";

            var networkElements = new List<Element>
            {
                new Element
                {
                    ElementId = elementId,
                    Type = elementType,
                    SoaStages = new List<SoaStages>
                    {
                        new SoaStages
                        {
                            StageId = (NullableOfSoaStage)stage,
                            Document = new NetworkDetailsUploadedDocument
                            {
                                FileName = fileName,
                                S3Key = s3Key,
                                UploadedBy = uploadedBy
                            }
                        }
                    }
                }
            };

            var heatNetworkData = new HeatNetworkResponse
            {
                HnId = hnId,
                NetworkElements = new NetworkElementsResponse
                {
                    Elements = networkElements
                }
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId))
                .Returns(hnId);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnName))
                .Returns(hnName);

            _heatNetworkServiceMock
                .Setup(x => x.GetAsync(hnId.ToUpper()))
                .ReturnsAsync(heatNetworkData);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns("/ElementSoa/DownloadFile");
            _controller.Url = urlHelperMock.Object;

            // Act
            var result = await _controller.SoaStagesToUpload(stage, elementId, elementType);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("SoaUpload", viewResult.ViewName);

            var model = Assert.IsType<ElementSoaUploadViewModel>(viewResult.Model);
            Assert.NotNull(model);
            Assert.Equal(elementId, model.ElementId);
            Assert.Equal(stage, model.SoaStage);
            Assert.Equal(elementType, model.Type);
            Assert.Equal(hnName, model.HeatNetworkName);

            Assert.NotNull(model.UploadedDocument);
            Assert.Equal(fileName, model.UploadedDocument.FileName);
            Assert.Equal(s3Key, model.UploadedDocument.S3Key);
            Assert.Equal(uploadedBy, model.UploadedDocument.UploadedBy);

            // Verify session save
            _sessionHelperMock.Verify(
                x => x.SaveToSession(
                    It.IsAny<HttpContext>(),
                    SessionKeys.ElementSoaUploadViewModelSessionKey,
                    It.IsAny<ElementSoaUploadViewModel>()),
                Times.Once);

            // Verify ViewBag properties
            Assert.NotNull(_controller.ViewBag.Heading);
            Assert.NotNull(_controller.ViewBag.Description1);
            Assert.NotNull(_controller.ViewBag.Description2);
        }

        [Fact]
        public async Task SoaStagesToUpload_Post_WithAllElementsCompleted_SetsStatusToComplete()
        {
            // Arrange
            var hnId = "HN0000001";
            var userId = "user123";
            var elementId = "00001";
            var stage = SoaStage.Stage1;

            var incompleteSoa = new ElementSoaProgressStatusTracking
            {
                AllElementsCompleted = true
            };

            var model = new ElementSoaUploadViewModel
            {
                ElementId = elementId,
                SoaStage = stage,
                Type = HeatNetworkElementDisplayType.EnergyCentre
            };

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.pdf");
            mockFile.Setup(f => f.Length).Returns(100);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<ElementSoaUploadViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.ElementSoaUploadViewModelSessionKey))
                .Returns(model);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<int?>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.CurrentStageIndexSessionKey))
                .Returns(0);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<ElementSoaProgressStatusTracking>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.ElementSoaIncompleteSoaSessionKey))
                .Returns(incompleteSoa);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnId))
                .Returns(hnId);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.UserModel_Id_SessionKey))
                .Returns(userId);

            _s3UploadServiceMock
                .Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync("s3key");

            _controller.Url = SetUpBackLink("SoaStages", "ElementSoa").Object;

            // Act
            var result = await _controller.SoaStagesToUpload(mockFile.Object);

            // Assert
            _soaServiceMock.Verify(
                x => x.UpdateDocumentSoa(It.Is<ElementSoaUploadDocumentRequest>(r =>
                    r.ElementSoaStatus == NetworkDetailsStatus.Complete)),
                Times.Once);
        }
    }
}