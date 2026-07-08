using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.NetworkElements;
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
    public class NetworkElementsControllerTests
    {
        private readonly Mock<ILogger<NetworkElementsController>> _loggerMock;        
        private readonly Mock<IHeatNetworkService> _heatNetworksApiMock;        
        private readonly Mock<ISessionHelper> _sessionHelperMock;

        private readonly NetworkElementsController _controller;

        public NetworkElementsControllerTests()
        {
            _loggerMock = new Mock<ILogger<NetworkElementsController>>();
            _heatNetworksApiMock = new Mock<IHeatNetworkService>();
            _sessionHelperMock = new Mock<ISessionHelper>();
            _controller = CreateController();
        }

        private NetworkElementsController CreateController()
        {
            var controller = new NetworkElementsController(_loggerMock.Object, _heatNetworksApiMock.Object, _sessionHelperMock.Object );

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
        public async Task SelectNetworkElementsAsync_ModelIsNotNull_ReturnViewResult()
        {
            MockSessionObjects(NullableOfHeatNetworkType.Communal, true);
            _controller.Url = SetUpBackLink("NetworkDetails", "HeatNetwork").Object;
            var result = await _controller.SelectNetworkElementsAsync() as ViewResult;
            Assert.NotNull( result );
        }

        [Theory]
        [InlineData(NullableOfHeatNetworkType.Communal, true)]
        [InlineData(NullableOfHeatNetworkType.Communal, false)]
        [InlineData(NullableOfHeatNetworkType.District, true)]
        [InlineData(NullableOfHeatNetworkType.District, false)]
        public async Task SelectNetworkElementsAsync_ReturnViewResult(NullableOfHeatNetworkType networkType, bool hasOwnEc)
        {
            MockSessionObjects(networkType, hasOwnEc);
            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkElementViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.NetworkElementsViewModelSessionKey))
                .Returns((NetworkElementViewModel)null!);
            _controller.Url = SetUpBackLink("NetworkDetails", "HeatNetwork").Object;
            var result = await _controller.SelectNetworkElementsAsync() as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SelectNetworkElements_Communal_RedirectToAction()
        {
            MockSessionObjects(NullableOfHeatNetworkType.Communal, true);
            _controller.Url = SetUpBackLink("NetworkDetails", "HeatNetwork").Object;

            var model = new NetworkElementViewModel
            {
                ElementCounts = new Dictionary<Api.Client.Model.HeatNetworkElementType, int?>
                    {
                        { Api.Client.Model.HeatNetworkElementType.EnergyCentre, 1 }
                    },
                SelectedElementIds = new List<Api.Client.Model.HeatNetworkElementType>
                    {
                        Api.Client.Model.HeatNetworkElementType.EnergyCentre
                    }
            };

            var result = await _controller.SelectNetworkElements(model) as RedirectToActionResult;
            Assert.Equal("NetworkElementsOverView", result?.ActionName);
        }

        [Fact]
        public async Task SelectNetworkElements_District_RedirectToAction()
        {
            MockSessionObjects(NullableOfHeatNetworkType.District, true);
            _controller.Url = SetUpBackLink("NetworkDetails", "HeatNetwork").Object;

            var model = new NetworkElementViewModel
            {
                ElementCounts = new Dictionary<Api.Client.Model.HeatNetworkElementType, int?>
                    {
                        { Api.Client.Model.HeatNetworkElementType.EnergyCentre, 1 }
                    },
                SelectedElementIds = new List<Api.Client.Model.HeatNetworkElementType>
                    {
                        Api.Client.Model.HeatNetworkElementType.EnergyCentre
                    }
            };

            var result = await _controller.SelectNetworkElements(model) as RedirectToActionResult;
            Assert.Equal("Substations", result?.ActionName);
        }

        [Fact]
        public async Task SelectNetworkElements_ModelStateIsInvalid()
        {
            MockSessionObjects(NullableOfHeatNetworkType.District, true);
            _controller.Url = SetUpBackLink("NetworkDetails", "HeatNetwork").Object;

            var model = new NetworkElementViewModel
            {
                ElementCounts = new Dictionary<Api.Client.Model.HeatNetworkElementType, int?>
                    {
                        { Api.Client.Model.HeatNetworkElementType.EnergyCentre, 0 }
                    },
                SelectedElementIds = new List<Api.Client.Model.HeatNetworkElementType>
                    {
                        Api.Client.Model.HeatNetworkElementType.EnergyCentre
                    }
            };

            var result = await _controller.SelectNetworkElements(model) as ViewResult;
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal("SelectNetworkElements", result?.ViewName);
        }

        [Fact]
        public void Substations_ModelIsNotNull_ReturnView()
        {
            MockSessionObjects(NullableOfHeatNetworkType.District, true);
            _controller.Url = SetUpBackLink("SelectNetworkElements", "NetworkElements").Object;

            var result = _controller.Substations() as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void Substations_ReturnView()
        {
            MockSessionObjects(NullableOfHeatNetworkType.District, true);
            _controller.Url = SetUpBackLink("SelectNetworkElements", "NetworkElements").Object;
            _sessionHelperMock
                .Setup(x => x.GetFromSession<SubstationsViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.SubstationViewModelKey))
                .Returns(new SubstationsViewModel { HasDistrictSubstation = true, NumberOfSubstations = 2 });
            var result = _controller.Substations() as ViewResult;
            Assert.NotNull(result);
        }

        private void MockSessionObjects(NullableOfHeatNetworkType networkType, bool hasOwnEnergyCentre)
        {
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.HnId))
                .Returns("hn1");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.HnName))
                .Returns("network");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkElementViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.NetworkElementsViewModelSessionKey))
                .Returns(new NetworkElementViewModel
                {
                    ElementCounts = new Dictionary<Api.Client.Model.HeatNetworkElementType, int?>
                    {
                        { Api.Client.Model.HeatNetworkElementType.EnergyCentre, 1 }
                    },
                    SelectedElementIds = new List<Api.Client.Model.HeatNetworkElementType>
                    {
                        Api.Client.Model.HeatNetworkElementType.EnergyCentre
                    }
                });

            _heatNetworksApiMock.Setup(n => n.GetAsync(It.IsAny<string>())).ReturnsAsync(
                new HeatNetworkResponse
                {
                    Name = "network",
                    Phase = "Design",
                    HeatNetworkType = networkType,
                    HasOwnEnergyCentre = hasOwnEnergyCentre,
                    NetworkElements = new NetworkElementsResponse
                    {
                        ElementsGroup = new List<ElementGroup>
                        {
                            new ElementGroup
                            {
                                Count = 1,
                                ElementDisplayType = HeatNetworkElementType.DistrictDistribution,
                                ElementType = ElementTypeInShort.DDN,
                                SoaStages = new List<SoaStages> {
                                    new SoaStages
                                    {
                                        Assessors = new List<SoaAssessor>
                                        {
                                            new SoaAssessor
                                            {
                                                Assessment = "Assessment 1",
                                                Email = "test",
                                                FirstName = "First",
                                                LastName = "Last",
                                                Status = UserStatus.Active,
                                            }
                                        },
                                        StageId = NullableOfSoaStage.Stage1,
                                        SoaStatuses = new List<SoaStatusWithCount>
                                        {
                                            new SoaStatusWithCount
                                            {
                                                Count = 1,
                                                SoaStatus = SoaStatus.InProgress
                                            }
                                        },
                                    }
                                }
                            },

                        }
                    }

                });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<SubstationsViewModel>(
                    It.IsAny<HttpContext>(), SessionKeys.SubstationViewModelKey))
                .Returns(new SubstationsViewModel { HasDistrictSubstation = true, NumberOfSubstations = 2});
        }
    }
}
