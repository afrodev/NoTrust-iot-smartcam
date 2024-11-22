using CameraBackend.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CameraBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SessionService _sessionService;

        public AuthController(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpPost("signin")]
        public IActionResult SignIn()
        {
            _sessionService.IsAuthenticated = true;
            return Ok();
        }

        [HttpPost("signout")]
        public new IActionResult SignOut()
        {
            _sessionService.IsAuthenticated = false;
            return Ok();
        }
    }
}
