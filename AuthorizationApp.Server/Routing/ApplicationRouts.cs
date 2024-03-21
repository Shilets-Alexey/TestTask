using AuthorizationApp.Server.Helpers;
using AuthorizationApp.Server.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;

namespace AuthorizationApp.Server.Routing
{
    public static class ApplicationRouts
    {
        private static readonly EmailAddressAttribute _emailAddressAttribute = new();

        public static void AddApplicationRouts(this IEndpointRouteBuilder endpoints)
        {
            RouteGroupBuilder routeGroup = endpoints.MapGroup("");

            routeGroup.MapPost("/register", async Task<Results<Ok, ValidationProblem>>
            ([FromBody] RegisterRequest registration, HttpContext context, [FromServices] IServiceProvider sp) =>
            {
                UserManager<ApplicationUser> userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                if (!userManager.SupportsUserEmail)
                {
                    throw new NotSupportedException($"{nameof(AddApplicationRouts)} requires a user store with email support.");
                }

                IUserStore<ApplicationUser> userStore = sp.GetRequiredService<IUserStore<ApplicationUser>>();
                IUserEmailStore<ApplicationUser> emailStore = (IUserEmailStore<ApplicationUser>)userStore;
                string email = registration.Email;

                if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
                {
                    return WorkWithUsersHelper.CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)));
                }

                ApplicationUser user = new ApplicationUser();
                await userStore.SetUserNameAsync(user, email, CancellationToken.None);
                await emailStore.SetEmailAsync(user, email, CancellationToken.None);
                user.LastLoginDate = DateTime.UtcNow;
                user.SuccesLoggCount = 1;
                IdentityResult result = await userManager.CreateAsync(user, registration.Password);
                if (!result.Succeeded)
                {
                    return WorkWithUsersHelper.CreateValidationProblem(result);
                }
                SignInManager<ApplicationUser> signInManager = sp.GetRequiredService<SignInManager<ApplicationUser>>();
                await signInManager.SignInAsync(user, false);
                return TypedResults.Ok();
            });

            routeGroup.MapPost("/login", async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>>
                ([FromBody] LoginRequest login, [FromQuery] bool? useCookies, [FromQuery] bool? useSessionCookies, [FromServices] IServiceProvider sp) =>
            {
                var signInManager = sp.GetRequiredService<SignInManager<ApplicationUser>>();

                var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
                var isPersistent = (useCookies == true) && (useSessionCookies != true);
                signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

                var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent, lockoutOnFailure: true);

                if (result.RequiresTwoFactor)
                {
                    if (!string.IsNullOrEmpty(login.TwoFactorCode))
                    {
                        result = await signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode, isPersistent, rememberClient: isPersistent);
                    }
                    else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
                    {
                        result = await signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
                    }
                }

                if (!result.Succeeded)
                {
                    return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
                }
                UserManager<ApplicationUser> userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                ApplicationUser? user = await userManager.FindByEmailAsync(login.Email);
                if (user != null && user != default)
                {
                    user.LastLoginDate = DateTime.UtcNow;
                    user.SuccesLoggCount += 1;
                    await userManager.UpdateAsync(user);
                }
                return TypedResults.Empty;
            });

            routeGroup.MapGet("/users", (IServiceProvider sp, ClaimsPrincipal CutrrentUser) =>
            {

                if (!CutrrentUser.Identity.IsAuthenticated)
                {
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "Need sign in" } } });
                }
                ApplicationDbContext dbContext = sp.GetService<ApplicationDbContext>();
                UserManager<ApplicationUser> userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                List<ApplicationUser> users = dbContext.Users.ToList();
                List<UserOnClient> resultList = new List<UserOnClient>();
                users.ForEach(user =>
                {
                    var result = userManager.GetRolesAsync(user);
                    UserOnClient userOnClient = user;
                    userOnClient.UserRoles = WorkWithUsersHelper.GetRolesString(result.Result);
                    userOnClient.IsBtnVisible = !WorkWithUsersHelper.HasAdminRights(result.Result) && CutrrentUser.IsInRole("Admin");
                    resultList.Add(userOnClient);
                });
                return Results.Json(resultList);
            });

            routeGroup.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager) =>
            {

                await signInManager.SignOutAsync();
                return Results.Ok();

            }).RequireAuthorization();

            routeGroup.MapGet("/pingauth", (ClaimsPrincipal user) =>
            {
                return Results.Json(new { isAuthenticated = user.Identity.IsAuthenticated });
            });

            routeGroup.MapDelete("/deleteUser", async Task<IResult> (IServiceProvider sp, string userId, ClaimsPrincipal CutrrentUser) =>
            {
                if (!CutrrentUser.Identity.IsAuthenticated)
                {
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "Need sign in" } } });
                }
                if (!CutrrentUser.IsInRole("Admin"))
                {
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "You don't have admin rights" } } });
                }
                UserManager<ApplicationUser> userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                ApplicationUser CurrntUserInfo = await userManager.FindByNameAsync(CutrrentUser.Identity.Name);
                if(CurrntUserInfo.Id == userId)
                {
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "Can't delete current user" } } });
                }
                ApplicationUser? user = await userManager.FindByIdAsync(userId);
                if (user == default)
                {
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "User not exist" } } });  
                }
                var roles = await userManager.GetRolesAsync(user);
                if (WorkWithUsersHelper.HasAdminRights(roles))
                {
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "Can't delete users with admin righs" } } });
                }
                await userManager.DeleteAsync(user);
                return Results.Ok();
            });

            routeGroup.MapPost("/setAdminRights", async Task<IResult> (IServiceProvider sp, string userId, ClaimsPrincipal CutrrentUser) =>
            {
                if (!CutrrentUser.Identity.IsAuthenticated)
                {
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "Need sign in" } } });
                }
                if (!CutrrentUser.IsInRole("Admin"))
                {
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "NotAdmin" } } });
                }
                UserManager<ApplicationUser> userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                ApplicationUser? user = await userManager.FindByIdAsync(userId);
                if (user != default)
                {
                    
                    await userManager.AddToRoleAsync(user, "Admin");
                }
                return Results.Ok();
            });

            routeGroup.MapGet("/currentUser", async Task<IResult> (IServiceProvider sp, ClaimsPrincipal CutrrentUser) =>
            {
                if (!CutrrentUser.Identity.IsAuthenticated)
                {
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "Need sign in" } } });
                }
                UserManager<ApplicationUser> userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                UserOnClient user = await userManager.FindByNameAsync(CutrrentUser.Identity.Name);
                return Results.Json(user);
            });

            routeGroup.MapPatch("/updatePhoto", async Task<IResult> (IServiceProvider sp, ClaimsPrincipal CutrrentUser, [FromForm] IFormFile file, string userId) =>
            {
                UserManager<ApplicationUser> userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                ApplicationUser CurrentUserInfo = await userManager.FindByNameAsync(CutrrentUser.Identity.Name);
                if (!CutrrentUser.Identity.IsAuthenticated)
                {
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "Need sign in" } } });
                }
                if (CurrentUserInfo.Id != userId)
                {
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "userId", new string[] { "Can't upload profile pic to another user" } } });
                }
                ApplicationUser user = await userManager.FindByIdAsync(userId);
                if(user == default)
                {
                    return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "userId", new string[] { "User with this id dosen't exist" } } });
                }
                if (file != user.ImageFile)
                {
                    if (file != null)
                    {

                        WorkWithUsersHelper.SetImage(user, file);
                        var setImageResult = await userManager.UpdateAsync(user);
                        if (!setImageResult.Succeeded)
                        {
                            return WorkWithUsersHelper.CreateValidationProblem(setImageResult);
                        }
                    }
                }
                
                return Results.Ok();
            }).DisableAntiforgery();
        }
    }
}
