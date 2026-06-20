using Core.DTOs.Auth;
using Core.Interfaces;
using ISHMS.BLL.Services;
using ISHMS.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ISHMS.API.Controllers
{
    [Route("api/[controller]")]
    //Auth

    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILdapAuthenticationService _ldapService;
        public AuthController(IAuthService authService, ILdapAuthenticationService ldapAuthenticationService)
        {
            _authService = authService;
            _ldapService = ldapAuthenticationService;
        }


        //[HttpPost("test-ldap")]
        //public IActionResult TestLdap(LoginDto dto)
        //{
        //    var result = _ldapAuthenticationService
        //        .Authenticate(dto.username, dto.Password);

        //    return Ok(new
        //    {
        //        ldapResult = result
        //    });
        //}
        // ==================== Register ====================

        [HttpPost("register")]
        //POST api/Auth/register
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(dto);

            if (!result.IsAuthenticated)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // ==================== Login ====================

        [HttpPost("login")]
        //POST api/Auth/login
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(dto);

            if (!result.IsAuthenticated)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        [HttpGet("test-user/{username}")]
        public IActionResult TestUser(string username)
        {
            var user = _ldapService.GetUserInfo(username);

            if (user == null)
                return NotFound();

            return Ok(user);
        }
    }
}