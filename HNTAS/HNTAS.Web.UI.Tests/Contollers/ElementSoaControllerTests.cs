using Amazon.Runtime.Internal.Util;
using Castle.Core.Configuration;
using DocumentFormat.OpenXml.EMMA;
using HNTAS.Api.Client.Api;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNTAS.Web.UI.Tests.Contollers
{
    public class ElementSoaControllerTests
    {
        private readonly Mock<ILogger<ElementSoaController>> _loggerMock;
        private readonly Mock<ISoaService> _soaServiceMock;
        private readonly Mock<IHeatNetworkService> _heatNetworksApiMock;
        private readonly Mock<IAssessorApi> _assessorApiMock;
        private readonly Mock<ISessionHelper> _sessionHelperMock;

        private readonly ElementSoaController _controller;

        public ElementSoaControllerTests()
        {
            _loggerMock = new Mock<ILogger<ElementSoaController>>();
            _soaServiceMock = new Mock<ISoaService>();
            _heatNetworksApiMock = new Mock<IHeatNetworkService>();
            _assessorApiMock = new Mock<IAssessorApi>();
            _sessionHelperMock = new Mock<ISessionHelper>();
            _controller = CreateController();
        }

        private ElementSoaController CreateController()
        {
            var controller = new ElementSoaController(
                _sessionHelperMock.Object, _soaServiceMock.Object, _loggerMock.Object, _heatNetworksApiMock.Object, _assessorApiMock.Object);

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
        public void UnderstandingSoa_Get_ReturnView()
        {
            var urlHelperMock = SetUpBackLink("NetworkDetails", "HeatNetwork");
            _controller.Url = urlHelperMock.Object;
            var result = _controller.UnderstandingSoa() as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void SubmitUnderstandingSoa_Post_ReturnsRedirectToAction()
        {
            // Act
            var result = _controller.SubmitUnderstandingSoa() as RedirectToActionResult;
            // Assert
            Assert.NotNull(result);
            Assert.Equal("SoaStages", result.ActionName);
        }

        [Fact]
        public async Task SoaStages_Get_ReturnView()
        {
            var urlHelperMock = SetUpBackLink("UnderstandingSoa", "ElementSoa");
            _controller.Url = urlHelperMock.Object;

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.HnId))
                .Returns("hn1");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.HnName))
                .Returns("network");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<int>(
                    It.IsAny<HttpContext>(), SessionKeys.CurrentStageIndexSessionKey))
                .Returns(1);

            _heatNetworksApiMock.Setup(n => n.GetAsync(It.IsAny<string>())).ReturnsAsync(
                new HeatNetworkResponse
                {
                    Name = "network",
                    Phase = "Design",
                    HeatNetworkType = NullableOfHeatNetworkType.Communal,
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

            var result = await _controller.SoaStages() as ViewResult;
            Assert.NotNull(result);
            Assert.Equal("SoaStages", result.ViewName);
        }

        [Theory]
        [InlineData(SoaStage.Stage1, ElementTypeInShort.DDN, HeatNetworkElementType.DistrictDistribution)]
        [InlineData(SoaStage.Stage2, ElementTypeInShort.CDN, HeatNetworkElementType.CommunalDistribution)]
        [InlineData(SoaStage.Stage3, ElementTypeInShort.SS, HeatNetworkElementType.Substation)]
        [InlineData(SoaStage.Stage4, ElementTypeInShort.EC, HeatNetworkElementType.EnergyCentre)]
        [InlineData(SoaStage.Stage5, ElementTypeInShort.CC, HeatNetworkElementType.ConsumerConnection)]
        [InlineData(SoaStage.Stage6, ElementTypeInShort.DDN, HeatNetworkElementType.DistrictDistribution)]
        public async Task SoaUpdateStatus_Get_ReturnView(SoaStage paramStage, ElementTypeInShort paramElementTypeInShort, HeatNetworkElementType paramElementType)
        {
            _sessionHelperMock
                .Setup(x => x.GetFromSession<int>(
                    It.IsAny<HttpContext>(), SessionKeys.CurrentStageIndexSessionKey))
                .Returns(1);

            var urlHelperMock = SetUpBackLink("SoaStages", "ElementSoa");
            _controller.Url = urlHelperMock.Object;

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.HnId))
                .Returns("hn1");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.HnName))
                .Returns("network");

            _heatNetworksApiMock.Setup(n => n.GetAsync(It.IsAny<string>())).ReturnsAsync(
                new HeatNetworkResponse
                {
                    Name = "network",
                    Phase = "Design",
                    HeatNetworkType = NullableOfHeatNetworkType.Communal,
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

            var result = await _controller.SoaUpdateStatus(paramStage, paramElementTypeInShort, paramElementType) as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SoaUpdateStatus_Post_RedirectToAction()
        {
            _sessionHelperMock
                .Setup(x => x.GetFromSession<int>(
                    It.IsAny<HttpContext>(), SessionKeys.CurrentStageIndexSessionKey))
                .Returns(1);

            var urlHelperMock = SetUpBackLink("SoaStages", "ElementSoa");
            _controller.Url = urlHelperMock.Object;

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.HnId))
                .Returns("hn1");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.HnName))
                .Returns("network");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns("UserModel_Id_SessionKey");

            var model = new ElementSoaUpdateStatusViewModel
            {
                ElementCount = 1,
                ElementDisplayType = HeatNetworkElementType.DistrictDistribution,
                SelectedSoaStatusOptions = new List<SoaStatus> { SoaStatus.InProgress },
                SoaStatusCounts = new Dictionary<SoaStatus, int?> { { SoaStatus.InProgress, 1 } },
            };

            _soaServiceMock.Setup(s => s.UpdateElementSoaStatus(It.IsAny<ElementSoaStatusUpdateRequest>())).
                Returns(Task.CompletedTask);

            var res = await _controller.SoaUpdateStatus(model);
            Assert.NotNull(res);
            var redirectResult = Assert.IsType<RedirectToActionResult>(res);
            Assert.Equal("SoaStages", redirectResult.ActionName);
        }

        [Fact]
        public void PrepareAssessorOnboarding_Get_RedirectToAction()
        {
            var result = _controller.PrepareAssessorOnboarding(SoaStage.Stage1, "Design", ElementTypeInShort.CC) as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal("AssessorOnboarding", result.ActionName);
        }

        [Fact]
        public void AssessorOnboarding_Get_ReturnView()
        {
            _sessionHelperMock
                .Setup(x => x.GetFromSession<SoaStage>(
                    It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                .Returns(SoaStage.Stage1);

            var urlHelperMock = SetUpBackLink("SoaStages", "ElementSoa");
            _controller.Url = urlHelperMock.Object;

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.SoaStageTitleOfAssessorOnboarding))
                .Returns("hn1");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.DefaultSelectedAssessor))
                .Returns(new AssessorDetails { Assessment = "test", Email = "test", FirstName = "firstName", FullNameWithEmail = null!, LastName = "lastname" });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.AssessorDetailsSessionKey))
                .Returns(new AssessorDetails { Assessment = "test", Email = "test", FirstName = "firstName", FullNameWithEmail = "test", LastName = "lastname" });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<int>(
                    It.IsAny<HttpContext>(), SessionKeys.CurrentStageIndexSessionKey))
                .Returns(1);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.HnId))
                .Returns("hn1");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.HnName))
                .Returns("network");
            var result = _controller.AssessorOnboarding() as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchAssessor_ReturnJson()
        {
            // Arrange
            var apiResponseMock = new Mock<IApiAssessorSearchGetApiResponse>();
            apiResponseMock.SetupGet(x => x.IsOk).Returns(true);
            apiResponseMock.SetupGet(x => x.RawContent).Returns("[]");

            _assessorApiMock.Setup(a => a.ApiAssessorSearchGetAsync(It.IsAny<Api.Client.Client.Option<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(apiResponseMock.Object);

            // Act
            var result = await _controller.SearchAssessor("test") as JsonResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchAssessor_NoInput_ReturnJson()
        {
            // Act
            var result = await _controller.SearchAssessor("") as JsonResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchAssessor_ThrowException()
        {
            // Arrange
            var apiResponseMock = new Mock<IApiAssessorSearchGetApiResponse>();
            apiResponseMock.SetupGet(x => x.IsOk).Returns(true);
            apiResponseMock.SetupGet(x => x.RawContent).Returns("[]");

            _assessorApiMock.Setup(a => a.ApiAssessorSearchGetAsync(It.IsAny<Api.Client.Client.Option<string>>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            // Act
            var result = await _controller.SearchAssessor("test") as JsonResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void SelectedAssessorOnboarding_Post_AssessorFromInputDoesNotExistFromTheAssessorList_ModelError()
        {
            var firstName = "First";
            var lastName = "Last";
            var email = "email";
            var fullNameWithEmail = $"{firstName} {lastName} ({email})";

            var urlHelperMock = SetUpBackLink("SoaStages", "ElementSoa");
            _controller.Url = urlHelperMock.Object;

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.SoaStageTitleOfAssessorOnboarding))
                .Returns("SoaStageTitleOfAssessorOnboarding");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.DefaultSelectedAssessor))
                .Returns(new AssessorDetails { Assessment = "test", Email = "test", FirstName = "firstName", FullNameWithEmail = null!, LastName = "lastname" });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<int>(
                    It.IsAny<HttpContext>(), SessionKeys.CurrentStageIndexSessionKey))
                .Returns(1);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<SoaStage>(
                    It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                .Returns(SoaStage.Stage1);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<List<AssessorSearchResult>>(
                    It.IsAny<HttpContext>(), SessionKeys.AssessorSearchResultsSessionKey))
                .Returns(new List<AssessorSearchResult> { new AssessorSearchResult { Email = "test", FirstName = "firstName", FullNameWithEmail = null!, LastName = "lastname" } });

            var result = _controller.SelectedAssessorOnboarding(firstName, lastName, email, fullNameWithEmail) as ViewResult;
            Assert.NotNull(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public void SelectedAssessorOnboarding_Post_AssessorFromInputDoesExistFromTheAssessorList_ReturnToAction()
        {
            var firstName = "First";
            var lastName = "Last";
            var email = "email";
            var fullNameWithEmail = $"{firstName} {lastName} ({email})";

            var urlHelperMock = SetUpBackLink("SoaStages", "ElementSoa");
            _controller.Url = urlHelperMock.Object;

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.SoaStageTitleOfAssessorOnboarding))
                .Returns("SoaStageTitleOfAssessorOnboarding");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.DefaultSelectedAssessor))
                .Returns(new AssessorDetails { Assessment = "test", Email = "test", FirstName = "firstName", FullNameWithEmail = null!, LastName = "lastname" });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<int>(
                    It.IsAny<HttpContext>(), SessionKeys.CurrentStageIndexSessionKey))
                .Returns(1);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<SoaStage>(
                    It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                .Returns(SoaStage.Stage1);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<List<AssessorSearchResult>>(
                    It.IsAny<HttpContext>(), SessionKeys.AssessorSearchResultsSessionKey))
                .Returns(new List<AssessorSearchResult> { new AssessorSearchResult { Email = "test", FirstName = "firstName", FullNameWithEmail = fullNameWithEmail, LastName = "lastname" } });

            var result = _controller.SelectedAssessorOnboarding(firstName, lastName, email, fullNameWithEmail) as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal("AssessorSelectElements", result.ActionName);
        }

        [Fact]
        public void SelectedAssessorOnboarding_Post_EmptyAssessorFromInput_ModelError()
        {
            var firstName = "First";
            var lastName = "Last";
            var email = "email";
            string fullNameWithEmail = null!;

            var urlHelperMock = SetUpBackLink("SoaStages", "ElementSoa");
            _controller.Url = urlHelperMock.Object;

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.SoaStageTitleOfAssessorOnboarding))
                .Returns("SoaStageTitleOfAssessorOnboarding");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.DefaultSelectedAssessor))
                .Returns(new AssessorDetails { Assessment = "test", Email = "test", FirstName = "firstName", FullNameWithEmail = null!, LastName = "lastname" });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<int>(
                    It.IsAny<HttpContext>(), SessionKeys.CurrentStageIndexSessionKey))
                .Returns(1);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<SoaStage>(
                    It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                .Returns(SoaStage.Stage1);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<List<AssessorSearchResult>>(
                    It.IsAny<HttpContext>(), SessionKeys.AssessorSearchResultsSessionKey))
                .Returns(new List<AssessorSearchResult> { new AssessorSearchResult { Email = "test", FirstName = "firstName", FullNameWithEmail = null!, LastName = "lastname" } });

            var result = _controller.SelectedAssessorOnboarding(firstName, lastName, email, fullNameWithEmail) as ViewResult;
            Assert.NotNull(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task AssessorSelectElementsAsync_Get_ReturnView()
        {
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(), SessionKeys.SoaStageTitleOfAssessorOnboarding))
                .Returns("SoaStageTitleOfAssessorOnboarding");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.AssessorDetailsSessionKey))
                .Returns(new AssessorDetails { Assessment = "test", Email = "test", FirstName = "firstName", FullNameWithEmail = null!, LastName = "lastname" });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<ElementTypeInShort>(
                    It.IsAny<HttpContext>(), SessionKeys.SoaElementTypeOfAssessorOnboarding))
                .Returns(ElementTypeInShort.EC);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<SoaStage>(
                    It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                .Returns(SoaStage.Stage1);

            _controller.Url = SetUpBackLink("AssessorOnboarding", "ElementSoa").Object;

            _heatNetworksApiMock.Setup(n => n.GetAsync(It.IsAny<string>())).ReturnsAsync(
                new HeatNetworkResponse
                {
                    Name = "network",
                    Phase = "Design",
                    HeatNetworkType = NullableOfHeatNetworkType.Communal,
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

            var result = await _controller.AssessorSelectElementsAsync() as ViewResult;
            Assert.NotNull(result);

        }

        [Theory]
        [InlineData(ElementTypeInShort.DDN, "AssessmentSelectionDdn")]
        [InlineData(ElementTypeInShort.CC, "AssessmentSelectionCc")]
        [InlineData(ElementTypeInShort.CDN, "AssessmentSelectionCdn")]
        [InlineData(ElementTypeInShort.SS, "AssessmentSelectionSs")]
        [InlineData(ElementTypeInShort.EC, "AssessmentSelectionEc")]
        public void AssessorSelectElements_Post_RedirectToAction(ElementTypeInShort elementTypeInShort, string actionName)
        {
            var model = new AssessorSelectElementsViewModel
            {
                ElementOptions = new List<AssessorSelectElementsOption>
                {
                    new AssessorSelectElementsOption
                    {
                        ElementType = elementTypeInShort,
                    }
                },
                SelectedElementIds = new List<ElementTypeInShort> { elementTypeInShort },
                SelectedElementLabel = new List<string> { "District Distribution Network" }
            };
            _controller.Url = SetUpBackLink("AssessorOnboarding", "ElementSoa").Object;
            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.AssessorDetailsSessionKey))
                .Returns(new AssessorDetails { Assessment = "test", Email = "test", FirstName = "firstName", FullNameWithEmail = null!, LastName = "lastname" });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<ElementTypeInShort>(
                    It.IsAny<HttpContext>(), SessionKeys.SoaElementTypeOfAssessorOnboarding))
                .Returns(elementTypeInShort);

            _sessionHelperMock
               .Setup(x => x.GetFromSession<SoaStage>(
                   It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
               .Returns(SoaStage.Stage1);

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(model);

            var result = _controller.AssessorSelectElements(model) as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal(actionName, result.ActionName);
        }

        [Fact]
        public void AssessmentSelectionEcAsync_Get_ReturnView()
        {
            var model = new AssessorSelectElementsViewModel
            {
                ElementOptions = new List<AssessorSelectElementsOption>
                {
                    new AssessorSelectElementsOption
                    {
                        ElementType = ElementTypeInShort.EC,
                    }
                },
                SelectedElementIds = new List<ElementTypeInShort> { ElementTypeInShort.EC },
                SelectedElementLabel = new List<string> { "District Distribution Network" }
            };

            _controller.Url = SetUpBackLink("AssessmentSelectionEc", "ElementSoa").Object;

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(model);

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                   It.IsAny<HttpContext>(), It.IsAny<string>()))
               .Returns(new AssessorAssessmentSelectionViewModel { ElementType = ElementTypeInShort.EC });

            var result = _controller.AssessmentSelectionEcAsync() as ViewResult;
            Assert.NotNull(result);
            Assert.Equal("AssessmentSelection", result.ViewName);
        }

        [Fact]
        public async Task AssessmentSelectionEcAsync_Post_RedirectToAction()
        {
            var model = new AssessorAssessmentSelectionViewModel
            {
                ElementType = ElementTypeInShort.EC,
                SelectedAssessmentOption = "Assessment 1",
            };

            _controller.Url = SetUpBackLink("AssessmentSelectionEc", "ElementSoa").Object;

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(new AssessorSelectElementsViewModel
               {
                   ElementOptions = new List<AssessorSelectElementsOption>
                {
                    new AssessorSelectElementsOption
                    {
                        ElementType = ElementTypeInShort.EC,
                    }
                },
                   SelectedElementIds = new List<ElementTypeInShort> { ElementTypeInShort.EC },
                   SelectedElementLabel = new List<string> { "District Distribution Network" }
               });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.AssessorDetailsSessionKey))
                .Returns(new AssessorDetails { Assessment = "test", Email = "test", FirstName = "firstName", FullNameWithEmail = "test", LastName = "lastname" });

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<SoaStage>(
                        It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                    .Returns(SoaStage.Stage1);

            MockNetworkData(HeatNetworkElementType.EnergyCentre, ElementTypeInShort.EC);

            var result = await _controller.AssessmentSelectionEcAsync(model) as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal("AssessorElementSelectionOverview", result.ActionName);

        }

        [Theory]
        [InlineData("test", "Assessment 1")]
        [InlineData("test2", "Assessment 1")]
        public async Task AssessmentSelectionEcAsync_Post_ValidationFailed(string assessorEmail, string selectedAssessmentOption)
        {
            var model = new AssessorAssessmentSelectionViewModel
            {
                ElementType = ElementTypeInShort.EC,
                SelectedAssessmentOption = selectedAssessmentOption,
            };

            _controller.Url = SetUpBackLink("AssessmentSelectionEc", "ElementSoa").Object;

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(new AssessorSelectElementsViewModel
               {
                   ElementOptions = new List<AssessorSelectElementsOption>
                {
                    new AssessorSelectElementsOption
                    {
                        ElementType = ElementTypeInShort.EC,
                    }
                },
                   SelectedElementIds = new List<ElementTypeInShort> { ElementTypeInShort.EC },
                   SelectedElementLabel = new List<string> { "District Distribution Network" }
               });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.AssessorDetailsSessionKey))
                .Returns(new AssessorDetails { Assessment = "test", Email = assessorEmail, FirstName = "firstName", FullNameWithEmail = "test", LastName = "lastname" });

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<SoaStage?>(
                        It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                    .Returns(SoaStage.Stage1);

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                        It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                    .Returns(model);

            MockNetworkData(HeatNetworkElementType.EnergyCentre, ElementTypeInShort.EC);

            var result = await _controller.AssessmentSelectionEcAsync(model) as RedirectToActionResult;
            Assert.False(_controller.ModelState.IsValid);

        }

        [Fact]
        public void AssessmentSelectionSsAsync_Get_ReturnView()
        {
            var model = new AssessorSelectElementsViewModel
            {
                ElementOptions = new List<AssessorSelectElementsOption>
                {
                    new AssessorSelectElementsOption
                    {
                        ElementType = ElementTypeInShort.SS,
                    }
                },
                SelectedElementIds = new List<ElementTypeInShort> { ElementTypeInShort.SS },
                SelectedElementLabel = new List<string> { "District Distribution Network" }
            };

            _controller.Url = SetUpBackLink("AssessmentSelectionEc", "ElementSoa").Object;

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(model);

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                   It.IsAny<HttpContext>(), It.IsAny<string>()))
               .Returns(new AssessorAssessmentSelectionViewModel { ElementType = ElementTypeInShort.SS });

            var result = _controller.AssessmentSelectionSs() as ViewResult;
            Assert.NotNull(result);
            Assert.Equal("AssessmentSelection", result.ViewName);
        }

        [Fact]
        public async Task AssessmentSelectionSsAsync_Post_RedirectToAction()
        {
            var model = new AssessorAssessmentSelectionViewModel
            {
                ElementType = ElementTypeInShort.SS,
                SelectedAssessmentOption = "Assessment 1",
            };

            _controller.Url = SetUpBackLink("AssessmentSelectionEc", "ElementSoa").Object;

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(new AssessorSelectElementsViewModel
               {
                   ElementOptions = new List<AssessorSelectElementsOption>
                {
                    new AssessorSelectElementsOption
                    {
                        ElementType = ElementTypeInShort.SS,
                    }
                },
                   SelectedElementIds = new List<ElementTypeInShort> { ElementTypeInShort.SS },
                   SelectedElementLabel = new List<string> { "District Distribution Network" }
               });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.AssessorDetailsSessionKey))
                .Returns(new AssessorDetails { Assessment = "test", Email = "test", FirstName = "firstName", FullNameWithEmail = "test", LastName = "lastname" });

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<SoaStage>(
                        It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                    .Returns(SoaStage.Stage1);

            MockNetworkData(HeatNetworkElementType.Substation, ElementTypeInShort.SS);

            var result = await _controller.AssessmentSelectionSsAsync(model) as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal("AssessorElementSelectionOverview", result.ActionName);

        }

        [Theory]
        [InlineData("test", "Assessment 1")]
        [InlineData("test2", "Assessment 1")]
        public async Task AssessmentSelectionSsAsync_Post_ValidationFailed(string assessorEmail, string selectedAssessmentOption)
        {
            var model = new AssessorAssessmentSelectionViewModel
            {
                ElementType = ElementTypeInShort.SS,
                SelectedAssessmentOption = selectedAssessmentOption,
            };

            _controller.Url = SetUpBackLink("AssessmentSelectionEc", "ElementSoa").Object;

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(new AssessorSelectElementsViewModel
               {
                   ElementOptions = new List<AssessorSelectElementsOption>
                {
                    new AssessorSelectElementsOption
                    {
                        ElementType = ElementTypeInShort.SS,
                    }
                },
                   SelectedElementIds = new List<ElementTypeInShort> { ElementTypeInShort.SS },
                   SelectedElementLabel = new List<string> { "District Distribution Network" }
               });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.AssessorDetailsSessionKey))
                .Returns(new AssessorDetails { Assessment = "test", Email = assessorEmail, FirstName = "firstName", FullNameWithEmail = "test", LastName = "lastname" });

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<SoaStage?>(
                        It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                    .Returns(SoaStage.Stage1);

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                        It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                    .Returns(model);

            MockNetworkData(HeatNetworkElementType.Substation, ElementTypeInShort.SS);

            var result = await _controller.AssessmentSelectionSsAsync(model) as RedirectToActionResult;
            Assert.False(_controller.ModelState.IsValid);

        }

        [Fact]
        public void AssessmentSelectionDdnAsync_Get_ReturnView()
        {
            var model = new AssessorSelectElementsViewModel
            {
                ElementOptions = new List<AssessorSelectElementsOption>
                {
                    new AssessorSelectElementsOption
                    {
                        ElementType = ElementTypeInShort.DDN,
                    }
                },
                SelectedElementIds = new List<ElementTypeInShort> { ElementTypeInShort.DDN },
                SelectedElementLabel = new List<string> { "District Distribution Network" }
            };

            _controller.Url = SetUpBackLink("AssessmentSelectionEc", "ElementSoa").Object;

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(model);

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                   It.IsAny<HttpContext>(), It.IsAny<string>()))
               .Returns(new AssessorAssessmentSelectionViewModel { ElementType = ElementTypeInShort.DDN });

            var result = _controller.AssessmentSelectionDdn() as ViewResult;
            Assert.NotNull(result);
            Assert.Equal("AssessmentSelection", result.ViewName);
        }

        [Fact]
        public async Task AssessmentSelectionDdnAsync_Post_RedirectToAction()
        {
            var model = new AssessorAssessmentSelectionViewModel
            {
                ElementType = ElementTypeInShort.DDN,
                SelectedAssessmentOption = "Assessment 1",
            };

            _controller.Url = SetUpBackLink("AssessmentSelectionEc", "ElementSoa").Object;

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(new AssessorSelectElementsViewModel
               {
                   ElementOptions = new List<AssessorSelectElementsOption>
                {
                    new AssessorSelectElementsOption
                    {
                        ElementType = ElementTypeInShort.DDN,
                    }
                },
                   SelectedElementIds = new List<ElementTypeInShort> { ElementTypeInShort.DDN },
                   SelectedElementLabel = new List<string> { "District Distribution Network" }
               });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.AssessorDetailsSessionKey))
                .Returns(new AssessorDetails { Assessment = "test", Email = "test", FirstName = "firstName", FullNameWithEmail = "test", LastName = "lastname" });

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<SoaStage>(
                        It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                    .Returns(SoaStage.Stage1);

            MockNetworkData(HeatNetworkElementType.DistrictDistribution, ElementTypeInShort.DDN);

            var result = await _controller.AssessmentSelectionDdnAsync(model) as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal("AssessorElementSelectionOverview", result.ActionName);

        }

        [Theory]
        [InlineData("test", "Assessment 1")]
        [InlineData("test2", "Assessment 1")]
        public async Task AssessmentSelectionDdnAsync_Post_ValidationFailed(string assessorEmail, string selectedAssessmentOption)
        {
            var model = new AssessorAssessmentSelectionViewModel
            {
                ElementType = ElementTypeInShort.DDN,
                SelectedAssessmentOption = selectedAssessmentOption,
            };

            _controller.Url = SetUpBackLink("AssessmentSelectionEc", "ElementSoa").Object;

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(new AssessorSelectElementsViewModel
               {
                   ElementOptions = new List<AssessorSelectElementsOption>
                {
                    new AssessorSelectElementsOption
                    {
                        ElementType = ElementTypeInShort.DDN,
                    }
                },
                   SelectedElementIds = new List<ElementTypeInShort> { ElementTypeInShort.DDN },
                   SelectedElementLabel = new List<string> { "District Distribution Network" }
               });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.AssessorDetailsSessionKey))
                .Returns(new AssessorDetails { Assessment = "test", Email = assessorEmail, FirstName = "firstName", FullNameWithEmail = "test", LastName = "lastname" });

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<SoaStage?>(
                        It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                    .Returns(SoaStage.Stage1);

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                        It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                    .Returns(model);

            MockNetworkData(HeatNetworkElementType.DistrictDistribution, ElementTypeInShort.DDN);

            var result = await _controller.AssessmentSelectionDdnAsync(model) as RedirectToActionResult;
            Assert.False(_controller.ModelState.IsValid);

        }

        [Fact]
        public void AssessmentSelectionCdnAsync_Get_ReturnView()
        {
            var model = new AssessorSelectElementsViewModel
            {
                ElementOptions = new List<AssessorSelectElementsOption>
                {
                    new AssessorSelectElementsOption
                    {
                        ElementType = ElementTypeInShort.CDN,
                    }
                },
                SelectedElementIds = new List<ElementTypeInShort> { ElementTypeInShort.CDN },
                SelectedElementLabel = new List<string> { "District Distribution Network" }
            };

            _controller.Url = SetUpBackLink("AssessmentSelectionEc", "ElementSoa").Object;

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(model);

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                   It.IsAny<HttpContext>(), It.IsAny<string>()))
               .Returns(new AssessorAssessmentSelectionViewModel { ElementType = ElementTypeInShort.CDN });

            var result = _controller.AssessmentSelectionCdn() as ViewResult;
            Assert.NotNull(result);
            Assert.Equal("AssessmentSelection", result.ViewName);
        }

        [Fact]
        public async Task AssessmentSelectionCdnAsync_Post_RedirectToAction()
        {
            var model = new AssessorAssessmentSelectionViewModel
            {
                ElementType = ElementTypeInShort.CDN,
                SelectedAssessmentOption = "Assessment 1",
            };

            _controller.Url = SetUpBackLink("AssessmentSelectionEc", "ElementSoa").Object;

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(new AssessorSelectElementsViewModel
               {
                   ElementOptions = new List<AssessorSelectElementsOption>
                {
                    new AssessorSelectElementsOption
                    {
                        ElementType = ElementTypeInShort.CDN,
                    }
                },
                   SelectedElementIds = new List<ElementTypeInShort> { ElementTypeInShort.CDN },
                   SelectedElementLabel = new List<string> { "District Distribution Network" }
               });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.AssessorDetailsSessionKey))
                .Returns(new AssessorDetails { Assessment = "test", Email = "test", FirstName = "firstName", FullNameWithEmail = "test", LastName = "lastname" });

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<SoaStage>(
                        It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                    .Returns(SoaStage.Stage1);

            MockNetworkData(HeatNetworkElementType.CommunalDistribution, ElementTypeInShort.CDN);

            var result = await _controller.AssessmentSelectionCdnAsync(model) as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal("AssessorElementSelectionOverview", result.ActionName);

        }

        [Theory]
        [InlineData("test", "Assessment 1")]
        [InlineData("test2", "Assessment 1")]
        public async Task AssessmentSelectionCdnAsync_Post_ValidationFailed(string assessorEmail, string selectedAssessmentOption)
        {
            var model = new AssessorAssessmentSelectionViewModel
            {
                ElementType = ElementTypeInShort.CDN,
                SelectedAssessmentOption = selectedAssessmentOption,
            };

            _controller.Url = SetUpBackLink("AssessmentSelectionEc", "ElementSoa").Object;

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(new AssessorSelectElementsViewModel
               {
                   ElementOptions = new List<AssessorSelectElementsOption>
                {
                    new AssessorSelectElementsOption
                    {
                        ElementType = ElementTypeInShort.CDN,
                    }
                },
                   SelectedElementIds = new List<ElementTypeInShort> { ElementTypeInShort.CDN },
                   SelectedElementLabel = new List<string> { "District Distribution Network" }
               });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.AssessorDetailsSessionKey))
                .Returns(new AssessorDetails { Assessment = "test", Email = assessorEmail, FirstName = "firstName", FullNameWithEmail = "test", LastName = "lastname" });

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<SoaStage?>(
                        It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                    .Returns(SoaStage.Stage1);

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                        It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                    .Returns(model);

            MockNetworkData(HeatNetworkElementType.CommunalDistribution, ElementTypeInShort.CDN);

            var result = await _controller.AssessmentSelectionCdnAsync(model) as RedirectToActionResult;
            Assert.False(_controller.ModelState.IsValid);

        }

        [Fact]
        public void AssessmentSelectionCcAsync_Get_ReturnView()
        {
            var model = new AssessorSelectElementsViewModel
            {
                ElementOptions = new List<AssessorSelectElementsOption>
                {
                    new AssessorSelectElementsOption
                    {
                        ElementType = ElementTypeInShort.CC,
                    }
                },
                SelectedElementIds = new List<ElementTypeInShort> { ElementTypeInShort.CC },
                SelectedElementLabel = new List<string> { "District Distribution Network" }
            };

            _controller.Url = SetUpBackLink("AssessmentSelectionEc", "ElementSoa").Object;

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(model);

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                   It.IsAny<HttpContext>(), It.IsAny<string>()))
               .Returns(new AssessorAssessmentSelectionViewModel { ElementType = ElementTypeInShort.CC });

            var result = _controller.AssessmentSelectionCc() as ViewResult;
            Assert.NotNull(result);
            Assert.Equal("AssessmentSelection", result.ViewName);
        }

        [Fact]
        public async Task AssessmentSelectionCcAsync_Post_RedirectToAction()
        {
            var model = new AssessorAssessmentSelectionViewModel
            {
                ElementType = ElementTypeInShort.CC,
                SelectedAssessmentOption = "Assessment 1",
            };

            _controller.Url = SetUpBackLink("AssessmentSelectionEc", "ElementSoa").Object;

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(new AssessorSelectElementsViewModel
               {
                   ElementOptions = new List<AssessorSelectElementsOption>
                {
                    new AssessorSelectElementsOption
                    {
                        ElementType = ElementTypeInShort.CC,
                    }
                },
                   SelectedElementIds = new List<ElementTypeInShort> { ElementTypeInShort.CC },
                   SelectedElementLabel = new List<string> { "District Distribution Network" }
               });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.AssessorDetailsSessionKey))
                .Returns(new AssessorDetails { Assessment = "test", Email = "test", FirstName = "firstName", FullNameWithEmail = "test", LastName = "lastname" });

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<SoaStage>(
                        It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                    .Returns(SoaStage.Stage1);

            MockNetworkData(HeatNetworkElementType.ConsumerConnection, ElementTypeInShort.CC);

            var result = await _controller.AssessmentSelectionEcAsync(model) as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal("AssessorElementSelectionOverview", result.ActionName);

        }

        [Theory]
        [InlineData("test", "Assessment 1")]
        [InlineData("test2", "Assessment 1")]
        public async Task AssessmentSelectionCcAsync_Post_ValidationFailed(string assessorEmail, string selectedAssessmentOption)
        {
            var model = new AssessorAssessmentSelectionViewModel
            {
                ElementType = ElementTypeInShort.CC,
                SelectedAssessmentOption = selectedAssessmentOption,
            };

            _controller.Url = SetUpBackLink("AssessmentSelectionEc", "ElementSoa").Object;

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(new AssessorSelectElementsViewModel
               {
                   ElementOptions = new List<AssessorSelectElementsOption>
                {
                    new AssessorSelectElementsOption
                    {
                        ElementType = ElementTypeInShort.CC,
                    }
                },
                   SelectedElementIds = new List<ElementTypeInShort> { ElementTypeInShort.CC },
                   SelectedElementLabel = new List<string> { "District Distribution Network" }
               });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.AssessorDetailsSessionKey))
                .Returns(new AssessorDetails { Assessment = "test", Email = assessorEmail, FirstName = "firstName", FullNameWithEmail = "test", LastName = "lastname" });

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<SoaStage?>(
                        It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                    .Returns(SoaStage.Stage1);

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                        It.IsAny<HttpContext>(), SessionKeys.SoaStageOfAssessorOnboarding))
                    .Returns(model);

            MockNetworkData(HeatNetworkElementType.ConsumerConnection, ElementTypeInShort.CC);

            var result = await _controller.AssessmentSelectionCcAsync(model) as RedirectToActionResult;
            Assert.False(_controller.ModelState.IsValid);

        }

        [Fact]
        public async Task AssessorElementSelectionOverviewAsync_Get_ReturnView()
        {
            _controller.Url = SetUpBackLink("AssessmentSelectionEc", "ElementSoa").Object;

            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(new AssessorSelectElementsViewModel
               {
                   ElementOptions = new List<AssessorSelectElementsOption>
                    {
                        new AssessorSelectElementsOption
                        {
                            ElementType = ElementTypeInShort.CC,
                        }
                    },
                   SelectedElementIds = new List<ElementTypeInShort> { ElementTypeInShort.EC, ElementTypeInShort.SS, ElementTypeInShort.CDN, ElementTypeInShort.DDN, ElementTypeInShort.CC },
                   SelectedElementLabel = new List<string> { "Energy centre", "Substation", "cdn", "District Distribution Network", "cc" }
               });

            MockNetworkData(HeatNetworkElementType.ConsumerConnection, ElementTypeInShort.CC);

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                        It.IsAny<HttpContext>(), SessionKeys.AssessorAssessmentSelectionEcViewModelSessionKey))
                    .Returns(new AssessorAssessmentSelectionViewModel());

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                        It.IsAny<HttpContext>(), SessionKeys.AssessorAssessmentSelectionSsViewModelSessionKey))
                    .Returns(new AssessorAssessmentSelectionViewModel());

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                        It.IsAny<HttpContext>(), SessionKeys.AssessorAssessmentSelectionCcViewModelSessionKey))
                    .Returns(new AssessorAssessmentSelectionViewModel());

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                        It.IsAny<HttpContext>(), SessionKeys.AssessorAssessmentSelectionCdnViewModelSessionKey))
                    .Returns(new AssessorAssessmentSelectionViewModel());

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                        It.IsAny<HttpContext>(), SessionKeys.AssessorAssessmentSelectionDdnViewModelSessionKey))
                    .Returns(new AssessorAssessmentSelectionViewModel());

            var result = await _controller.AssessorElementSelectionOverviewAsync() as ViewResult;
            Assert.NotNull(result);            
        }

        [Fact]
        public async Task AssessorElementSelectionOverviewConfirm_Post_RedirectToAction()
        {
            _controller.Url = SetUpBackLink("AssessmentSelectionEc", "ElementSoa").Object;
            var model = new AssessorElementSelectionOverviewModel();
            _sessionHelperMock
               .Setup(x => x.GetFromSession<AssessorSelectElementsViewModel>(
                   It.IsAny<HttpContext>(), SessionKeys.AssessorSelectedElementSessionKey))
               .Returns(new AssessorSelectElementsViewModel
               {
                   ElementOptions = new List<AssessorSelectElementsOption>
                    {
                        new AssessorSelectElementsOption
                        {
                            ElementType = ElementTypeInShort.CC,
                        }
                    },
                   SelectedElementIds = new List<ElementTypeInShort> { ElementTypeInShort.EC, ElementTypeInShort.SS, ElementTypeInShort.CDN, ElementTypeInShort.DDN, ElementTypeInShort.CC },
                   SelectedElementLabel = new List<string> { "Energy centre", "Substation", "cdn", "District Distribution Network", "cc" }
               });            

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                        It.IsAny<HttpContext>(), SessionKeys.AssessorAssessmentSelectionEcViewModelSessionKey))
                    .Returns(new AssessorAssessmentSelectionViewModel());

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                        It.IsAny<HttpContext>(), SessionKeys.AssessorAssessmentSelectionSsViewModelSessionKey))
                    .Returns(new AssessorAssessmentSelectionViewModel());

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                        It.IsAny<HttpContext>(), SessionKeys.AssessorAssessmentSelectionCcViewModelSessionKey))
                    .Returns(new AssessorAssessmentSelectionViewModel());

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                        It.IsAny<HttpContext>(), SessionKeys.AssessorAssessmentSelectionCdnViewModelSessionKey))
                    .Returns(new AssessorAssessmentSelectionViewModel());

            _sessionHelperMock
                    .Setup(x => x.GetFromSession<AssessorAssessmentSelectionViewModel>(
                        It.IsAny<HttpContext>(), SessionKeys.AssessorAssessmentSelectionDdnViewModelSessionKey))
                    .Returns(new AssessorAssessmentSelectionViewModel());

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AssessorDetails>(
                    It.IsAny<HttpContext>(), SessionKeys.AssessorDetailsSessionKey))
                .Returns(new AssessorDetails { Assessment = "test", Email = "test", FirstName = "firstName", FullNameWithEmail = "test", LastName = "lastname" });

            _soaServiceMock.Setup(x => x.AssignAssessor(It.IsAny<ElementSoaAssignAssessorRequest>())).Returns(Task.CompletedTask);

            var result = await _controller.AssessorElementSelectionOverviewConfirm(model) as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal("AssessorAssignedConfirmation", result.ActionName);
        }

        [Fact]
        public void AssessorAssignedConfirmation_Get_ReturnView()
        {
            var result = _controller.AssessorAssignedConfirmation() as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void AssessorAssignedConfirmationOk_Post_RedirectToAction()
        {
            var result = _controller.AssessorAssignedConfirmationOk() as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal("SoaStages", result.ActionName);
        }

        private void MockNetworkData(HeatNetworkElementType displayType, ElementTypeInShort elementType)
        {
            _heatNetworksApiMock.Setup(n => n.GetAsync(It.IsAny<string>())).ReturnsAsync(
                new HeatNetworkResponse
                {
                    Name = "network",
                    Phase = "Design",
                    HeatNetworkType = NullableOfHeatNetworkType.Communal,
                    NetworkElements = new NetworkElementsResponse
                    {
                        ElementsGroup = new List<ElementGroup>
                        {
                            new ElementGroup
                            {
                                Count = 1,
                                ElementDisplayType = displayType,
                                ElementType = elementType,
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
        }
    }
}
