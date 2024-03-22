using AuthorizationApp.Server.Helpers;
using AuthorizationApp.Server.Models;
using Microsoft.AspNetCore.Http;
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

        [HttpDelete, Route("{userId}")]
        public async Task<IResult> DeleteUser(Guid userId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "Need sign in" } } });
            }
            if (!User.IsInRole("Admin"))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "You don't have admin rights" } } });
            }
            ApplicationUser CurrntUserInfo = await _userManager.FindByNameAsync(User.Identity.Name);
            if (CurrntUserInfo.Id == userId.ToString())
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "Can't delete current user" } } });
            }
            ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == default)
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "User not exist" } } });
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (WorkWithUsersHelper.HasAdminRights(roles))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "Can't delete users with admin righs" } } });
            }
            await _userManager.DeleteAsync(user);
            return Results.Ok();
        }

        [HttpPatch, Route("{userId}/role/{roleId}")]
        public async Task<IResult> SetAdminRights(Guid userId, Guid roleId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "Need sign in" } } });
            }
            if (!User.IsInRole("Admin"))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "NotAdmin" } } });
            }
            ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != default)
            {

                await _userManager.AddToRoleAsync(user, "Admin");
            }
            return Results.Ok();
        }
    }
}
