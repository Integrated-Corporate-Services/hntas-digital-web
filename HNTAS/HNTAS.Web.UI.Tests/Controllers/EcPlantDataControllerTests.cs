using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.EcPlantData;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class EcPlantDataControllerTests
    {
        private readonly Mock<ILogger<EcPlantDataController>> _loggerMock = new();
        private readonly Mock<IHeatNetworkService> _heatNetworkServiceMock = new();
        private readonly Mock<IEnergyCentrePlantDataService> _energyCentrePlantDataServiceMock = new();
        private readonly Mock<ISessionHelper> _sessionHelperMock = new();

        private EcPlantDataController CreateController()
        {
            var httpContext = new DefaultHttpContext();
            var controller = new EcPlantDataController(
                _loggerMock.Object,
                _heatNetworkServiceMock.Object,
                _energyCentrePlantDataServiceMock.Object,
                _sessionHelperMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("/mocked-url");
            controller.Url = urlHelperMock.Object;

            return controller;
        }

        private void SetupDefaultSession()
        {
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId)).Returns("hn-1");
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnName)).Returns("Network 1");
        }

        private void SetupLookupServiceData()
        {
            _energyCentrePlantDataServiceMock.Setup(x => x.GetFuelTypes()).ReturnsAsync(
            [
                new FuelType { FuelId = 1, Type = "Gas" },
                new FuelType { FuelId = 2, Type = "Electricity" }
            ]);

            _energyCentrePlantDataServiceMock.Setup(x => x.GetChpOperationalModes()).ReturnsAsync(
            [
                new CHPOperationalMode { ChpId = 1, OperationalMode = "Mode 1" }
            ]);

            _energyCentrePlantDataServiceMock.Setup(x => x.GetHeatRecoverySources()).ReturnsAsync(
            [
                new HeatRecovery { Ident = "HR1", HeatRecoverySource = "Data Centre" }
            ]);
        }

        private static EcPlantOptionsViewModel CreateValidPlantOptionsModel()
        {
            return new EcPlantOptionsViewModel
            {
                NumberOfChp = 1,
                NumberOfHeatPump = 1,
                NumberOfHighHeatRecoverySources = 1,
                NumberOfBoilerPlants = 1
            };
        }

        private static ChpInformationViewModel CreateValidChpModel()
        {
            return new ChpInformationViewModel
            {
                ChpInformationList =
                [
                    new ChpInformation
                    {
                        MaximumHeatOutput = 10,
                        MaximumElectricOutput = 8,
                        FuelOrHeatSource = 1,
                        PercentageOfElectricityExported = 15
                    }
                ]
            };
        }

        private static HeatPumpInformationViewModel CreateValidHeatPumpModel()
        {
            return new HeatPumpInformationViewModel
            {
                HeatPumpInformationList =
                [
                    new HeatPumpInformation
                    {
                        MaximumHeatOutput = 12,
                        FuelOrHeatSource = 2
                    }
                ]
            };
        }

        private static HighHeatRecoveryInformationViewModel CreateValidHeatRecoveryModel()
        {
            return new HighHeatRecoveryInformationViewModel
            {
                HighHeatRecoveryInformationList =
                [
                    new HighHeatRecoveryInformation
                    {
                        MaximumHeatOutput = 7,
                        FuelOrHeatSource = "HR1"
                    }
                ]
            };
        }

        private static BoilerPlantInformationViewModel CreateValidBoilerModel()
        {
            return new BoilerPlantInformationViewModel
            {
                BoilerPlantInformationList =
                [
                    new BoilerPlantInformation
                    {
                        MaximumHeatOutput = 20,
                        FuelOrHeatSource = 1
                    }
                ]
            };
        }

        private static HeatNetworkResponse CreateHeatNetworkWithEc(NullableOfNetworkDetailsStatus? ecStatus = null)
        {
            return new HeatNetworkResponse
            {
                NetworkElements = new NetworkElementsResponse
                {
                    Elements =
                    [
                        new Element
                        {
                            ElementId = "EC001",
                            ElementName = "Main EC",
                            ElementType = ElementTypeInShort.EC,
                            EnergyCentrePlantData = ecStatus.HasValue
                                ? new EnergyCentrePlantData { EnergyCentreStatus = ecStatus.Value }
                                : null
                        }
                    ]
                }
            };
        }

        [Fact]
        public async Task EcSupplyingNewBuildings_ReturnsEcElements_AndClearsSessions()
        {
            var controller = CreateController();
            SetupDefaultSession();
            _heatNetworkServiceMock.Setup(x => x.GetAsync("HN-1")).ReturnsAsync(CreateHeatNetworkWithEc());

            var result = Assert.IsType<ViewResult>(await controller.EcSupplyingNewBuildings());
            var model = Assert.IsType<List<Element>>(result.Model);

            Assert.Single(model);
            Assert.Equal(ElementTypeInShort.EC, model[0].ElementType);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.ChpInformationViewModelSessionKey), Times.Once);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.HeatPumpInformationViewModelSessionKey), Times.Once);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.HighHeatRecoveryInformationViewModelSessionKey), Times.Once);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.BoilerPlantInformationViewModelSessionKey), Times.Once);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.EcPlantOptionsViewModelSessionKey), Times.Once);
        }

        [Fact]
        public async Task SelectEcPlantOptionsAsync_WhenEcStatusComplete_RedirectsToOverview()
        {
            var controller = CreateController();
            SetupDefaultSession();
            _heatNetworkServiceMock.Setup(x => x.GetAsync("HN-1")).ReturnsAsync(CreateHeatNetworkWithEc(NullableOfNetworkDetailsStatus.Complete));

            var result = Assert.IsType<RedirectToActionResult>(await controller.SelectEcPlantOptionsAsync("EC001", "Main EC"));

            Assert.Equal(nameof(EcPlantDataController.EcPlantOverview), result.ActionName);
            Assert.Equal("EC001", controller.TempData["EcElementId"]);
            Assert.Equal("Main EC", controller.TempData["EcElementName"]);
        }

        [Fact]
        public async Task SelectEcPlantOptionsAsync_WhenEcStatusNotComplete_RedirectsToOptions()
        {
            var controller = CreateController();
            SetupDefaultSession();
            _heatNetworkServiceMock.Setup(x => x.GetAsync("HN-1")).ReturnsAsync(CreateHeatNetworkWithEc());

            var result = Assert.IsType<RedirectToActionResult>(await controller.SelectEcPlantOptionsAsync("EC001", "Main EC"));

            Assert.Equal(nameof(EcPlantDataController.EcPlantOptions), result.ActionName);
        }

        [Fact]
        public async Task EcPlantOptions_Get_WithSavedModel_ReturnsSavedModel()
        {
            var controller = CreateController();
            SetupDefaultSession();
            var savedModel = CreateValidPlantOptionsModel();

            _sessionHelperMock.Setup(x => x.GetFromSession<EcPlantOptionsViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcPlantOptionsViewModelSessionKey)).Returns(savedModel);

            var result = Assert.IsType<ViewResult>(await controller.EcPlantOptions());

            Assert.Same(savedModel, result.Model);
        }

        [Fact]
        public async Task EcPlantOptions_Post_WithInvalidModelState_ReturnsView()
        {
            var controller = CreateController();
            SetupDefaultSession();
            var model = CreateValidPlantOptionsModel();
            controller.ModelState.AddModelError("NumberOfChp", "Invalid");

            var result = Assert.IsType<ViewResult>(await controller.EcPlantOptions(model));

            Assert.Same(model, result.Model);
        }

        [Fact]
        public async Task EcPlantOptions_Post_WithNoPlantCount_ReturnsValidationError()
        {
            var controller = CreateController();
            SetupDefaultSession();
            var model = new EcPlantOptionsViewModel
            {
                NumberOfChp = 0,
                NumberOfHeatPump = 0,
                NumberOfHighHeatRecoverySources = 0,
                NumberOfBoilerPlants = 0
            };

            var result = Assert.IsType<ViewResult>(await controller.EcPlantOptions(model));

            Assert.Same(model, result.Model);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task EcPlantOptions_Post_WithValidModel_SavesAndRedirectsToFirstAction()
        {
            var controller = CreateController();
            SetupDefaultSession();
            var model = CreateValidPlantOptionsModel();

            _sessionHelperMock.Setup(x => x.GetFromSession<EcPlantOptionsViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcPlantOptionsViewModelSessionKey)).Returns(model);

            var result = Assert.IsType<RedirectToActionResult>(await controller.EcPlantOptions(model));

            Assert.Equal("ChpInformation", result.ActionName);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.EcPlantOptionsViewModelSessionKey, model), Times.Once);
        }

        [Fact]
        public async Task ChpInformation_Get_WithSavedModel_ReturnsSavedModel()
        {
            var controller = CreateController();
            SetupDefaultSession();
            SetupLookupServiceData();
            var options = CreateValidPlantOptionsModel();
            var savedModel = CreateValidChpModel();

            _sessionHelperMock.Setup(x => x.GetFromSession<EcPlantOptionsViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcPlantOptionsViewModelSessionKey)).Returns(options);
            _sessionHelperMock.Setup(x => x.GetFromSession<ChpInformationViewModel>(It.IsAny<HttpContext>(), SessionKeys.ChpInformationViewModelSessionKey)).Returns(savedModel);

            var result = Assert.IsType<ViewResult>(await controller.ChpInformation());

            Assert.Same(savedModel, result.Model);
        }

        [Fact]
        public async Task ChpInformation_Post_WithValidModel_SavesAndRedirects()
        {
            var controller = CreateController();
            SetupDefaultSession();
            SetupLookupServiceData();
            _sessionHelperMock.Setup(x => x.GetFromSession<EcPlantOptionsViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcPlantOptionsViewModelSessionKey)).Returns(CreateValidPlantOptionsModel());
            var model = CreateValidChpModel();

            var result = Assert.IsType<RedirectToActionResult>(await controller.ChpInformation(model));

            Assert.Equal("HeatPumpInformation", result.ActionName);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.ChpInformationViewModelSessionKey, model), Times.Once);
        }

        [Fact]
        public async Task HeatPumpInformation_Get_WithoutSavedModel_BuildsFromPlantOptions()
        {
            var controller = CreateController();
            SetupDefaultSession();
            SetupLookupServiceData();
            var options = CreateValidPlantOptionsModel();

            _sessionHelperMock.Setup(x => x.GetFromSession<EcPlantOptionsViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcPlantOptionsViewModelSessionKey)).Returns(options);
            _sessionHelperMock.Setup(x => x.GetFromSession<HeatPumpInformationViewModel>(It.IsAny<HttpContext>(), SessionKeys.HeatPumpInformationViewModelSessionKey)).Returns((HeatPumpInformationViewModel?)null);

            var result = Assert.IsType<ViewResult>(await controller.HeatPumpInformation());
            var model = Assert.IsType<HeatPumpInformationViewModel>(result.Model);

            Assert.Single(model.HeatPumpInformationList);
        }

        [Fact]
        public async Task HeatPumpInformation_Post_WithValidModel_SavesAndRedirects()
        {
            var controller = CreateController();
            SetupDefaultSession();
            SetupLookupServiceData();
            _sessionHelperMock.Setup(x => x.GetFromSession<EcPlantOptionsViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcPlantOptionsViewModelSessionKey)).Returns(CreateValidPlantOptionsModel());
            var model = CreateValidHeatPumpModel();

            var result = Assert.IsType<RedirectToActionResult>(await controller.HeatPumpInformation(model));

            Assert.Equal("HighHeatRecoveryInformation", result.ActionName);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.HeatPumpInformationViewModelSessionKey, model), Times.Once);
        }

        [Fact]
        public async Task HighHeatRecoveryInformation_Get_WithoutSavedModel_BuildsFromPlantOptions()
        {
            var controller = CreateController();
            SetupDefaultSession();
            SetupLookupServiceData();
            var options = CreateValidPlantOptionsModel();

            _sessionHelperMock.Setup(x => x.GetFromSession<EcPlantOptionsViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcPlantOptionsViewModelSessionKey)).Returns(options);
            _sessionHelperMock.Setup(x => x.GetFromSession<HighHeatRecoveryInformationViewModel>(It.IsAny<HttpContext>(), SessionKeys.HighHeatRecoveryInformationViewModelSessionKey)).Returns((HighHeatRecoveryInformationViewModel?)null);

            var result = Assert.IsType<ViewResult>(await controller.HighHeatRecoveryInformation());
            var model = Assert.IsType<HighHeatRecoveryInformationViewModel>(result.Model);

            Assert.Single(model.HighHeatRecoveryInformationList);
        }

        [Fact]
        public async Task HighHeatRecoveryInformation_Post_WithValidModel_SavesAndRedirects()
        {
            var controller = CreateController();
            SetupDefaultSession();
            SetupLookupServiceData();
            _sessionHelperMock.Setup(x => x.GetFromSession<EcPlantOptionsViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcPlantOptionsViewModelSessionKey)).Returns(CreateValidPlantOptionsModel());
            var model = CreateValidHeatRecoveryModel();

            var result = Assert.IsType<RedirectToActionResult>(await controller.HighHeatRecoveryInformation(model));

            Assert.Equal("BoilerPlantInformation", result.ActionName);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.HighHeatRecoveryInformationViewModelSessionKey, model), Times.Once);
        }

        [Fact]
        public async Task BoilerPlantInformation_Get_WithoutSavedModel_BuildsFromPlantOptions()
        {
            var controller = CreateController();
            SetupDefaultSession();
            SetupLookupServiceData();
            var options = CreateValidPlantOptionsModel();

            _sessionHelperMock.Setup(x => x.GetFromSession<EcPlantOptionsViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcPlantOptionsViewModelSessionKey)).Returns(options);
            _sessionHelperMock.Setup(x => x.GetFromSession<BoilerPlantInformationViewModel>(It.IsAny<HttpContext>(), SessionKeys.BoilerPlantInformationViewModelSessionKey)).Returns((BoilerPlantInformationViewModel?)null);

            var result = Assert.IsType<ViewResult>(await controller.BoilerPlantInformation());
            var model = Assert.IsType<BoilerPlantInformationViewModel>(result.Model);

            Assert.Single(model.BoilerPlantInformationList);
        }

        [Fact]
        public async Task BoilerPlantInformation_Post_WithValidModel_SavesAndRedirects()
        {
            var controller = CreateController();
            SetupDefaultSession();
            SetupLookupServiceData();
            _sessionHelperMock.Setup(x => x.GetFromSession<EcPlantOptionsViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcPlantOptionsViewModelSessionKey)).Returns(CreateValidPlantOptionsModel());
            var model = CreateValidBoilerModel();

            var result = Assert.IsType<RedirectToActionResult>(await controller.BoilerPlantInformation(model));

            Assert.Equal("EcPlantSummary", result.ActionName);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.BoilerPlantInformationViewModelSessionKey, model), Times.Once);
        }

        [Fact]
        public async Task EcPlantSummary_Get_ReturnsMappedSummaryModel()
        {
            var controller = CreateController();
            SetupDefaultSession();
            SetupLookupServiceData();

            _sessionHelperMock.Setup(x => x.GetFromSession<EcPlantOptionsViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcPlantOptionsViewModelSessionKey)).Returns(CreateValidPlantOptionsModel());
            _sessionHelperMock.Setup(x => x.GetFromSession<ChpInformationViewModel>(It.IsAny<HttpContext>(), SessionKeys.ChpInformationViewModelSessionKey)).Returns(CreateValidChpModel());
            _sessionHelperMock.Setup(x => x.GetFromSession<HeatPumpInformationViewModel>(It.IsAny<HttpContext>(), SessionKeys.HeatPumpInformationViewModelSessionKey)).Returns(CreateValidHeatPumpModel());
            _sessionHelperMock.Setup(x => x.GetFromSession<HighHeatRecoveryInformationViewModel>(It.IsAny<HttpContext>(), SessionKeys.HighHeatRecoveryInformationViewModelSessionKey)).Returns(CreateValidHeatRecoveryModel());
            _sessionHelperMock.Setup(x => x.GetFromSession<BoilerPlantInformationViewModel>(It.IsAny<HttpContext>(), SessionKeys.BoilerPlantInformationViewModelSessionKey)).Returns(CreateValidBoilerModel());

            var result = Assert.IsType<ViewResult>(await controller.EcPlantSummary());
            var model = Assert.IsType<EcPlantSummaryViewModel>(result.Model);

            Assert.Single(model.ChpInformationSummaryList!);
            Assert.Single(model.HeatPumpInformationSummaryList!);
            Assert.Single(model.HighHeatRecoveryInformationSummaryList!);
            Assert.Single(model.BoilerPlantInformationSummaryList!);
        }

        [Fact]
        public async Task EcPlantSummary_Post_UpdatesPlantData_ClearsSession_AndRedirects()
        {
            var controller = CreateController();
            SetupDefaultSession();
            controller.TempData["EcElementId"] = "EC001";

            _sessionHelperMock.Setup(x => x.GetFromSession<EcPlantOptionsViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcPlantOptionsViewModelSessionKey)).Returns(CreateValidPlantOptionsModel());
            _sessionHelperMock.Setup(x => x.GetFromSession<ChpInformationViewModel>(It.IsAny<HttpContext>(), SessionKeys.ChpInformationViewModelSessionKey)).Returns(CreateValidChpModel());
            _sessionHelperMock.Setup(x => x.GetFromSession<HeatPumpInformationViewModel>(It.IsAny<HttpContext>(), SessionKeys.HeatPumpInformationViewModelSessionKey)).Returns(CreateValidHeatPumpModel());
            _sessionHelperMock.Setup(x => x.GetFromSession<HighHeatRecoveryInformationViewModel>(It.IsAny<HttpContext>(), SessionKeys.HighHeatRecoveryInformationViewModelSessionKey)).Returns(CreateValidHeatRecoveryModel());
            _sessionHelperMock.Setup(x => x.GetFromSession<BoilerPlantInformationViewModel>(It.IsAny<HttpContext>(), SessionKeys.BoilerPlantInformationViewModelSessionKey)).Returns(CreateValidBoilerModel());

            var result = Assert.IsType<RedirectToActionResult>(await controller.EcPlantSummary(new EcPlantSummaryViewModel()));

            Assert.Equal("EcSupplyingNewBuildings", result.ActionName);
            Assert.Equal("EcPlantData", result.ControllerName);
            _heatNetworkServiceMock.Verify(x => x.UpdateEnergyCentrePlantData(It.Is<EcPlantDataRequest>(r =>
                r.HnId == "HN-1" &&
                r.ElementId == "EC001" &&
                r.EnergyCentrePlantData != null &&
                r.EnergyCentrePlantData.CombinedHeatAndPowerSystems!.Count == 1 &&
                r.EnergyCentrePlantData.HeatPumpSystems!.Count == 1 &&
                r.EnergyCentrePlantData.HeatRecoverySources!.Count == 1 &&
                r.EnergyCentrePlantData.BoilerPlants!.Count == 1)), Times.Once);

            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.ChpInformationViewModelSessionKey), Times.Once);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.HeatPumpInformationViewModelSessionKey), Times.Once);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.HighHeatRecoveryInformationViewModelSessionKey), Times.Once);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.BoilerPlantInformationViewModelSessionKey), Times.Once);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.EcPlantOptionsViewModelSessionKey), Times.Once);
        }

        [Fact]
        public async Task EcPlantOverview_Get_ReturnsMappedOverviewModel()
        {
            var controller = CreateController();
            SetupDefaultSession();
            SetupLookupServiceData();
            controller.TempData["EcElementId"] = "EC001";

            _heatNetworkServiceMock.Setup(x => x.GetAsync("HN-1")).ReturnsAsync(new HeatNetworkResponse
            {
                NetworkElements = new NetworkElementsResponse
                {
                    Elements =
                    [
                        new Element
                        {
                            ElementId = "EC001",
                            ElementType = ElementTypeInShort.EC,
                            EnergyCentrePlantData = new EnergyCentrePlantData
                            {
                                CombinedHeatAndPowerSystems =
                                [
                                    new CombinedHeatAndPowerSystem
                                    {
                                        MaximumHeatOutput = 10,
                                        MaximumElectricOutput = 8,
                                        FuelId = 1,
                                        PercentageOfElectricityExported = 20
                                    }
                                ],
                                HeatPumpSystems =
                                [
                                    new HeatPumpSystem
                                    {
                                        MaximumHeatOutput = 12,
                                        FuelId = 2
                                    }
                                ],
                                HeatRecoverySources =
                                [
                                    new HeatRecoverySource
                                    {
                                        MaximumHeatOutput = 6,
                                        HeatSource = "HR1"
                                    }
                                ],
                                BoilerPlants =
                                [
                                    new BoilerPlant
                                    {
                                        MaximumHeatOutput = 18,
                                        FuelId = 1
                                    }
                                ]
                            }
                        }
                    ]
                }
            });

            var result = Assert.IsType<ViewResult>(await controller.EcPlantOverview());
            var model = Assert.IsType<EcPlantSummaryViewModel>(result.Model);

            Assert.Equal("EcPlantOverview", result.ViewName);
            Assert.Single(model.ChpInformationSummaryList!);
            Assert.Single(model.HeatPumpInformationSummaryList!);
            Assert.Single(model.HighHeatRecoveryInformationSummaryList!);
            Assert.Single(model.BoilerPlantInformationSummaryList!);
        }
    }
}
