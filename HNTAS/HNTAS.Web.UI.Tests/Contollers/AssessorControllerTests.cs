using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Soa;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;

namespace HNTAS.Web.UI.Tests.Contollers
{
    public class AssessorControllerTests
    {

        private readonly Mock<ILogger<AssessorController>> _mockLogger;
        private readonly Mock<ISessionHelper> _mockSessionHelper;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IHeatNetworkService> _mockHeatNetworkService;
        private readonly Mock<IS3UploadService> _mockS3UploadService;
        private readonly Mock<ISoaService> _mockSoaService;
        private readonly Mock<IInvitationService> _mockInvitationService;
        private readonly Mock<IInvitationTokenService> _mockInvitationTokenService;
        private readonly Mock<CertifierEmailGeneratorService> _mockCertifierEmailGeneratorService;

        private readonly AssessorController _controller;

        public AssessorControllerTests()
        {

            _mockLogger = new Mock<ILogger<AssessorController>>();
            _mockSessionHelper = new Mock<ISessionHelper>();
            _mockUserService = new Mock<IUserService>();
            _mockHeatNetworkService = new Mock<IHeatNetworkService>();
            _mockS3UploadService = new Mock<IS3UploadService>();
            _mockSoaService = new Mock<ISoaService>();
            _mockInvitationService = new Mock<IInvitationService>();
            _mockInvitationTokenService = new Mock<IInvitationTokenService>();
            _mockCertifierEmailGeneratorService = new Mock<CertifierEmailGeneratorService>();
            _controller = CreateController();
        }

        private AssessorController CreateController()
        {
            var controller = new AssessorController(
                _mockLogger.Object,
                _mockSessionHelper.Object,
                _mockUserService.Object,
                _mockHeatNetworkService.Object,
                _mockSoaService.Object,
                _mockS3UploadService.Object,
                _mockInvitationService.Object,
                _mockInvitationTokenService.Object,
                _mockCertifierEmailGeneratorService.Object
            );
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new MockHttpSession();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
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

        private UserDetailsResponse GetUserDetailsResponse(string userId)
        {
            var userDetails = new HNTAS.Api.Client.Model.UserDetailsResponse
            {
                Id = userId,
                OneLoginId = "one-login-id",
                FirstName = "Test",
                LastName = "User",
                FullName = "Test User",
                EmailId = "test@email.com",
                JobTitle = "Assessor",
                MobileNumber = "1234567890",
                Status = UserStatus.Active,
                Roles = new List<UserRole> { UserRole.Assessor },
                Organisation = new OrganisationResponse
                {
                    OrgId = "org-id",
                    Name = "Test Organisation",
                    CompaniesHouseNumber = "12345678",
                    Type = OrganisationType.UkCompaniesHouse,
                    RegisteredAddress = new RegisteredAddress("123 Test St", "TE1 1ST", "Test Area", "Test Town", "Test County", "Test Country")
                },
                HeatNetworks = new List<HeatNetworkUserResponse>()
                {
                    new HeatNetworkUserResponse
                    {
                        HnId = "hn-1",
                        Name = "Heat Network 1",
                        Location = "Location 1"
                    },
                    new HeatNetworkUserResponse
                    {
                        HnId = "hn-2",
                        Name = "Heat Network 2",
                        Location = "Location 2"
                    }
                }
            };

            return userDetails;
        }

        private HeatNetworkResponse MockGetHNDetails(string hnId)
        {
            return new HeatNetworkResponse
            {
                Id = "heat-network-id",
                HnId = hnId,
                Location = "Test Location",
                Name = "Test Network",
                Pathway = "1",
                Soa = new SoaResponse
                {
                    Status = SoaStatus.InProgress,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "user",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "user",
                    JourneyData = new JourneyDataResponse
                    {
                        NetworkType = new NetworkTypeResponse(),
                        ConnectionTypes = new List<string>(),
                        HeatNetworkElements = new List<HeatNetworkElementResponse>()
                        {
                            new HeatNetworkElementResponse(){
                                Name = HeatNetworkElementType.EnergyCentre.ToString(),
                                Count = 1,
                                Locations = new List<string>{ "Location 1" },
                                Documents = new List<UploadedDocumentResponse>{
                                    new UploadedDocumentResponse(){
                                        FileName = "energy_centre_doc.docx",
                                        S3Key = "energy_centre_123",
                                        Phase = "Phase1",
                                        Stage = "Stage1",
                                        UploadedAt = DateTime.UtcNow,
                                        UploadedBy = "user"
                                    }
                                }
                            },
                            new HeatNetworkElementResponse(){
                                Name = HeatNetworkElementType.DistributionNetwork.ToString(),
                                Count = 1,
                                Locations = new List<string>{ "Location 2" },
                                Documents = new List<UploadedDocumentResponse>{
                                    new UploadedDocumentResponse(){
                                        FileName = "distribution_network_doc.docx",
                                        S3Key = "distribution_network_123",
                                        Phase = "Phase1",
                                        Stage = "Stage1",
                                        UploadedAt = DateTime.UtcNow,
                                        UploadedBy = "user"
                                    }
                                }
                            },
                            new HeatNetworkElementResponse(){
                                Name = HeatNetworkElementType.ThermalSubStation.ToString(),
                                Count = 2,
                                Locations = new List<string>{ "Location 3", "Location 4" },
                                Documents = new List<UploadedDocumentResponse>{
                                    new UploadedDocumentResponse(){
                                        FileName = "thermal_sub_station_doc1.docx",
                                        S3Key = "thermal_sub_station_123",
                                        Phase = "1",
                                        Stage = "1",
                                        UploadedAt = DateTime.UtcNow,
                                        UploadedBy = "user"
                                    },
                                    new UploadedDocumentResponse(){
                                        FileName = "thermal_sub_station_doc2.docx",
                                        S3Key = "thermal_sub_station_456",
                                        Phase = "1",
                                        Stage = "1",
                                        UploadedAt = DateTime.UtcNow,
                                        UploadedBy = "user"
                                    }
                                }
                            }
                        },
                        AssessmentDocs = new List<UploadedAssessmentDocumentResponse> {
                            new UploadedAssessmentDocumentResponse(){
                                FileName = "testfile123.docx",
                                S3Key = "test123",
                                Phase = "Phase1",
                                Stage = "Stage1",
                                UploadedAt = DateTime.UtcNow,
                                UploadedBy = "user"
                            }, },
                        AssessorDocs = new List<UploadedAssessorDocumentResponse>() {
                            new UploadedAssessorDocumentResponse(){
                                FileName = "testfile456.docx",
                                S3Key = "test456",
                                Phase = "Phase1",
                                Stage = "Stage1",
                                UploadedAt = DateTime.UtcNow,
                                UploadedBy = "user"
                            }, },
                        CertifierDocs = new List<UploadedCertifierDocumentResponse>() {
                            new UploadedCertifierDocumentResponse(){
                                FileName = "testfile789.docx",
                                S3Key = "test789",
                                Phase = "Phase1",
                                Stage = "Stage1",
                                UploadedAt = DateTime.UtcNow,
                                UploadedBy = "user"
                            }, },
                    }
                } // assuming this is a valid populated object
            };
        }

        [Fact]
        public void UserDetails_ReturnsViewResult_WithUserDetails()
        {
            // Arrange
            var userId = "test-user-id";
            var userDetails = GetUserDetailsResponse(userId);
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey)).Returns(userId);
            _mockUserService.Setup(s => s.GetUserDetails(userId)).ReturnsAsync(userDetails);
            var controller = CreateController();
            controller.Url = SetUpBackLink("UserAccount", "Dashboard").Object;
            // Act
            var result = controller.UserDetails().Result;
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HNTAS.Api.Client.Model.UserDetailsResponse>(viewResult.Model);
            Assert.Equal(userId, model.Id);
        }

        [Fact]

        public async Task UserDetails_ThrowsException_WhenUserIsNull()
        {
            // Arrange
            var userId = "invalid-user-id";

            _mockSessionHelper.Setup(s => s.GetFromSession<string>(_controller.HttpContext, SessionKeys.UserModel_Id_SessionKey)).Returns(userId);

            _mockUserService.Setup(u => u.GetUserDetails(userId)).ReturnsAsync((UserDetailsResponse)null);
            var controller = CreateController();
            controller.Url = SetUpBackLink("UserAccount", "Dashboard").Object;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => controller.UserDetails());
            Assert.Equal("Unable to retrieve user information. Please try again later.", exception.Message);
        }

        [Fact]
        public void DeclarationOfImpartiality_ReturnsViewResult_WhenNotDeclared()
        {
            // Arrange
            var hnid = "HN123";
            var model = new DeclationOfImpartialityModel();

            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), "HasDeclaredImpartiality"))
                              .Returns("false");

            _mockSessionHelper.Setup(s => s.GetFromSession<DeclationOfImpartialityModel>(It.IsAny<HttpContext>(), SessionKeys.DeclarationOfImpartialityModelKey))
                              .Returns(model);

            var controller = CreateController();
            controller.Url = SetUpBackLink("HeatNetworks", "UserManagement").Object;

            // Act
            var result = controller.DeclarationOfImpartiality(hnid);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<DeclationOfImpartialityModel>(viewResult.Model);
            Assert.Equal(hnid, returnedModel.HnId);
        }

        [Fact]
        public void DeclarationOfImpartiality_Redirects_WhenAlreadyDeclared()
        {
            // Arrange
            var hnid = "HN123";

            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), "HasDeclaredImpartiality"))
                              .Returns("true");

            var controller = CreateController();

            // Act
            var result = controller.DeclarationOfImpartiality(hnid);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkDetails", redirectResult.ActionName);
            Assert.Equal("Assessor", redirectResult.ControllerName);
            Assert.Equal(hnid, redirectResult.RouteValues["hnId"]);
        }

        [Fact]
        public async Task HeatNetworkDetails_ReturnsViewResult_WhenDataIsValid()
        {
            // Arrange
            var hnid = "HN123";
            var userId = "user-001";
            var userDetails = GetUserDetailsResponse(userId);
            var hnDetails = MockGetHNDetails(hnid);

            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey)).Returns(userId);

            _mockUserService.Setup(s => s.GetUserDetails(userId)).ReturnsAsync(userDetails);

            _mockHeatNetworkService.Setup(h => h.GetAsync(hnid.ToUpper())).ReturnsAsync(hnDetails);

            var controller = CreateController();
            controller.Url = SetUpBackLink("HeatNetworks", "UserManagement").Object;

            // Act
            var result = await controller.HeatNetworkDetails(hnid);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HeatNetworkDetailsViewModel>(viewResult.Model);
            Assert.Equal(hnid.ToUpper(), model.HnId);
            Assert.Equal("Test Network", model.HnName);
        }

        [Fact]
        public async Task HeatNetworkDetails_ReturnsBadRequest_WhenUserOrHnDetailsAreInvalid()
        {
            // Arrange
            var hnid = "HN123";
            var userId = "user-001";
            var hnDetails = new HeatNetworkResponse
            {
                Id = "heat-network-id",
                HnId = hnid,
                Location = "Test Location",
                Name = "Test Network",
                Pathway = "1",
                Soa = null // Simulate missing Soa
            };
            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey)).Returns(userId);
            _mockUserService.Setup(s => s.GetUserDetails(userId)).ReturnsAsync((UserDetailsResponse)null); // Simulate missing user
            _mockHeatNetworkService.Setup(h => h.GetAsync(hnid.ToUpper())).ReturnsAsync(hnDetails); // Simulate missing hnDetails

            var controller = CreateController();
            controller.Url = SetUpBackLink("HeatNetworks", "UserManagement").Object;

            // Act
            var result = await controller.HeatNetworkDetails(hnid);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DownloadTheDocuments_ReturnsViewResult_WhenDataIsValid()
        {
            // Arrange
            var hnId = "HN123";
            var phase = 1;

            var heatNetworkResponse = MockGetHNDetails(hnId);

            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId))
                              .Returns(hnId);

            _mockHeatNetworkService.Setup(h => h.GetAsync(hnId))
                                   .ReturnsAsync(heatNetworkResponse);

            var controller = CreateController();
            controller.Url = new Mock<IUrlHelper>().Object;

            // Act
            var result = await controller.DownloadTheDocuments(phase);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SOAReviewSummaryViewModel>(viewResult.Model);
            Assert.Equal(phase.ToString(), model.Phase);
            Assert.NotEmpty(model.Elements);
            Assert.NotEmpty(model.ElementDocuments);
            Assert.NotNull(model.AssessmentPlanDocument);
        }

        // This test case is commented out because the expected behavior when Soa is null is not defined.
        //[Fact]
        //public async Task DownloadTheDocuments_ReturnsViewResult_WithEmptyModel_WhenHeatNetworkIsNull()
        //{
        //    // Arrange
        //    var hnId = "HN123";
        //    var phase = 1;
        //    var hnDetails = new HeatNetworkResponse
        //    {
        //        Id = "heat-network-id",
        //        HnId = hnId,
        //        Location = "Test Location",
        //        Name = "Test Network",
        //        Pathway = "1",
        //        Soa = new SoaResponse()
        //        {
        //            JourneyData = null
        //        } // Simulate missing Soa
        //    };

        //    _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId))
        //                      .Returns(hnId);

        //    _mockHeatNetworkService.Setup(h => h.GetAsync(hnId))
        //                           .ReturnsAsync(hnDetails); // Simulate missing data

        //    var controller = CreateController();
        //    controller.Url = new Mock<IUrlHelper>().Object;

        //    // Act
        //    var result = await controller.DownloadTheDocuments(phase);

        //    // Assert
        //    // what to assert here?
        //}

        [Fact]
        public async Task SubmitDownloadTheDocuments_RedirectsToUploadSOC_WhenPhaseIsValid()
        {
            // Arrange
            var phase = 1;

            var controller = CreateController();
            controller.Url = new Mock<IUrlHelper>().Object;

            // Act
            var result = await controller.SubmitDownloadTheDocuments(phase);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UploadSOC", redirectResult.ActionName);
            Assert.Equal("Assessor", redirectResult.ControllerName);

            Assert.True(redirectResult.RouteValues.ContainsKey("PhaseNumber"));
            Assert.Equal(phase, redirectResult.RouteValues["PhaseNumber"]);
        }

        // even with an invalid phase, it should redirect to UploadSOC - need to introduce validation and modify this testcase as appropriate
        [Fact]
        public async Task SubmitDownloadTheDocuments_RedirectsToUploadSOC_EvenWithInvalidPhase()
        {
            // Arrange
            var invalidPhase = -1;

            var controller = CreateController();
            controller.Url = new Mock<IUrlHelper>().Object;

            // Act
            var result = await controller.SubmitDownloadTheDocuments(invalidPhase);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UploadSOC", redirectResult.ActionName);
            Assert.Equal("Assessor", redirectResult.ControllerName);

            Assert.True(redirectResult.RouteValues.ContainsKey("PhaseNumber"));
            Assert.Equal(invalidPhase, redirectResult.RouteValues["PhaseNumber"]); // Still passed through
        }

        [Fact]
        public void UploadSOC_ReturnsViewResult_WithCorrectModel_WhenPhaseIsValid()
        {
            // Arrange
            var phase = 1;
            var controller = CreateController();
            controller.Url = new Mock<IUrlHelper>().Object;

            // Act
            var result = controller.UploadSOC(phase);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UploadSOCViewModel>(viewResult.Model);
            Assert.Equal(phase, model.PhaseNumber);
        }

        [Fact]
        public void UploadSOC_ReturnsViewResult_WithModel_WhenPhaseIsNegative()
        {
            // Arrange
            var invalidPhase = -1;
            var controller = CreateController();
            controller.Url = new Mock<IUrlHelper>().Object;

            // Act
            var result = controller.UploadSOC(invalidPhase);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UploadSOCViewModel>(viewResult.Model);
            Assert.Equal(invalidPhase, model.PhaseNumber);
        }

        [Fact]
        public async Task SaveUploadSOC_RedirectsToCheckYourAnswers_WhenFileIsValid()
        {
            // Arrange
            var phase = 1;
            var hnId = "HN123";
            var userId = "user-001";

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns("soc.pdf");

            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId))
                              .Returns(hnId);

            _mockSessionHelper.Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                              .Returns(userId);

            _mockS3UploadService.Setup(s => s.UploadFileAsync(mockFile.Object, It.IsAny<string>()))
                                .ReturnsAsync("s3/key/path");

            _mockSoaService.Setup(s => s.UpdateDocument(It.IsAny<UpdateDocumentRequest>()))
                           .Returns(Task.CompletedTask);

            var controller = CreateController();
            controller.Url = new Mock<IUrlHelper>().Object;

            // Act
            var result = await controller.SaveUploadSOC(phase, mockFile.Object);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CheckYourAnswers", redirectResult.ActionName);
        }

        [Fact]
        public async Task SaveUploadSOC_ReturnsViewResult_WithModelError_WhenFileIsEmpty()
        {
            // Arrange
            var phase = 1;

            var emptyFileMock = new Mock<IFormFile>();
            emptyFileMock.Setup(f => f.Length).Returns(0); // Simulate empty file
            emptyFileMock.Setup(f => f.FileName).Returns("empty.pdf");

            var controller = CreateController();
            controller.Url = new Mock<IUrlHelper>().Object;

            // Act
            var result = await controller.SaveUploadSOC(phase, emptyFileMock.Object);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UploadAssessmentPlanViewModel>(viewResult.Model);
            Assert.Equal(phase + 1, model.PhaseNumber);
            Assert.True(controller.ModelState.ContainsKey("assessmentPlan"));
            var error = controller.ModelState["assessmentPlan"].Errors.FirstOrDefault();
            Assert.NotNull(error);
            Assert.Equal("Please select a file to upload.", error.ErrorMessage);
        }
    }
}