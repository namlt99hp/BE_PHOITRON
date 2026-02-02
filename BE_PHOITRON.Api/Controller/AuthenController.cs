using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_PHOITRON.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenController(IAuthenService service) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(loginDto.username) || string.IsNullOrEmpty(loginDto.password))
                return BadRequest(new { message = "Thiếu tên tài khoản hoặc mật khẩu" });

            var result = await service.LoginAsync(loginDto, ct);

            if (result == null)
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu" });

            return Ok(result);
        }

    }
}
