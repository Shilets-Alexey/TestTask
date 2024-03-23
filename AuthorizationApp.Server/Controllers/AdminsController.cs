using AuthorizationApp.BusinessLogic.Services;
using AuthorizationApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationApp.Web.Controllers
{
    [Route("admins")]
    [ApiController]
    public class AdminsController : ControllerBase
    {
        private readonly AdminsService _adminsService;

        public AdminsController(AdminsService adminsService)
        {
            _adminsService = adminsService;
        }

        [Authorize]
        [HttpDelete, Route("{userId}")]
        public async Task<ValidationResult> DeleteUserAsync(Guid userId)
        {   
            return await _adminsService.DeleteUserAsync(userId, User.Identity.Name, User.IsInRole("Admin"));
        }

        [Authorize]
        [HttpPatch, Route("{userId}/role/{role}")]
        public async Task<ValidationResult> SetRoleAsync(Guid userId, string role)
        {
            return await _adminsService.SetRoleAsync(userId, role, User.IsInRole("Admin"));
        }
    }
}
