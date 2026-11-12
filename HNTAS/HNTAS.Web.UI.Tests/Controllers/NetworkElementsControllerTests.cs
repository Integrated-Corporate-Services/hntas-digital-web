using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.NetworkElements;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq;
using System.Threading.Tasks;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class NetworkElementsControllerTests
    {
        private readonly Mock<ILogger<NetworkElementsController>> _loggerMock = new();
        private readonly Mock<IHeatNetworkService> _heatNetworkServiceMock = new();
        private readonly Mock<ISessionHelper> _sessionHelperMock = new();

        private NetworkElementsController CreateController()
        {
            var httpContext = new DefaultHttpContext();
            var controller = new NetworkElementsController(_loggerMock.Object, _heatNetworkServiceMock.Object, _sessionHelperMock.Object)
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

        private static NetworkElementViewModel CreateValidNetworkElementModel(int existingCount = 0)
        {
            return new NetworkElementViewModel
            {
                ElementOptions =
                [
                    new NetworkElementOption
                    {
                        Id = HeatNetworkElementType.EnergyCentre,
                        Label = "Energy centres",
                        ExistingCount = existingCount,
                        NewCount = 1
                    }
                ]
            };
        }

        private static NetworkEcNameViewModel CreateValidEcNameModel(string existingName = "Existing EC")
        {
            return new NetworkEcNameViewModel
            {
                ExistingEcNameOptions = [new EcNameOption { EcName = existingName }],
                NewEcNameOptions = [new EcNameOption { EcName = "Main EC", IsMainEc = true }]
            };
        }

        private static EnergyCentresSupplyViewModel CreateEnergySupplySelectionModel(bool selected = true)
        {
            return new EnergyCentresSupplyViewModel
            {
                ExistingEnergyCentresSupply = [new ExistingEnergyCentreSupply { EcName = "Existing EC", IsChecked = selected }]
            };
        }

        [Fact]
        public async Task NetworkElementsListAsync_Get_WithSavedModel_ReturnsSavedModel()
        {
            var controller = CreateController();
            var savedModel = CreateValidNetworkElementModel();

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId)).Returns("hn-1");
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnName)).Returns("Network 1");
            _sessionHelperMock.Setup(x => x.GetFromSession<NetworkElementViewModel>(It.IsAny<HttpContext>(), SessionKeys.NetworkElementsViewModelSessionKey)).Returns(savedModel);
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.NetworkBuildStatusSessionKey)).Returns(NetworkBuildStatus.New.ToString());

            var result = Assert.IsType<ViewResult>(await controller.NetworkElementsListAsync());

            Assert.Same(savedModel, result.Model);
            _heatNetworkServiceMock.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task NetworkElementsListAsync_Get_WithoutSavedModel_GetsNetworkAndReturnsView()
        {
            var controller = CreateController();
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId)).Returns("hn-1");
            _sessionHelperMock.Setup(x => x.GetFromSession<NetworkElementViewModel>(It.IsAny<HttpContext>(), SessionKeys.NetworkElementsViewModelSessionKey)).Returns((NetworkElementViewModel?)null);
            _heatNetworkServiceMock.Setup(x => x.GetAsync("HN-1")).ReturnsAsync(new HeatNetworkResponse
            {
                HeatNetworkType = NullableOfHeatNetworkType.District,
                NetworkBuildStatus = NullableOfNetworkBuildStatus.New
            });

            var result = Assert.IsType<ViewResult>(await controller.NetworkElementsListAsync());
            var model = Assert.IsType<NetworkElementViewModel>(result.Model);

            Assert.NotEmpty(model.ElementOptions);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.NetworkBuildStatusSessionKey, NetworkBuildStatus.New.ToString()), Times.Once);
        }

        [Fact]
        public async Task NetworkElementsListAsync_Post_WithInvalidModel_ReturnsView()
        {
            var controller = CreateController();
            var model = new NetworkElementViewModel
            {
                ElementOptions =
                [
                    new NetworkElementOption
                    {
                        Id = HeatNetworkElementType.EnergyCentre,
                        Label = "Energy centre",
                        NewCount = null
                    }
                ]
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.NetworkBuildStatusSessionKey)).Returns(NetworkBuildStatus.New.ToString());

            var result = Assert.IsType<ViewResult>(await controller.NetworkElementsListAsync(model));

            Assert.Same(model, result.Model);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task NetworkElementsListAsync_Post_WithValidModel_SavesAndRedirects()
        {
            var controller = CreateController();
            var model = CreateValidNetworkElementModel(existingCount: 0);
            var savedModel = CreateValidNetworkElementModel(existingCount: 1);

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.NetworkBuildStatusSessionKey)).Returns(NetworkBuildStatus.New.ToString());
            _sessionHelperMock.Setup(x => x.GetFromSession<NetworkElementViewModel>(It.IsAny<HttpContext>(), SessionKeys.NetworkElementsViewModelSessionKey)).Returns(savedModel);

            var result = Assert.IsType<RedirectToActionResult>(await controller.NetworkElementsListAsync(model));

            Assert.Equal("NetworkEnergyCentreName", result.ActionName);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.NetworkElementsViewModelSessionKey, model), Times.Once);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.NetworkEcNameViewModelSessionKey), Times.Once);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.EcSupplyCheckViewModelSessionKey), Times.Once);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.EcSupplyViewModelSessionKey), Times.Once);
        }

        [Fact]
        public async Task NetworkEnergyCentreName_Get_WithSavedModel_ReturnsView()
        {
            var controller = CreateController();
            var savedModel = CreateValidEcNameModel();

            _sessionHelperMock.Setup(x => x.GetFromSession<NetworkEcNameViewModel>(It.IsAny<HttpContext>(), SessionKeys.NetworkEcNameViewModelSessionKey)).Returns(savedModel);

            var result = Assert.IsType<ViewResult>(await controller.NetworkEnergyCentreName());

            Assert.Same(savedModel, result.Model);
        }

        [Fact]
        public async Task NetworkEnergyCentreName_Get_WithoutSavedModel_BuildsModelFromNetworkElements()
        {
            var controller = CreateController();
            var networkElements = new NetworkElementViewModel
            {
                ElementOptions =
                [
                    new NetworkElementOption
                    {
                        Id = HeatNetworkElementType.EnergyCentre,
                        ExistingCount = 1,
                        NewCount = 1
                    }
                ]
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<NetworkEcNameViewModel>(It.IsAny<HttpContext>(), SessionKeys.NetworkEcNameViewModelSessionKey)).Returns((NetworkEcNameViewModel?)null);
            _sessionHelperMock.Setup(x => x.GetFromSession<NetworkElementViewModel>(It.IsAny<HttpContext>(), SessionKeys.NetworkElementsViewModelSessionKey)).Returns(networkElements);

            var result = Assert.IsType<ViewResult>(await controller.NetworkEnergyCentreName());
            var model = Assert.IsType<NetworkEcNameViewModel>(result.Model);

            Assert.Single(model.ExistingEcNameOptions);
            Assert.Single(model.NewEcNameOptions);
        }

        [Fact]
        public void NetworkEnergyCentreName_Post_WithInvalidModel_ReturnsView()
        {
            var controller = CreateController();
            var model = new NetworkEcNameViewModel
            {
                ExistingEcNameOptions = [new EcNameOption { EcName = "" }],
                NewEcNameOptions = [new EcNameOption { EcName = "Main EC", IsMainEc = true }]
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.NetworkBuildStatusSessionKey)).Returns(NetworkBuildStatus.NewAndExisting.ToString());

            var result = Assert.IsType<ViewResult>(controller.NetworkEnergyCentreName(model));

            Assert.Same(model, result.Model);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public void NetworkEnergyCentreName_Post_WithValidModel_SavesAndRedirects()
        {
            var controller = CreateController();
            var model = CreateValidEcNameModel();

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.NetworkBuildStatusSessionKey)).Returns(NetworkBuildStatus.NewAndExisting.ToString());

            var result = Assert.IsType<RedirectToActionResult>(controller.NetworkEnergyCentreName(model));

            Assert.Equal("EnergyCentreSupplyCheck", result.ActionName);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.NetworkEcNameViewModelSessionKey, model), Times.Once);
        }

        [Fact]
        public void EnergyCentreSupplyCheck_Get_ReturnsSavedModel()
        {
            var controller = CreateController();
            var savedModel = new EnergyCentreSupplyViewModel { IsExistingEcSupply = "yes" };

            _sessionHelperMock.Setup(x => x.GetFromSession<EnergyCentreSupplyViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcSupplyCheckViewModelSessionKey)).Returns(savedModel);

            var result = Assert.IsType<ViewResult>(controller.EnergyCentreSupplyCheck());

            Assert.Same(savedModel, result.Model);
        }

        [Fact]
        public void EnergyCentreSupplyCheck_Post_WithInvalidModelState_ReturnsView()
        {
            var controller = CreateController();
            var model = new EnergyCentreSupplyViewModel { IsExistingEcSupply = "yes" };
            controller.ModelState.AddModelError("IsExistingEcSupply", "Required");

            var result = Assert.IsType<ViewResult>(controller.EnergyCentreSupplyCheck(model));

            Assert.Same(model, result.Model);
        }

        [Fact]
        public void EnergyCentreSupplyCheck_Post_WhenYes_RedirectsToEnergyCentresSupply()
        {
            var controller = CreateController();
            var model = new EnergyCentreSupplyViewModel { IsExistingEcSupply = "yes" };

            var result = Assert.IsType<RedirectToActionResult>(controller.EnergyCentreSupplyCheck(model));

            Assert.Equal("EnergyCentresSupply", result.ActionName);
            Assert.Equal("NetworkElements", result.ControllerName);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.EcSupplyCheckViewModelSessionKey, model), Times.Once);
        }

        [Fact]
        public void EnergyCentreSupplyCheck_Post_WhenNo_ClearsSupplySessionAndRedirectsToSummary()
        {
            var controller = CreateController();
            var model = new EnergyCentreSupplyViewModel { IsExistingEcSupply = "no" };

            var result = Assert.IsType<RedirectToActionResult>(controller.EnergyCentreSupplyCheck(model));

            Assert.Equal("NetworkElementSummary", result.ActionName);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.EcSupplyViewModelSessionKey), Times.Once);
        }

        [Fact]
        public void EnergyCentresSupply_Get_WithSavedModel_ReturnsView()
        {
            var controller = CreateController();
            var savedModel = CreateEnergySupplySelectionModel();

            _sessionHelperMock.Setup(x => x.GetFromSession<EnergyCentresSupplyViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcSupplyViewModelSessionKey)).Returns(savedModel);

            var result = Assert.IsType<ViewResult>(controller.EnergyCentresSupply());

            Assert.Same(savedModel, result.Model);
        }

        [Fact]
        public void EnergyCentresSupply_Get_WithoutSavedModel_BuildsFromEcNames()
        {
            var controller = CreateController();
            var ecNames = new NetworkEcNameViewModel
            {
                ExistingEcNameOptions = [new EcNameOption { EcName = "Existing EC 1" }, new EcNameOption { EcName = "Existing EC 2" }]
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<EnergyCentresSupplyViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcSupplyViewModelSessionKey)).Returns((EnergyCentresSupplyViewModel?)null);
            _sessionHelperMock.Setup(x => x.GetFromSession<NetworkEcNameViewModel>(It.IsAny<HttpContext>(), SessionKeys.NetworkEcNameViewModelSessionKey)).Returns(ecNames);

            var result = Assert.IsType<ViewResult>(controller.EnergyCentresSupply());
            var model = Assert.IsType<EnergyCentresSupplyViewModel>(result.Model);

            Assert.Equal(2, model.ExistingEnergyCentresSupply.Count);
        }

        [Fact]
        public void EnergyCentresSupply_Post_WithoutSelection_ReturnsViewWithError()
        {
            var controller = CreateController();
            var model = CreateEnergySupplySelectionModel(selected: false);

            var result = Assert.IsType<ViewResult>(controller.EnergyCentresSupply(model));

            Assert.Same(model, result.Model);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public void EnergyCentresSupply_Post_WithSelection_SavesAndRedirects()
        {
            var controller = CreateController();
            var model = CreateEnergySupplySelectionModel(selected: true);

            var result = Assert.IsType<RedirectToActionResult>(controller.EnergyCentresSupply(model));

            Assert.Equal("NetworkElementSummary", result.ActionName);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.EcSupplyViewModelSessionKey, model), Times.Once);
        }

        [Fact]
        public void NetworkElementSummary_Get_ReturnsViewWithMappedModel()
        {
            var controller = CreateController();
            var networkElements = CreateValidNetworkElementModel(existingCount: 1);
            var ecNames = CreateValidEcNameModel();
            var supplyCheck = new EnergyCentreSupplyViewModel { IsExistingEcSupply = "yes" };
            var selectedSupply = CreateEnergySupplySelectionModel(selected: true);

            _sessionHelperMock.Setup(x => x.GetFromSession<EnergyCentreSupplyViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcSupplyCheckViewModelSessionKey)).Returns(supplyCheck);
            _sessionHelperMock.Setup(x => x.GetFromSession<NetworkElementViewModel>(It.IsAny<HttpContext>(), SessionKeys.NetworkElementsViewModelSessionKey)).Returns(networkElements);
            _sessionHelperMock.Setup(x => x.GetFromSession<NetworkEcNameViewModel>(It.IsAny<HttpContext>(), SessionKeys.NetworkEcNameViewModelSessionKey)).Returns(ecNames);
            _sessionHelperMock.Setup(x => x.GetFromSession<EnergyCentresSupplyViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcSupplyViewModelSessionKey)).Returns(selectedSupply);
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.NetworkBuildStatusSessionKey)).Returns(NetworkBuildStatus.NewAndExisting.ToString());

            var result = Assert.IsType<ViewResult>(controller.NetworkElementSummary());
            var model = Assert.IsType<NetworkElementSummaryViewModel>(result.Model);

            Assert.NotNull(model.ExistingNetworkElements);
            Assert.NotNull(model.NewNetworkElements);
            Assert.NotNull(model.ExistingNetworkEcNames);
            Assert.NotNull(model.NewNetworkEcNames);
            Assert.NotNull(model.ExistingEcSupplyingNewBuilding);
            Assert.True((bool)controller.ViewBag.SupplyToNewBuildings);
        }

        [Fact]
        public async Task NetworkElementSummaryAsync_Post_WhenNotConfirmed_ReturnsView()
        {
            var controller = CreateController();
            var model = new NetworkElementSummaryViewModel { ConfirmedDeclaration = false };

            _sessionHelperMock.Setup(x => x.GetFromSession<EnergyCentreSupplyViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcSupplyCheckViewModelSessionKey)).Returns(new EnergyCentreSupplyViewModel { IsExistingEcSupply = "no" });
            _sessionHelperMock.Setup(x => x.GetFromSession<NetworkElementViewModel>(It.IsAny<HttpContext>(), SessionKeys.NetworkElementsViewModelSessionKey)).Returns(CreateValidNetworkElementModel());
            _sessionHelperMock.Setup(x => x.GetFromSession<NetworkEcNameViewModel>(It.IsAny<HttpContext>(), SessionKeys.NetworkEcNameViewModelSessionKey)).Returns(CreateValidEcNameModel());
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.NetworkBuildStatusSessionKey)).Returns(NetworkBuildStatus.New.ToString());

            var result = Assert.IsType<ViewResult>(await controller.NetworkElementSummaryAsync(model));

            Assert.Same(model, result.Model);
            _heatNetworkServiceMock.Verify(x => x.UpdateNetworkElements(It.IsAny<string>(), It.IsAny<NetworkElements2>()), Times.Never);
        }

        [Fact]
        public async Task NetworkElementSummaryAsync_Post_WhenConfirmed_UpdatesAndRedirects()
        {
            var controller = CreateController();
            var model = new NetworkElementSummaryViewModel { ConfirmedDeclaration = true };
            var networkElements = CreateValidNetworkElementModel(existingCount: 1);
            var ecNames = CreateValidEcNameModel();
            var supplyCheck = new EnergyCentreSupplyViewModel { IsExistingEcSupply = "yes" };
            var selectedSupply = CreateEnergySupplySelectionModel(selected: true);

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId)).Returns("HN-1");
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey)).Returns("user-1");
            _sessionHelperMock.Setup(x => x.GetFromSession<EnergyCentreSupplyViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcSupplyCheckViewModelSessionKey)).Returns(supplyCheck);
            _sessionHelperMock.Setup(x => x.GetFromSession<NetworkElementViewModel>(It.IsAny<HttpContext>(), SessionKeys.NetworkElementsViewModelSessionKey)).Returns(networkElements);
            _sessionHelperMock.Setup(x => x.GetFromSession<NetworkEcNameViewModel>(It.IsAny<HttpContext>(), SessionKeys.NetworkEcNameViewModelSessionKey)).Returns(ecNames);
            _sessionHelperMock.Setup(x => x.GetFromSession<EnergyCentresSupplyViewModel>(It.IsAny<HttpContext>(), SessionKeys.EcSupplyViewModelSessionKey)).Returns(selectedSupply);
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.NetworkBuildStatusSessionKey)).Returns(NetworkBuildStatus.NewAndExisting.ToString());

            var result = Assert.IsType<RedirectToActionResult>(await controller.NetworkElementSummaryAsync(model));

            Assert.Equal("NetworkConfirmation", result.ActionName);
            _heatNetworkServiceMock.Verify(x => x.UpdateNetworkElements("HN-1", It.Is<NetworkElements2>(req =>
                req.Elements.Count == 2 &&
                req.ElementsGroup.Count == 1 &&
                req.IsExistingEnergyCentreSupplyNewBuildings == true &&
                req.CreatedBy == "user-1")), Times.Once);

            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.NetworkElementsViewModelSessionKey), Times.Once);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.NetworkEcNameViewModelSessionKey), Times.Once);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.EcSupplyCheckViewModelSessionKey), Times.Once);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.EcSupplyViewModelSessionKey), Times.Once);
        }

        [Fact]
        public void NetworkConfirmationAsync_Get_ReturnsView()
        {
            var controller = CreateController();

            var result = controller.NetworkConfirmationAsync();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task NetworkElementsOverViewAsync_Get_ReturnsMappedOverviewModel()
        {
            var controller = CreateController();

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId)).Returns("hn-1");
            _heatNetworkServiceMock.Setup(x => x.GetAsync("HN-1")).ReturnsAsync(new HeatNetworkResponse
            {
                HeatNetworkType = NullableOfHeatNetworkType.District,
                NetworkBuildStatus = NullableOfNetworkBuildStatus.NewAndExisting,
                EnergyCentreType = NullableOfEnergyCentreType.SuppliesMultipleBuildings,
                Phase = "Design",
                Address = new RegisteredAddress("Line 1", "AB1 2CD"),
                EcDetails = new ECDetails { Latitude = 51.5, Longitude = -0.1 },
                HeatNetworkConnections = new HeatNetworkConnections
                {
                    NoOfExistingCommunalBuilding = 1,
                    NoOfNewCommunalBuilding = 2
                },
                NetworkElements = new NetworkElementsResponse
                {
                    ElementsGroup =
                    [
                        new ElementGroup
                        {
                            BuildType = NetworkBuildStatus.New,
                            Count = 1,
                            ElementDisplayType = HeatNetworkElementType.EnergyCentre,
                            ElementType = ElementTypeInShort.EC
                        }
                    ],
                    Elements =
                    [
                        new Element
                        {
                            BuildType = NetworkBuildStatus.New,
                            ElementType = ElementTypeInShort.EC,
                            ElementName = "Main EC"
                        },
                        new Element
                        {
                            BuildType = NetworkBuildStatus.Existing,
                            ElementType = ElementTypeInShort.EC,
                            ElementName = "Existing EC",
                            IsSupplyNewBuildings = true
                        }
                    ],
                    IsExistingEnergyCentreSupplyNewBuildings = true
                }
            });

            var result = Assert.IsType<ViewResult>(await controller.NetworkElementsOverViewAsync());
            var model = Assert.IsType<NetworkElementsOverViewModel>(result.Model);

            Assert.Equal("Design", model.Phase);
            Assert.Equal("District", model.NetworkType);
            Assert.Equal("51.5,-0.1", model.Coordinates);
            Assert.NotNull(model.NewNetworkElements);
            Assert.NotNull(model.ExistingNetworkElements);
            Assert.NotNull(model.NewNetworkEcNames);
            Assert.NotNull(model.ExistingNetworkEcNames);
            Assert.NotNull(model.ExistingEcSupplyingNewBuilding);
            Assert.True((bool)controller.ViewBag.SupplyToNewBuildings);
        }

    }
}
