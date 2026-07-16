using GovUk.OneLogin.AspNetCore;
using HNTAS.Web.UI.Controllers;
using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Session;
using Moq;
//using Xunit;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class AccountControllerTests
    {
        private Mock<ISession> _mockSession;
        private Mock<HttpContext> _mockHttpContext;
        private AccountController _controller;

        public AccountControllerTests()
        {
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();
            _controller = new AccountController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
            _mockHttpContext.Setup(x => x.Session).Returns(_mockSession.Object);
        }

        #region SignIn Tests

        [Fact]
        public void SignIn_WithDefaultReturnUrl_ReturnsChallengeResult()
        {
            // Arrange
            var defaultUrl = "/";

            // Act
            var result = _controller.SignIn();

            // Assert
            var challengeResult = Assert.IsType<ChallengeResult>(result);
            Assert.Contains(OneLoginDefaults.AuthenticationScheme, challengeResult.AuthenticationSchemes);
            Assert.NotNull(challengeResult.Properties);
            Assert.Equal(defaultUrl, challengeResult.Properties.RedirectUri);
        }

        [Theory]
        [InlineData("/dashboard")]
        [InlineData("/profile")]
        [InlineData("/admin/panel")]
        public void SignIn_WithCustomReturnUrl_ReturnsChallengeResultWithCorrectRedirectUri(string returnUrl)
        {
            // Act
            var result = _controller.SignIn(returnUrl);

            // Assert
            var challengeResult = Assert.IsType<ChallengeResult>(result);
            Assert.Contains(OneLoginDefaults.AuthenticationScheme, challengeResult.AuthenticationSchemes);
            Assert.Equal(returnUrl, challengeResult.Properties.RedirectUri);
        }

        #endregion

        #region SignOut Tests

        [Fact]
        public async Task SignOut_ClearsSessionAndSignsOutFromCookie()
        {
            // Arrange
            var mockAuthService = new Mock<IAuthenticationService>();
            _mockHttpContext
                .Setup(x => x.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(mockAuthService.Object);

            // Act
            var result = _controller.SignOut();

            // Assert
            _mockSession.Verify(x => x.Clear(), Times.Once);
        }

        [Fact]
        public async Task SignOut_WithSimulatorEnabled_ReturnsSignOutResultWithGovUkSimulatorScheme()
        {
            // Arrange
            Environment.SetEnvironmentVariable("SIMULATOR_PROP4", "true");
            var mockAuthService = new Mock<IAuthenticationService>();
            _mockHttpContext
                .Setup(x => x.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(mockAuthService.Object);

            // Act
            var result = await _controller.SignOut();

            // Assert
            var signOutResult = Assert.IsType<SignOutResult>(result);
            Assert.Contains("GovUkSimulator", signOutResult.AuthenticationSchemes);
            Environment.SetEnvironmentVariable("SIMULATOR_PROP4", null);
        }

        [Fact]
        public async Task SignOut_WithSimulatorDisabled_ReturnsSignOutResultWithOneLoginScheme()
        {
            // Arrange
            Environment.SetEnvironmentVariable("SIMULATOR_PROP4", "false");
            var mockAuthService = new Mock<IAuthenticationService>();
            _mockHttpContext
                .Setup(x => x.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(mockAuthService.Object);

            // Act
            var result = await _controller.SignOut();

            // Assert
            var signOutResult = Assert.IsType<SignOutResult>(result);
            Assert.Contains(OneLoginDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
            Environment.SetEnvironmentVariable("SIMULATOR_PROP4", null);
        }

        [Fact]
        public async Task SignOut_WithoutSimulatorEnvironmentVariable_ReturnsSignOutResultWithOneLoginScheme()
        {
            // Arrange
            Environment.SetEnvironmentVariable("SIMULATOR_PROP4", null);
            var mockAuthService = new Mock<IAuthenticationService>();
            _mockHttpContext
                .Setup(x => x.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(mockAuthService.Object);

            // Act
            var result = await _controller.SignOut();

            // Assert
            var signOutResult = Assert.IsType<SignOutResult>(result);
            Assert.Contains(OneLoginDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
        }

        [Theory]
        [InlineData("true")]
        [InlineData("True")]
        [InlineData("TRUE")]
        public async Task SignOut_WithSimulatorEnabledCaseInsensitive_ReturnsSignOutResultWithGovUkSimulatorScheme(string simulatorValue)
        {
            // Arrange
            Environment.SetEnvironmentVariable("SIMULATOR_PROP4", simulatorValue);
            var mockAuthService = new Mock<IAuthenticationService>();
            _mockHttpContext
                .Setup(x => x.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(mockAuthService.Object);

            // Act
            var result = await _controller.SignOut();

            // Assert
            var signOutResult = Assert.IsType<SignOutResult>(result);
            Assert.Contains("GovUkSimulator", signOutResult.AuthenticationSchemes);
            Environment.SetEnvironmentVariable("SIMULATOR_PROP4", null);
        }

        #endregion

        #region OneLoginCallback Tests

        [Fact]
        public void OneLoginCallback_ReturnsRedirectToHomeIndex()
        {
            // Act
            var result = _controller.OneLoginCallback();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            Assert.Null(redirectResult.RouteValues);
        }

        [Fact]
        public void OneLoginCallback_RouteIsCorrect()
        {
            // Verify the route attribute matches expected callback path
            var methodInfo = typeof(AccountController).GetMethod(nameof(AccountController.OneLoginCallback));
            var routeAttribute = methodInfo.GetCustomAttributes(typeof(RouteAttribute), false)
                .FirstOrDefault() as RouteAttribute;

            Assert.NotNull(routeAttribute);
            Assert.Equal("/onelogin-callback", routeAttribute.Template);
        }

        #endregion

        #region OneLoginLogoutCallback Tests

        [Fact]
        public void OneLoginLogoutCallback_ReturnsRedirectToHomeIndex()
        {
            // Act
            var result = _controller.OneLoginLogoutCallback();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            Assert.Null(redirectResult.RouteValues);
        }

        [Fact]
        public void OneLoginLogoutCallback_RouteIsCorrect()
        {
            // Verify the route attribute matches expected callback path
            var methodInfo = typeof(AccountController).GetMethod(nameof(AccountController.OneLoginLogoutCallback));
            var routeAttribute = methodInfo.GetCustomAttributes(typeof(RouteAttribute), false)
                .FirstOrDefault() as RouteAttribute;

            Assert.NotNull(routeAttribute);
            Assert.Equal("/onelogin-logout-callback", routeAttribute.Template);
        }

        #endregion

        #region AccessDenied Tests

        [Fact]
        public void AccessDenied_ReturnsRedirectToHomeErrorWith403Code()
        {
            // Act
            var result = _controller.AccessDenied();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Error", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            Assert.NotNull(redirectResult.RouteValues);
            Assert.True(redirectResult.RouteValues.ContainsKey("code"));
            Assert.Equal(403, redirectResult.RouteValues["code"]);
        }

        [Fact]
        public void AccessDenied_IsAllowAnonymous()
        {
            // Verify the method has AllowAnonymous attribute
            var methodInfo = typeof(AccountController).GetMethod(nameof(AccountController.AccessDenied));
            var allowAnonymousAttribute = methodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), false)
                .FirstOrDefault() as AllowAnonymousAttribute;

            Assert.NotNull(allowAnonymousAttribute);
        }

        #endregion

        #region Authorization Tests

        [Fact]
        public void SignIn_IsAllowAnonymous()
        {
            var methodInfo = typeof(AccountController).GetMethod(nameof(AccountController.SignIn), new[] { typeof(string) });
            var allowAnonymousAttribute = methodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), false)
                .FirstOrDefault() as AllowAnonymousAttribute;

            Assert.NotNull(allowAnonymousAttribute);
        }

        [Fact]
        public void SignOut_IsAuthorized()
        {
            var methodInfo = typeof(AccountController).GetMethod(nameof(AccountController.SignOut), new[] { typeof(string) });
            var authorizeAttribute = methodInfo.GetCustomAttributes(typeof(AuthorizeAttribute), false)
                .FirstOrDefault() as AuthorizeAttribute;

            Assert.NotNull(authorizeAttribute);
        }

        [Fact]
        public void OneLoginCallback_IsAllowAnonymous()
        {
            var methodInfo = typeof(AccountController).GetMethod(nameof(AccountController.OneLoginCallback), new[] { typeof(string) });
            var allowAnonymousAttribute = methodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), false)
                .FirstOrDefault() as AllowAnonymousAttribute;

            Assert.NotNull(allowAnonymousAttribute);
        }

        [Fact]
        public void OneLoginLogoutCallback_IsAllowAnonymous()
        {
            var methodInfo = typeof(AccountController).GetMethod(nameof(AccountController.OneLoginLogoutCallback), new[] { typeof(string) });
            var allowAnonymousAttribute = methodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), false)
                .FirstOrDefault() as AllowAnonymousAttribute;

            Assert.NotNull(allowAnonymousAttribute);
        }

        #endregion
    }
}