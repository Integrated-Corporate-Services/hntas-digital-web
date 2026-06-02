//using HNTAS.Api.Client.Api;
//using HNTAS.Api.Client.Model;
//using HNTAS.Web.UI.Controllers;
//using HNTAS.Web.UI.Helpers;
//using HNTAS.Web.UI.Models.ElementSoa;
//using HNTAS.Web.UI.Services.Core;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Routing;
//using Microsoft.AspNetCore.Mvc.ViewFeatures;
//using Microsoft.Extensions.Logging;
//using Moq;

//namespace HNTAS.Web.UI.Tests.Controllers
//{
//    public class ElementSoaControllerTests
//    {
//        private readonly Mock<ILogger<ElementSoaController>> _loggerMock;
//        private readonly Mock<ISessionHelper> _sessionHelperMock;
//        private readonly Mock<IHeatNetworkService> _heatNetworkServiceMock;
//        private readonly Mock<ISoaService> _soaServiceMock;
//        private readonly Mock<IAssessorApi> _assessorMock;
//        private readonly ElementSoaController _controller;

//        public ElementSoaControllerTests()
//        {
//            _loggerMock = new Mock<ILogger<ElementSoaController>>();
//            _sessionHelperMock = new Mock<ISessionHelper>();
//            _heatNetworkServiceMock = new Mock<IHeatNetworkService>();
//            _soaServiceMock = new Mock<ISoaService>();
//            _assessorMock = new Mock<IAssessorApi>();
//            _controller = CreateController();
//        }

//        private ElementSoaController CreateController()
//        {
//            var _controller = new ElementSoaController(
//                _sessionHelperMock.Object,
//                _soaServiceMock.Object,
//                _loggerMock.Object,
//                _heatNetworkServiceMock.Object,
//                _assessorMock.Object

//            );
//            var httpContext = new DefaultHttpContext();
//            httpContext.Session = new MockHttpSession();

//            _controller.ControllerContext = new ControllerContext
//            {
//                HttpContext = httpContext
//            };

//            var tempDataProvider = new Mock<ITempDataProvider>();
//            _controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);

//            return _controller;
//        }

//        private Mock<IUrlHelper> SetUpBackLink(string controller, string action)
//        {
//            var urlHelperMock = new Mock<IUrlHelper>();
//            urlHelperMock
//                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
//                    ctx.Action == action && ctx.Controller == controller)))
//                .Returns($"{controller}/{action}");
//            return urlHelperMock;
//        }

//        private Mock<IUrlHelper> SetUpBackLink(string controller, string action, string fragment)
//        {
//            var urlHelperMock = new Mock<IUrlHelper>();
//            urlHelperMock
//                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
//                    ctx.Action == action && ctx.Controller == controller && ctx.Fragment == fragment)))
//                .Returns($"{controller}/{action}/{fragment}");
//            return urlHelperMock;
//        }

//        [Fact]
//        public void UnderstandingSoa_Get_ReturnsView()
//        {
//            // Arrange

//            //_sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkNameModel>(It.IsAny<HttpContext>(), SessionKeys.HeatNetworkNameModelKey)).Returns(model);
//            _controller.Url = SetUpBackLink("NetworkDetails", "HeatNetwork").Object;

//            // Act
//            var result = _controller.UnderstandingSoa();

//            // Assert
//            Assert.IsType<ViewResult>(result);

//        }

//        [Fact]
//        public void SubmitUnderstandingSoa_Post_RedirectsToSoaStages()
//        {
//            // Arrange

//            // Act
//            var result = _controller.SubmitUnderstandingSoa();
//            // Assert
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal("SoaStages", redirectResult.ActionName);
//        }

//        [Fact]
//        public async Task SoaStages_Get_ReturnsViewWithModelAsync()
//        {
//            // Arrange
//            var hnId = "HN0000001";
//            var phase = "Design";
//            var currentStageIndex = 1;

//            var networkElements = new List<Element>
//            {
//                new Element
//                {
//                    ElementId = "00001",
//                    Type = HeatNetworkElementType.EnergyCentre,
//                    SoaStages = new List<SoaStages>
//                    {
//                        new SoaStages
//                        {
//                            StageId = (NullableOfSoaStage)SoaStage.Stage1,

//                        }
//                    }
//                }
//            };

//            var heatNetworkData = new HeatNetworkResponse
//            {
//                HnId = hnId,
//                Phase = phase,
//                NetworkElements = new NetworkElementsResponse
//                {
//                    Elements = networkElements
//                }
//            };

//            _sessionHelperMock
//                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId))
//                .Returns(hnId);

//            _sessionHelperMock
//                .Setup(x => x.GetFromSession<int?>(It.IsAny<HttpContext>(), SessionKeys.CurrentStageIndexSessionKey))
//                .Returns(currentStageIndex);

//            _heatNetworkServiceMock
//                .Setup(x => x.GetAsync(hnId.ToUpper()))
//                .ReturnsAsync(heatNetworkData);

//            _sessionHelperMock
//                .Setup(x => x.SaveToSession(
//                    It.IsAny<HttpContext>(),
//                    SessionKeys.ElementSoaIncompleteSoaSessionKey,
//                    It.IsAny<ElementSoaProgressStatusTracking>()));

//            _controller.Url = SetUpBackLink("UnderstandingSoa", "ElementSoa").Object;

//            // Act
//            var result = await _controller.SoaStages();

//            // Assert
//            var viewResult = Assert.IsType<ViewResult>(result);
//            Assert.Equal("SoaStages", viewResult.ViewName);

//            var model = Assert.IsType<ElementSoaViewModel>(viewResult.Model);
//            Assert.NotNull(model);
//            Assert.Equal(1, model.EligibleStageIndex);
//        }

//        [Fact]
//        public async Task SoaUpdateStatus_Get_ReturnsViewWithModelAsync()
//        {
//            // Arrange
//            var stage = SoaStage.Stage1;
//            var elementId = "00001";
//            var elementType = HeatNetworkElementType.EnergyCentre;
//            var hnId = "HN0000001";
//            _sessionHelperMock
//                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId))
//                .Returns(hnId);
//            _sessionHelperMock
//                .Setup(x => x.GetFromSession<int?>(It.IsAny<HttpContext>(), SessionKeys.CurrentStageIndexSessionKey))
//                .Returns(0);

//            _heatNetworkServiceMock
//                .Setup(x => x.GetAsync(It.IsAny<string>()))
//                .ReturnsAsync(new HeatNetworkResponse
//                {
//                    NetworkElements = new NetworkElementsResponse
//                    {
//                        Elements = new List<Element>
//                        {
//                            new Element
//                            {
//                                ElementId = elementId,
//                                Type = elementType,
//                                SoaStages = new List<SoaStages>
//                                {
//                                    new SoaStages
//                                    {
//                                        StageId = (NullableOfSoaStage)stage,
//                                        SoaStatuses = new List<SoaStatusWithCount>()
//                                    }
//                                }
//                            },
//                        }
//                    }
//                });

//            _controller.Url = SetUpBackLink("NetworkDetails", "HeatNetwork", "someFragment").Object;

//            // Act
//            var result = await _controller.SoaUpdateStatus(stage, elementId, elementType);
//            Assert.NotNull(result);
//        }

//        [Fact]
//        public async Task SoaUpdateStatus_Post_RedirectToAction()
//        {
//            // Arrange
//            var stage = SoaStage.Stage1;
//            var elementId = "99999";
//            var hnId = "HN0000001";
//            _sessionHelperMock
//                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId))
//                .Returns(hnId);

//            _sessionHelperMock
//                .Setup(x => x.GetFromSession<ElementSoaUpdateStatusViewModel>(It.IsAny<HttpContext>(), SessionKeys.ElementSoaStatusUpdateModelSessionKey))
//                .Returns(new ElementSoaUpdateStatusViewModel
//                {
//                    SoaStage = stage,
//                    ElementId = elementId,
//                    //SelectedSoaStatus = "In Progress"
//                });

//            _sessionHelperMock
//                .Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
//                .Returns("userid");

//            _sessionHelperMock
//                .Setup(x => x.GetFromSession<ElementSoaProgressStatusTracking>(It.IsAny<HttpContext>(), SessionKeys.ElementSoaIncompleteSoaSessionKey))
//                .Returns(new ElementSoaProgressStatusTracking
//                {
//                    AllElementsCompleted = false,
//                });

//            _heatNetworkServiceMock
//                .Setup(x => x.GetAsync(It.IsAny<string>()))
//                .ReturnsAsync(new HeatNetworkResponse());

//            var request = new ElementSoaUpdateStatusViewModel
//            {
//                SoaStage = stage,
//                ElementId = elementId,
//                //SelectedSoaStatus = "In progress"
//            };

//            _controller.Url = SetUpBackLink("NetworkDetails", "HeatNetwork", "someFragment").Object;

//            // Act
//            var result = await _controller.SoaUpdateStatus(request);

//            // Assert
//            Assert.NotNull(result);
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal("SoaStages", redirectResult.ActionName);
//        }

//    }
//}