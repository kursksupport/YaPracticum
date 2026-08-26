using Microsoft.AspNetCore.Mvc;
using Users.Application;

namespace Users.Api;

[ApiController, Route("auth")] public sealed class AuthController(IUserService service) : ControllerBase
{
    [HttpPost("register")] public async Task<IActionResult> Register(RegisterRequest request) 
    { 
        await service.RegisterAsync(request); return NoContent(); 
    }
    [HttpPost("login")] public async Task<IActionResult> Login(LoginRequest request) => Ok(new { token = await service.LoginAsync(request) });
}
