using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Client;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.ElementSoa;
using HNTAS.Web.UI.Models.NetworkElements;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class ExistingElementSoaControllerTests
    {
        private readonly Mock<ISessionHelper> _sessionHelperMock = new();
        private readonly Mock<ISoaService> _soaServiceMock = new();
        private readonly Mock<ILogger<ExistingElementSoaController>> _loggerMock = new();
        private readonly Mock<IHeatNetworkService> _heatNetworkServiceMock = new();
        private readonly Mock<IAssessorApi> _assessorApiMock = new();

        private ExistingElementSoaController CreateController()
        {
            var httpContext = new DefaultHttpContext();
            var controller = new ExistingElementSoaController(
                _sessionHelperMock.Object,
                _soaServiceMock.Object,
                _loggerMock.Object,
                _heatNetworkServiceMock.Object,
                _assessorApiMock.Object)
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
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId)).Returns("HN-1");
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnName)).Returns("Heat Network");
        }

        private static HeatNetworkResponse CreateHeatNetworkResponseForSoa()
        {
            return new HeatNetworkResponse
            {
                Phase = "Design",
                HeatNetworkType = NullableOfHeatNetworkType.District,
                HasOwnEnergyCentre = true,
                NetworkElements = new NetworkElementsResponse
                {
                    ElementsGroup =
                    [
                        new ElementGroup
                        {
                            ElementType = ElementTypeInShort.EC,
                            ElementDisplayType = HeatNetworkElementType.EnergyCentre,
                            Count = 1
                        }
                    ]
                }
            };
        }

        private static ExistingElementSoaUpdateStatusViewModel CreateInvalidSoaStatusModel()
        {
            return new ExistingElementSoaUpdateStatusViewModel
            {
                Milestone = Milestone.Milestone2,
                ElementType = ElementTypeInShort.EC,
                ElementDisplayType = HeatNetworkElementType.EnergyCentre,
                ElementCount = 1,
                SelectedSoaStatusOptions = [SoaStatus.NotStarted],
                SoaStatusCounts = new Dictionary<SoaStatus, int?>
                {
                    { SoaStatus.NotStarted, null }
                }
            };
        }

        private static AssessorSelectElementsViewModel CreateSelectedElementsModel()
        {
            return new AssessorSelectElementsViewModel
            {
                SelectedElementIds = [ElementTypeInShort.EC, ElementTypeInShort.SS],
                ElementOptions =
                [
                    new AssessorSelectElementsOption { ElementType = ElementTypeInShort.EC, Label = "Energy centre" },
                    new AssessorSelectElementsOption { ElementType = ElementTypeInShort.SS, Label = "Substations" }
                ]
            };
        }

        private static AssessorAssessmentSelectionViewModel CreateAssessmentSessionModel(ElementTypeInShort type)
        {
            return new AssessorAssessmentSelectionViewModel
            {
                ElementType = type,
                AssessmentOptions = [new AssessmentOption { Label = "Assessment 1", Hint = "Hint" }]
            };
        }

        private void SetupAssessmentSelectionCommon()
        {
            SetupDefaultSession();
            _sessionHelperMock.Setup(x => x.GetFromSession<Milestone?>(It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding)).Returns(Milestone.Milestone3B);
            _sessionHelperMock.Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey)).Returns(CreateSelectedElementsModel());
        }

        [Fact]
        public void UnderstandingSoa_Get_ReturnsView()
        {
            var controller = CreateController();

            var result = controller.UnderstandingSoa();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void SubmitUnderstandingSoa_Post_RedirectsToSoaMilestones()
        {
            var controller = CreateController();

            var result = Assert.IsType<RedirectToActionResult>(controller.SubmitUnderstandingSoa());

            Assert.Equal("SoaMilestones", result.ActionName);
        }

        [Fact]
        public async Task SoaMilestones_Get_ReturnsView()
        {
            var controller = CreateController();
            SetupDefaultSession();
            _sessionHelperMock.Setup(x => x.GetFromSession<int?>(It.IsAny<HttpContext>(), SessionKeys.CurrentStageIndexSessionKey)).Returns(0);
            _heatNetworkServiceMock.Setup(x => x.GetAsync("HN-1")).ReturnsAsync(CreateHeatNetworkResponseForSoa());

            var result = Assert.IsType<ViewResult>(await controller.SoaMilestones());

            Assert.Equal("SoaMilestones", result.ViewName);
            Assert.NotNull(result.Model);
        }

        [Fact]
        public async Task SoaUpdateStatus_Get_ReturnsViewAndSavesModelInSession()
        {
            var controller = CreateController();
            SetupDefaultSession();
            _heatNetworkServiceMock.Setup(x => x.GetAsync("HN-1")).ReturnsAsync(CreateHeatNetworkResponseForSoa());

            controller.TempData["Soa_EC_Milestone2_Phase"] = "Design";
            controller.TempData["Soa_EC_Milestone2_Element"] = "Energy centre";

            var result = Assert.IsType<ViewResult>(await controller.SoaUpdateStatus(Milestone.Milestone2, ElementTypeInShort.EC, HeatNetworkElementType.EnergyCentre));

            Assert.IsType<ExistingElementSoaUpdateStatusViewModel>(result.Model);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.CurrentStageIndexSessionKey, It.IsAny<int>()), Times.Once);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.ElementSoaStatusUpdateModelSessionKey, It.IsAny<ExistingElementSoaUpdateStatusViewModel>()), Times.Once);
        }

        [Fact]
        public async Task SoaUpdateStatus_Post_WithInvalidCounts_ReturnsView()
        {
            var controller = CreateController();
            SetupDefaultSession();
            _sessionHelperMock.Setup(x => x.GetFromSession<int?>(It.IsAny<HttpContext>(), SessionKeys.CurrentStageIndexSessionKey)).Returns(1);
            var model = CreateInvalidSoaStatusModel();

            var result = Assert.IsType<ViewResult>(await controller.SoaUpdateStatus(model));

            Assert.Same(model, result.Model);
            _soaServiceMock.Verify(x => x.UpdateElementSoaStatusForExistingNetwork(It.IsAny<ElementSoaStatusUpdateRequestForExistingNetwork>()), Times.Never);
        }

        [Fact]
        public void PrepareAssessorOnboarding_Get_SavesContextAndRedirects()
        {
            var controller = CreateController();

            var result = Assert.IsType<RedirectToActionResult>(controller.PrepareAssessorOnboarding(Milestone.Milestone2, "Stage 2", ElementTypeInShort.EC));

            Assert.Equal("AssessorOnboarding", result.ActionName);
            Assert.Equal("ExistingElementSoa", result.ControllerName);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding, Milestone.Milestone2), Times.Once);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.SoaStageTitleOfAssessorOnboarding, "Stage 2"), Times.Once);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.SoaElementTypeOfAssessorOnboarding, ElementTypeInShort.EC), Times.Once);
        }

        [Fact]
        public void AssessorOnboarding_Get_ReturnsView()
        {
            var controller = CreateController();
            SetupDefaultSession();
            _sessionHelperMock.Setup(x => x.GetFromSession<Milestone>(It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding)).Returns(Milestone.Milestone2);
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.SoaStageTitleOfAssessorOnboarding)).Returns("Stage 1");

            var result = controller.AssessorOnboarding();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task SearchAssessor_WithShortQuery_ReturnsEmptyJson()
        {
            var controller = CreateController();

            var result = await controller.SearchAssessor("a");

            Assert.NotNull(result);
            _assessorApiMock.Verify(
            x => x.ApiAssessorSearchGetAsync(It.IsAny<Option<string>>(), It.IsAny<CancellationToken>()),
            Times.Never);
        }

        [Fact]
        public async Task SelectedAssessorOnboarding_Post_WithMissingSelection_ReturnsOnboardingView()
        {
            var controller = CreateController();
            _sessionHelperMock.Setup(x => x.GetFromSession<int?>(It.IsAny<HttpContext>(), SessionKeys.CurrentStageIndexSessionKey)).Returns(1);
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.SoaStageTitleOfAssessorOnboarding)).Returns("Stage 1");

            var result = Assert.IsType<ViewResult>(await controller.SelectedAssessorOnboarding("A", "B", "a@b.com", null!));

            Assert.Equal("AssessorOnboarding", result.ViewName);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task AssessorSelectElementsAsync_Get_WithSavedModel_ReturnsView()
        {
            var controller = CreateController();
            SetupDefaultSession();
            var savedModel = CreateSelectedElementsModel();

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.SoaStageTitleOfAssessorOnboarding)).Returns("Stage 3B");
            _sessionHelperMock.Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey)).Returns(savedModel);

            var result = Assert.IsType<ViewResult>(await controller.AssessorSelectElementsAsync());

            Assert.Same(savedModel, result.Model);
        }

        [Fact]
        public void AssessorSelectElements_Post_SavesAndRedirectsToAssessmentRoute()
        {
            var controller = CreateController();
            SetupDefaultSession();
            var sessionModel = CreateSelectedElementsModel();
            var postedModel = new AssessorSelectElementsViewModel
            {
                SelectedElementIds = [ElementTypeInShort.EC],
                SelectedElementLabel = []
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey)).Returns(sessionModel);

            var result = Assert.IsType<RedirectToActionResult>(controller.AssessorSelectElements(postedModel));

            Assert.Equal("AssessmentSelectionEc", result.ActionName);
            Assert.Equal("ExistingElementSoa", result.ControllerName);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey, postedModel), Times.Once);
        }

        [Fact]
        public void AssessmentSelectionEcAsync_Get_ReturnsAssessmentSelectionView()
        {
            var controller = CreateController();
            SetupAssessmentSelectionCommon();

            var result = Assert.IsType<ViewResult>(controller.AssessmentSelectionEcAsync());

            Assert.Equal("AssessmentSelection", result.ViewName);
        }

        [Fact]
        public async Task AssessmentSelectionEcAsync_Post_WithMissingSelection_ReturnsView()
        {
            var controller = CreateController();
            SetupAssessmentSelectionCommon();
            var sessionModel = CreateAssessmentSessionModel(ElementTypeInShort.EC);
            _sessionHelperMock.Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(It.IsAny<HttpContext>(), SessionKeys.AssessorAssessmentSelectionEcViewModelSessionKey)).Returns(sessionModel);

            var result = Assert.IsType<ViewResult>(await controller.AssessmentSelectionEcAsync(new AssessorAssessmentSelectionViewModel { ElementType = ElementTypeInShort.EC }));

            Assert.Equal("AssessmentSelection", result.ViewName);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public void AssessmentSelectionSs_Get_ReturnsAssessmentSelectionView()
        {
            var controller = CreateController();
            SetupAssessmentSelectionCommon();

            var result = Assert.IsType<ViewResult>(controller.AssessmentSelectionSs());

            Assert.Equal("AssessmentSelection", result.ViewName);
        }

        [Fact]
        public async Task AssessmentSelectionSsAsync_Post_WithMissingSelection_ReturnsView()
        {
            var controller = CreateController();
            SetupAssessmentSelectionCommon();
            var sessionModel = CreateAssessmentSessionModel(ElementTypeInShort.SS);
            _sessionHelperMock.Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(It.IsAny<HttpContext>(), SessionKeys.AssessorAssessmentSelectionSsViewModelSessionKey)).Returns(sessionModel);

            var result = Assert.IsType<ViewResult>(await controller.AssessmentSelectionSsAsync(new AssessorAssessmentSelectionViewModel { ElementType = ElementTypeInShort.SS }));

            Assert.Equal("AssessmentSelection", result.ViewName);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public void AssessmentSelectionCc_Get_ReturnsAssessmentSelectionView()
        {
            var controller = CreateController();
            SetupAssessmentSelectionCommon();

            var result = Assert.IsType<ViewResult>(controller.AssessmentSelectionCc());

            Assert.Equal("AssessmentSelection", result.ViewName);
        }

        [Fact]
        public async Task AssessmentSelectionCcAsync_Post_WithMissingSelection_ReturnsView()
        {
            var controller = CreateController();
            SetupAssessmentSelectionCommon();
            var sessionModel = CreateAssessmentSessionModel(ElementTypeInShort.CC);
            _sessionHelperMock.Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(It.IsAny<HttpContext>(), SessionKeys.AssessorAssessmentSelectionCcViewModelSessionKey)).Returns(sessionModel);

            var result = Assert.IsType<ViewResult>(await controller.AssessmentSelectionCcAsync(new AssessorAssessmentSelectionViewModel { ElementType = ElementTypeInShort.CC }));

            Assert.Equal("AssessmentSelection", result.ViewName);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public void AssessmentSelectionCdn_Get_ReturnsAssessmentSelectionView()
        {
            var controller = CreateController();
            SetupAssessmentSelectionCommon();

            var result = Assert.IsType<ViewResult>(controller.AssessmentSelectionCdn());

            Assert.Equal("AssessmentSelection", result.ViewName);
        }

        [Fact]
        public async Task AssessmentSelectionCdnAsync_Post_WithMissingSelection_ReturnsView()
        {
            var controller = CreateController();
            SetupAssessmentSelectionCommon();
            var sessionModel = CreateAssessmentSessionModel(ElementTypeInShort.CDN);
            _sessionHelperMock.Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(It.IsAny<HttpContext>(), SessionKeys.AssessorAssessmentSelectionCdnViewModelSessionKey)).Returns(sessionModel);

            var result = Assert.IsType<ViewResult>(await controller.AssessmentSelectionCdnAsync(new AssessorAssessmentSelectionViewModel { ElementType = ElementTypeInShort.CDN }));

            Assert.Equal("AssessmentSelection", result.ViewName);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public void AssessmentSelectionDdn_Get_ReturnsAssessmentSelectionView()
        {
            var controller = CreateController();
            SetupAssessmentSelectionCommon();

            var result = Assert.IsType<ViewResult>(controller.AssessmentSelectionDdn());

            Assert.Equal("AssessmentSelection", result.ViewName);
        }

        [Fact]
        public async Task AssessmentSelectionDdnAsync_Post_WithMissingSelection_ReturnsView()
        {
            var controller = CreateController();
            SetupAssessmentSelectionCommon();
            var sessionModel = CreateAssessmentSessionModel(ElementTypeInShort.DDN);
            _sessionHelperMock.Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(It.IsAny<HttpContext>(), SessionKeys.AssessorAssessmentSelectionDdnViewModelSessionKey)).Returns(sessionModel);

            var result = Assert.IsType<ViewResult>(await controller.AssessmentSelectionDdnAsync(new AssessorAssessmentSelectionViewModel { ElementType = ElementTypeInShort.DDN }));

            Assert.Equal("AssessmentSelection", result.ViewName);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task AssessorElementSelectionOverviewAsync_Get_ReturnsViewAndSavesOverview()
        {
            var controller = CreateController();
            SetupDefaultSession();
            _sessionHelperMock.Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey)).Returns(new AssessorSelectElementsViewModel
            {
                SelectedElementIds = [ElementTypeInShort.EC]
            });
            _sessionHelperMock.Setup(x => x.GetFromSession<AssessorDetails>(It.IsAny<HttpContext>(), SessionKeys.AssessorDetailsSessionKey)).Returns(new AssessorDetails { FirstName = "A", LastName = "B", Email = "a@b.com" });
            _sessionHelperMock.Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(It.IsAny<HttpContext>(), SessionKeys.AssessorAssessmentSelectionEcViewModelSessionKey))
                .Returns(new AssessorAssessmentSelectionViewModel { ElementType = ElementTypeInShort.EC, SelectedAssessmentOption = "Assessment 1" });
            _heatNetworkServiceMock.Setup(x => x.GetAsync("HN-1")).ReturnsAsync(new HeatNetworkResponse { Phase = "Design" });

            var result = Assert.IsType<ViewResult>(await controller.AssessorElementSelectionOverviewAsync());

            Assert.IsType<AssessorElementSelectionOverviewModel>(result.Model);
            _sessionHelperMock.Verify(x => x.SaveToSession(It.IsAny<HttpContext>(), SessionKeys.AssessorElementSelectionOverviewModelSessionKey, It.IsAny<AssessorElementSelectionOverviewModel>()), Times.Once);
        }

        [Fact]
        public async Task AssessorElementSelectionOverviewConfirm_Post_AssignsAndRedirects()
        {
            var controller = CreateController();
            SetupDefaultSession();
            _sessionHelperMock.Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey)).Returns(new AssessorSelectElementsViewModel
            {
                SelectedElementIds = [ElementTypeInShort.EC]
            });
            _sessionHelperMock.Setup(x => x.GetFromSession<AssessorDetails>(It.IsAny<HttpContext>(), SessionKeys.AssessorDetailsSessionKey)).Returns(new AssessorDetails { FirstName = "A", LastName = "B", Email = "a@b.com" });
            _sessionHelperMock.Setup(x => x.GetFromSession<Milestone?>(It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding)).Returns(Milestone.Milestone2);
            _sessionHelperMock.Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(It.IsAny<HttpContext>(), SessionKeys.AssessorAssessmentSelectionEcViewModelSessionKey))
                .Returns(new AssessorAssessmentSelectionViewModel { ElementType = ElementTypeInShort.EC, SelectedAssessmentOption = "Assessment 1" });
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey)).Returns("user-1");

            var result = Assert.IsType<RedirectToActionResult>(await controller.AssessorElementSelectionOverviewConfirm(new AssessorElementSelectionOverviewModel()));

            Assert.Equal("AssessorAssignedConfirmation", result.ActionName);
            _soaServiceMock.Verify(x => x.AssignAssessorForExistingNetwork(It.IsAny<ElementSoaAssignAssessorRequestForExistingNetwork>()), Times.Once);
        }

        [Fact]
        public void AssessorAssignedConfirmation_Get_ReturnsViewWithSessionModel()
        {
            var controller = CreateController();
            var sessionModel = new AssessorElementSelectionOverviewModel
            {
                AssessorDetails = new AssessorDetails { FirstName = "A", LastName = "B", Email = "a@b.com" },
                AssessorSelectedElements = new AssessorSelectElementsViewModel(),
                AssessorAssessments = new List<AssessorAssessmentSelectionViewModel>(),
                HeatNetworkPhase = "Design",
                HeatNetworkStage = "Stage 1"
            };
            _sessionHelperMock.Setup(x => x.GetFromSession<AssessorElementSelectionOverviewModel>(It.IsAny<HttpContext>(), SessionKeys.AssessorElementSelectionOverviewModelSessionKey)).Returns(sessionModel);

            var result = Assert.IsType<ViewResult>(controller.AssessorAssignedConfirmation());

            Assert.Same(sessionModel, result.Model);
        }

        [Fact]
        public void AssessorAssignedConfirmationOk_Post_ClearsSessionAndRedirects()
        {
            var controller = CreateController();
            _sessionHelperMock.Setup(x => x.GetFromSession<int?>(It.IsAny<HttpContext>(), SessionKeys.CurrentStageIndexSessionKey)).Returns(1);

            var result = Assert.IsType<RedirectToActionResult>(controller.AssessorAssignedConfirmationOk());

            Assert.Equal("SoaMilestones", result.ActionName);
            Assert.Equal("ExistingElementSoa", result.ControllerName);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.AssessorDetailsSessionKey), Times.Once);
            _sessionHelperMock.Verify(x => x.ClearFromSession(It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey), Times.Once);
        }
    }
}
