using CameraBackend.Api.Controllers;
using CameraBackend.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CameraBackend.Api.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly SessionService _sessionService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _sessionService = new SessionService();
            _controller = new AuthController(_sessionService);
        }

        [Fact]
        public void SignIn_ShouldAuthenticateAndReturnOk()
        {
            // Act
            var result = _controller.SignIn();

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.True(_sessionService.IsAuthenticated);
        }

        [Fact]
        public void SignOut_ShouldDeauthenticateAndReturnOk()
        {
            // Arrange
            _sessionService.IsAuthenticated = true;

            // Act
            var result = _controller.SignOut();

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.False(_sessionService.IsAuthenticated);
        }
    }
}
