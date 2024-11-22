using CameraBackend.Api.Controllers;
using CameraBackend.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CameraBackend.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly AuthController _controller;
    private readonly SessionService _sessionService;

    public AuthControllerTests()
    {
        _sessionService = new SessionService();
        _controller = new AuthController(_sessionService);
    }

    [Fact]
    public void SignIn_ShouldReturnOkResult()
    {
        // Act
        var result = _controller.SignIn();

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public void SignIn_ShouldSetIsAuthenticatedToTrue()
    {
        // Act
        _controller.SignIn();

        // Assert
        Assert.True(_sessionService.IsAuthenticated);
    }

    [Fact]
    public void SignOut_ShouldReturnOkResult()
    {
        // Act
        var result = _controller.SignOut();

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public void SignOut_ShouldSetIsAuthenticatedToFalse()
    {
        // Arrange
        _sessionService.IsAuthenticated = true;

        // Act
        _controller.SignOut();

        // Assert
        Assert.False(_sessionService.IsAuthenticated);
    }
}