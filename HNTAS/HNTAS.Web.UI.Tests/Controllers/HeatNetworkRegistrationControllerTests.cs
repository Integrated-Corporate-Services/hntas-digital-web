using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Models.HeatNetworkRegistration;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class HeatNetworkRegistrationControllerTests
    {
        private readonly Mock<ISessionHelper> _sessionHelperMock;
        private readonly Mock<IHeatNetworkService> _heatNetworkServiceMock;
        private readonly Mock<IOrganisationService> _organisationServiceMock;
        private readonly Mock<IAddressLookupService> _addressLookupServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ILogger<HeatNetworkRegistrationController>> _loggerMock;

        private readonly HeatNetworkRegistrationController _controller;

        public HeatNetworkRegistrationControllerTests()
        {
            _sessionHelperMock = new Mock<ISessionHelper>();
            _heatNetworkServiceMock = new Mock<IHeatNetworkService>();
            _organisationServiceMock = new Mock<IOrganisationService>();
            _addressLookupServiceMock = new Mock<IAddressLookupService>();
            _userServiceMock = new Mock<IUserService>();
            _loggerMock = new Mock<ILogger<HeatNetworkRegistrationController>>();
            _controller = CreateController();
        }

        private Mock<IUrlHelper> SetUpBackLink(string action)
        {
            return TestingUtility.SetUpBackLink("HeatNetworkRegistration", action);
        }

        private HeatNetworkRegistrationController CreateController()
        {
            var controller = new HeatNetworkRegistrationController(
                _sessionHelperMock.Object,
                _heatNetworkServiceMock.Object,
                _organisationServiceMock.Object,
                _addressLookupServiceMock.Object,
                _userServiceMock.Object,
                _loggerMock.Object
            );
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new MockHttpSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var tempDataProvider = new Mock<ITempDataProvider>();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);
            controller.Url = new Mock<IUrlHelper>().Object;
            return controller;
        }

        [Fact]
        public void HeatNetworkDwellingsCheck_Get_ReturnsView_WithSessionModel()
        {
            // Arrange
            var expectedModel = new HowManyDwellingsIncludedModel { HowManyDwellingsIncluded = "yes" };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<HowManyDwellingsIncludedModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HowManyDwellingsIncludedModelKey))
                .Returns(expectedModel);
            _controller.Url = TestingUtility.SetUpBackLink("UserManagement", "HeatNetworks").Object;

            // Act
            var result = _controller.HeatNetworkDwellingsCheck();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(expectedModel, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkDwellingsCheck_Get_ReturnsView_WithNewModel_WhenSessionNull()
        {
            // Arrange
            _sessionHelperMock
                .Setup(x => x.GetFromSession<HowManyDwellingsIncludedModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HowManyDwellingsIncludedModelKey))
                .Returns((HowManyDwellingsIncludedModel)null);

            // Act
            var result = _controller.HeatNetworkDwellingsCheck();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<HowManyDwellingsIncludedModel>(viewResult.Model);
        }

        [Fact]
        public void HeatNetworkDwellingsCheck_Post_ValidYes_RedirectsToOrganisation()
        {
            // Arrange
            var model = new HowManyDwellingsIncludedModel
            {
                HowManyDwellingsIncluded = "yes"
            };
            _controller.Url = TestingUtility.SetUpBackLink("UserManagement", "HeatNetworks").Object;

            // Act
            var result = _controller.HeatNetworkDwellingsCheck(model);

            // Assert
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.HowManyDwellingsIncludedModelKey,
                model), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkStatus", redirect.ActionName);
        }

        [Fact]
        public void HeatNetworkDwellingsCheck_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var model = new HowManyDwellingsIncludedModel();
            _controller.ModelState.AddModelError("HowManyDwellingsIncluded", "Required");

            // Act
            var result = _controller.HeatNetworkDwellingsCheck(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<HowManyDwellingsIncludedModel>()),
                Times.Never);
        }

        [Fact]
        public void SixOrMoreDwellingsAnswerNo_Get_ReturnsView()
        {
            // Act
            var result = _controller.SixOrMoreDwellingsAnswerNo();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void SixOrMoreDwellingsAnswerNo_Get_DoesNotRedirect()
        {
            // Act
            var result = _controller.SixOrMoreDwellingsAnswerNo();

            // Assert
            Assert.IsNotType<RedirectToActionResult>(result);
        }

        #region HeatNetworkStatus Tests

        [Fact]
        public void HeatNetworkStatus_Get_ReturnsView_WithSessionModel()
        {
            // Arrange
            var expectedModel = new NetworkStatusModel
            {
                BuildStatus = NullableOfNetworkBuildStatus.New.ToString()
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkStatusModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NetworkStatusModelKey))
                .Returns(expectedModel);

            // Act
            var result = _controller.HeatNetworkStatus();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(expectedModel, viewResult.Model);
        }

        [Fact]
        public void SaveHeatNetworkStatus_Post_New_RedirectsToNewNetwork()
        {
            // Arrange
            var model = new NetworkStatusModel
            {
                BuildStatus = NullableOfNetworkBuildStatus.New.ToString()
            };

            // Act
            var result = _controller.SaveHeatNetworkStatus(model);

            // Assert
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.NetworkStatusModelKey,
                model), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkEcNewNetwork", redirect.ActionName);
        }

        [Fact]
        public void SaveHeatNetworkStatus_Post_Existing_RedirectsToOfgemSummary()
        {
            // Arrange
            var model = new NetworkStatusModel
            {
                BuildStatus = NullableOfNetworkBuildStatus.Existing.ToString()
            };

            // Act
            var result = _controller.SaveHeatNetworkStatus(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ExistingHeatNetworkOfgemSummary", redirect.ActionName);
            Assert.Equal("HeatNetworkRegistration", redirect.ControllerName);
        }

        [Fact]
        public void SaveHeatNetworkStatus_Post_InvalidModel_ReturnsViewWithoutSaving()
        {
            // Arrange
            var model = new NetworkStatusModel();
            _controller.ModelState.AddModelError(nameof(NetworkStatusModel.BuildStatus), "Required");

            // Act
            var result = _controller.SaveHeatNetworkStatus(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<NetworkStatusModel>()), Times.Never);
        }

        #endregion

        #region HeatNetworkEcNewNetwork Tests

        [Fact]
        public void HeatNetworkEcNewNetwork_Get_ReturnsView_WithSessionModel()
        {
            // Arrange
            var expectedModel = new NetworkSupplyTypeModel
            {
                EnergyCentreOption = NullableOfEnergyCentreType.SuppliesMultipleBuildings.ToString()
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkSupplyTypeModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NetworkSupplyTypeModelKey))
                .Returns(expectedModel);

            // Act
            var result = _controller.HeatNetworkEcNewNetwork();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkEcNewNetwork", viewResult.ViewName);
            Assert.Equal(expectedModel, viewResult.Model);
            Assert.Equal("SaveEcNewNetwork", _controller.ViewBag.PostMethod);
        }

        [Fact]
        public void HeatNetworkEcNewNetwork_Get_ReturnsNewModel_WhenSessionNull()
        {
            // Arrange
            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkSupplyTypeModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NetworkSupplyTypeModelKey))
                .Returns((NetworkSupplyTypeModel)null);

            // Act
            var result = _controller.HeatNetworkEcNewNetwork();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<NetworkSupplyTypeModel>(viewResult.Model);
        }

        [Fact]
        public void SaveEcNewNetwork_Post_MultipleBuildings_RedirectsToConnections()
        {
            // Arrange
            var model = new NetworkSupplyTypeModel
            {
                EnergyCentreOption = NullableOfEnergyCentreType.SuppliesMultipleBuildings.ToString()
            };

            // Act
            var result = _controller.SaveEcNewNetwork(model);

            // Assert
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.NetworkSupplyTypeModelKey,
                model), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkConnections", redirect.ActionName);
        }

        [Fact]
        public void SaveEcNewNetwork_Post_SingleCommunalBuilding_RedirectsToConnectionSummary()
        {
            // Arrange
            var model = new NetworkSupplyTypeModel
            {
                EnergyCentreOption = NullableOfEnergyCentreType.SuppliesOneCommunalBuildingOnly.ToString()
            };

            // Act
            var result = _controller.SaveEcNewNetwork(model);

            // Assert
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.NetworkConnectionsModelKey,
                It.Is<NetworkConnectionsViewModel>(x =>
                    x.NewCommunalBuildings == 1 &&
                    x.ShowNewColumn == false &&
                    x.ShowExistingColumn == false)), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkConnectionSummary", redirect.ActionName);
        }

        [Fact]
        public void SaveEcNewNetwork_Post_InvalidModel_ReturnsViewWithoutSaving()
        {
            // Arrange
            var model = new NetworkSupplyTypeModel();
            _controller.ModelState.AddModelError(nameof(NetworkSupplyTypeModel.EnergyCentreOption), "Required");

            // Act
            var result = _controller.SaveEcNewNetwork(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkEcNewNetwork", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<NetworkSupplyTypeModel>()), Times.Never);
        }

        #endregion

        #region HeatNetworkConnections Tests

        [Fact]
        public void HeatNetworkConnections_Get_ReturnsView_WithSessionModelAndDisplayFlags()
        {
            // Arrange
            var expectedModel = new NetworkConnectionsViewModel
            {
                ExistingCommunalBuildings = 1
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkStatusModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NetworkStatusModelKey))
                .Returns(new NetworkStatusModel
                {
                    BuildStatus = NullableOfNetworkBuildStatus.NewAndExisting.ToString()
                });
            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkSupplyTypeModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NetworkSupplyTypeModelKey))
                .Returns(new NetworkSupplyTypeModel
                {
                    EnergyCentreOption = NullableOfEnergyCentreType.SuppliesMultipleBuildings.ToString()
                });
            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkConnectionsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NetworkConnectionsModelKey))
                .Returns(expectedModel);

            // Act
            var result = _controller.HeatNetworkConnections();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("TellUsAboutHeatNetworkConnections", viewResult.ViewName);
            Assert.Same(expectedModel, viewResult.Model);
            Assert.True(expectedModel.ShowExistingColumn);
            Assert.True(expectedModel.ShowNewColumn);
            Assert.True(expectedModel.ShowOtherDistrictNetworksRow);
        }

        [Fact]
        public void HeatNetworkConnections_Get_ReturnsNewModel_WhenSessionNull()
        {
            // Act
            var result = _controller.HeatNetworkConnections();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<NetworkConnectionsViewModel>(viewResult.Model);
        }

        [Fact]
        public void SaveHeatNetworkConnections_Post_WithCommunalBuildings_RedirectsToCommunalResponsibility()
        {
            // Arrange
            var model = new NetworkConnectionsViewModel
            {
                ShowNewColumn = true,
                NewCommunalBuildings = 1,
                NewHousesOrMaisonettes = 1
            };

            // Act
            var result = _controller.SaveHeatNetworkConnections(model);

            // Assert
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.NetworkConnectionsModelKey,
                model), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CommunalBuildingResponsibility", redirect.ActionName);
        }

        [Fact]
        public void SaveHeatNetworkConnections_Post_WithOtherDistrictNetworks_RedirectsToDistrictResponsibility()
        {
            // Arrange
            var model = new NetworkConnectionsViewModel
            {
                ShowNewColumn = true,
                NewOtherDistrictNetworks = 1
            };

            // Act
            var result = _controller.SaveHeatNetworkConnections(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("DistrictNetworksResponsibility", redirect.ActionName);
        }

        [Fact]
        public void SaveHeatNetworkConnections_Post_InvalidModel_ReturnsViewWithoutSaving()
        {
            // Arrange
            var model = new NetworkConnectionsViewModel
            {
                ShowNewColumn = true
            };
            _controller.ModelState.AddModelError(nameof(NetworkConnectionsViewModel.NewCommunalBuildings), "Required");

            // Act
            var result = _controller.SaveHeatNetworkConnections(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("TellUsAboutHeatNetworkConnections", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<NetworkConnectionsViewModel>()), Times.Never);
        }

        #endregion

        #region CommunalBuildingResponsibility Tests

        [Fact]
        public void CommunalBuildingResponsibility_Get_ReturnsView_WithSessionModelAndMaximum()
        {
            // Arrange
            var expectedModel = new CommunalBuildingResponsibilityViewModel
            {
                ResponsibleCommunalNetworks = 1
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkConnectionsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NetworkConnectionsModelKey))
                .Returns(new NetworkConnectionsViewModel { NewCommunalBuildings = 3 });
            _sessionHelperMock
                .Setup(x => x.GetFromSession<CommunalBuildingResponsibilityViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.CommunalBuildingResponsibilityModelKey))
                .Returns(expectedModel);

            // Act
            var result = _controller.CommunalBuildingResponsibility();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("CommunalBuildingResponsibility", viewResult.ViewName);
            Assert.Same(expectedModel, viewResult.Model);
            Assert.Equal(3, expectedModel.MaxAllowedCommunalBuildings);
        }

        [Fact]
        public void CommunalBuildingResponsibility_Get_ReturnsNewModel_WhenSessionNull()
        {
            // Arrange
            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkConnectionsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NetworkConnectionsModelKey))
                .Returns((NetworkConnectionsViewModel)null);

            // Act
            var result = _controller.CommunalBuildingResponsibility();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CommunalBuildingResponsibilityViewModel>(viewResult.Model);
            Assert.Equal(0, model.MaxAllowedCommunalBuildings);
        }

        [Fact]
        public void SaveCommunalBuildingResponsibility_Post_NoDistrictNetworks_RedirectsToSummary()
        {
            // Arrange
            var model = new CommunalBuildingResponsibilityViewModel
            {
                ResponsibleCommunalNetworks = 1
            };
            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkConnectionsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NetworkConnectionsModelKey))
                .Returns(new NetworkConnectionsViewModel { NewCommunalBuildings = 2 });

            // Act
            var result = _controller.SaveCommunalBuildingResponsibility(model);

            // Assert
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.CommunalBuildingResponsibilityModelKey,
                model), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkConnectionSummary", redirect.ActionName);
        }

        [Fact]
        public void SaveCommunalBuildingResponsibility_Post_WithDistrictNetworks_RedirectsToDistrictResponsibility()
        {
            // Arrange
            var model = new CommunalBuildingResponsibilityViewModel
            {
                ResponsibleCommunalNetworks = 1
            };
            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkConnectionsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NetworkConnectionsModelKey))
                .Returns(new NetworkConnectionsViewModel
                {
                    NewCommunalBuildings = 2,
                    NewOtherDistrictNetworks = 1
                });

            // Act
            var result = _controller.SaveCommunalBuildingResponsibility(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("DistrictNetworksResponsibility", redirect.ActionName);
        }

        [Fact]
        public void SaveCommunalBuildingResponsibility_Post_ExceedsMaximum_ReturnsViewWithoutSaving()
        {
            // Arrange
            var model = new CommunalBuildingResponsibilityViewModel
            {
                ResponsibleCommunalNetworks = 3
            };
            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkConnectionsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NetworkConnectionsModelKey))
                .Returns(new NetworkConnectionsViewModel { NewCommunalBuildings = 2 });

            // Act
            var result = _controller.SaveCommunalBuildingResponsibility(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("CommunalBuildingResponsibility", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<CommunalBuildingResponsibilityViewModel>()), Times.Never);
        }

        #endregion

        #region DistrictNetworksResponsibility Tests

        [Fact]
        public void DistrictNetworksResponsibility_Get_ReturnsView_WithSessionModelAndMaximum()
        {
            // Arrange
            var expectedModel = new DistrictNetworksResponsibilityViewModel
            {
                ResponsibleDistrictNetworks = 1
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkConnectionsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NetworkConnectionsModelKey))
                .Returns(new NetworkConnectionsViewModel { NewOtherDistrictNetworks = 3 });
            _sessionHelperMock
                .Setup(x => x.GetFromSession<DistrictNetworksResponsibilityViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.DistrictNetworksResponsibilityModelKey))
                .Returns(expectedModel);

            // Act
            var result = _controller.DistrictNetworksResponsibility();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("DistrictNetworksResponsibility", viewResult.ViewName);
            Assert.Same(expectedModel, viewResult.Model);
            Assert.Equal(3, expectedModel.MaxAllowedDistrictNetworks);
        }

        [Fact]
        public void DistrictNetworksResponsibility_Get_ReturnsNewModel_WhenSessionNull()
        {
            // Act
            var result = _controller.DistrictNetworksResponsibility();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DistrictNetworksResponsibilityViewModel>(viewResult.Model);
            Assert.Equal(0, model.MaxAllowedDistrictNetworks);
        }

        [Fact]
        public void DistrictNetworksResponsibility_Post_Valid_RedirectsToSummary()
        {
            // Arrange
            var model = new DistrictNetworksResponsibilityViewModel
            {
                ResponsibleDistrictNetworks = 1
            };
            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkConnectionsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NetworkConnectionsModelKey))
                .Returns(new NetworkConnectionsViewModel { NewOtherDistrictNetworks = 2 });

            // Act
            var result = _controller.DistrictNetworksResponsibility(model);

            // Assert
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.DistrictNetworksResponsibilityModelKey,
                model), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkConnectionSummary", redirect.ActionName);
        }

        [Fact]
        public void DistrictNetworksResponsibility_Post_ExceedsMaximum_ReturnsViewWithoutSaving()
        {
            // Arrange
            var model = new DistrictNetworksResponsibilityViewModel
            {
                ResponsibleDistrictNetworks = 3
            };
            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkConnectionsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NetworkConnectionsModelKey))
                .Returns(new NetworkConnectionsViewModel { NewOtherDistrictNetworks = 2 });

            // Act
            var result = _controller.DistrictNetworksResponsibility(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("DistrictNetworksResponsibility", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<DistrictNetworksResponsibilityViewModel>()), Times.Never);
        }

        #endregion

        #region HeatNetworkConnectionSummary Tests

        [Fact]
        public void HeatNetworkConnectionSummary_Get_ReturnsPopulatedViewModel()
        {
            // Arrange
            var connections = new NetworkConnectionsViewModel
            {
                ShowNewColumn = true,
                NewCommunalBuildings = 2
            };
            var energyCentre = new NetworkSupplyTypeModel
            {
                EnergyCentreOption = NullableOfEnergyCentreType.SuppliesMultipleBuildings.ToString()
            };
            var responsibility = new CommunalBuildingResponsibilityViewModel
            {
                ResponsibleCommunalNetworks = 1
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkConnectionsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NetworkConnectionsModelKey))
                .Returns(connections);
            _sessionHelperMock
                .Setup(x => x.GetFromSession<NetworkSupplyTypeModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.NetworkSupplyTypeModelKey))
                .Returns(energyCentre);
            _sessionHelperMock
                .Setup(x => x.GetFromSession<CommunalBuildingResponsibilityViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.CommunalBuildingResponsibilityModelKey))
                .Returns(responsibility);

            // Act
            var result = _controller.HeatNetworkConnectionSummary();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkConnectionSummary", viewResult.ViewName);
            var viewModel = Assert.IsType<HeatNetworkConnectionSummaryViewModel>(viewResult.Model);
            Assert.Same(connections, viewModel.Connections);
            Assert.Equal(energyCentre.EnergyCentreOption, viewModel.EnergyCentreOption);
            Assert.Same(responsibility, viewModel.Responsibility);
            Assert.Equal("HeatNetworkConnections", _controller.ViewBag.ChangeAction);
        }

        [Fact]
        public void HeatNetworkConnectionSummary_Get_ReturnsEmptyViewModel_WhenSessionNull()
        {
            // Act
            var result = _controller.HeatNetworkConnectionSummary();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsType<HeatNetworkConnectionSummaryViewModel>(viewResult.Model);
            Assert.NotNull(viewModel.Connections);
            Assert.Empty(viewModel.EnergyCentreOption);
            Assert.Null(viewModel.Responsibility);
        }

        [Fact]
        public void SaveHeatNetworkConnectionSummary_Post_RedirectsToHeatNetworkName()
        {
            // Act
            var result = _controller.SaveHeatNetworkConnectionSummary();

            // Assert
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                "backActionFromHnName",
                "HeatNetworkConnectionSummary"), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkName", redirect.ActionName);
        }

        #endregion

        #region HeatNetworkNameAndAddress Tests

        [Fact]
        public void HeatNetworkName_Get_ReturnsSessionModel()
        {
            var expectedModel = new HeatNetworkNameModel { HeatNetworkName = "Test Network" };
            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkNameModel>(
                It.IsAny<HttpContext>(), SessionKeys.HeatNetworkNameModelKey)).Returns(expectedModel);

            var result = _controller.HeatNetworkName();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/HeatNetworkName", viewResult.ViewName);
            Assert.Same(expectedModel, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkName_Post_Valid_RedirectsToPostcodeQuestion()
        {
            var model = new HeatNetworkNameModel { HeatNetworkName = "Test Network" };

            var result = _controller.HeatNetworkName(model);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(), SessionKeys.HeatNetworkNameModelKey, model), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("DoesHNHaveAPostcode", redirect.ActionName);
        }

        [Fact]
        public async Task DoesHNHaveAPostcode_Post_NoPostcode_RedirectsToCoordinates()
        {
            var model = new DoesHNHaveAPostcodeViewModel { HasPostcode = false };

            var result = await _controller.DoesHNHaveAPostcode(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ECCoordinates", redirect.ActionName);
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(), SessionKeys.DoesHNHaveAPostcodeViewModelKey, model), Times.Once);
        }

        [Fact]
        public async Task DoesHNHaveAPostcode_Post_ValidPostcode_RedirectsToSearchResults()
        {
            _addressLookupServiceMock.Setup(x => x.PostcodeLookupAsync(" sw1a 2aa "))
                .ReturnsAsync(new SearchAddressByPostcodeModel
                {
                    Addresses = new[] { "10 Downing Street, London, SW1A 2AA" }
                });
            var model = new DoesHNHaveAPostcodeViewModel { HasPostcode = true, Postcode = " sw1a 2aa " };

            var result = await _controller.DoesHNHaveAPostcode(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("SearchByPostcodeResults", redirect.ActionName);
            Assert.Equal("SW1A 2AA", model.Postcode);
        }

        [Fact]
        public void SearchByPostcodeResults_Get_ReturnsResultsModel()
        {
            var expectedModel = new SearchAddressByPostcodeModel
            {
                Addresses = new[] { "10 Downing Street, London, SW1A 2AA" }
            };
            _sessionHelperMock.Setup(x => x.GetFromSession<SearchAddressByPostcodeModel>(
                It.IsAny<HttpContext>(), SessionKeys.SearchAddressByPostcodeModelSessionKey))
                .Returns(expectedModel);

            var result = _controller.SearchByPostcodeResults();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/SearchByPostcodeResults", viewResult.ViewName);
            Assert.Same(expectedModel, viewResult.Model);
        }

        [Fact]
        public void SearchByPostcodeResults_Get_WhenSessionNull_ReturnsPostcodeView()
        {
            var result = _controller.SearchByPostcodeResults();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/DoesHNHaveAPostcode", viewResult.ViewName);
        }

        [Fact]
        public void SelectAddress_Get_ValidAddress_SavesAddressAndRedirects()
        {
            _sessionHelperMock.Setup(x => x.GetFromSession<SearchAddressByPostcodeModel>(
                It.IsAny<HttpContext>(), SessionKeys.SearchAddressByPostcodeModelSessionKey))
                .Returns(new SearchAddressByPostcodeModel());
            const string selectedAddress = "10 Downing Street, London, SW1A 2AA";

            var result = _controller.SelectAddress(selectedAddress);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(), SessionKeys.AddressByStreetOrTownModelSessionKey,
                It.Is<AddressByStreetOrTownModel>(m =>
                    m.StreetAddress == "10 downing street" &&
                    m.TownOrCity == "London" &&
                    m.Postalcode == "SW1A 2AA" &&
                    m.Country == "United Kingdom")), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ConfirmAddress", redirect.ActionName);
        }

        [Fact]
        public void SelectAddress_Get_WhenSessionMissing_ReturnsBadRequest()
        {
            var result = _controller.SelectAddress("10 Downing Street, London, SW1A 2AA");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void AddressManualEntry_Get_ReturnsDefaultCountry()
        {
            var result = _controller.AddressManualEntry();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AddressByStreetOrTownModel>(viewResult.Model);
            Assert.Equal("United Kingdom", model.Country);
        }

        [Fact]
        public void AddressManualEntry_Post_Valid_RedirectsToCoordinates()
        {
            var model = new AddressByStreetOrTownModel
            {
                StreetAddress = "10 Downing Street",
                TownOrCity = "London",
                Postalcode = "sw1a 2aa",
                Country = "United Kingdom"
            };

            var result = _controller.AddressManualEntry(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ECCoordinates", redirect.ActionName);
            Assert.Equal("10 Downing Street, London, SW1A 2AA, United Kingdom", model.Fulladdress);
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(), SessionKeys.HeatNetworkLocationModelKey,
                It.Is<HeatNetworkLocationModel>(m => m.HNAddressByStreet == model)), Times.Once);
        }

        [Fact]
        public void ConfirmAddress_Get_ReturnsSessionAddress()
        {
            var expectedModel = new AddressByStreetOrTownModel { StreetAddress = "10 Downing Street" };
            _sessionHelperMock.Setup(x => x.GetFromSession<AddressByStreetOrTownModel>(
                It.IsAny<HttpContext>(), SessionKeys.AddressByStreetOrTownModelSessionKey))
                .Returns(expectedModel);

            var result = _controller.ConfirmAddress();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/ConfirmAddress", viewResult.ViewName);
            Assert.Same(expectedModel, viewResult.Model);
        }

        [Fact]
        public void ConfirmAddress_Post_RedirectsToSaveAddress()
        {
            var result = _controller.ConfirmAddress(new AddressByStreetOrTownModel());

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("SaveHNAddressByPostcode", redirect.ActionName);
        }

        [Fact]
        public void SaveHNAddressByPostcode_Get_WithPostcode_SavesLocationAndRedirects()
        {
            var address = new AddressByStreetOrTownModel { StreetAddress = "10 Downing Street" };
            _sessionHelperMock.Setup(x => x.GetFromSession<AddressByStreetOrTownModel>(
                It.IsAny<HttpContext>(), SessionKeys.AddressByStreetOrTownModelSessionKey)).Returns(address);
            _sessionHelperMock.Setup(x => x.GetFromSession<DoesHNHaveAPostcodeViewModel>(
                It.IsAny<HttpContext>(), SessionKeys.DoesHNHaveAPostcodeViewModelKey))
                .Returns(new DoesHNHaveAPostcodeViewModel { HasPostcode = true });

            var result = _controller.SaveHNAddressByPostcode();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ECCoordinates", redirect.ActionName);
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(), SessionKeys.HeatNetworkLocationModelKey,
                It.Is<HeatNetworkLocationModel>(m => m.HNAddressByStreet == address)), Times.Once);
        }

        [Fact]
        public void ECCoordinates_Get_ReturnsSessionModel()
        {
            var expectedModel = new ECDetailsModel { LatitudeLongitude = "51.5,-0.1" };
            _sessionHelperMock.Setup(x => x.GetFromSession<ECDetailsModel>(
                It.IsAny<HttpContext>(), SessionKeys.ECDetailsModelSessionKey)).Returns(expectedModel);

            var result = _controller.ECCoordinates();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/ECCoordinates", viewResult.ViewName);
            Assert.Same(expectedModel, viewResult.Model);
        }

        [Fact]
        public void ECCoordinates_Post_ValidCoordinates_SavesAndRedirectsToPhase()
        {
            var model = new ECDetailsModel { LatitudeLongitude = "51.5, -0.1" };

            var result = _controller.ECCoordinates(model);

            Assert.Equal(51.5m, model.ECAddressByLatLong.Latitude);
            Assert.Equal(-0.1m, model.ECAddressByLatLong.Longitude);
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(), SessionKeys.ECDetailsModelSessionKey, model), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkPhase", redirect.ActionName);
        }

        [Fact]
        public void ECCoordinates_Post_InvalidCoordinates_ReturnsViewWithoutSaving()
        {
            var model = new ECDetailsModel { LatitudeLongitude = "not coordinates" };

            var result = _controller.ECCoordinates(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/ECCoordinates", viewResult.ViewName);
            Assert.False(_controller.ModelState.IsValid);
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(), SessionKeys.ECDetailsModelSessionKey, It.IsAny<ECDetailsModel>()), Times.Never);
        }

        #endregion

        #region HeatNetworkPhaseAndSubmission Tests

        [Fact]
        public void HeatNetworkPhase_Get_ReturnsSessionModel()
        {
            var expectedModel = new HeatNetworkPhaseModel { HeatNetworkPhase = "Design" };
            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkPhaseModel>(
                It.IsAny<HttpContext>(), SessionKeys.HeatNetworkPhaseModelKey)).Returns(expectedModel);

            var result = _controller.HeatNetworkPhase();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/HeatNetworkPhase", viewResult.ViewName);
            Assert.Same(expectedModel, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkPhase_Post_ValidPhase_SavesPhaseAndPathway()
        {
            var model = new HeatNetworkPhaseModel { HeatNetworkPhase = "Feasibility" };

            var result = _controller.HeatNetworkPhase(model);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(), SessionKeys.HeatNetworkPhaseModelKey, model), Times.Once);
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(), SessionKeys.PathwayModelKey,
                It.Is<PathwayModel>(m => m.Pathway == "1")), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CheckYourAnswers", redirect.ActionName);
            Assert.Equal("HeatNetworkRegistration", redirect.ControllerName);
        }

        [Fact]
        public void HeatNetworkPhase_Post_InvalidPhase_ReturnsViewWithoutSaving()
        {
            var model = new HeatNetworkPhaseModel { HeatNetworkPhase = "Invalid" };

            var result = _controller.HeatNetworkPhase(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/HeatNetworkPhase", viewResult.ViewName);
            Assert.False(_controller.ModelState.IsValid);
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(), SessionKeys.HeatNetworkPhaseModelKey, model), Times.Once);
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(), SessionKeys.PathwayModelKey, It.IsAny<PathwayModel>()), Times.Never);
        }

        [Fact]
        public void HeatNetworkLeaveService_Get_ReturnsView()
        {
            var result = _controller.HeatNetworkLeaveService();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/HeatNetworkLeaveService", viewResult.ViewName);
        }

        [Fact]
        public async Task CheckYourAnswersAsync_MissingRequiredSession_RedirectsToDashboard()
        {
            var result = await _controller.CheckYourAnswersAsync();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UserAccount", redirect.ActionName);
            Assert.Equal("Dashboard", redirect.ControllerName);
        }

        [Fact]
        public async Task CheckYourAnswersAsync_CompleteSession_ReturnsSummaryView()
        {
            _sessionHelperMock.Setup(x => x.GetFromSession<HowManyDwellingsIncludedModel>(
                It.IsAny<HttpContext>(), SessionKeys.HowManyDwellingsIncludedModelKey))
                .Returns(new HowManyDwellingsIncludedModel { HowManyDwellingsIncluded = "yes" });
            _sessionHelperMock.Setup(x => x.GetFromSession<NetworkStatusModel>(
                It.IsAny<HttpContext>(), SessionKeys.NetworkStatusModelKey))
                .Returns(new NetworkStatusModel { BuildStatus = NullableOfNetworkBuildStatus.New.ToString() });
            _sessionHelperMock.Setup(x => x.GetFromSession<NetworkSupplyTypeModel>(
                It.IsAny<HttpContext>(), SessionKeys.NetworkSupplyTypeModelKey))
                .Returns(new NetworkSupplyTypeModel { EnergyCentreOption = NullableOfEnergyCentreType.SuppliesMultipleBuildings.ToString() });
            _sessionHelperMock.Setup(x => x.GetFromSession<NetworkConnectionsViewModel>(
                It.IsAny<HttpContext>(), SessionKeys.NetworkConnectionsModelKey))
                .Returns(new NetworkConnectionsViewModel { NewCommunalBuildings = 2 });
            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkNameModel>(
                It.IsAny<HttpContext>(), SessionKeys.HeatNetworkNameModelKey))
                .Returns(new HeatNetworkNameModel { HeatNetworkName = "Test Network" });
            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkPhaseModel>(
                It.IsAny<HttpContext>(), SessionKeys.HeatNetworkPhaseModelKey))
                .Returns(new HeatNetworkPhaseModel { HeatNetworkPhase = "Design" });

            var result = await _controller.CheckYourAnswersAsync();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/CheckYourAnswers", viewResult.ViewName);
            Assert.IsType<CheckYourAnswersHeatNetworkModel>(viewResult.Model);
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(), SessionKeys.CheckYourAnswersHeatNetworkModelKey,
                It.IsAny<CheckYourAnswersHeatNetworkModel>()), Times.Once);
        }

        [Fact]
        public async Task SubmitAnswers_NotConfirmed_ReturnsSummaryViewWithoutSubmitting()
        {
            var result = await _controller.SubmitAnswers(false);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/CheckYourAnswers", viewResult.ViewName);
            _heatNetworkServiceMock.Verify(x => x.AddHeatNetwork(It.IsAny<HeatNetwork>()), Times.Never);
        }

        [Fact]
        public async Task SubmitAnswers_ConfirmedWithoutUserOrOrganisation_ReturnsSummaryViewWithError()
        {
            _sessionHelperMock.Setup(x => x.GetFromSession<CheckYourAnswersHeatNetworkModel>(
                It.IsAny<HttpContext>(), SessionKeys.CheckYourAnswersHeatNetworkModelKey))
                .Returns(new CheckYourAnswersHeatNetworkModel());

            var result = await _controller.SubmitAnswers(true);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/CheckYourAnswers", viewResult.ViewName);
            Assert.Equal("An error occurred while submitting your heat network details. Please try again later",
                _controller.TempData["ErrorMessage"]);
            _heatNetworkServiceMock.Verify(x => x.AddHeatNetwork(It.IsAny<HeatNetwork>()), Times.Never);
        }

        #endregion

        #region Registration Completion Tests

        [Fact]
        public async Task HeatNetworkRegistrationComplete_Get_ReturnsCompletionViewWithTempData()
        {
            _controller.TempData["Confirmation_HN_Id"] = "hn-123";
            _controller.TempData["HNName"] = "Test Network";
            _controller.TempData["AdditionalDescription"] = "Additional details";

            var result = await _controller.HeatNetworkRegistrationComplete();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/HeatNetworkRegistrationComplete", viewResult.ViewName);
            Assert.Equal("hn-123", _controller.ViewBag.HNId);
            Assert.Equal("Test Network, Additional details", _controller.ViewBag.HNNameWithDescription);
        }

        [Fact]
        public async Task HeatNetworkSuccessRedirection_Get_ReturnsSavedModelAndNotification()
        {
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(), SessionKeys.HnId)).Returns("hn-123");
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(), SessionKeys.HnName)).Returns("Test Network");
            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkSuccessRedirection>(
                It.IsAny<HttpContext>(), SessionKeys.HeatNetworkSuccessRedirectionSessionKey))
                .Returns(new HeatNetworkSuccessRedirection { NextAction = "Dashboard" });
            _heatNetworkServiceMock.Setup(x => x.GetAsync("hn-123"))
                .ReturnsAsync(new HeatNetworkResponse
                {
                    NoOfResponsibleCommunalBuilding = 2,
                    NoOfResponsibleDistrictNetwork = 1
                });

            var result = await _controller.HeatNetworkSuccessRedirection();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/HeatNetworkSuccessRedirection", viewResult.ViewName);
            Assert.IsType<HeatNetworkSuccessRedirection>(viewResult.Model);
            Assert.Contains("2 new communal buildings and 1 other new district networks", (string)_controller.ViewBag.NotificationMessage);
        }

        [Fact]
        public async Task HeatNetworkSuccessRedirection_Get_WhenNoSavedModel_ReturnsNewModel()
        {
            _heatNetworkServiceMock.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HeatNetworkResponse());

            var result = await _controller.HeatNetworkSuccessRedirection();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<HeatNetworkSuccessRedirection>(viewResult.Model);
            Assert.Empty((string)_controller.ViewBag.NotificationMessage);
        }

        [Fact]
        public void HeatNetworkSuccessRedirection_Post_HnDetails_RedirectsToDetails()
        {
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(), SessionKeys.HnId)).Returns("hn-123");

            var result = _controller.HeatNetworkSuccessRedirection(
                new HeatNetworkSuccessRedirection { NextAction = "HNDetails" });

            _sessionHelperMock.Verify(x => x.ClearFromSession(
                It.IsAny<HttpContext>(), SessionKeys.HeatNetworkSuccessRedirectionSessionKey), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AddNetworkDetails", redirect.ActionName);
            Assert.Equal("HeatNetwork", redirect.ControllerName);
            Assert.Equal("hn-123", redirect.RouteValues["hnid"]);
        }

        [Fact]
        public void HeatNetworkSuccessRedirection_Post_AddHn_RedirectsToRegistration()
        {
            var result = _controller.HeatNetworkSuccessRedirection(
                new HeatNetworkSuccessRedirection { NextAction = "AddHN" });

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkDwellingsCheck", redirect.ActionName);
            Assert.Equal("HeatNetworkRegistration", redirect.ControllerName);
        }

        [Fact]
        public void HeatNetworkSuccessRedirection_Post_Dashboard_RedirectsToDashboard()
        {
            var result = _controller.HeatNetworkSuccessRedirection(
                new HeatNetworkSuccessRedirection { NextAction = "Dashboard" });

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UserAccount", redirect.ActionName);
            Assert.Equal("Dashboard", redirect.ControllerName);
        }

        [Fact]
        public void HeatNetworkSuccessRedirection_Post_InvalidModel_ReturnsViewWithoutClearingSession()
        {
            _controller.ModelState.AddModelError(nameof(HeatNetworkSuccessRedirection.NextAction), "Required");
            var model = new HeatNetworkSuccessRedirection();

            var result = _controller.HeatNetworkSuccessRedirection(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/HeatNetworkSuccessRedirection", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
            _sessionHelperMock.Verify(x => x.ClearFromSession(
                It.IsAny<HttpContext>(), It.IsAny<string>()), Times.Never);
        }

        #endregion


    }
}