using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Models.CompaniesHouse;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
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
        private readonly Mock<ICompaniesHouseService> _companiesHouseServiceMock;
        private readonly Mock<ILogger<OrganisationController>> _loggerMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ISessionHelper> _sessionHelperMock;
        private readonly Mock<IAddressLookupService> _addressLookUpServiceMock;
        private readonly OrganisationController _controller;

        public OrganisationControllerTests()
        {
            _companiesHouseServiceMock = new Mock<ICompaniesHouseService>();
            _loggerMock = new Mock<ILogger<OrganisationController>>();
            _userServiceMock = new Mock<IUserService>();
            _sessionHelperMock = new Mock<ISessionHelper>();
            _addressLookUpServiceMock = new Mock<IAddressLookupService>();
            _controller = CreateController();
        }

        private OrganisationController CreateController()
        {
            var controller = new OrganisationController(
                _companiesHouseServiceMock.Object,
                _loggerMock.Object,
                _userServiceMock.Object,
                _sessionHelperMock.Object,
                _addressLookUpServiceMock.Object
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

            _sessionHelperMock.Verify(x => x.ClearAllFlowRelatedSessionData(httpContext), Times.Once);
            _sessionHelperMock.Verify(x => x.SetIsCheckAnswerFlow(httpContext, false), Times.Once);
        }

        [Fact]
        public void OrganisationType_ReturnsView_WithExpectedModelAndViewBag()
        {
            var expectedModel = new OrganisationModel
            {
                OrganisationTypes = new List<SelectListItem>(),
                SelectedOrganisationType = "UkCompaniesHouse"
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<OrganisationModel>(_controller.ControllerContext.HttpContext, SessionKeys.OrganisationCreation_SessionKey))
                .Returns(expectedModel);

            _sessionHelperMock
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
        public void OrganisationAddress_ReturnsViewWithModel()
        {
            // Arange & Act
            _controller.Url = SetUpBackLink("OrganisationName", "Organisation").Object;
            var result = _controller.OrganisationAddress() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("OrganisationAddress", result.ViewName);
            Assert.IsType<AddressByStreetOrTownModel>(result.Model);
        }

        [Fact]
        public async Task OrganisationAddressByPostcode_ValidPostcode_ReturnsSearchResultsView()
        {
            // Arrange
            _addressLookUpServiceMock.Setup(s => s.PostcodeLookupAsync("UB3 4JT"))
                .ReturnsAsync(new SearchAddressByPostcodeModel
                {
                    Postcode = "UB3 4JT",
                    Addresses = [ "10 Downing Street, London, UB3 4JT" ]
                });

            _controller.Url = SetUpBackLink("OrganisationName", "Organisation").Object;
            

            // Act
            var result = await _controller.OrganisationAddressByPostcode("UB3 4JT") as ViewResult;

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
            _sessionHelperMock.Setup(s => s.GetFromSession<SearchAddressByPostcodeModel>(
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
            _addressLookUpServiceMock.Setup(s => s.PostcodeLookupAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("API failure"));
            _controller.Url = SetUpBackLink("OrganisationName", "Organisation").Object;
            // Act
            var result = await _controller.OrganisationAddressByPostcode("UB3 4JT") as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("OrganisationAddressByPostcode", result.ViewName);
            Assert.True(_controller.ModelState.ContainsKey(string.Empty));
            Assert.Equal("Unable to retrieve address data.", _controller.ModelState[string.Empty].Errors[0].ErrorMessage);
        }

        [Fact]
        public void OrganisationAddressSearchResults_ReturnsViewWithModel()
        {
            // Arrange
            var model = new SearchAddressByPostcodeModel
            {
                Postcode = "UB3 4JT",
                Addresses = new[] { "10 Downing Street, London" }
            };
            _controller.Url = SetUpBackLink("OrganisationAddressByPostcode", "Organisation").Object;

            // Act
            var result = _controller.OrganisationAddressSearchResults(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(model, result.Model);
        }

        [Fact]
        public void SelectAddress_ValidAddress_RedirectsToSaveOrganisationAddress()
        {
            // Arrange
            var selectedAddress = "123 Baker Street, London, NW1 6XE";
            var sessionModel = new SearchAddressByPostcodeModel();

            _sessionHelperMock
                .Setup(x => x.GetFromSession<SearchAddressByPostcodeModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.SearchAddressByPostcodeModelSessionKey))
                .Returns(sessionModel);

            // Act
            var result = _controller.SelectAddress(selectedAddress);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("SaveOrganisationAddressByPostcode", redirectResult.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession<AddressByStreetOrTownModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.AddressByStreetOrTownModelSessionKey,
                It.IsAny<AddressByStreetOrTownModel>()), Times.Once);
        }

        [Fact]
        public void SelectAddress_MalformedAddress_ThrowsArgumentException()
        {
            // Arrange
            var malformedAddress = "OnlyStreet"; // Not enough parts
            var sessionModel = new SearchAddressByPostcodeModel();

            _sessionHelperMock
                .Setup(x => x.GetFromSession<SearchAddressByPostcodeModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.SearchAddressByPostcodeModelSessionKey))
                .Returns(sessionModel);

            // Act & Assert
            var result = _controller.SelectAddress(malformedAddress);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Selected address is not in the expected format. It must contain at least street, town/city, and postcode.", badRequestResult.Value);
        }

        [Fact]
        public void SaveOrganisationAddressByPostcode_ValidSession_RedirectsToCompanyConfirm()
        {
            // Arrange

            var addressModel = new AddressByStreetOrTownModel { Fulladdress = "123 Baker Street, London, NW1 6XE" };
            var organisationModel = new OrganisationModel { CompanyDetails = new CompanyDetailsModel { RegisteredOfficeAddress = new RegisteredOfficeAddressModel()} };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AddressByStreetOrTownModel>(
                    It.IsAny<HttpContext>(), SessionKeys.AddressByStreetOrTownModelSessionKey))
                .Returns(addressModel);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<OrganisationModel>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationCreation_SessionKey))
                .Returns(organisationModel);            

            // Act
            var result = _controller.SaveOrganisationAddressByPostcode();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CompanyConfirm", redirectResult.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(), SessionKeys.OrganisationCreation_SessionKey, organisationModel), Times.Once);


            Assert.Equal(addressModel.StreetAddress, organisationModel.CompanyDetails.RegisteredOfficeAddress.AddressLine1);
            Assert.Equal(addressModel.Postalcode, organisationModel.CompanyDetails.RegisteredOfficeAddress.PostalCode);
            Assert.Equal(addressModel.Country, organisationModel.CompanyDetails.RegisteredOfficeAddress.Country);
        }

        [Fact]
        public void SaveOrganisationAddressByPostcode_MissingOrganisationModel_ReturnsBadRequest()
        {
            // Arrange

            var addressModel = new AddressByStreetOrTownModel { Fulladdress = "123 Baker Street, London, NW1 6XE" };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<AddressByStreetOrTownModel>(
                    It.IsAny<HttpContext>(), SessionKeys.AddressByStreetOrTownModelSessionKey))
                .Returns(addressModel);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<OrganisationModel>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationCreation_SessionKey))
                .Returns((OrganisationModel)null); // Simulate missing organisation model

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.SaveOrganisationAddressByPostcode();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing session data", badRequestResult.Value);
        }

        [Fact]
        public void SaveOrganisationAddress_ValidPostcode_RedirectsToCompanyConfirm()
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

            _sessionHelperMock
                .Setup(x => x.GetFromSession<OrganisationModel>(
                    It.IsAny<HttpContext>(), SessionKeys.OrganisationCreation_SessionKey))
                .Returns(organisationModel);

           
            // Act
            var result = _controller.SaveOrganisationAddress(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CompanyConfirm", redirectResult.ActionName);
            Assert.Equal(model.Postalcode, organisationModel.CompanyDetails.RegisteredOfficeAddress.PostalCode);
        }

        [Fact]
        public void SaveOrganisationAddress_InvalidPostcode_ReturnsViewWithError()
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
            var result = _controller.SaveOrganisationAddress(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("OrganisationAddress", viewResult.ViewName);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(nameof(model.Postalcode)));
            Assert.Equal("Please enter a valid UK postcode.", _controller.ModelState[nameof(model.Postalcode)].Errors.First().ErrorMessage);
        }
    }
}