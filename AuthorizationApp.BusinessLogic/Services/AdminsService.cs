using AuthorizationApp.Web.Helpers;
using AuthorizationApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationApp.BusinessLogic.Services
{
    public class AdminsService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminsService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ValidationResult> DeleteUserAsync(Guid userId, string currentUserNae, bool isCurrntUserAdmin)
        {
            ApplicationUser currntUser = await _userManager.FindByNameAsync(currentUserNae);
            ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
            ValidationResult validationInfo = ValidationHelper.OnDeleteUserValidate(isCurrntUserAdmin, currntUser, user);
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

        public async Task<ValidationResult> SetRoleAsync(Guid userId, string role, bool isCurrntUserAdmin)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
            ValidationResult validationInfo = ValidationHelper.OnSetRoleValidate(isCurrntUserAdmin, user, role);
            if (!validationInfo.IsValid)
            {
                return validationInfo;
            }
            await _userManager.AddToRoleAsync(user, role);
            return validationInfo;
        }
    }
}
