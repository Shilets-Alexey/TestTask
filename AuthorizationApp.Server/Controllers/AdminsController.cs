using AuthorizationApp.Server.Helpers;
using AuthorizationApp.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationApp.Server.Controllers
{
    [Route("admins")]
    [ApiController]
    public class AdminsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminsController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [Authorize]
        [HttpDelete, Route("{userId}")]
        public async Task<ValidationResult> DeleteUserAsync(Guid userId)
        {   
            ApplicationUser CurrntUserInfo = await _userManager.FindByNameAsync(User.Identity.Name);
            ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
            ValidationResult validationInfo = ValidationHelper.OnDeleteUserValidate(User, CurrntUserInfo, user);
            if (!validationInfo.IsValid)
            {
                return validationInfo;
            }
            IList<string> roles = await _userManager.GetRolesAsync(user);
            validationInfo = ValidationHelper.HasAdminRightsValidate(roles);
            if (!validationInfo.IsValid)
            {
                return validationInfo;
            }
            await _userManager.DeleteAsync(user);
            return validationInfo;
        }

        [Authorize]
        [HttpPatch, Route("{userId}/role/{role}")]
        public async Task<ValidationResult> SetRoleAsync(Guid userId, string role)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
            ValidationResult validationInfo = ValidationHelper.OnSetRoleValidate(User, user, role);
            if (!validationInfo.IsValid)
            {
                return validationInfo;
            }
            await _userManager.AddToRoleAsync(user, role);
            return validationInfo;
        }
    }
}
