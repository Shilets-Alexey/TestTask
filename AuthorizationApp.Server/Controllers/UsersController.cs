using AuthorizationApp.BusinessLogic.Services;
using AuthorizationApp.Web.Models;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationApp.Web.Controllers
{

    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;

        public UsersController(UsersService service)
        {
            _usersService = service;
        }

        [Authorize]
        [HttpGet]
        public Task<ServiceResult<IEnumerable<User>>> GetUsersc()
        {
            return _usersService.GetUsersAsync(User.IsInRole("Admin"));
        }

        [Authorize]
        [HttpGet, Route("current")]
        public async Task<ServiceResult<User>> GetCurrentUserAsync()
        {
            return  await _usersService.GetCurrentUserAsync(User.Identity.Name);
        }

        [Authorize]
        [HttpPatch, Route("{userId}/photo")]
        public async Task<ValidationResult> UpdatePhotoAsync([FromForm] IFormFile file, Guid userId)
        {
           
            return await _usersService.UpdatePhotoAsync(file, userId, User.Identity.Name);
        }

        [HttpPost, Route("login")]
        public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> LoginAsync(
            [FromBody] LoginRequest login, 
            [FromQuery] bool? useCookies,
            [FromQuery] bool? useSessionCookies)
        {
            return await _usersService.LoginAsync(login, useCookies, useSessionCookies);
        }

        [HttpPost, Route("register")]
        public async Task<ValidationResult> RegisterAsync([FromBody] RegisterRequest registration)
        {
            return await _usersService.RegisterAsync(registration);
        }

        [Authorize]
        [HttpPost, Route("logout")]
        public async Task LogoutAsync()
        {
             await _usersService.LogoutAsync();
        }

        [HttpGet, Route("pingauth")]
        public ServiceResult<object> PingAuth()
        {
            return _usersService.PingAuth(User.Identity.IsAuthenticated, User.Identity.Name);
        }


    }
}
