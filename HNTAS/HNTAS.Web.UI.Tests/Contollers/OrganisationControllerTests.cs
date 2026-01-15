using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Models.CompaniesHouse;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;

namespace HNTAS.Web.UI.Tests.Contollers
{
    public class OrganisationControllerTests
    {
        private readonly Mock<ICompaniesHouseService> _mockCompaniesHouseService;
        private readonly Mock<ILogger<OrganisationController>> _mockLogger;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ISessionHelper> _mockSessionHelper;
        private readonly Mock<IAddressLookupService> _mockAddressLookUpService;
        private readonly Mock<IOrganisationService> _mockOrganisationService;
        private readonly Mock<ICountriesAndTerritoriesService> _mockCountriesAndTerritoriesService;
        private readonly OrganisationController _controller;

        public OrganisationControllerTests()
        {
            _mockCompaniesHouseService = new Mock<ICompaniesHouseService>();
            _mockLogger = new Mock<ILogger<OrganisationController>>();
            _mockUserService = new Mock<IUserService>();
            _mockSessionHelper = new Mock<ISessionHelper>();
            _mockAddressLookUpService = new Mock<IAddressLookupService>();
            _mockOrganisationService = new Mock<IOrganisationService>();
            _mockCountriesAndTerritoriesService = new Mock<ICountriesAndTerritoriesService>();
            _controller = CreateController();
        }

        private OrganisationController CreateController()
        {
            var controller = new OrganisationController(
                _mockCompaniesHouseService.Object,
                _mockLogger.Object,
                _mockUserService.Object,
                _mockSessionHelper.Object,
                _mockAddressLookUpService.Object,
                _mockCountriesAndTerritoriesService.Object,
                _mockOrganisationService.Object
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

        [Fact]
        public void Start_RedirectsToOrganisationType_And_CallsSessionHelpers()
        {
            // Arrange
            var controller = CreateController();
            var httpContext = controller.ControllerContext.HttpContext;

            // Act
            var result = controller.Start();

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("OrganisationType", redirect.ActionName);

            _mockSessionHelper.Verify(x => x.ClearAllFlowRelatedSessionData(httpContext), Times.Once);
            _mockSessionHelper.Verify(x => x.SetIsCheckAnswerFlow(httpContext, false), Times.Once);
        }

        [Fact]
        public void OrganisationType_ReturnsView_WithExpectedModelAndViewBag()
        {
            var expectedModel = new OrganisationModel
            {
                OrganisationTypes = new List<SelectListItem>(),
                SelectedOrganisationType = "UkCompaniesHouse"
            };

            _mockSessionHelper
                .Setup(x => x.GetFromSession<OrganisationModel>(_controller.ControllerContext.HttpContext, SessionKeys.OrganisationCreation_SessionKey))
                .Returns(expectedModel);

            _mockSessionHelper
                .Setup(x => x.GetIsCheckAnswerFlow(_controller.ControllerContext.HttpContext))
                .Returns(false);
            _controller.Url = SetUpBackLink("Index", "Home").Object;

            var result = _controller.OrganisationType();


            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OrganisationModel>(viewResult.Model);

            Assert.Equal(expectedModel.SelectedOrganisationType, model.SelectedOrganisationType);
            Assert.NotNull(model.OrganisationTypes);
        }

        [Fact]
        public void Type_InvalidModel_ReturnsViewWithModel()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("SelectedOrganisationType", "Required");

            var model = new OrganisationModel();
            var result = controller.OrganisationType(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("OrganisationType", viewResult.ViewName);
            Assert.IsType<OrganisationModel>(viewResult.Model);
        }

        [Fact]
        public void Type_ValidModel_RedirectsToCompanyNumberOrOrganisationName()
        {
            var controller = CreateController();
            var model = new OrganisationModel
            {
                SelectedOrganisationType = "UkCompaniesHouse"
            };
            controller.ModelState.Clear();

            var result = controller.OrganisationType(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.True(redirect.ActionName == "CompanyNumber" || redirect.ActionName == "OrganisationName");
        }


        /* Test cases for DHB-293 only */
        [Fact]
        public async Task OrganisationAddress_ReturnsViewWithModel()
        {
            // Arange & Act
            _controller.Url = SetUpBackLink("OrganisationName", "Organisation").Object;
            var result = await _controller.OrganisationAddressAsync() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("OrganisationAddress", result.ViewName);
            Assert.IsType<AddressByStreetOrTownModel>(result.Model);
        }

        [Fact]
        public async Task OrganisationAddressByPostcode_ValidPostcode_ReturnsSearchResultsView()
        {
            // Arrange
            _mockAddressLookUpService.Setup(s => s.PostcodeLookupAsync("UB3 4JT"))
                .ReturnsAsync(new SearchAddressByPostcodeModel
                {
                    Postcode = "UB3 4JT",
                    Addresses = ["10 Downing Street, London, UB3 4JT"]
                });

            _controller.Url = SetUpBackLink("OrganisationName", "Organisation").Object;


            // Act
            var result = await _controller.OrganisationAddressByPostcode(new SearchAddressByPostcodeModel { Postcode = "UB3 4JT" }) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("OrganisationAddressSearchResults", result.ViewName);
            var model = Assert.IsType<SearchAddressByPostcodeModel>(result.Model);
            Assert.Single(model.Addresses);
            Assert.Equal(model.Postcode, "UB3 4JT");
        }

        [Fact]
        public void OrganisationAddressByPostcode_ReturnsNewModel_WhenSessionIsNull()
        {
            // Arrange
            _mockSessionHelper.Setup(s => s.GetFromSession<SearchAddressByPostcodeModel>(
                It.IsAny<HttpContext>(), It.IsAny<string>()))
                .Returns((SearchAddressByPostcodeModel)null);
            _controller.Url = SetUpBackLink("OrganisationName", "Organisation").Object;

            // Act
            var result = _controller.OrganisationAddressByPostcode() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SearchAddressByPostcodeModel>(result.Model);
            Assert.Null(((SearchAddressByPostcodeModel)result.Model).Postcode);
        }

        [Fact]
        public async Task OrganisationAddressByPostcode_ApiThrowsException_ReturnsSameViewWithError()
        {
            // Arrange
            _mockAddressLookUpService.Setup(s => s.PostcodeLookupAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("API failure"));
            _controller.Url = SetUpBackLink("OrganisationName", "Organisation").Object;
            // Act
            var result = await _controller.OrganisationAddressByPostcode(new SearchAddressByPostcodeModel { Postcode = "UB3 4JT" }) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("OrganisationAddressByPostcode", result.ViewName);
            Assert.True(_controller.ModelState.ContainsKey(string.Empty));
            Assert.Equal("Unable to retrieve address data.", _controller.ModelState[string.Empty].Errors[0].ErrorMessage);
        }        

        [Fact]
        public async Task SaveOrganisationAddress_ValidPostcode_RedirectsToCompanyConfirm()
        {
            // Arrange

            var model = new AddressByStreetOrTownModel
            {
                StreetAddress = "123 Baker Street",
                TownOrCity = "London",
                Postalcode = "NW1 6XE",
                Country = "United Kingdom"
            };
            var organisationModel = new OrganisationModel { CompanyDetails = new CompanyDetailsModel { RegisteredOfficeAddress = new RegisteredOfficeAddressModel() } };

            _mockSessionHelper
                .Setup(x => x.GetFromSession<OrganisationModel>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationCreation_SessionKey))
                .Returns(organisationModel);


            // Act
            var result = await _controller.SaveOrganisationAddressAsync(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CompanyConfirm", redirectResult.ActionName);
            Assert.Equal(model.Postalcode, organisationModel.CompanyDetails.RegisteredOfficeAddress.PostalCode);
        }

        [Fact]
        public async Task SaveOrganisationAddress_InvalidPostcode_ReturnsViewWithError()
        {
            // Arrange
            var model = new AddressByStreetOrTownModel
            {
                StreetAddress = "123 Baker Street",
                TownOrCity = "London",
                Postalcode = "INVALID",
                Country = "United Kingdom"
            };
            _controller.Url = SetUpBackLink("OrganisationName", "Organisation").Object;

            // Act
            var result = await _controller.SaveOrganisationAddressAsync(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("OrganisationAddress", viewResult.ViewName);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(nameof(model.Postalcode)));
            Assert.Equal("Please enter a valid UK postcode.", _controller.ModelState[nameof(model.Postalcode)].Errors.First().ErrorMessage);
        }

        [Fact]
        public async Task UpdateOrganisationDetailsConfirmation_ValidSessionData_ReturnsView()
        {
            // Arrange

            var userId = "user123";
            var orgId = "org456";
            var organisationModel = new OrganisationModel
            {
                CompanyDetails = new CompanyDetailsModel
                {
                    Title = "Test Company",
                    RegisteredOfficeAddress = new RegisteredOfficeAddressModel
                    {
                        AddressLine1 = "123 Street",
                        Locality = "Town",
                        PostalCode = "AB12CD",
                        Country = "UK"
                    }
                },
                SelectedOrganisationType = "PrivateLimitedCompany",
                CompanyNumber = "12345678"
            };

            var userDetails = TestingUtility.MockValid_UserService_GetUserDetails(userId);

            _mockSessionHelper.Setup(x => x.GetFromSession<OrganisationModel>(It.IsAny<HttpContext>(), SessionKeys.OrganisationCreation_SessionKey))
                .Returns(organisationModel);
            _mockSessionHelper.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns(userId);
            _mockUserService.Setup(x => x.GetUserDetails(userId)).ReturnsAsync(userDetails);
            _mockOrganisationService
                .Setup(x => x.EditOrganisationDetails(orgId, It.IsAny<OrganisationRequest>(), userId))
                .ReturnsAsync(new User());



            // Act
            var result = await _controller.UpdateOrganisationDetailsConfirmation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task UpdateOrganisationDetailsConfirmation_MissingCompanyDetails_ReturnsBadRequest()
        {
            // Arrange            
            var organisationModel = new OrganisationModel
            {
                CompanyDetails = null,
                SelectedOrganisationType = "PrivateLimitedCompany"
            };
            _mockSessionHelper.Setup(x => x.GetFromSession<OrganisationModel>(It.IsAny<HttpContext>(), SessionKeys.OrganisationCreation_SessionKey))
                .Returns(organisationModel);

            // Act
            var result = await _controller.UpdateOrganisationDetailsConfirmation();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Required session data is missing.", badRequestResult.Value);
        }
    }
}