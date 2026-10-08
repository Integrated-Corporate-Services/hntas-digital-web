using HNTAS.Api.Client.Api;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class CarbonCalculatorControllerTests
    {
        private readonly Mock<ILogger<CarbonCalculatorController>> _loggerMock;
        private readonly Mock<ICarbonCalculatorService> _carbonCalculatorServiceMock;
        private readonly CarbonCalculatorController _controller;

        public CarbonCalculatorControllerTests()
        {
            _loggerMock = new Mock<ILogger<CarbonCalculatorController>>();
            _carbonCalculatorServiceMock = new Mock<ICarbonCalculatorService>();
            _controller = CreateController();
        }

        private CarbonCalculatorController CreateController()
        {
            var controller = new CarbonCalculatorController(_carbonCalculatorServiceMock.Object, _loggerMock.Object);

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
        public void Index_ReturnsViewResult()
        {
            // Arrange
            var urlHelperMock = SetUpBackLink("Home", "Index");
            _controller.Url = urlHelperMock.Object;
            // Act
            var result = _controller.Index();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", viewResult.ViewName);
        }

        [Fact]
        public async Task Result_ReturnsViewResult()
        {
            // Arrange
            var urlHelperMock = SetUpBackLink("CarbonCalculator", "Index");
            _controller.Url = urlHelperMock.Object;
            var viewModel = new HNTAS.Web.UI.Models.CarbonCalculatorViewModel
            {
                Request = new HNTAS.Web.UI.Models.CarbonCalculatorRequest
                {
                    Background = new HNTAS.Web.UI.Models.Background
                    {
                        DateWorkbookCompleted = "2025-11-10",
                        NetworkStatus = "existing",
                        NetworkServiceProvision = "both",
                        Name = "Sample API Call",
                        NetworkID = "HN0001234",
                        NetworkName = "Sample Heat Network",
                        PostcodeOfThePrimaryEnergyCentre = "AA00 0A1",
                        ContactEmail = ""
                    },
                    Energy = new Models.Energy
                    {
                        YearCount = 5,
                        StartYear = 2025,
                        EnergyHeatNetworkPrimaryLossesCsv = "0.1,0.2,0.3,0.4,0.5",
                        ChpCount = 1,
                        HeatPumpCount = 1,
                        RecoveredCount = 1,
                        BoilerCount = 1,
                        ChpInputs = new List<Models.ChpInput>
                        {
                            new Models.ChpInput
                            {
                                ChpUsefulHeatValueCsv = "100,200,300,400,500",
                                ChpElectricityGeneratedValueCsv = "50,100,150,200,250",
                                ChpFuelUsedValueCsv = "30,60,90,120,150",
                                ChpHeatCoolingValueCsv = "10,20,30,40,50",
                                ChpSleevingPCentValueCsv = "5,10,15,20,25",
                                ChpMaxHeatOutput = 500,
                                ChpMaxElectricityOutput = 250
                            }
                        },
                        HeatPumpInputs = new List<Models.HeatPumpInput>
                        {
                            new Models.HeatPumpInput
                            {
                                HpmUsefulHeatGeneratedValueCsv = "100,200,300,400,500",
                                HpmEnergyUsedValueCsv = "50,100,150,200,250",
                                HpmUsefulCoolingGeneratedValueCsv = "10,20,30,40,50",
                                HpmSleevingPCentValueCsv = "5,10,15,20,25",
                                HpmMaxHeatOutput = 500
                            }
                        },
                        RecoveredInputs = new List<Models.RecoveredInput>
                        {
                            new Models.RecoveredInput
                            {
                                HrwUsefulHeatGeneratedValueCsv = "100,200,300,400,500",
                                HrwHeatUsedByCoolingProductionValueCsv = "50,100,150,200,250",
                                HrwSleevingPCentValueCsv = "10,20,30,40,50",                                
                            }
                        },
                        BoilerInputs = new List<Models.BoilerInput>
                        {
                            new Models.BoilerInput
                            {
                                BlrUsefulHeatGeneratedValueCsv = "100,200,300,400,500",
                                BlrFuelUsedByValueCsv = "50,100,150,200,250",
                                BlrHeatUsedForCoolingProductionValueCsv = "10,20,30,40,50",
                                BlrSleevingPCentValueCsv = "5,10,15,20,25",
                                BlrMaxHeatOutput = 500
                            }
                        }
                    }
                }
            };

            var mockApi = new Mock<IApiCarbonCalculatorRunPostApiResponse>();
            mockApi.Setup(api => api.IsOk).Returns(true);
            mockApi.Setup(api => api.RawContent).Returns("{\"totalCarbonEmission\": 123.45}");

            _carbonCalculatorServiceMock
                .Setup(service => service.CalculateAsync(It.IsAny<Api.Client.Model.CarbonCalculatorRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockApi.Object);

            // Act
            var result = await _controller.Result(viewModel);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Result", viewResult.ViewName);
        }

        [Fact]
        public async Task Result_ThrowException()
        {
            // Arrange
            var urlHelperMock = SetUpBackLink("CarbonCalculator", "Index");
            _controller.Url = urlHelperMock.Object;
            var viewModel = new HNTAS.Web.UI.Models.CarbonCalculatorViewModel
            {
                Request = new HNTAS.Web.UI.Models.CarbonCalculatorRequest
                {
                    Background = new HNTAS.Web.UI.Models.Background
                    {
                        DateWorkbookCompleted = "2025-11-10",
                        NetworkStatus = "existing",
                        NetworkServiceProvision = "both",
                        Name = "Sample API Call",
                        NetworkID = "HN0001234",
                        NetworkName = "Sample Heat Network",
                        PostcodeOfThePrimaryEnergyCentre = "AA00 0A1",
                        ContactEmail = ""
                    },
                    Energy = new Models.Energy
                    {
                        YearCount = 5,
                        StartYear = 2025,
                        EnergyHeatNetworkPrimaryLossesCsv = "0.1,0.2,0.3,0.4,0.5",
                        ChpCount = 1,
                        HeatPumpCount = 1,
                        RecoveredCount = 1,
                        BoilerCount = 1,
                        ChpInputs = new List<Models.ChpInput>
                        {
                            new Models.ChpInput
                            {
                                ChpUsefulHeatValueCsv = "100,200,300,400,500",
                                ChpElectricityGeneratedValueCsv = "50,100,150,200,250",
                                ChpFuelUsedValueCsv = "30,60,90,120,150",
                                ChpHeatCoolingValueCsv = "10,20,30,40,50",
                                ChpSleevingPCentValueCsv = "5,10,15,20,25",
                                ChpMaxHeatOutput = 500,
                                ChpMaxElectricityOutput = 250
                            }
                        },
                        HeatPumpInputs = new List<Models.HeatPumpInput>
                        {
                            new Models.HeatPumpInput
                            {
                                HpmUsefulHeatGeneratedValueCsv = "100,200,300,400,500",
                                HpmEnergyUsedValueCsv = "50,100,150,200,250",
                                HpmUsefulCoolingGeneratedValueCsv = "10,20,30,40,50",
                                HpmSleevingPCentValueCsv = "5,10,15,20,25",
                                HpmMaxHeatOutput = 500
                            }
                        },
                        RecoveredInputs = new List<Models.RecoveredInput>
                        {
                            new Models.RecoveredInput
                            {
                                HrwUsefulHeatGeneratedValueCsv = "100,200,300,400,500",
                                HrwHeatUsedByCoolingProductionValueCsv = "50,100,150,200,250",
                                HrwSleevingPCentValueCsv = "10,20,30,40,50",
                            }
                        },
                        BoilerInputs = new List<Models.BoilerInput>
                        {
                            new Models.BoilerInput
                            {
                                BlrUsefulHeatGeneratedValueCsv = "100,200,300,400,500",
                                BlrFuelUsedByValueCsv = "50,100,150,200,250",
                                BlrHeatUsedForCoolingProductionValueCsv = "10,20,30,40,50",
                                BlrSleevingPCentValueCsv = "5,10,15,20,25",
                                BlrMaxHeatOutput = 500
                            }
                        }
                    }
                }
            };
            
            _carbonCalculatorServiceMock
                .Setup(service => service.CalculateAsync(It.IsAny<Api.Client.Model.CarbonCalculatorRequest>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            // Act
            var result = await _controller.Result(viewModel);
                            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to calculate carbon emission.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
